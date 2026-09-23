using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlashlightItemInteraction : MonoBehaviour
{
    public InventoryItem item;
    public Light spotLight;
    public static FlashlightItemInteraction Current { get; private set; }
    public bool IsOn => isActiveAndEnabled && !item.SystemRelease && spotLight.enabled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() { Current = null; }

    void OnEnable()
    {
        // Inventory recreates ordinary items. Each new instance starts switched off.
        spotLight.enabled = false;
        Current = this;
    }

    public void Use(ActivateEventArgs args)
    {
        if (InventoryController.BlocksWorld || item.SystemRelease || !item.Grab.isSelected) return;
        if (args.interactorObject == null || args.interactorObject.transform != item.Grab.interactorsSelecting[0].transform) return;
        spotLight.enabled = !spotLight.enabled;
    }

    void OnDisable()
    {
        spotLight.enabled = false;
        if (Current == this) Current = null;
    }
}
