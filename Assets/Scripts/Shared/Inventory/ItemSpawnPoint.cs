using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    public string itemId;
    public string targetId;
    public InventoryItem prefab;
    public bool spawnOnStart = true;
    public bool HasBeenAcquired { get; private set; }
    bool requested;

    void Start() { if (spawnOnStart) requested = true; }
    void Update()
    {
        if (requested && InventoryController.Instance != null)
        {
            InventoryController.Instance.Spawn(this);
            requested = false;
        }
    }
    public void SpawnOnce() { requested = true; }
    public void MarkAcquired() { HasBeenAcquired = true; }
}
