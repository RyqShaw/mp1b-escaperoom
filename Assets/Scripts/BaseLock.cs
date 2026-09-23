using System;
using UnityEngine;
using UnityEngine.Events;

public class BaseLock : MonoBehaviour
{
    public UnityEvent<GameObject> OnUnlock;
    public GameObject keyNeeded;
    public bool destroyOnUnlock = true;
    public AudioClip unlockSound;
    

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
    protected virtual void UnlockObstacle(GameObject key)
    {
        OnUnlock.Invoke(key);
        Debug.Log($"{key.name} was used on {gameObject.name}");
        if (unlockSound) AudioSource.PlayClipAtPoint(unlockSound, gameObject.transform.position);
        if (destroyOnUnlock) Destroy(gameObject);
    }
}
