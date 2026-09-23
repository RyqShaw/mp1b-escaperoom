using UnityEngine.XR.Interaction.Toolkit.Interactors;

public interface IItemSocketRule
{
    bool Accepts(XRSocketInteractor socket);
}
