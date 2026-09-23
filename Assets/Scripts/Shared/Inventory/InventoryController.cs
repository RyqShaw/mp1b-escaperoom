using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DefaultExecutionOrder(-110)]
public class InventoryController : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string id, targetId, displayName, state;
        public InventoryItem prefab;
    }

    [Header("Assign in the scene Inspector")]
    public XRBaseInputInteractor leftHand;
    public XRBaseInputInteractor rightHand;
    public InputActionReference toggleAction;
    public InputActionReference leftClickAction, rightClickAction;
    public InventoryPanel panel;

    public static InventoryController Instance { get; private set; }
    public static bool BlocksWorld => Instance != null && (Instance.IsOpen || Instance.waitForTriggerRelease);
    public bool IsOpen { get; private set; }
    public string Feedback { get; private set; } = "Point and press Trigger. Left Primary opens or closes inventory.";
    public IReadOnlyList<Entry> Entries => entries;

    readonly List<Entry> entries = new List<Entry>();
    readonly HashSet<string> spawned = new HashSet<string>();
    readonly Dictionary<string, InventoryItem> live = new Dictionary<string, InventoryItem>();
    readonly XRInputButtonReader blockedGrip = new XRInputButtonReader(inputSourceMode: XRInputButtonReader.InputSourceMode.Unused);
    IXRInputButtonReader leftGripBeforeMenu, rightGripBeforeMenu;
    bool leftActivateBeforeMenu, rightActivateBeforeMenu;
    bool inputsBlocked, waitForTriggerRelease;
    int lastTransactionFrame = -1, lastToggleFrame = -1;
    InventoryItem retrieving;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() { Instance = null; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        if (leftHand == null || rightHand == null || panel == null || toggleAction == null || toggleAction.action == null ||
            leftClickAction == null || leftClickAction.action == null || rightClickAction == null || rightClickAction.action == null)
        {
            Debug.LogError("Assign inventory hands, Toggle and both Click actions, and the panel in the Inspector.", this);
            enabled = false;
            return;
        }
        toggleAction.action.Enable();
        leftClickAction.action.Enable();
        rightClickAction.action.Enable();
    }

    void Update()
    {
        if (toggleAction.action.WasPressedThisFrame()) SetOpen(!IsOpen);
        if (!IsOpen && Time.frameCount > lastToggleFrame &&
            !leftClickAction.action.IsPressed() && !rightClickAction.action.IsPressed())
        {
            waitForTriggerRelease = false;
            SetInputsBlocked(false);
        }
    }

    public void SetOpen(bool open)
    {
        IsOpen = open;
        waitForTriggerRelease = true;
        lastToggleFrame = Time.frameCount;
        SetInputsBlocked(true);
        panel.SetVisible(open);
        if (open && InteractionHUD.Instance != null) InteractionHUD.Instance.panel.SetActive(false);
    }

    void SetInputsBlocked(bool blocked)
    {
        if (inputsBlocked == blocked) return;
        inputsBlocked = blocked;
        if (blocked)
        {
            leftGripBeforeMenu = leftHand.selectInput.bypass;
            rightGripBeforeMenu = rightHand.selectInput.bypass;
            leftActivateBeforeMenu = leftHand.allowActivate;
            rightActivateBeforeMenu = rightHand.allowActivate;
        }
        // Toggle keeps the existing hold while the menu ignores new Grip presses.
        if (leftHand != null)
        {
            leftHand.selectInput.bypass = blocked ? blockedGrip : leftGripBeforeMenu;
            leftHand.allowActivate = !blocked && leftActivateBeforeMenu;
        }
        if (rightHand != null)
        {
            rightHand.selectInput.bypass = blocked ? blockedGrip : rightGripBeforeMenu;
            rightHand.allowActivate = !blocked && rightActivateBeforeMenu;
        }
    }

    public InventoryItem Held(bool left)
    {
        var hand = left ? leftHand : rightHand;
        if (hand == null || !hand.hasSelection) return null;
        return hand.firstInteractableSelected.transform.GetComponent<InventoryItem>();
    }

    public string HandLabel(bool left)
    {
        var hand = left ? leftHand : rightHand;
        if (hand == null || !hand.isActiveAndEnabled) return "Not connected";
        if (!hand.hasSelection) return "Empty";
        var item = Held(left);
        return item != null ? item.displayName : "Other object (cannot store)";
    }

    public void Spawn(ItemSpawnPoint point)
    {
        if (point.prefab == null || string.IsNullOrWhiteSpace(point.itemId))
        {
            Debug.LogError("Spawn point needs a prefab and unique item ID.", point);
            return;
        }
        if (!spawned.Add(point.itemId)) return;
        var item = Instantiate(point.prefab, point.transform.position, point.transform.rotation);
        item.Initialize(point.itemId, point.targetId, point.prefab, source: point);
        live.Add(point.itemId, item);
    }

    public void Store(bool left)
    {
        if (!BeginTransaction()) return;
        var hand = left ? leftHand : rightHand;
        var item = Held(left);
        if (item == null) { Feedback = "Hold a portable item in that hand first."; return; }
        if (!item.CanStore) { Feedback = "This item cannot be stored now."; return; }
        if (item.Prefab == null || string.IsNullOrEmpty(item.ItemId) || entries.Exists(e => e.id == item.ItemId))
        {
            Feedback = "Item identity is missing or already stored.";
            return;
        }
        var entry = new Entry
        {
            id = item.ItemId, targetId = item.TargetId, displayName = item.displayName,
            prefab = item.Prefab, state = item.stateJson
        };
        // Set this BEFORE SelectExit so storing cannot be mistaken for installing.
        item.SystemRelease = true;
        hand.interactionManager.SelectExit((IXRSelectInteractor)hand, (IXRSelectInteractable)item.Grab);
        if (item.Grab.isSelected)
        {
            item.SystemRelease = false;
            Feedback = "The item is still held. Try again.";
            return;
        }
        entries.Add(entry);
        live.Remove(item.ItemId);
        item.gameObject.SetActive(false);
        Destroy(item.gameObject);
        Feedback = "Stored " + entry.displayName + ".";
    }

    public void Retrieve(string id, bool left)
    {
        if (!BeginTransaction()) return;
        var hand = left ? leftHand : rightHand;
        if (hand == null || !hand.isActiveAndEnabled) { Feedback = "That hand is not connected."; return; }
        if (hand.hasSelection) { Feedback = "Empty that hand before taking an item."; return; }
        var entry = entries.Find(e => e.id == id);
        if (entry == null || entry.prefab == null) { Feedback = "Select an available item first."; return; }
        if (live.TryGetValue(id, out var existing) && existing != null)
        {
            Feedback = "This item already exists in the room.";
            return;
        }
        var item = Instantiate(entry.prefab, hand.transform.position, hand.transform.rotation);
        item.Initialize(entry.id, entry.targetId, entry.prefab, entry.state);
        // Allow only this item through the menu's selection filter during handover.
        retrieving = item;
        try
        {
            hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)item.Grab);
        }
        finally { retrieving = null; }
        if (!hand.IsSelecting(item.Grab))
        {
            item.SystemRelease = true;
            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
            Feedback = "Could not hand over the item. It remains in inventory.";
            return;
        }
        live[id] = item;
        entries.Remove(entry);
        Feedback = "Taken " + entry.displayName + ". Close inventory; Grip releases it.";
    }

    public bool IsRetrieving(InventoryItem item) => retrieving == item;

    bool BeginTransaction()
    {
        // Also blocks a second hand or a callback from repeating a transaction this frame.
        if (!IsOpen || lastTransactionFrame == Time.frameCount) return false;
        lastTransactionFrame = Time.frameCount;
        return true;
    }

    void OnDisable()
    {
        if (Instance != this) return;
        if (toggleAction != null) toggleAction.action.Disable();
        if (leftClickAction != null) leftClickAction.action.Disable();
        if (rightClickAction != null) rightClickAction.action.Disable();
        IsOpen = false;
        waitForTriggerRelease = false;
        SetInputsBlocked(false);
        if (panel != null) panel.SetVisible(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
