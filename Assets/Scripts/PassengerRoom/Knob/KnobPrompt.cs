using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KnobPrompt : MonoBehaviour
{
    public XRGrabInteractable knob;
    public KnobSocketFilter socketFilter;
    public KnobTurn knobTurn;
    public KnobAimInstall installation;

    private bool wasComplete;
    private float unlockedUntil;

    void Update()
    {
        InteractionHUD hud = InteractionHUD.Instance;
        if (hud == null)
        {
            return;
        }
        if (InventoryController.BlocksWorld || socketFilter == null)
        {
            hud.Hide(this);
            return;
        }

        bool playerHovering = false;
        foreach (var interactor in knob.interactorsHovering)
        {
            if (!(interactor is XRSocketInteractor))
            {
                playerHovering = true;
                break;
            }
        }

        if (knobTurn.IsComplete && !wasComplete)
        {
            unlockedUntil = Time.time + 1.5f;
        }
        wasComplete = knobTurn.IsComplete;

        string message = null;
        if (installation.IsInstalling)
        {
            message = "Installing...";
        }
        else if (!socketFilter.IsInstalled)
        {
            if (installation.IsHeld)
            {
                message = installation.IsAiming
                    ? "Press Grip again to fit the knob"
                    : "Point at the door fitting";
            }
            else if (playerHovering)
            {
                message = "Press Grip to pick up the knob";
            }
        }
        else if (knobTurn.IsComplete)
        {
            if (Time.time < unlockedUntil)
            {
                message = "Unlocked";
            }
        }
        else if (knobTurn.IsTurning)
        {
            message = $"Turning...  {knobTurn.CompletedSteps}/{knobTurn.requiredSteps}";
        }
        else if (playerHovering)
        {
            message = $"Press Trigger to turn  {knobTurn.CompletedSteps}/{knobTurn.requiredSteps}";
        }

        if (message != null)
        {
            hud.Show(this, message);
        }
        else
        {
            hud.Hide(this);
        }
    }

    void OnDisable()
    {
        if (InteractionHUD.Instance != null)
        {
            InteractionHUD.Instance.Hide(this);
        }
    }
}
