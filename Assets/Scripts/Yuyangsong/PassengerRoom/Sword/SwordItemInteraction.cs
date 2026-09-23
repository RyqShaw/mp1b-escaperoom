using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SwordItemInteraction : MonoBehaviour, IInventoryStorageRule, IXRSelectFilter, IXRHoverFilter
{
    public InventoryItem item;
    public SwordAimInstall installation;
    public SwordPrompt prompt;
    ItemTarget boundTarget;
    SwordTarget target;

    // Resolve directly so the first frame cannot bypass the closed toolbox.
    public bool IsAccessible
    {
        get
        {
            var room = item.Target != null ? item.Target.GetComponent<SwordTarget>() : null;
            return room == null || room.toolbox == null || room.toolbox.IsOpen;
        }
    }
    public bool CanStore => IsAccessible && !installation.IsInstalling &&
        (target == null || !target.socketFilter.InstallationRequested);
    public bool canProcess => isActiveAndEnabled;

    void Update()
    {
        var next = item.Target;
        if (next == boundTarget) return;
        if (installation.preview != null) installation.preview.SetActive(false);
        boundTarget = next;
        target = next != null ? next.GetComponent<SwordTarget>() : null;
        installation.socketFilter = target != null ? target.socketFilter : null;
        installation.target = target != null ? target.aimCollider : null;
        installation.preview = target != null ? target.preview : null;
        prompt.socketFilter = installation.socketFilter;
        prompt.turn = target != null ? target.turn : null;
        if (target != null) target.turn.installation = installation;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) =>
        IsAccessible && (interactor is XRSocketInteractor || CanStore);

    public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable) => IsAccessible;

    public void Use(ActivateEventArgs args)
    {
        if (InventoryController.BlocksWorld || !item.Installed || target == null) return;
        if (args.interactorObject is XRBaseInteractor hand && hand.hasSelection) return;
        UseFromPointer();
    }

    public void UseFromPointer()
    {
        if (!isActiveAndEnabled || InventoryController.BlocksWorld || item.SystemRelease ||
            !item.Installed || target == null) return;
        target.turn.Turn();
    }
}
