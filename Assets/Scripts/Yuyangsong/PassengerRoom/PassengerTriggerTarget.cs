using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Put only on the passenger room levers and installable tools.
[DefaultExecutionOrder(-90)]
public class PassengerTriggerTarget : MonoBehaviour
{
    [Tooltip("Scene connection for fixed levers. Spawned tools use their InventoryItem instead.")]
    public InventoryController inventory;
    public InventoryItem item;
    public XRBaseInteractable interactable;
    [Min(0f)] public float maxDistance = 10f;
    public UnityEvent use = new UnityEvent();
    public bool IsPointedAt { get; private set; }

    void Update()
    {
        IsPointedAt = false;
        var controls = item != null ? item.Inventory : inventory;
        if (controls == null || !controls.isActiveAndEnabled || InventoryController.BlocksWorld ||
            interactable == null || !interactable.isActiveAndEnabled) return;
        // Portable tools can only be turned after their socket has accepted them.
        if (item != null && (!item.Installed || item.SystemRelease)) return;

        bool left = PointsAt(controls.leftHand, controls.leftAimOrigin);
        bool right = PointsAt(controls.rightHand, controls.rightAimOrigin);
        IsPointedAt = left || right;
        if ((left && controls.leftClickAction.action.WasPressedThisFrame()) ||
            (right && controls.rightClickAction.action.WasPressedThisFrame()))
            use.Invoke();
    }

    bool PointsAt(XRBaseInputInteractor hand, Transform origin)
    {
        if (hand == null || !hand.isActiveAndEnabled || hand.hasSelection ||
            origin == null || !origin.gameObject.activeInHierarchy) return false;

        Collider nearest = null;
        float distance = maxDistance;
        foreach (var hit in Physics.RaycastAll(origin.position, origin.forward, maxDistance,
                     Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider is CharacterController && origin.IsChildOf(hit.collider.transform)) continue;
            bool targetCollider = interactable.colliders.Contains(hit.collider);
            if (hit.collider.isTrigger && !targetCollider) continue;
            if (hit.distance > distance) continue;
            distance = hit.distance;
            nearest = hit.collider;
        }
        // Any closer solid scenery or other object blocks this particular target.
        return nearest != null && interactable.colliders.Contains(nearest);
    }

    void OnDisable() { IsPointedAt = false; }
}
