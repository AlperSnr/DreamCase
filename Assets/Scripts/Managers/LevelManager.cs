using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] private TMP_Text movesLeftText;
    [SerializeField] private GoalStruct[] goals;
    [SerializeField] private RectTransform bgPanel;

    private Dictionary<TileType, GoalStruct> goalDict = new();
    private Level curLevel;
    private int movesLeft;

    private GameSceneUIManager uiManager => GameSceneUIManager.instance;

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

        foreach (var goal in goals)
        {
            goalDict.Add(goal.type, goal);
        }

    }
    private void Start()
    {
        int level = PlayerPrefs.GetInt("Level", 1);
        curLevel = JsonUtility.FromJson<Level>(Resources.Load<TextAsset>($"Levels/level_{level}").text);
        GridManager.instance.InitializeGrid(curLevel);

    }

    private void SetupUI()
    {
        movesLeftText.SetText(curLevel.move_count.ToString());
        movesLeft = curLevel.move_count;

        float bgHeight = bgPanel.rect.height;
        float newHeight = bgHeight * curLevel.grid_height / GridManager.instance.gridSize.y;
        bgPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        float bgWidth = bgPanel.rect.width;
        float newWidth = bgWidth * curLevel.grid_width / GridManager.instance.gridSize.x;
        bgPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);


        int validGoal = 0;
        List<RectTransform> activeGoalTransforms = new();
        foreach (var pair in goalDict)
        {
            if (pair.Value.goalLeft <= 0)
            {
                pair.Value.checkImage.transform.parent.gameObject.SetActive(false);
                continue;
            }
            pair.Value.countText.SetText(pair.Value.goalLeft.ToString());
            validGoal++;
            activeGoalTransforms.Add(pair.Value.checkImage.transform.parent.GetComponent<RectTransform>());
        }

        if (validGoal == 1)
        {
            activeGoalTransforms[0].anchoredPosition = new Vector2(0, 0);
        }
        else if (validGoal == 2)
        {
            activeGoalTransforms[0].anchoredPosition = new Vector2(-50, 0);
            activeGoalTransforms[1].anchoredPosition = new Vector2(50, 0);
        }
        else
        {
            //
        }
    }


    public void SetGoals(Dictionary<TileType, int> newGoals)
    {
        foreach (var goal in newGoals)
        {
            var goalStruct = goalDict[goal.Key];
            goalStruct.goalLeft = goal.Value;
            goalDict[goal.Key] = goalStruct;
        }
        SetupUI();
    }

    public void UpdateGoal(TileType type, int amount)
    {
        var goalStruct = goalDict[type];
        goalStruct.goalLeft += amount;
        goalStruct.countText.SetText(goalStruct.goalLeft.ToString());
        goalDict[type] = goalStruct;

        if (goalStruct.goalLeft <= 0)
        {
            goalStruct.checkImage.gameObject.SetActive(true);
            goalStruct.countText.enabled = false;
            CheckGoalsCompleted();
        }

    }

    private void CheckGoalsCompleted()
    {
        foreach (var goal in goalDict)
        {
            if (goal.Value.goalLeft > 0)
            {
                return;
            }
        }
        uiManager.ShowGameWinPanel();
    }

    public void DecreaseMoves()
    {
        movesLeft--;
        movesLeftText.text = movesLeft.ToString();

        if (movesLeft <= 0)
        {
            uiManager.ShowGameOverPanel();
        }
    }

    [System.Serializable]
    private struct GoalStruct
    {
        public TileType type;
        public TMP_Text countText;
        public Image checkImage;
        [HideInInspector] public int goalLeft;
    }
}
