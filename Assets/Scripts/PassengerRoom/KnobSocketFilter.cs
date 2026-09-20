using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KnobSocketFilter : MonoBehaviour, IXRSelectFilter
{
    public XRSocketInteractor socket;
    public XRGrabInteractable knob;

    private bool installed;
    public bool IsInstalled => installed;
    public bool InstallationRequested { get; private set; }

    public bool canProcess => isActiveAndEnabled;

    public bool Process(
        IXRSelectInteractor interactor,
        IXRSelectInteractable interactable)
    {
        if (ReferenceEquals(interactor, socket))
        {
            var item = interactable.transform.GetComponent<InventoryItem>();
            return item != null && !item.SystemRelease && item.Target != null && item.Target.Accepts(item, socket);
        }
        return !(interactor is XRSocketInteractor) && !installed;
    }

    public void MarkInstalled()
    {
        installed = true;
        InstallationRequested = false;
        if (socket.firstInteractableSelected != null)
            socket.firstInteractableSelected.transform.GetComponent<InventoryItem>()?.MarkInstalled();
    }

    public void RequestInstall(InventoryItem item)
    {
        if (installed || InstallationRequested) return;
        InstallationRequested = true;
        StartCoroutine(InstallAfterRelease(item));
    }

    IEnumerator InstallAfterRelease(InventoryItem item)
    {
        yield return null;
        if (!installed && item != null && !item.SystemRelease && !item.Grab.isSelected && socket.IsHovering(item.Grab))
            socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)item.Grab);
        InstallationRequested = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        InstallationRequested = false;
    }
}
