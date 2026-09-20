using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WrenchSocketFilter : MonoBehaviour, IXRSelectFilter
{
    public XRSocketInteractor socket;
    public XRGrabInteractable wrench;

    public bool IsInstalled { get; private set; }
    public bool InstallationRequested { get; set; }
    public bool canProcess => isActiveAndEnabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (ReferenceEquals(interactor, socket))
        {
            var item = interactable.transform.GetComponent<InventoryItem>();
            return item != null && !item.SystemRelease && item.Target != null && item.Target.Accepts(item, socket);
        }

        // Other drawers and the door-knob socket must not take this tool.
        if (interactor is XRSocketInteractor)
            return false;

        return !IsInstalled && !InstallationRequested;
    }

    public void MarkInstalled()
    {
        IsInstalled = true;
        InstallationRequested = false;
        if (socket.firstInteractableSelected != null)
            socket.firstInteractableSelected.transform.GetComponent<InventoryItem>()?.MarkInstalled();
    }
}
