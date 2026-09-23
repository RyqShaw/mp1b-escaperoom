using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KnobItemInteraction : MonoBehaviour, IInventoryStorageRule, IXRSelectFilter
{
    public InventoryItem item;
    public KnobAimInstall installation;
    public KnobPrompt prompt;
    public KnobTurn turn;
    ItemTarget boundTarget;
    KnobTarget target;

    public bool CanStore => !installation.IsInstalling &&
        (target == null || !target.socketFilter.InstallationRequested);
    public bool canProcess => isActiveAndEnabled;

    void Update()
    {
        var next = item.Target;
        if (next == boundTarget) return;
        if (installation.preview != null) installation.preview.SetActive(false);
        boundTarget = next;
        target = next != null ? next.GetComponent<KnobTarget>() : null;
        installation.socketFilter = target != null ? target.socketFilter : null;
        installation.target = target != null ? target.aimCollider : null;
        installation.preview = target != null ? target.preview : null;
        prompt.socketFilter = installation.socketFilter;
        turn.socketFilter = prompt.socketFilter;
        turn.door = target != null ? target.door : null;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        return interactor is XRSocketInteractor || CanStore;
    }

    public void Use(ActivateEventArgs args)
    {
        if (InventoryController.BlocksWorld || !item.Installed || target == null) return;
        if (args.interactorObject is XRBaseInteractor hand && hand.hasSelection) return;
        turn.Turn();
    }
}
