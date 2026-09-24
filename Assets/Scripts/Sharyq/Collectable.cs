using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Collectable : MonoBehaviour
{
    private XRGrabInteractable _grabInteract;
    [SerializeField] private Score score;
    public ParticleSystem p;
    public AudioClip sound;

    void Awake() {
        _grabInteract = GetComponent<XRGrabInteractable>();
    }

    void Start() {
        Debug.LogWarning("SCORE NOT SET: Please Drag Score gameObject into Collectable");
    }

    private void OnEnable()
    {
        _grabInteract.selectEntered.AddListener(Grabbed);
    }

    private void OnDisable()
    {
        _grabInteract.selectEntered.RemoveListener(Grabbed);
    }

    public void Grabbed(SelectEnterEventArgs args) {
        ParticleSystem inst = Instantiate(p, transform.position,Quaternion.identity);
        inst.Play();
        if (sound) AudioSource.PlayClipAtPoint(sound, gameObject.transform.position);
        if (score) score.value++;
        Destroy(gameObject);
    }
}
