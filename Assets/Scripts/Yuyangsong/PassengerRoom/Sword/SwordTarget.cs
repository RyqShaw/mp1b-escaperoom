using UnityEngine;

public class SwordTarget : MonoBehaviour
{
    public SwordSocketFilter socketFilter;
    public SwordTurn turn;
    public ToolboxLid toolbox;
    public Collider aimCollider;
    public GameObject preview;
    public Material previewMaterial;

    void Awake()
    {
        if (preview == null) return;
        // Keep the model and material editable in the room, without creating a second item.
        if (previewMaterial != null)
            foreach (var renderer in preview.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++) materials[i] = previewMaterial;
                renderer.sharedMaterials = materials;
            }
        preview.SetActive(false);
    }

    void OnDisable()
    {
        if (preview != null) preview.SetActive(false);
    }
}
