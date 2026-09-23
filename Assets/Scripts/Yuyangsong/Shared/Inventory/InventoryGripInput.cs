using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Only changes input during inventory holding and the first Grip handover afterward.
sealed class InventoryGripInput : IXRInputButtonReader
{
    readonly InventoryController inventory;
    readonly XRBaseInputInteractor hand;
    readonly IXRInputButtonReader original;
    public IXRInputButtonReader Previous { get; }
    IXRSelectInteractable heldForMenu;

    public bool WaitingForGrip => heldForMenu != null && !inventory.InputsBlocked;

    public InventoryGripInput(InventoryController inventory, XRBaseInputInteractor hand)
    {
        this.inventory = inventory;
        this.hand = hand;
        Previous = hand.selectInput.bypass;
        original = Previous ?? hand.selectInput;
    }

    public void RememberSelection()
    {
        heldForMenu = hand.firstInteractableSelected;
    }

    bool KeepHold()
    {
        if (inventory.InputsBlocked)
        {
            RememberSelection();
            return hand.hasSelection;
        }
        if (heldForMenu != null && !hand.IsSelecting(heldForMenu))
            heldForMenu = null;
        // Already holding Grip, or the next Grip press: return control to State Change.
        if (heldForMenu != null && original.ReadIsPerformed())
            heldForMenu = null;
        return heldForMenu != null;
    }

    // XRI's bypass guard lets these calls read the original reader without recursion.
    // KeepHold is called only from these reader callbacks, never from Update.
    public bool ReadIsPerformed()
    {
        bool keep = KeepHold();
        return keep || (!inventory.InputsBlocked && original.ReadIsPerformed());
    }

    public bool ReadWasPerformedThisFrame()
    {
        bool keep = KeepHold();
        return !inventory.InputsBlocked && !keep && original.ReadWasPerformedThisFrame();
    }

    public bool ReadWasCompletedThisFrame()
    {
        bool keep = KeepHold();
        return !inventory.InputsBlocked && !keep && original.ReadWasCompletedThisFrame();
    }

    public float ReadValue()
    {
        if (KeepHold()) return 1f;
        return inventory.InputsBlocked ? 0f : original.ReadValue();
    }

    public bool TryReadValue(out float value)
    {
        if (KeepHold()) { value = 1f; return true; }
        if (inventory.InputsBlocked) { value = 0f; return true; }
        return original.TryReadValue(out value);
    }
}
