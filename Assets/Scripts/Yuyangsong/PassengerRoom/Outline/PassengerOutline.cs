using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// A visual only component: either one portable prop or an Inspector-assigned lock group.
[DisallowMultipleComponent]
public class PassengerOutline : MonoBehaviour
{
    public InventoryItem item;
    [Tooltip("Optional subtree for imported models. Explicit renderers take precedence.")]
    public Transform visualRoot;
    public Renderer[] renderers;
    public Material outlineMaterial;
    [Header("Lock group (not needed on portable props)")]
    public string matchingItemId;
    public DrawerOpen drawer;
    public DoorOpen door;
    [Header("Appearance")]
    public Color grabYellow = new Color(1f, 0.82f, 0.02f, 1f);
    public Color defaultLockColor = new Color(0.65f, 0.3f, 1f, 0.85f);
    public Color activeMatchingColor = new Color(0f, 1f, 0.85f, 1f);
    [Min(0f)] public float outlineWidth = 0.004f;
    [Min(0f)] public float intensity = 1.5f;
    [Min(0.01f)] public float fadeDuration = 0.5f;

    readonly List<Renderer> shells = new List<Renderer>();
    readonly List<Renderer> sources = new List<Renderer>();
    readonly List<Bounds> meshBounds = new List<Bounds>();
    MaterialPropertyBlock properties;
    bool solved;
    float fadeStart;
    Color shownColor;
    static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");

    void Awake()
    {
        properties = new MaterialPropertyBlock();
        shownColor = item != null ? grabYellow : defaultLockColor;
        if (outlineMaterial == null) return;
        var targets = renderers != null && renderers.Length > 0 ? renderers :
            (visualRoot != null ? visualRoot : transform).GetComponentsInChildren<Renderer>(true);
        foreach (var source in targets)
        {
            if (source == null || sources.Contains(source)) continue;
            Mesh mesh = null;
            var skin = source as SkinnedMeshRenderer;
            if (skin != null) mesh = skin.sharedMesh;
            else if (source is MeshRenderer && source.TryGetComponent<MeshFilter>(out var filter))
                mesh = filter.sharedMesh;
            if (mesh == null) continue; // Never outline particles, text or light beams.
            var child = new GameObject("Outline (visual only)");
            child.layer = source.gameObject.layer;
            child.transform.SetParent(source.transform, false);
            Renderer shell;
            if (skin != null)
            {
                var copy = child.AddComponent<SkinnedMeshRenderer>();
                copy.sharedMesh = mesh;
                copy.bones = skin.bones;
                copy.rootBone = skin.rootBone;
                copy.localBounds = skin.localBounds;
                shell = copy;
            }
            else
            {
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                shell = child.AddComponent<MeshRenderer>();
            }
            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++) materials[i] = outlineMaterial;
            shell.sharedMaterials = materials;
            shell.shadowCastingMode = ShadowCastingMode.Off;
            shell.receiveShadows = false;
            shell.lightProbeUsage = LightProbeUsage.Off;
            shell.reflectionProbeUsage = ReflectionProbeUsage.Off;
            sources.Add(source);
            shells.Add(shell);
            meshBounds.Add(mesh.bounds);
        }
    }

    // Deterministic single focus when both hands hold props: right hand, otherwise left.
    // Query live inventory hands every frame; stored/destroyed copies never stay active.
    static InventoryItem HeldItem()
    {
        var inventory = InventoryController.Instance;
        if (inventory == null) return null;
        var held = inventory.Held(false);
        if (held == null) held = inventory.Held(true);
        return held != null && !held.SystemRelease && !held.Installed && held.isActiveAndEnabled
            ? held : null;
    }

    bool IsSolved()
    {
        if (item == null) return (drawer != null && drawer.IsOpen) || (door != null && door.IsOpen);
        if (!item.Installed || item.Target == null) return false;
        // Resolve through existing stable target IDs after every inventory reconstruction.
        if (item.Target.TryGetComponent<WrenchTarget>(out var wrench))
            return wrench.drawer != null && wrench.drawer.IsOpen;
        if (item.Target.TryGetComponent<KnobTarget>(out var knob))
            return knob.door != null && knob.door.IsOpen;
        if (item.Target.TryGetComponent<SwordTarget>(out var sword))
            return sword.turn != null && sword.turn.door != null && sword.turn.door.IsOpen;
        return false;
    }

    void LateUpdate()
    {
        if (!solved && IsSolved()) { solved = true; fadeStart = Time.time; }
        float visibility = 1f;
        if (solved)
            visibility = 1f - Mathf.SmoothStep(0f, 1f, (Time.time - fadeStart) / Mathf.Max(0.01f, fadeDuration));
        else
        {
            var held = HeldItem();
            bool matching = held != null && (item != null
                ? held == item && !string.IsNullOrEmpty(item.TargetId)
                : held.ItemId == matchingItemId);
            shownColor = matching ? activeMatchingColor :
                item != null && !item.Installed ? grabYellow : defaultLockColor;
        }
        Color color = shownColor;
        color.r *= intensity; color.g *= intensity; color.b *= intensity;
        color.a *= visibility;
        properties.SetColor(ColorId, color);
        properties.SetFloat(WidthId, outlineWidth);
        for (int i = 0; i < shells.Count; i++)
        {
            if (shells[i] == null) continue;
            shells[i].enabled = isActiveAndEnabled && sources[i] != null && sources[i].enabled && visibility > 0f;
            properties.SetVector("_BoundsCenter", meshBounds[i].center);
            properties.SetVector("_BoundsExtents", meshBounds[i].extents);
            shells[i].SetPropertyBlock(properties);
        }
    }

    void OnDisable() { foreach (var shell in shells) if (shell != null) shell.enabled = false; }
    void OnDestroy() { foreach (var shell in shells) if (shell != null) Destroy(shell.gameObject); }
}
