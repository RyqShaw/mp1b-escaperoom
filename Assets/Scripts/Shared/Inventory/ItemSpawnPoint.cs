using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    public string itemId;
    public string targetId;
    public InventoryItem prefab;
    public bool spawnOnStart = true;
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
}
