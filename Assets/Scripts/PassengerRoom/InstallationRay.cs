using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Shared aiming rules for all three keys; each item keeps its own installation component.
public static class InstallationRay
{
    public static bool Hits(IXRSelectInteractor hand, Transform item, Collider target, float maxDistance)
    {
        if (hand == null || target == null || !target.enabled || !target.gameObject.activeInHierarchy) return false;
        Transform origin = hand.transform;
        if (hand is NearFarInteractor nearFar && nearFar.farInteractionCaster != null)
            origin = nearFar.farInteractionCaster.effectiveCastOrigin;
        else if (hand is XRRayInteractor ray && ray.rayOriginTransform != null)
            origin = ray.rayOriginTransform;
        if (origin == null) return false;

        Collider nearest = null;
        float nearestDistance = maxDistance;
        foreach (var hit in Physics.RaycastAll(origin.position, origin.forward,
            maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.transform.IsChildOf(item)) continue;
            if (hit.collider is CharacterController && hand.transform.IsChildOf(hit.collider.transform)) continue;
            if (hit.collider.isTrigger && hit.collider != target) continue;
            if (hit.distance >= nearestDistance) continue;
            nearest = hit.collider;
            nearestDistance = hit.distance;
        }
        // Solid scenery blocks installation; the held item and unrelated triggers do not.
        return nearest == target;
    }
}
