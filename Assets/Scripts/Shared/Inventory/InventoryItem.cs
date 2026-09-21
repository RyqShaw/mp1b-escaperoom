using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class InventoryItem : MonoBehaviour, IXRSelectFilter
{
    public string displayName;
    public bool allowStorage = true;
    public string stateJson = "{}";
    [SerializeField] XRGrabInteractable grab;
    [Tooltip("Components implementing IInventoryStorageRule. All must allow storage.")]
    public MonoBehaviour[] storageRules = new MonoBehaviour[0];

    public string ItemId { get; private set; }
    public string TargetId { get; private set; }
    public InventoryItem Prefab { get; private set; }
    public XRGrabInteractable Grab => grab;
    public bool SystemRelease { get; set; }
    public bool Installed { get; private set; }
    public ItemTarget Target => ItemTarget.Find(TargetId);
    public bool canProcess => isActiveAndEnabled;
    public bool CanStore
    {
        get
        {
            if (!allowStorage || Installed || SystemRelease) return false;
            foreach (var component in storageRules)
                if (!(component is IInventoryStorageRule rule) || !rule.CanStore) return false;
            return true;
        }
    }

    public void Initialize(string id, string targetId, InventoryItem prefab, string state = "{}")
    {
        ItemId = id;
        TargetId = targetId;
        Prefab = prefab;
        stateJson = state;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (SystemRelease) return false;
        if (interactor is XRSocketInteractor socket)
            return Target != null && Target.Accepts(this, socket);
        if (Installed) return false;
        if (Grab.isSelected && !Grab.interactorsSelecting.Contains(interactor)) return false;
        // Keep existing holds and allow the inventory's explicit handover.
        return !InventoryController.BlocksWorld || Grab.interactorsSelecting.Contains(interactor) ||
            (InventoryController.Instance != null && InventoryController.Instance.IsRetrieving(this));
    }

    public void MarkInstalled() { Installed = true; }
}
