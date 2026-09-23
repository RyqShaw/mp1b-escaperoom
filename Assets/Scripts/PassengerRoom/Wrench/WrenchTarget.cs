using UnityEngine;

public class WrenchTarget : MonoBehaviour
{
    public WrenchSocketFilter socketFilter;
    public WrenchTurn turn;
    public DrawerOpen drawer;
    public Collider aimCollider;
    public GameObject preview;

    void OnDisable()
    {
        if (preview != null) preview.SetActive(false);
    }
}
