using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Identity belongs to a spawn, not to the name or instance ID of its GameObject.
[RequireComponent(typeof(XRGrabInteractable))]
public class InventoryItem : MonoBehaviour, IXRSelectFilter
{
    public string displayName;
    public bool allowStorage = true;
    public string stateJson = "{}";
    [Header("Local prefab components")]
    [SerializeField] XRGrabInteractable grab = null;
    public WrenchAimInstall wrenchInstallation;
    public WrenchPrompt wrenchPrompt;
    public KnobPrompt knobPrompt;
    public KnobTurn knobTurn;
    public string ItemId { get; private set; }
    public string TargetId { get; private set; }
    public InventoryItem Prefab { get; private set; }
    public XRGrabInteractable Grab => grab;
    public bool SystemRelease { get; set; }
    public bool Installed { get; private set; }
    public ItemTarget Target { get; private set; }
    public bool canProcess => isActiveAndEnabled;
    public bool CanStore => allowStorage && !Installed && !SystemRelease &&
        (wrenchInstallation == null || !wrenchInstallation.IsInstalling);

    public void Initialize(string id, string targetId, InventoryItem prefab, string state = "{}")
    {
        ItemId = id;
        TargetId = targetId;
        Prefab = prefab;
        stateJson = state;
        BindTarget();
    }

    void Update() { BindTarget(); }

    void BindTarget()
    {
        ItemTarget next = ItemTarget.Find(TargetId);
        if (next == Target) return;
        Target = next;
        if (wrenchInstallation != null)
        {
            wrenchInstallation.socketFilter = next != null ? next.wrenchFilter : null;
            wrenchInstallation.target = next != null ? next.aimCollider : null;
            wrenchInstallation.preview = next != null ? next.preview : null;
            if (next != null && next.wrenchFilter != null && next.wrenchTurn != null)
            {
                next.wrenchFilter.wrench = Grab;
                next.wrenchTurn.installation = wrenchInstallation;
            }
        }
        if (wrenchPrompt != null)
        {
            wrenchPrompt.socketFilter = next != null ? next.wrenchFilter : null;
            wrenchPrompt.wrenchTurn = next != null ? next.wrenchTurn : null;
            wrenchPrompt.drawer = next != null ? next.drawer : null;
        }
        if (knobPrompt != null)
        {
            knobPrompt.socketFilter = next != null ? next.knobFilter : null;
            knobTurn.socketFilter = knobPrompt.socketFilter;
            knobTurn.door = next != null ? next.door : null;
            if (next != null && next.knobFilter != null) next.knobFilter.knob = Grab;
        }
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (SystemRelease) return false;
        if (interactor is XRSocketInteractor socket)
            return Target != null && Target.Accepts(this, socket);
        if (Installed || (Target != null && Target.knobFilter != null && Target.knobFilter.InstallationRequested)) return false;
        if (wrenchInstallation != null && wrenchInstallation.IsInstalling) return false;
        if (Grab.isSelected && !Grab.interactorsSelecting.Contains(interactor)) return false;
        // An existing hold can continue, but the open panel blocks new world grabs.
        return !InventoryController.BlocksWorld || Grab.interactorsSelecting.Contains(interactor) ||
            (InventoryController.Instance != null && InventoryController.Instance.IsRetrieving(this));
    }

    public void MarkInstalled() { Installed = true; }

    // Connected to XR Grab Interactable events in the prefab Inspector.
    public void Released(SelectExitEventArgs args)
    {
        if (args.isCanceled || SystemRelease || Installed || InventoryController.BlocksWorld ||
            args.interactorObject is XRSocketInteractor || Target == null || Target.knobFilter == null) return;
        if (Target.knobFilter.socket.IsHovering(Grab)) Target.knobFilter.RequestInstall(this);
    }

    public void Use(ActivateEventArgs args)
    {
        if (InventoryController.BlocksWorld || !Installed || Target == null) return;
        if (args.interactorObject is XRBaseInteractor hand && hand.hasSelection) return;
        if (Target.wrenchTurn != null) Target.wrenchTurn.Turn();
        else if (knobTurn != null) knobTurn.Turn();
    }
}
