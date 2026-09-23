using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Vent : MonoBehaviour
{
    public Transform OpenTransform;
    public GameObject RevealedGameObject;
    public int screws;
    public float rate = 10f;
    [FormerlySerializedAs("_isOpen")] public bool isOpen = false;

    void Start()
    {
        Transform children = gameObject.GetComponentInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.CompareTag("Screw")) screws++;
        }
    }

    public virtual void RemoveScrew(GameObject item)
    {
        screws--;
        if (screws == 0)
        {
            if (RevealedGameObject) RevealedGameObject.SetActive(true);
            isOpen = true;
        }
    }

    void Update()
    {
        OpenAction();
    }

    protected virtual void OpenAction()
    {
        if (isOpen)
        {
            transform.position = Vector3.Lerp(transform.position, OpenTransform.position, rate * Time.deltaTime);
            if (Vector3.Distance(transform.position, OpenTransform.position) < 0.1f) isOpen = false;
        }
    }
}
