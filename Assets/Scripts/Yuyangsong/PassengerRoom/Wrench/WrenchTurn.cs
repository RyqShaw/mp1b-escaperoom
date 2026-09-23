using UnityEngine;

public class WrenchTurn : MonoBehaviour
{
    public DrawerOpen drawer;
    public WrenchSocketFilter socketFilter;
    public WrenchAimInstall installation;
    public float stepAngle = 30f;
    public int requiredSteps = 3;
    public float speed = 60f;

    private Quaternion targetRotation;
    private bool isTurning;
    private int completedSteps;
    public bool IsTurning => isTurning;
    public int CompletedSteps => completedSteps;
    public bool IsComplete => completedSteps >= requiredSteps;

    void Update()
    {
        if (!isTurning)
            return;

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, targetRotation, speed * Time.deltaTime);

        if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
        {
            transform.localRotation = targetRotation;
            isTurning = false;
            completedSteps++;
            if (IsComplete)
                drawer.Open();
        }
    }

    [ContextMenu("Turn One Step")]
    public void Turn()
    {
        if (InventoryController.BlocksWorld || installation == null || !socketFilter.IsInstalled || installation.IsInstalling || isTurning || IsComplete)
            return;

        targetRotation = transform.localRotation * Quaternion.Euler(0f, 0f, stepAngle);
        isTurning = true;
    }
}
