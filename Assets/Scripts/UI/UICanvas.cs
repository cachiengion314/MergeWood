using UnityEngine;
using UnityEngine.SceneManagement;

public class UICanvas : MonoBehaviour
{
    [Header("Injected Dependencies")]
    [SerializeField] RectTransform gameOverPanel;
    [SerializeField] RectTransform gameMenuPanel;

    private void Start()
    {
        gameOverPanel.gameObject.SetActive(false);
        LevelManager.Instance.onGameStateChanged += LevelManager_onGameStateChanged;
    }

    private void OnDestroy()
    {
        LevelManager.Instance.onGameStateChanged -= LevelManager_onGameStateChanged;
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void PlayGame()
    {
        LevelManager.Instance.SetGameState(GameState.Gameplay);
    }

    private void LevelManager_onGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                gameMenuPanel.gameObject.SetActive(false);
                gameOverPanel.gameObject.SetActive(false);
                break;

            case GameState.Gameover:
                gameOverPanel.gameObject.SetActive(true);
                gameMenuPanel.gameObject.SetActive(false);
                break;

            case GameState.Menu:
                gameMenuPanel.gameObject.SetActive(true);
                gameOverPanel.gameObject.SetActive(false);
                break;

            default:
                break;
        }
    }
}
