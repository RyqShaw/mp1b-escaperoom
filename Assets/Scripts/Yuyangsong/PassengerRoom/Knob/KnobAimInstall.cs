using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class KnobAimInstall : MonoBehaviour
{
    public InventoryItem item;
    public KnobSocketFilter socketFilter;
    public Collider target;
    public Transform toolTip;
    public GameObject preview;
    public float maxDistance = 3f;

    public bool IsAiming { get; private set; }
    public bool IsInstalling { get; private set; }
    public bool IsHeld => socketFilter != null && item.Grab.isSelected && !item.Installed &&
        !item.SystemRelease && !IsInstalling && !(item.Grab.firstInteractorSelecting is XRSocketInteractor);

    void Update()
    {
        IsAiming = !InventoryController.BlocksWorld && IsHeld &&
            InstallationRay.Hits(item.Grab.firstInteractorSelecting, item.transform, target, maxDistance);
        if (preview != null) preview.SetActive(IsAiming);
    }

    // Connected to Select Exited in the item prefab Inspector.
    public void OnReleased(SelectExitEventArgs args)
    {
        if (args.isCanceled || item.SystemRelease || item.Installed || IsInstalling ||
            InventoryController.BlocksWorld || toolTip == null || socketFilter == null ||
            !socketFilter.isActiveAndEnabled || args.interactorObject is XRSocketInteractor) return;
        // Do not install from a stale preview, or from the inventory's system release.
        if (!InstallationRay.Hits(args.interactorObject, item.transform, target, maxDistance)) return;
        IsInstalling = true;
        IsAiming = false;
        if (preview != null) preview.SetActive(false);
        StartCoroutine(InstallAfterRelease());
    }

    IEnumerator InstallAfterRelease()
    {
        yield return null;
        if (socketFilter == null || !socketFilter.isActiveAndEnabled || socketFilter.socket == null ||
            !socketFilter.socket.isActiveAndEnabled || item.SystemRelease || item.Grab.isSelected ||
            item.Target == null || item.Target.socketRule != socketFilter)
        {
            IsInstalling = false;
            yield break;
        }

        var grab = item.Grab;
        var socket = socketFilter.socket;
        Transform grip = grab.attachTransform;
        // Switch to the installation point BEFORE allowing socket selection.
        grab.attachTransform = toolTip;
        socketFilter.InstallationRequested = true;
        socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)grab);
        if (!socket.IsSelecting(grab))
        {
            grab.attachTransform = grip;
            socketFilter.InstallationRequested = false;
            IsInstalling = false;
            yield break;
        }

        yield return new WaitForSeconds(grab.attachEaseInTime);
        if (socketFilter != null) socketFilter.InstallationRequested = false;
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
