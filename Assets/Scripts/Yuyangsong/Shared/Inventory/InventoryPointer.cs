using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

// Menu rays and button tests share one path. The room's curve can snap to held objects.
[DefaultExecutionOrder(10)]
public class InventoryPointer : MonoBehaviour
{
    [Serializable]
    public class HandPointer
    {
        public CurveVisualController worldVisual;
        public LineRenderer menuLine;
        [NonSerialized] public bool worldWasActive;
    }

    public InventoryController inventory;
    public InventoryPanel panel;
    public Button[] buttons;
    public HandPointer left, right;
    public float maxDistance = 5f;
    Button leftHover, rightHover;
    bool showingMenuRays;

    void LateUpdate()
    {
        SetMenuRays(inventory.IsOpen);
        if (!inventory.IsOpen) return;
        SetHover(Aim(left), Aim(right));
        if (Time.frameCount <= panel.OpenedFrame) return;

        // One click per frame, with the same Trigger input for empty and full hands.
        if (leftHover != null && inventory.leftClickAction.action.WasPressedThisFrame())
            leftHover.onClick.Invoke();
        else if (rightHover != null && inventory.rightClickAction.action.WasPressedThisFrame())
            rightHover.onClick.Invoke();
        // Close can change the menu state during the callback above.
        SetMenuRays(inventory.IsOpen);
    }

    Button Aim(HandPointer hand)
    {
        Transform origin = hand.worldVisual.lineOriginTransform;
        hand.menuLine.enabled = origin.gameObject.activeInHierarchy;
        if (!hand.menuLine.enabled) return null;
        var ray = new Ray(origin.position, origin.forward);
        var plane = new Plane(panel.panelRoot.forward, panel.panelRoot.position);
        Vector3 end = ray.GetPoint(maxDistance);
        Button hit = null;
        if (plane.Raycast(ray, out float distance) && distance <= maxDistance)
        {
            Vector3 point = ray.GetPoint(distance);
            if (panel.panelRoot.rect.Contains(panel.panelRoot.InverseTransformPoint(point)))
            {
                end = point;
                hit = ButtonAt(point);
            }
        }
        // Draw exactly the path used above, ending at the panel intersection.
        hand.menuLine.SetPosition(0, ray.origin);
        hand.menuLine.SetPosition(1, end);
        return hit;
    }

    Button ButtonAt(Vector3 point)
    {
        foreach (var button in buttons)
        {
            if (!button.isActiveAndEnabled || !button.IsInteractable()) continue;
            var rect = (RectTransform)button.transform;
            if (rect.rect.Contains(rect.InverseTransformPoint(point))) return button;
        }
        return null;
    }

    void SetHover(Button nextLeft, Button nextRight)
    {
        if (leftHover == nextLeft && rightHover == nextRight) return;
        if (leftHover != null) leftHover.OnPointerExit(null);
        if (rightHover != null && rightHover != leftHover) rightHover.OnPointerExit(null);
        leftHover = nextLeft;
        rightHover = nextRight;
        if (leftHover != null) leftHover.OnPointerEnter(null);
        if (rightHover != null && rightHover != leftHover) rightHover.OnPointerEnter(null);
    }

    void SetMenuRays(bool visible)
    {
        if (showingMenuRays == visible) return;
        showingMenuRays = visible;
        SetHandVisible(left, visible);
        SetHandVisible(right, visible);
        if (!visible) SetHover(null, null);
    }

    static void SetHandVisible(HandPointer hand, bool visible)
    {
        if (visible) hand.worldWasActive = hand.worldVisual.gameObject.activeSelf;
        hand.worldVisual.gameObject.SetActive(visible ? false : hand.worldWasActive);
        hand.menuLine.enabled = visible;
    }

    void OnDisable() { SetMenuRays(false); }
}
