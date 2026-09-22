using UnityEngine;
using UnityEngine.Rendering;

// Reads existing progression only. Waypoints are relative to the room, on its floor.
[DisallowMultipleComponent]
public class PassengerGroundLine : MonoBehaviour
{
    public PassengerProgressBoard progress;
    public Material lineMaterial;
    [ColorUsage(true, true)] public Color lineColor = new Color(0.2f, 1f, 0.35f, 1f);
    [Min(0f)] public float brightness = 2f;
    [Min(0.001f)] public float lineWidth = 0.055f;
    public float groundHeightOffset = 0.025f;
    public Vector3[] initialToolsWaypoints;
    public Vector3[] hintBoardWaypoints;
    public Vector3[] luggageDoorWaypoints;
    public Vector3[] exitWaypoints;
    LineRenderer line;
    MaterialPropertyBlock properties;

    void Awake()
    {
        var child = new GameObject("Ground guidance (visual only)");
        child.transform.SetParent(transform, false);
        child.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        line = child.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.alignment = LineAlignment.TransformZ;
        line.sharedMaterial = lineMaterial;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.lightProbeUsage = LightProbeUsage.Off;
        line.numCornerVertices = 0;
        line.numCapVertices = 0;
        properties = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (progress == null || lineMaterial == null) { line.enabled = false; return; }
        bool complete = progress.toolbox.IsSolved && progress.emergencyBox.IsOpen &&
            progress.sword.HasBeenAcquired && progress.spray.HasBeenAcquired &&
            progress.screwdriver.HasBeenAcquired;
        Vector3[] points;
        if (complete) points = exitWaypoints;
        else if (progress.luggageDoor.IsOpen) points = null; // Internal exploration: no line.
        else if (progress.knob.HasBeenAcquired) points = luggageDoorWaypoints;
        else if (progress.wrench.HasBeenAcquired && progress.flashlight.HasBeenAcquired) points = hintBoardWaypoints;
        else points = initialToolsWaypoints;
        line.enabled = points != null && points.Length > 1;
        if (!line.enabled) return;
        line.positionCount = points.Length;
        line.widthMultiplier = lineWidth;
        for (int i = 0; i < points.Length; i++)
            line.SetPosition(i, transform.TransformPoint(points[i] + Vector3.up * groundHeightOffset));
        Color color = lineColor;
        color.r *= brightness; color.g *= brightness; color.b *= brightness;
        properties.SetColor("_BaseColor", color);
        line.SetPropertyBlock(properties);
    }

    void OnDisable() { if (line != null) line.enabled = false; }
    void OnDestroy() { if (line != null) Destroy(line.gameObject); }
}
