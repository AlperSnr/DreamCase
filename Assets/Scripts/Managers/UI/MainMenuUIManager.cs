using TMPro;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private bool overrideLevel = false;
    [SerializeField] private int levelToOverride = 1;

    private int levelCounter = 1;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            levelCounter = PlayerPrefs.GetInt("Level");
        }
        else
        {
            levelCounter = 1;
        }

        if (overrideLevel)
        {
            levelCounter = levelToOverride;
            PlayerPrefs.SetInt("Level", levelCounter);
        }

        if (levelCounter > 10)
        {
            buttonText.SetText("Finished");
        }
        else
        {
            buttonText.SetText($"Level {levelCounter}");
        }
    }

    public void StartLevel()
    {
        if(levelCounter > 10)
        {
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

}
