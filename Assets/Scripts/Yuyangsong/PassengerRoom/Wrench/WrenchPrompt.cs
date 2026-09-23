using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WrenchPrompt : MonoBehaviour
{
    public PassengerTriggerTarget triggerTarget;
    public XRGrabInteractable wrench;
    public WrenchAimInstall installation;
    public WrenchSocketFilter socketFilter;
    public WrenchTurn wrenchTurn;
    public DrawerOpen drawer;

    private bool wasOpen;
    private float messageUntil;

    void Update()
    {
        InteractionHUD hud = InteractionHUD.Instance;
        if (hud == null)
            return;
        if (InventoryController.BlocksWorld || socketFilter == null || drawer == null)
        {
            hud.Hide(this);
            return;
        }

        bool playerHovering = triggerTarget != null && triggerTarget.IsPointedAt;
        foreach (var interactor in wrench.interactorsHovering)
        {
            if (!(interactor is XRSocketInteractor))
            {
                playerHovering = true;
                break;
            }
        }

        if (drawer.IsOpen && !wasOpen)
            messageUntil = Time.time + 1.5f;
        wasOpen = drawer.IsOpen;

        string message = null;
        if (installation.IsInstalling)
            message = "Installing...";
        else if (!socketFilter.IsInstalled)
        {
            if (installation.IsHeld)
                message = installation.IsAiming
                    ? (installation.WaitingForGrip ? "Hold then release Grip to fit the wrench" : "Release Grip to fit the wrench")
                    : "Point at the correct drawer fitting";
            else if (playerHovering)
                message = "Hold Grip to pick up the wrench";
        }
        else if (drawer.IsOpen)
        {
            if (Time.time < messageUntil)
                message = "Drawer unlocked";
        }
        else if (wrenchTurn.IsComplete)
            message = "Opening drawer...";
        else if (wrenchTurn.IsTurning)
            message = $"Turning...  {wrenchTurn.CompletedSteps}/{wrenchTurn.requiredSteps}";
        else if (playerHovering)
            message = $"Press Trigger to turn  {wrenchTurn.CompletedSteps}/{wrenchTurn.requiredSteps}";

        if (message != null)
            hud.Show(this, message);
        else
            hud.Hide(this);
    }

    void OnDisable()
    {
        if (InteractionHUD.Instance != null)
            InteractionHUD.Instance.Hide(this);
    }
}
