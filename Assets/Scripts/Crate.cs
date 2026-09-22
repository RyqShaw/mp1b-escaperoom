using System;
using UnityEngine;

public class Crate : BaseLock
{
    public Transform OpenTransform;
    public bool _isOpen = false;
    public float rate = 10f;
    protected override void UnlockObstacle(GameObject key)
    {
        OnUnlock.Invoke(key);
        if (unlockSound) AudioSource.PlayClipAtPoint(unlockSound, gameObject.transform.position);
        _isOpen = true;
    }

    private void Update()
    {
        if (_isOpen && OpenTransform != null)
        {
            transform.position = Vector3.Lerp(transform.position, OpenTransform.position, rate * Time.deltaTime);
            if (Vector3.Distance(transform.position, OpenTransform.position) < 0.1f) Destroy(gameObject);
        }
    }
}
