using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform exit;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) other.gameObject.transform.position = exit.position;
    }
}
