using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] private TMP_Text movesLeftText;
    [SerializeField] private Image[] goalImages;

    private Dictionary<TileType, int> goals = new()
    {
        { TileType.box,0 },
        { TileType.stone, 0 },
        { TileType.vase, 0 },
    };
    private Dictionary<TileType, ImageTextPair> goalTexts = new();
    private Level curLevel;
    private int movesLeft;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There is already a LevelManager in the scene");
            return;
        }

        int index = 0;
        foreach (var pair in goals)
        {
            goalImages[index].sprite = TileManager.instance.GetTileSprite(pair.Key, 0);
            goalTexts[pair.Key] = new ImageTextPair
            {
                image = goalImages[index],
                text = goalImages[index].GetComponentInChildren<TMP_Text>()
            };
            index++;
        }
    }
    private void Start()
    {
        curLevel = JsonUtility.FromJson<Level>(Resources.Load<TextAsset>("Levels/level_01").text);
        GridManager.instance.InitializeGrid(curLevel);
    }

    public void SetGoals(Dictionary<TileType,int> goals)
    {
        this.goals = goals;

        UpdateUI();
    }

    public void UpdateGoal(TileType type,int amount)
    {
        goals[type] += amount;
        goalTexts[type].text.text = goals[type].ToString();
    }

    public void DecreaseMoves()
    {
        movesLeft--;
        movesLeftText.text = movesLeft.ToString();
    }

    private void UpdateUI()
    {
        movesLeftText.text = curLevel.move_count.ToString();
        movesLeft = curLevel.move_count;

        int goalCount = goals.Count;

        foreach (var goal in goals)
        {
            if(goal.Value <= 0){
                goalCount--;
                goalTexts[goal.Key].image.gameObject.SetActive(false);
                continue;
            }
            goalTexts[goal.Key].image.gameObject.SetActive(true);
            goalTexts[goal.Key].text.text = goal.Value.ToString();
        }

        if (goalCount == 1)
        {
            goalImages[0].rectTransform.sizeDelta = new Vector2(100, 100);
            goalImages[0].rectTransform.anchoredPosition = new Vector2(0, 0);

        }
        else if (goalCount == 2)
        {
            goalImages[0].rectTransform.sizeDelta = new Vector2(75, 75);
            goalImages[1].rectTransform.sizeDelta = new Vector2(75, 75);

            goalImages[0].rectTransform.anchoredPosition = new Vector2(-50, 0);
            goalImages[1].rectTransform.anchoredPosition = new Vector2(50, 0);
        }
        else if (goalCount == 3)
        {
            goalImages[0].rectTransform.sizeDelta = new Vector2(50, 50);
            goalImages[1].rectTransform.sizeDelta = new Vector2(50, 50);
            goalImages[2].rectTransform.sizeDelta = new Vector2(50, 50);

            goalImages[0].rectTransform.anchoredPosition = new Vector2(-50, 25);
            goalImages[1].rectTransform.anchoredPosition = new Vector2(50, 25);
            goalImages[2].rectTransform.anchoredPosition = new Vector2(0, -25);
        }
    }

    private struct ImageTextPair
    {
        public Image image;
        public TMP_Text text;
    }
}
