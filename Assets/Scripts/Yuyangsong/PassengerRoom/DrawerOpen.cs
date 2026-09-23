using UnityEngine;

public class DrawerOpen : MonoBehaviour
{
    public float openDistance = 0.38f;
    public float speed = 0.25f;
    public ItemSpawnPoint reward;

    private Vector3 openPosition;
    private bool isOpening;
    public bool IsOpen { get; private set; }
    [SerializeField] AudioSource unlockAudio;

    void Start()
    {
        openPosition = transform.localPosition + Vector3.right * openDistance;
    }

    void Update()
    {
        if (!isOpening)
            return;

        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, openPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, openPosition) < 0.001f)
        {
            transform.localPosition = openPosition;
            isOpening = false;
            IsOpen = true;
            if (reward != null) reward.SpawnOnce();
        }
    }

    [ContextMenu("Open Drawer")]
    public void Open()
    {
        if (!IsOpen)
        {
            // Only the transition into opening plays feedback; IsOpen still means fully open.
            if (!isOpening && unlockAudio != null) unlockAudio.Play();
            isOpening = true;
        }
    }
}
