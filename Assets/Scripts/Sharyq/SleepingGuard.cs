using UnityEngine;

public class SleepingGuard : MonoBehaviour
{
    public GameObject hand;

    public void OnKilled()
    {
        if (hand) hand.SetActive(true);
        Destroy(gameObject);
    }
}
