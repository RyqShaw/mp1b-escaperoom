using UnityEngine;
using UnityEngine.Events;

public class ToolboxLid : MonoBehaviour
{
    public Vector3 openEuler = new Vector3(0, 0, 105);
    public float speed = 60f;
    public UnityEvent opened;
    Quaternion openRotation;
    bool opening;
    public bool IsOpen { get; private set; }

    void Awake() { openRotation = transform.localRotation * Quaternion.Euler(openEuler); }

    [ContextMenu("Open Toolbox")]
    public void Open()
    {
        if (!IsOpen) opening = true;
    }

    void Update()
    {
        if (!opening) return;
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, openRotation, speed * Time.deltaTime);
        if (Quaternion.Angle(transform.localRotation, openRotation) >= 0.1f) return;
        transform.localRotation = openRotation;
        opening = false;
        IsOpen = true;
        opened.Invoke();
    }
}
