using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Guard_Behavior : MonoBehaviour
{
    private float duration = 2f;
    public GameObject money;
    private bool moving = false;
    public ParticleSystem particle;
    public GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (moving) {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, duration * Time.deltaTime);
            if (transform.position == target.transform.position) {
                moving = false;
            }
        }
        
    }

    void OnCollisionEnter(Collision other) {
        if(other.gameObject == money) {
            Debug.Log("MONEY!");
            moving = true;
            particle.transform.position = other.gameObject.transform.position;
            particle.Play();
            Destroy(other.gameObject);
        }
        if(other.gameObject.CompareTag("Player")) {
            Debug.Log("No.");
        }
    }   
}
