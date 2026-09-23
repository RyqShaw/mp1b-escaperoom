using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CountdownTimer : MonoBehaviour
{
    [Min(0)] public float durationSeconds = 1200f;
    public EndGame endGame;
    public Transform playerCamera;
    public Transform hud;
    public TMP_Text timeText;
    public Vector3 cameraOffset = new Vector3(0.32f, -0.22f, 1.2f);

    public float InitialDuration { get; private set; }
    public float RemainingTime { get; private set; }
    public bool IsRunning { get; private set; }
    int displayedSeconds = -1;

    void Awake()
    {
        InitialDuration = Mathf.Max(0f, durationSeconds);
        RemainingTime = InitialDuration;
    }

    void Start()
    {
        if (endGame == null || playerCamera == null || hud == null || timeText == null)
        {
            Debug.LogError("Assign CountdownTimer's EndGame, camera, HUD and text.", this);
            enabled = false;
            return;
        }
        hud.SetParent(playerCamera, false);
        hud.localPosition = cameraOffset;
        hud.localRotation = Quaternion.identity;
        IsRunning = endGame.CurrentResult == EndGame.Result.None;
        RefreshText();
    }

    void Update()
    {
        if (!IsRunning) return;
        RemainingTime = Mathf.Max(0f, RemainingTime - Time.unscaledDeltaTime);
        RefreshText();
        if (RemainingTime > 0f) return;
        IsRunning = false;
        endGame.Lose();
    }

    public void StopTimer()
    {
        IsRunning = false;
        RefreshText();
    }

    void RefreshText()
    {
        int seconds = Mathf.CeilToInt(RemainingTime);
        if (seconds == displayedSeconds || timeText == null) return;
        displayedSeconds = seconds;
        timeText.text = $"{seconds / 60:00}:{seconds % 60:00}";
    }
}
