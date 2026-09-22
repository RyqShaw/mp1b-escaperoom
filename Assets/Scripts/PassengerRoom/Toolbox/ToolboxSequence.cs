using UnityEngine;
using UnityEngine.Events;

public class ToolboxSequence : MonoBehaviour
{
    public ToolboxLever[] levers;
    public int[] order = { 2, 3, 1 };
    public UnityEvent solved;
    [SerializeField] AudioSource errorAudio;
    public int Progress { get; private set; }
    public bool IsSolved { get; private set; }

    public void Press(ToolboxLever lever)
    {
        if (InventoryController.BlocksWorld || IsSolved || order.Length == 0 ||
            System.Array.IndexOf(levers, lever) < 0) return;
        foreach (var candidate in levers)
            if (candidate.IsMoving) return;

        if (lever.number != order[Progress])
        {
            Progress = 0;
            foreach (var candidate in levers) candidate.SetPulled(false);
            if (errorAudio != null) errorAudio.Play();
            return;
        }

        lever.SetPulled(true);
        Progress++;
        if (Progress < order.Length) return;
        IsSolved = true;
        solved.Invoke();
    }
}
