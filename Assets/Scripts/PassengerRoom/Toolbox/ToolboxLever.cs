using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToolboxLever : MonoBehaviour, IXRSelectFilter
{
    public ToolboxSequence sequence;
    public int number;
    public Transform handlePivot;
    public float angle = -45f;
    public float speed = 180f;
    Quaternion restRotation;
    Quaternion targetRotation;
    public bool IsMoving => Quaternion.Angle(handlePivot.localRotation, targetRotation) > 0.1f;
    public bool canProcess => isActiveAndEnabled;

    // The lever is fixed to the lid; use Trigger while hovering, not Grip selection.
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) => false;

    void Awake() { restRotation = targetRotation = handlePivot.localRotation; }

    void Update()
    {
        handlePivot.localRotation = Quaternion.RotateTowards(handlePivot.localRotation, targetRotation, speed * Time.deltaTime);
    }

    public void Use(ActivateEventArgs args)
    {
        if (InventoryController.BlocksWorld) return;
        if (args.interactorObject is XRBaseInteractor hand && hand.hasSelection) return;
        sequence.Press(this);
    }

    public void SetPulled(bool pulled)
    {
        targetRotation = restRotation * Quaternion.Euler(0, 0, pulled ? angle : 0);
    }
}
