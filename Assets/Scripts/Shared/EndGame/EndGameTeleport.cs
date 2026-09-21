using Unity.XR.CoreUtils;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class EndGameTeleport : MonoBehaviour
{
    public XROrigin xrOrigin;
    public CharacterController characterController;
    [Tooltip("Floor position; local +Z is the player's viewing direction.")]
    public Transform spawnPoint;
    bool requested;

    public void EnterRoom() { requested = true; }

    void LateUpdate()
    {
        if (!requested) return;
        requested = false;
        if (xrOrigin == null || xrOrigin.Camera == null || spawnPoint == null)
        {
            Debug.LogError("Assign EndGameTeleport's XR Origin and SpawnPoint.", this);
            return;
        }
        bool wasEnabled = characterController != null && characterController.enabled;
        if (wasEnabled) characterController.enabled = false;
        xrOrigin.MatchOriginUpCameraForward(spawnPoint.up, spawnPoint.forward);
        xrOrigin.MoveCameraToWorldLocation(spawnPoint.position +
            spawnPoint.up * xrOrigin.CameraInOriginSpaceHeight);
        if (wasEnabled) characterController.enabled = true;
    }
}
