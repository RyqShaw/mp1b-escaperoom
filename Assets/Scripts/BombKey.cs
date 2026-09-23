using System.Collections.Generic;
using UnityEngine;

public class BombKey : MonoBehaviour
{
    public GameObject particles;
    public void Explode()
    {
        var inst = Instantiate(particles, transform.position, Quaternion.identity);
        ParticleSystem[] explosionElements = inst.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in explosionElements)
        {
            p.Play();
        }
    }
}
