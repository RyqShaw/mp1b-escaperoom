using UnityEngine;

public class BaseLock : MonoBehaviour
{
    public GameObject keyNeeded;

    /// <summary>
    /// Waits for Key needed to be presented to the object
    /// </summary>
    /// <param name="other">Collider Presented</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == keyNeeded)
        {
            UnlockObstacle(other.gameObject);
        }
    }

    /// <summary>
    /// Runs when Key is put on lock
    /// By Default: Destroys Lock; Meant to be Overrided
    /// </summary>
    void UnlockObstacle(GameObject key)
    {
        Debug.Log($"{key.name} was used on {gameObject.name}");
        Destroy(gameObject);
    }
}
