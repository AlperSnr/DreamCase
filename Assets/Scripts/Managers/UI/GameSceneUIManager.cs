using UnityEngine;

public class GameSceneUIManager : MonoBehaviour
{
    public static GameSceneUIManager instance;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWinPanel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ClosePanels();
    }

    private void ClosePanels()
    {
        gameOverPanel.SetActive(false);
        //gameWinPanel.SetActive(false);
    }

    public void ShowGameOverPanel()
    {
        ClosePanels();
        gameOverPanel.SetActive(true);
    }

    public void ShowGameWinPanel()
    {
        ClosePanels();
        gameWinPanel.SetActive(true);
    }

    public void NextLevelBtn()
    {
        int level = PlayerPrefs.GetInt("Level", 1);
        PlayerPrefs.SetInt("Level", level + 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void MainMenuBtn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void RestartBtn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }



}
