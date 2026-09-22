using System;
using System.ComponentModel;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Category("Traits")]
    public Transform OpenTransform;
    public bool _isOpen = false;
    
    [Category("Door Movement")]
    public float rate = 0.25f;
    public float dampening = 0.5f;
    public float control = 2f;
    private Vector3 _velocity;

    public void Open()
    {
        _isOpen = true;
    }

    public void LateUpdate()
    {
        if (_isOpen && OpenTransform != null)
        {
            transform.position += _velocity * Time.deltaTime;
            _velocity += new Vector3(rate * (control - transform.position.x) - dampening * _velocity.x, 0, 0);
            
            if (Mathf.Abs(transform.position.x - OpenTransform.position.x) < 0.01f) Destroy(gameObject);
        }
    }
}
