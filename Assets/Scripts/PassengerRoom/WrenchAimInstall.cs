using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WrenchAimInstall : MonoBehaviour
{
    public XRGrabInteractable wrench;
    public WrenchSocketFilter socketFilter;
    public Collider target;
    public Transform toolTip;
    public GameObject preview;
    public float maxDistance = 3f;

    public bool IsAiming { get; private set; }
    public bool IsInstalling { get; private set; }
    public bool IsHeld => socketFilter != null && wrench.isSelected && !socketFilter.IsInstalled && !IsInstalling;

    void Update()
    {
        IsAiming = !InventoryController.BlocksWorld && IsHeld && IsPointingAtTarget(wrench.firstInteractorSelecting);
        if (preview != null) preview.SetActive(IsAiming);
    }

    bool IsPointingAtTarget(IXRSelectInteractor hand)
    {
        if (hand == null || target == null) return false;
        // Read the holding controller's ray origin, even while its caster is busy grabbing.
        Transform origin = hand.transform;
        if (hand is NearFarInteractor nearFar && nearFar.farInteractionCaster != null)
            origin = nearFar.farInteractionCaster.effectiveCastOrigin;
        else if (hand is XRRayInteractor ray && ray.rayOriginTransform != null)
            origin = ray.rayOriginTransform;

        if (origin == null)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(origin.position, origin.forward,
            maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        Collider nearest = null;
        float nearestDistance = maxDistance;
        foreach (RaycastHit hit in hits)
        {
            // The tool in the player's hand and the player's body must not block their own ray.
            if (hit.collider.transform.IsChildOf(wrench.transform))
                continue;
            if (hit.collider is CharacterController && hand.transform.IsChildOf(hit.collider.transform))
                continue;
            if (hit.collider.isTrigger && hit.collider != target)
                continue;

            if (hit.distance < nearestDistance)
            {
                nearest = hit.collider;
                nearestDistance = hit.distance;
            }
        }
        // Walls, seats and drawer fronts can block the target. No aiming through scenery.
        return nearest == target;
    }

    public void OnReleased(SelectExitEventArgs args)
    {
        var item = GetComponent<InventoryItem>();
        if (InventoryController.BlocksWorld || (item != null && item.SystemRelease) ||
            socketFilter == null || args.isCanceled || socketFilter.IsInstalled || IsInstalling)
            return;
        if (args.interactorObject is XRSocketInteractor)
            return;
        // Check again at release; do not install using last frame's stale preview.
        if (!IsPointingAtTarget(args.interactorObject))
            return;

        IsInstalling = true;
        IsAiming = false;
        preview.SetActive(false);
        StartCoroutine(InstallAfterRelease());
    }

    IEnumerator InstallAfterRelease()
    {
        // Let XRI finish releasing the hand before starting a new selection.
        yield return null;
        if (socketFilter == null || !socketFilter.isActiveAndEnabled || wrench.isSelected)
        {
            if (socketFilter != null) socketFilter.InstallationRequested = false;
            IsInstalling = false;
            yield break;
        }
        Transform handAttach = wrench.attachTransform;
        wrench.attachTransform = toolTip;
        socketFilter.InstallationRequested = true;
        wrench.interactionManager.SelectEnter(
            (IXRSelectInteractor)socketFilter.socket, (IXRSelectInteractable)wrench);

        if (!socketFilter.socket.IsSelecting(wrench))
        {
            wrench.attachTransform = handAttach;
            socketFilter.InstallationRequested = false;
            IsInstalling = false;
            yield break;
        }

        // Do not allow turning during the grab component's short alignment transition.
        yield return new WaitForSeconds(wrench.attachEaseInTime);
        IsInstalling = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        IsAiming = false;
        IsInstalling = false;
        if (socketFilter != null) socketFilter.InstallationRequested = false;
        if (preview != null) preview.SetActive(false);
    }
}
