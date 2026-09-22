using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabOutline : MonoBehaviour
{
    public Material outlineMaterial;

    private Renderer[] _renderers;
    private Material[][] _baseMaterials;
    private XRGrabInteractable _grabInteract;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _grabInteract = GetComponent<XRGrabInteractable>();

        _baseMaterials = new Material[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _baseMaterials[i] = _renderers[i].materials;
        }
    }

    void OnEnable()
    {

        _grabInteract.hoverEntered.AddListener(OnHoverEntered);
        _grabInteract.hoverExited.AddListener(OnHoverExited);
    }

    void OnDisable()
    {
        _grabInteract.hoverEntered.RemoveListener(OnHoverEntered);
        _grabInteract.hoverExited.RemoveListener(OnHoverExited);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        SetOutline(true);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        SetOutline(false);
    }

    private void SetOutline(bool outlineEnabled)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (outlineEnabled)
            {
                var mats = new Material[_baseMaterials[i].Length + 1];
                _baseMaterials[i].CopyTo(mats, 0);
                mats[mats.Length - 1] = outlineMaterial;
                _renderers[i].materials = mats;
            }
            else
            {
                _renderers[i].materials = _baseMaterials[i];
            }
        }
    }
}