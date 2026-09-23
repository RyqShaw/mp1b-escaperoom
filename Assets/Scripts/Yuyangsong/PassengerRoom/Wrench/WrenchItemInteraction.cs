using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WrenchItemInteraction : MonoBehaviour, IInventoryStorageRule, IXRSelectFilter
{
    public InventoryItem item;
    public WrenchAimInstall installation;
    public WrenchPrompt prompt;
    ItemTarget boundTarget;
    WrenchTarget target;

    public bool CanStore => !installation.IsInstalling &&
        (target == null || !target.socketFilter.InstallationRequested);
    public bool canProcess => isActiveAndEnabled;

    void Update()
    {
        var next = item.Target;
        if (next == boundTarget) return;
        if (installation.preview != null) installation.preview.SetActive(false);
        boundTarget = next;
        target = next != null ? next.GetComponent<WrenchTarget>() : null;
        installation.socketFilter = target != null ? target.socketFilter : null;
        installation.target = target != null ? target.aimCollider : null;
        installation.preview = target != null ? target.preview : null;
        prompt.socketFilter = installation.socketFilter;
        prompt.wrenchTurn = target != null ? target.turn : null;
        prompt.drawer = target != null ? target.drawer : null;
        if (target == null) return;
        target.turn.installation = installation;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        return interactor is XRSocketInteractor || CanStore;
    }

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
