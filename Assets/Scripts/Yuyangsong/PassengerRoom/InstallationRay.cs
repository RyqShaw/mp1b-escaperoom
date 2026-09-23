using UnityEngine;

// Shared aiming rules for all three keys; each item keeps its own installation component.
public static class InstallationRay
{
    public static bool Hits(Transform origin, Transform item, Collider target, float maxDistance)
    {
        if (origin == null || !origin.gameObject.activeInHierarchy || target == null ||
            !target.enabled || !target.gameObject.activeInHierarchy) return false;

        Collider nearest = null;
        float nearestDistance = maxDistance;
        foreach (var hit in Physics.RaycastAll(origin.position, origin.forward,
            maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.transform.IsChildOf(item)) continue;
            if (hit.collider is CharacterController && origin.IsChildOf(hit.collider.transform)) continue;
            if (hit.collider.isTrigger && hit.collider != target) continue;
            if (hit.distance >= nearestDistance) continue;
            nearest = hit.collider;
            nearestDistance = hit.distance;
        }
        // Solid scenery blocks installation; the held item and unrelated triggers do not.
        return nearest == target;
    }
}
