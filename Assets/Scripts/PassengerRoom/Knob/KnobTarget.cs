using UnityEngine;

public class KnobTarget : MonoBehaviour
{
    public KnobSocketFilter socketFilter;
    public DoorOpen door;
    public Collider aimCollider;
    public GameObject preview;

    void OnDisable()
    {
        if (preview != null) preview.SetActive(false);
    }
}
