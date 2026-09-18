using System.Collections.Generic;
using UnityEngine;

public class Vent : MonoBehaviour
{
    public GameObject RevealedGameObject;
    public int screws;

    void Start()
    {
        Transform children = gameObject.GetComponentInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.CompareTag("Screw")) screws++;
        }
    }

    public void RemoveScrew(GameObject item)
    {
        screws--;
        if (screws == 0)
        {
            RevealedGameObject.SetActive(true);
            Destroy(gameObject);
        }
    }
}
