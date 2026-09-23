using UnityEngine;
using UnityEngine.Events;

public class DoorOpen : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 90f;
    public UnityEvent opened = new UnityEvent();
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpening;
    public bool IsOpen { get; private set; }
    [SerializeField] AudioSource unlockAudio;

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
                IsOpen = true;
                opened.Invoke();
            }
        }
    }

    [ContextMenu("Open Door")]
    public void Open()
    {
        // Reuse the opening transition for audio only; keep the existing Open behavior.
        if (!isOpening && !IsOpen && unlockAudio != null) unlockAudio.Play();
        isOpening = true;
    }
}
