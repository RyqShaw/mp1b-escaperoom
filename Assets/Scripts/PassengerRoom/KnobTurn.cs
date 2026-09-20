using UnityEngine;

public class KnobTurn : MonoBehaviour
{
    public DoorOpen door;
    public KnobSocketFilter socketFilter;
    public float stepAngle = 30f;
    public int requiredSteps = 3;
    public float speed = 60f;

    private Quaternion targetRotation;
    private bool isTurning = false;
    private int completedSteps = 0;

    public bool IsTurning => isTurning;
    public int CompletedSteps => completedSteps;
    public bool IsComplete => completedSteps >= requiredSteps;


    void Update()
    {
        if (!isTurning)
        {
            return;
        }

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            speed * Time.deltaTime
        );

        if (Quaternion.Angle(
            transform.localRotation, targetRotation) < 0.1f)
        {
            transform.localRotation = targetRotation;
            isTurning = false;

            completedSteps++;

            if (completedSteps >= requiredSteps)
            {
                door.Open();
            }
        }
    }

    [ContextMenu("Turn One Step")]
    public void Turn()
    {
        if (InventoryController.BlocksWorld || socketFilter == null || door == null || !socketFilter.IsInstalled)
        {
            return;
        }

        if (isTurning || completedSteps >= requiredSteps)
        {
            return;
        }

        targetRotation = transform.localRotation
            * Quaternion.Euler(0f, 0f, stepAngle);

        isTurning = true;
    }
}
