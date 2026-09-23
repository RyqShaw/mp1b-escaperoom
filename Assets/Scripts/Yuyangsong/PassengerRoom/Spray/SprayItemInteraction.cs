using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SprayItemInteraction : MonoBehaviour
{
    public InventoryItem item;
    public Transform sprayPoint;
    public ParticleSystem mist;
    [Min(0.1f)] public float hitDistance = 1.4f;
    [Tooltip("Include walls and doors as well as Guard so solid objects block the spray.")]
    public LayerMask hitMask = Physics.DefaultRaycastLayers;
    bool spraying;

    void OnEnable() { StopSpraying(); }

    public void Use(ActivateEventArgs args)
    {
        if (InventoryController.BlocksWorld || item.SystemRelease || !item.Grab.isSelected) return;
        if (args.interactorObject == null || args.interactorObject.transform != item.Grab.interactorsSelecting[0].transform) return;
        if (spraying) return;
        spraying = true;
        mist.Play();
    }

    public void StopUse(DeactivateEventArgs args)
    {
        if (!item.Grab.isSelected || (args.interactorObject != null &&
            args.interactorObject.transform == item.Grab.interactorsSelecting[0].transform))
            StopSpraying();
    }

    void Update()
    {
        if (!spraying) return;
        if (InventoryController.BlocksWorld || item.SystemRelease || !item.Grab.isSelected)
        {
            StopSpraying();
            return;
        }
        // The first solid hit wins: spray cannot reach Guard through a wall.
        if (Physics.Raycast(sprayPoint.position, sprayPoint.forward, out var hit,
            hitDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            var guard = hit.collider.GetComponentInParent<GuardRetreat>();
            if (guard != null) guard.Retreat();
        }
    }

    public void StopSpraying()
    {
        spraying = false;
        mist.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnDisable() { StopSpraying(); }
}
