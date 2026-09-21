using TMPro;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text lossMessage;
    public TMP_Text completionTime;
    public ParticleSystem celebration;

    public void Show(EndGame.Result result, float elapsedSeconds)
    {
        bool won = result == EndGame.Result.Win;
        title.text = won ? "YOU ESCAPED!" : "TIME'S UP";
        lossMessage.text = "YOU FAILED TO ESCAPE";
        lossMessage.gameObject.SetActive(!won);
        completionTime.gameObject.SetActive(won);
        if (won)
        {
            int seconds = Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds));
            completionTime.text = $"TIME: {seconds / 60:00}:{seconds % 60:00}";
        }
        celebration.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        celebration.gameObject.SetActive(won);
        if (won) celebration.Play(true);
    }
}
