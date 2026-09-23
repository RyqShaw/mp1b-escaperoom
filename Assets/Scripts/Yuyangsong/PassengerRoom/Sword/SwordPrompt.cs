using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SwordPrompt : MonoBehaviour
{
    public PassengerTriggerTarget triggerTarget;
    public InventoryItem item;
    public SwordItemInteraction interaction;
    public SwordAimInstall installation;
    public SwordSocketFilter socketFilter;
    public SwordTurn turn;
    bool wasComplete;
    float unlockedUntil;

    void Update()
    {
        var hud = InteractionHUD.Instance;
        if (hud == null) return;
        if (InventoryController.BlocksWorld || !interaction.IsAccessible)
        {
            hud.Hide(this);
            return;
        }

        bool playerHovering = triggerTarget != null && triggerTarget.IsPointedAt;
        foreach (var interactor in item.Grab.interactorsHovering)
            if (!(interactor is XRSocketInteractor)) { playerHovering = true; break; }

        bool complete = turn != null && turn.IsComplete;
        if (complete && !wasComplete) unlockedUntil = Time.time + 1.5f;
        wasComplete = complete;

        string message = null;
        if (installation.IsInstalling)
            message = "Installing...";
        else if (!item.Installed)
        {
            if (item.Grab.isSelected)
                message = installation.IsAiming
                    ? (installation.WaitingForGrip ? "Hold then release Grip to fit the sword" : "Release Grip to fit the sword") : "Point at the cabinet fitting";
            else if (playerHovering)
                message = "Hold Grip to pick up the sword";
        }
        else if (complete)
        {
            if (Time.time < unlockedUntil) message = "Unlocked";
        }
        else if (turn != null && turn.IsTurning)
            message = $"Turning...  {turn.CompletedSteps}/{turn.requiredSteps}";
        else if (playerHovering && turn != null)
            message = $"Press Trigger to turn  {turn.CompletedSteps}/{turn.requiredSteps}";

        if (message != null) hud.Show(this, message);
        else hud.Hide(this);
    }

    void OnDisable()
    {
        if (InteractionHUD.Instance != null) InteractionHUD.Instance.Hide(this);
    }
}
