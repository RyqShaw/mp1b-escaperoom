using UnityEngine;

public class Bowl_Behavior : MonoBehaviour
{
    public GameObject food;
    public GameObject empty;
    public GameObject newBowl;
    private Vector3 location;
    public GameObject corgi;
    private Vector3 location2;
    public GameObject table;
    public GameObject target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        location = transform.position;
        location2 = target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision other) {
        if(other.gameObject == food) {
            GameObject bowl2 = Instantiate(newBowl, location, Quaternion.identity);
            bowl2.name = "second";
            corgi.transform.position = location2;
            if (table != null) {
                table.AddComponent<Rigidbody>();
                table.AddComponent<GrabOutline>();
            }
            Destroy(other.gameObject);
            Destroy(empty);
        }

    }   
}
