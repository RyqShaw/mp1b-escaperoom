using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Jumpscare : MonoBehaviour
{
    

    private bool jumpscare = false;
    private float duration = 20f;
    public GameObject camera;
    public GameObject spider;
    public GameObject box;
    private float check = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }



    // Update is called once per frame
    void Update()
    {
        if (box == null) {
            jumpscare = true;
            Jump();
            Debug.Log("Jumped");
            check++;
        }
        
    }

    public void Jump() {
        while(jumpscare){
            transform.position = Vector3.Lerp(transform.position, camera.transform.position, duration * Time.deltaTime);
            if (transform.position == camera.transform.position) {
                jumpscare = false;
                
            }
        }
        
    }
}
