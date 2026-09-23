using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class EndGame : MonoBehaviour
{
    public enum Result { None, Win, Loss }
    public CountdownTimer timer;
    public ResultScreen resultScreen;
    public XRBaseInputInteractor leftHand, rightHand;
    public UnityEvent enterResultRoom = new UnityEvent();

    public Result CurrentResult { get; private set; }
    public float CompletionTime { get; private set; }

    public void Win() { Finish(Result.Win); }
    public void Lose() { Finish(Result.Loss); }

    void Finish(Result result)
    {
        if (!Application.isPlaying || CurrentResult != Result.None) return;
        if (timer == null || resultScreen == null)
        {
            Debug.LogError("Assign EndGame's timer and ResultScreen.", this);
            return;
        }
        CurrentResult = result;
        timer.StopTimer();
        CompletionTime = result == Result.Win
            ? Mathf.Max(0f, timer.InitialDuration - timer.RemainingTime) : 0f;
        DestroyHeldItem(leftHand);
        DestroyHeldItem(rightHand);
        resultScreen.Show(CurrentResult, CompletionTime);
        enterResultRoom.Invoke();
    }

    void DestroyHeldItem(XRBaseInputInteractor hand)
    {
        if (hand == null || !hand.hasSelection) return;
        var item = hand.firstInteractableSelected.transform.GetComponent<InventoryItem>();
        if (item == null) return;
        // Release without running aim-install; no storage or state preservation at game end.
        item.SystemRelease = true;
        hand.interactionManager.SelectExit((IXRSelectInteractor)hand, (IXRSelectInteractable)item.Grab);
        item.gameObject.SetActive(false);
        Destroy(item.gameObject);
    }

    [ContextMenu("Test Win")]
    void TestWin() { Win(); }
}
