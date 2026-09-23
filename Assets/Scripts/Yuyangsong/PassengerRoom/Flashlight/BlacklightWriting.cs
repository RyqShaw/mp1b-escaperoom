using TMPro;
using UnityEngine;

public class BlacklightWriting : MonoBehaviour
{
    public TMP_Text writing;
    public Transform occlusionPoint;
    public LayerMask obstructionLayers = ~0;
    [Min(0)] public float surfaceOffset = 0.01f;
    Material material;

    void Awake() { material = writing.fontMaterial; }

    void LateUpdate()
    {
        var lamp = FlashlightItemInteraction.Current;
        bool visible = lamp != null && lamp.IsOn;
        if (visible)
        {
            var light = lamp.spotLight;
            Vector3 origin = light.transform.position;
            Vector3 delta = occlusionPoint.position - origin;
            // One ray to the writing centre: deliberately simple whole-panel occlusion.
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized,
                         Mathf.Max(0, delta.magnitude - surfaceOffset), obstructionLayers,
                         QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(lamp.transform)) continue;
                visible = false;
                break;
            }
            material.SetVector("_PRLightPosition", origin);
            material.SetVector("_PRLightDirection", light.transform.forward);
            material.SetFloat("_PRLightRange", light.range);
            material.SetFloat("_PRConeThreshold", Mathf.Cos(light.spotAngle * 0.5f * Mathf.Deg2Rad));
        }
        material.SetFloat("_PRVisible", visible ? 1 : 0);
    }

    void OnDisable() { if (material != null) material.SetFloat("_PRVisible", 0); }
    void OnDestroy() { if (material != null) Destroy(material); }
}
