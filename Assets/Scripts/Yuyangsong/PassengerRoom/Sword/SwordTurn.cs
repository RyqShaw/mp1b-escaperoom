using UnityEngine;

public class SwordTurn : MonoBehaviour
{
    public SwordSocketFilter socketFilter;
    public SwordAimInstall installation;
    public DoorOpen door;
    public float stepAngle = 30f;
    public int requiredSteps = 3;
    public float speed = 60f;
    Quaternion targetRotation;

    public bool IsTurning { get; private set; }
    public int CompletedSteps { get; private set; }
    public bool IsComplete => CompletedSteps >= requiredSteps;

    void Update()
    {
        if (!IsTurning) return;
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, targetRotation, speed * Time.deltaTime);
        if (Quaternion.Angle(transform.localRotation, targetRotation) >= 0.1f) return;
        transform.localRotation = targetRotation;
        IsTurning = false;
        CompletedSteps++;
        if (IsComplete) door.Open();
    }

    [ContextMenu("Turn One Step")]
    public void Turn()
    {
        if (InventoryController.BlocksWorld || socketFilter == null || door == null ||
            !socketFilter.IsInstalled || installation == null || installation.IsInstalling || IsTurning || IsComplete) return;
        targetRotation = transform.localRotation * Quaternion.Euler(0, 0, stepAngle);
        IsTurning = true;
    }
}
