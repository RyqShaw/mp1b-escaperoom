using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Room-owned connections. Portable prefabs never reference a particular room.
public class ItemTarget : MonoBehaviour
{
    public string targetId;
    public string acceptedItemId;
    [Tooltip("A component implementing IItemSocketRule.")]
    public MonoBehaviour socketRule;
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
    }

    public static ItemTarget Find(string id)
    {
        return id != null && targets.TryGetValue(id, out var target) ? target : null;
    }

    public bool Accepts(InventoryItem item, XRSocketInteractor socket)
    {
        if (item.ItemId != acceptedItemId || item.TargetId != targetId) return false;
        return socketRule != null && socketRule.isActiveAndEnabled &&
            socketRule is IItemSocketRule rule && rule.Accepts(socket);
    }
}
