using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Room-owned connections. Portable prefabs never reference a particular room.
public class ItemTarget : MonoBehaviour
{
    public string targetId;
    public string acceptedItemId;
    public KnobSocketFilter knobFilter;
    public DoorOpen door;
    public WrenchSocketFilter wrenchFilter;
    public WrenchTurn wrenchTurn;
    public DrawerOpen drawer;
    public Collider aimCollider;
    public GameObject preview;
    static readonly Dictionary<string, ItemTarget> targets = new Dictionary<string, ItemTarget>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetRegistry() { targets.Clear(); }

    void OnEnable()
    {
        if (string.IsNullOrWhiteSpace(targetId)) return;
        if (targets.TryGetValue(targetId, out var existing) && existing != null && existing != this)
        {
            Debug.LogError("Duplicate lock ID: " + targetId, this);
            return;
        }
        targets[targetId] = this;
    }

    void OnDisable()
    {
        if (Find(targetId) == this) targets.Remove(targetId);
        if (preview != null) preview.SetActive(false);
    }

    public static ItemTarget Find(string id)
    {
        return id != null && targets.TryGetValue(id, out var target) ? target : null;
    }

    public bool Accepts(InventoryItem item, XRSocketInteractor socket)
    {
        if (item.ItemId != acceptedItemId || item.TargetId != targetId) return false;
        if (knobFilter != null) return socket == knobFilter.socket &&
            (knobFilter.InstallationRequested || knobFilter.IsInstalled);
        return wrenchFilter != null && socket == wrenchFilter.socket &&
            (wrenchFilter.InstallationRequested || wrenchFilter.IsInstalled);
    }
}
