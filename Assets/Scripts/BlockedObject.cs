using UnityEngine;

public class BlockedObject : Vent
{
    public Crate blockedObject;
    public override void RemoveScrew(GameObject item)
    {
        base.RemoveScrew(item);
        if (isOpen)
        {
            blockedObject.enabled = true;
        }
    }

    protected override void OpenAction()
    {
        return;
    }
}