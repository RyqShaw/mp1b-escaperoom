using System;
using UnityEngine;

public class ExplosionLock : BaseLock
{
    public String specifiedTag = "Dynamite";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(specifiedTag))
        {
            UnlockObstacle(other.gameObject);
        }
    }

    protected override void UnlockObstacle(GameObject key)
    {
        OnUnlock.Invoke(key);
        var bomb = key.GetComponent<BombKey>();
        if (bomb)
        {
            bomb.Explode();
            Destroy(key);
            Destroy(gameObject);
        }
    }
}
