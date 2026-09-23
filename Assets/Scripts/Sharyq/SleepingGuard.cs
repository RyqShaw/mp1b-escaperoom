using UnityEngine;

public class SleepingGuard : MonoBehaviour
{
    public GameObject hand;

    public void OnKilled()
    {
        hand.SetActive(true);
        Destroy(gameObject);
    }
}
