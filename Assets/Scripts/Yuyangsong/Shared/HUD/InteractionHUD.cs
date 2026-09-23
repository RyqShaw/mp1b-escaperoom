using TMPro;
using UnityEngine;

public class InteractionHUD : MonoBehaviour
{
    public static InteractionHUD Instance { get; private set; }

    public GameObject panel;
    public TMP_Text messageText;
    public Vector3 cameraOffset = new Vector3(0f, 0.22f, 1.2f);
    private MonoBehaviour currentSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Only one InteractionHUD should be active.", this);
            gameObject.SetActive(false);
            return;
        }
        Instance = this;
        panel.SetActive(false);
    }

    void Start()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("InteractionHUD needs a camera tagged MainCamera.", this);
            gameObject.SetActive(false);
            return;
        }

        // Follow the camera through parenting, rather than moving the HUD every frame.
        transform.SetParent(playerCamera.transform, false);
        transform.localPosition = cameraOffset;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void Show(MonoBehaviour source, string message)
    {
        if (InventoryController.BlocksWorld) return;
        currentSource = source;
        if (messageText.text != message)
        {
            messageText.text = message;
        }
        panel.SetActive(true);
    }

    public void Hide(MonoBehaviour source)
    {
        // Do not hide a message that another object just supplied.
        if (currentSource != source)
        {
            return;
        }
        currentSource = null;
        panel.SetActive(false);
    }

    void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
