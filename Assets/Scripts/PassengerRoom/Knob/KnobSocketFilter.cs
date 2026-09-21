using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KnobSocketFilter : MonoBehaviour, IXRSelectFilter, IItemSocketRule
{
    public XRSocketInteractor socket;
    public bool IsInstalled { get; private set; }
    public bool InstallationRequested { get; set; }
    public bool canProcess => isActiveAndEnabled;

    public bool Accepts(XRSocketInteractor candidate) =>
        candidate == socket && (InstallationRequested || IsInstalled);

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!ReferenceEquals(interactor, socket)) return false;
        var item = interactable.transform.GetComponent<InventoryItem>();
        return item != null && !item.SystemRelease && item.Target != null && item.Target.Accepts(item, socket);
    }

    // Connected to the socket's Select Entered event in the room Inspector.
    public void MarkInstalled()
    {
        if (socket.firstInteractableSelected == null) return;
        IsInstalled = true;
        InstallationRequested = false;
        socket.firstInteractableSelected.transform.GetComponent<InventoryItem>()?.MarkInstalled();
    }

    void OnDisable() { InstallationRequested = false; }
}
