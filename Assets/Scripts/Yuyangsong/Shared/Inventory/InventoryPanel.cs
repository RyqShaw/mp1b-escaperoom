using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    [Serializable]
    public class ItemSlot
    {
        public Button button;
        public TMP_Text label;
        public GameObject selectionMark;
    }

    [Header("Scene references")]
    public InventoryController inventory;
    public Transform playerCamera;
    [Header("Prefab UI")]
    public RectTransform panelRoot;
    public Vector3 cameraOffset = new Vector3(0, 0, 0.9f);
    public TMP_Text handsLabel, pageLabel, feedback;
    public ItemSlot[] slots;
    public Button previous, next, storeLeft, storeRight, takeLeft, takeRight;

    int page;
    string selectedId;
    public int OpenedFrame { get; private set; }

    void Start() { SetVisible(false); }

    public void SetVisible(bool visible)
    {
        OpenedFrame = Time.frameCount;
        // Place once on opening, so looking around does not move the target buttons.
        if (visible)
            panelRoot.SetPositionAndRotation(playerCamera.TransformPoint(cameraOffset), playerCamera.rotation);
        panelRoot.gameObject.SetActive(visible);
    }

    void LateUpdate()
    {
        if (!inventory.IsOpen) return;
        Refresh();
    }

    void Refresh()
    {
        int count = inventory.Entries.Count;
        int pageSize = slots.Length;
        int pages = Mathf.Max(1, (count + pageSize - 1) / pageSize);
        page = Mathf.Clamp(page, 0, pages - 1);
        bool selectionExists = false;
        foreach (var entry in inventory.Entries)
            if (entry.id == selectedId) selectionExists = true;
        if (!selectionExists) selectedId = null;

        handsLabel.text = "LEFT: " + inventory.HandLabel(true) + "\nRIGHT: " + inventory.HandLabel(false);
        pageLabel.text = $"{page + 1} / {pages}    ({count} items)";
        feedback.text = inventory.Feedback;
        previous.interactable = page > 0;
        next.interactable = page + 1 < pages;
        takeLeft.interactable = selectedId != null && !inventory.leftHand.hasSelection;
        takeRight.interactable = selectedId != null && !inventory.rightHand.hasSelection;
        storeLeft.interactable = inventory.Held(true) != null && inventory.Held(true).CanStore;
        storeRight.interactable = inventory.Held(false) != null && inventory.Held(false).CanStore;

        for (int i = 0; i < pageSize; i++)
        {
            int index = page * pageSize + i;
            bool hasItem = index < count;
            slots[i].button.interactable = hasItem;
            slots[i].label.text = hasItem ? inventory.Entries[index].displayName : "";
            slots[i].selectionMark.SetActive(hasItem && inventory.Entries[index].id == selectedId);
        }
        if (count == 0) slots[0].label.text = "No stored items. Hold an item, then choose Store.";
    }

    // These methods are connected in each Button's On Click list in the prefab.
    public void SelectSlot(int slot)
    {
        int index = page * slots.Length + slot;
        if (index >= 0 && index < inventory.Entries.Count) selectedId = inventory.Entries[index].id;
    }

    public void ChangePage(int direction) { page += direction; selectedId = null; }
    public void TakeToHand(bool left) { inventory.Retrieve(selectedId, left); }
}
