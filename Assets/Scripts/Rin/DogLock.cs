using UnityEngine;

public class DogLock : MonoBehaviour
{
    public GameObject plate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        if (plate == null) {
            Destroy(GetComponent<BoxCollider>());
        }
    }
}
