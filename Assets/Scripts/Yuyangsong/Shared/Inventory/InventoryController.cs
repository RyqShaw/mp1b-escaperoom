using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DefaultExecutionOrder(-110)]
public class InventoryController : MonoBehaviour, IXRSelectFilter
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
    public Transform leftAimOrigin, rightAimOrigin;
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
    readonly XRInputButtonReader blockedActivate = new XRInputButtonReader(inputSourceMode: XRInputButtonReader.InputSourceMode.Unused);
    InventoryGripInput leftGrip, rightGrip;
    IXRInputButtonReader leftActivateBeforeMenu, rightActivateBeforeMenu;
    bool inputsBlocked, waitForTriggerRelease;
    int lastTransactionFrame = -1, lastToggleFrame = -1;
    InventoryItem retrieving;
    InventoryItem pendingRetrieve;
    XRBaseInputInteractor pendingHand;

    internal bool InputsBlocked => inputsBlocked;
    public bool canProcess => isActiveAndEnabled;

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
        if (leftHand == null || rightHand == null || leftAimOrigin == null || rightAimOrigin == null || panel == null || toggleAction == null || toggleAction.action == null ||
            leftClickAction == null || leftClickAction.action == null || rightClickAction == null || rightClickAction.action == null)
        {
            Debug.LogError("Assign inventory hands, aim origins, Toggle and both Click actions, and the panel in the Inspector.", this);
            enabled = false;
            return;
        }
        leftGrip = new InventoryGripInput(this, leftHand);
        rightGrip = new InventoryGripInput(this, rightHand);
        leftHand.selectInput.bypass = leftGrip;
        rightHand.selectInput.bypass = rightGrip;
        leftHand.selectFilters.Add(this);
        rightHand.selectFilters.Add(this);
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
        if (blocked)
        {
            leftGrip?.RememberSelection();
            rightGrip?.RememberSelection();
            leftActivateBeforeMenu = leftHand.activateInput.bypass;
            rightActivateBeforeMenu = rightHand.activateInput.bypass;
            leftHand.activateInput.bypass = blockedActivate;
            rightHand.activateInput.bypass = blockedActivate;
        }
        else
        {
            if (leftHand != null && leftHand.activateInput.bypass == blockedActivate)
                leftHand.activateInput.bypass = leftActivateBeforeMenu;
            if (rightHand != null && rightHand.activateInput.bypass == blockedActivate)
                rightHand.activateInput.bypass = rightActivateBeforeMenu;
        }
        inputsBlocked = blocked;
    }

    // This filter also protects team props, which do not have InventoryItem.
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!inputsBlocked) return true;
        if (interactor.interactablesSelected.Contains(interactable)) return true;
        return retrieving != null && ReferenceEquals(interactable, retrieving.Grab) && ReferenceEquals(interactor, pendingHand);
    }

    public Transform AimOrigin(IXRSelectInteractor hand)
    {
        if (ReferenceEquals(hand, leftHand)) return leftAimOrigin;
        if (ReferenceEquals(hand, rightHand)) return rightAimOrigin;
        return null;
    }

    public bool WaitingForGrip(IXRSelectInteractor hand)
    {
        if (ReferenceEquals(hand, leftHand)) return leftGrip != null && leftGrip.WaitingForGrip;
        if (ReferenceEquals(hand, rightHand)) return rightGrip != null && rightGrip.WaitingForGrip;
        return false;
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
        item.Initialize(point.itemId, point.targetId, point.prefab, this, source: point);
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
        if (hand == null || !hand.isActiveAndEnabled || hand.interactionManager == null) { Feedback = "That hand is not connected."; return; }
        if (hand.hasSelection) { Feedback = "Empty that hand before taking an item."; return; }
        var entry = entries.Find(e => e.id == id);
        if (entry == null || entry.prefab == null) { Feedback = "Select an available item first."; return; }
        if (live.TryGetValue(id, out var existing) && existing != null)
        {
            Feedback = "This item already exists in the room.";
            return;
        }
        var attach = hand.GetAttachTransform(entry.prefab.Grab);
        var item = Instantiate(entry.prefab, attach.position, attach.rotation);
        item.Initialize(entry.id, entry.targetId, entry.prefab, this, entry.state);
        pendingRetrieve = item;
        pendingHand = hand;
        // Allow only this item through the menu's selection filter during handover.
        retrieving = item;
        try
        {
            hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)item.Grab);
        }
        finally { retrieving = null; }
        if (!hand.IsSelecting(item.Grab))
        {
            CancelRetrieve();
            Feedback = "Could not hand over the item. It remains in inventory.";
            return;
        }
        (left ? leftGrip : rightGrip).RememberSelection();
        Feedback = "Taking " + entry.displayName + "...";
        StartCoroutine(ConfirmRetrieve(entry));
    }

    IEnumerator ConfirmRetrieve(Entry entry)
    {
        // Verify after an interaction update, before removing the inventory entry.
        yield return null;
        if (pendingRetrieve == null || pendingHand == null || !pendingHand.isActiveAndEnabled ||
            !pendingHand.IsSelecting(pendingRetrieve.Grab))
        {
            CancelRetrieve();
            Feedback = "Could not keep the item in hand. It remains in inventory.";
            yield break;
        }
        live[entry.id] = pendingRetrieve;
        entries.Remove(entry);
        pendingRetrieve = null;
        pendingHand = null;
        Feedback = "Taken " + entry.displayName + ". Close inventory; hold Grip, then release to let go.";
    }

    void CancelRetrieve()
    {
        if (pendingRetrieve != null)
        {
            pendingRetrieve.SystemRelease = true;
            if (pendingHand != null && pendingHand.interactionManager != null &&
                pendingHand.IsSelecting(pendingRetrieve.Grab))
                pendingHand.interactionManager.SelectExit((IXRSelectInteractor)pendingHand, (IXRSelectInteractable)pendingRetrieve.Grab);
            pendingRetrieve.gameObject.SetActive(false);
            Destroy(pendingRetrieve.gameObject);
        }
        pendingRetrieve = null;
        pendingHand = null;
    }

    public bool IsRetrieving(InventoryItem item) => retrieving == item;

    bool BeginTransaction()
    {
        // Also blocks a second hand or a callback from repeating a transaction this frame.
        if (!IsOpen || pendingRetrieve != null || lastTransactionFrame == Time.frameCount) return false;
        lastTransactionFrame = Time.frameCount;
        return true;
    }

    void OnDisable()
    {
        if (Instance != this) return;
        if (toggleAction != null) toggleAction.action.Disable();
        if (leftClickAction != null) leftClickAction.action.Disable();
        if (rightClickAction != null) rightClickAction.action.Disable();
        StopAllCoroutines();
        CancelRetrieve();
        IsOpen = false;
        waitForTriggerRelease = false;
        SetInputsBlocked(false);
        if (leftHand != null)
        {
            leftHand.selectFilters.Remove(this);
            if (leftGrip != null && leftHand.selectInput.bypass == leftGrip)
                leftHand.selectInput.bypass = leftGrip.Previous;
        }
        if (rightHand != null)
        {
            rightHand.selectFilters.Remove(this);
            if (rightGrip != null && rightHand.selectInput.bypass == rightGrip)
                rightHand.selectInput.bypass = rightGrip.Previous;
        }
        if (panel != null) panel.SetVisible(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
