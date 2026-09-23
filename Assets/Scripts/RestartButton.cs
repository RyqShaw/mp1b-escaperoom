using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public string gameScene = "GameScene";
    
    public void Restart()
    {
        Debug.Log("Restart");
        SceneManager.LoadScene(gameScene);
    }
}
