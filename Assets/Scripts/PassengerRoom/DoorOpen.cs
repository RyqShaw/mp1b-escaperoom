using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 90f;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpening;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closedRotation = transform.localRotation;

        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpening)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                openRotation,
                speed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.localRotation, openRotation) < 0.1f)
            {
                transform.localRotation = openRotation;
                isOpening = false;
            }
        }
    }

    [ContextMenu("Open Door")]
    public void Open()
    {
        isOpening = true;
    }
}
