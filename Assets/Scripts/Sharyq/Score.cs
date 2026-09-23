using UnityEngine;
using TMPro;
public class Score : MonoBehaviour
{
    public int value = 0;
    private TMP_Text score_text;

    void Awake() {
        score_text = GetComponent<TMP_Text>();
    }

    void LateUpdate() {
        score_text.text = "Score: " + value;
    }
}
