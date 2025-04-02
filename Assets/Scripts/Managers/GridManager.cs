using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEditor.PlayerSettings;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    [SerializeField] private Transform gridParent;
    public Vector2 gridSize = new(9, 9);
    private int width;
    private int height;
    public GameObject tilePrefab;
    public GameObject rocketPrefab;
    private Tile[,] grid;
    private float rocketTime = 1.2f;

    private Vector2 calculatedScale = new(1, 1);

    private void Awake()
    {
        instance = this;
    }
    public void InitializeGrid(Level curLevel)
    {
        width = curLevel.grid_width;
        height = curLevel.grid_height;
        grid = new Tile[width, height * 2];

        int bigger = Mathf.Max(width, height);
        calculatedScale = new Vector2(gridSize.x / bigger, gridSize.y / bigger) * tilePrefab.transform.localScale;

        gridParent.localPosition += new Vector3((gridSize.x - width) / 2, 0);

        PoolingManager.instance.CreatePool(Tile.poolingTag, tilePrefab, width * height);
        CreateGrid(curLevel);
        FindAllLargeGroups();
    }

    void CreateGrid(Level curLevel)
    {
        Dictionary<TileType, int> goals = new()
        {
            { TileType.box, 0 },
            { TileType.vase, 0 },
            { TileType.stone, 0 },
        };
        int counter = 0;
        foreach (string s in curLevel.grid)
        {

            TileType type = TileType.rand;
            switch (s)
            {
                case "r":
                    type = TileType.red;
                    break;
                case "g":
                    type = TileType.green;
                    break;
                case "b":
                    type = TileType.blue;
                    break;
                case "y":
                    type = TileType.yellow;
                    break;
                case "vro":
                    type = TileType.rocketV;
                    break;
                case "hro":
                    type = TileType.rocketH;
                    break;
                case "bo":
                    type = TileType.box;
                    goals[TileType.box]++;
                    break;
                case "s":
                    type = TileType.stone;
                    goals[TileType.stone]++;
                    break;
                case "v":
                    type = TileType.vase;
                    goals[TileType.vase]++;
                    break;

            }
            SpawnTile(counter % curLevel.grid_width, counter / curLevel.grid_width, type);
            counter++;
        }

        LevelManager.instance.SetGoals(goals);
    }

    void SpawnTile(int x, int y, TileType type)
    {
        GameObject tile = PoolingManager.instance.GetFromPool(Tile.poolingTag);
        tile.transform.parent = gridParent;
        tile.transform.localPosition = new Vector2(x, height + 1);
        tile.transform.localScale = calculatedScale;
        grid[x, y] = tile.GetComponent<Tile>();

        switch (type)
        {
            case TileType.rand:
                int randomType = Random.Range(0, 4);
                grid[x, y].SetType((TileType)randomType);
                break;
            default:
                grid[x, y].SetType(type);
                break;
        }

        tile.transform.DOLocalMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
    }

    public void CheckMatches(int x, int y)
    {
        if (grid[x, y] == null) return;
        List<Vector2Int> matchedBlocks = new();
        List<Vector2Int> obstacles = new();

        FindMatches(x, y, grid[x, y].type, matchedBlocks, obstacles);

        if (matchedBlocks.Count < 2) return;
        LevelManager.instance.DecreaseMoves();

        List<int> effectedColumns = new();

        if (matchedBlocks.Count >= 4)
        {
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                grid[x, y].SetType(TileType.rocketV);
            }
            else
            {
                grid[x, y].SetType(TileType.rocketH);
            }
            matchedBlocks.Remove(new Vector2Int(x, y));
        }

        foreach (var pos in matchedBlocks)
        {
            DestroyTile(pos.x, pos.y);
            if (!effectedColumns.Contains(pos.x))
            {
                effectedColumns.Add(pos.x);
            }
        }

        foreach (var pos in obstacles)
        {
            if (grid[pos.x, pos.y].Damage())
            {
                DestroyTile(pos.x, pos.y);
                if (!effectedColumns.Contains(pos.x))
                {
                    effectedColumns.Add(pos.x);
                }
            }
        }

        DropTiles(effectedColumns);
    }

    private void DestroyTile(int x, int y)
    {
        if (grid[x, y] == null) return;

        if (grid[x, y].isObstacle)
            LevelManager.instance.UpdateGoal(grid[x, y].type, -1);

        PoolingManager.instance.ReturnToPool(Tile.poolingTag, grid[x, y].gameObject);
        grid[x, y] = null;
    }

    private void FindMatches(int x, int y, TileType type, List<Vector2Int> matchedBlocks, List<Vector2Int> obstacles)
    {
        if (!IsValidCoordinate(x, y)) return;
        if (grid[x, y] == null) return;

        if (grid[x, y].isObstacle && !obstacles.Contains(new Vector2Int(x, y)))
        {
            obstacles.Add(new Vector2Int(x, y));
            return;
        }

        if (grid[x, y].type != type) return;
        if (matchedBlocks.Contains(new Vector2Int(x, y))) return;
        matchedBlocks.Add(new Vector2Int(x, y));
        FindMatches(x + 1, y, type, matchedBlocks, obstacles);
        FindMatches(x - 1, y, type, matchedBlocks, obstacles);
        FindMatches(x, y + 1, type, matchedBlocks, obstacles);
        FindMatches(x, y - 1, type, matchedBlocks, obstacles);
    }

    private void FindAllLargeGroups()
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null && !visited[x, y])
                {
                    List<Vector2Int> matchedBlocks = new List<Vector2Int>();
                    List<Vector2Int> obstacles = new List<Vector2Int>();
                    FindMatches(x, y, grid[x, y].type, matchedBlocks, obstacles);

                    ToggleHintOfGroup(matchedBlocks, matchedBlocks.Count >= 4);

                    foreach (var pos in matchedBlocks)
                    {
                        visited[pos.x, pos.y] = true;
                    }
                }
            }
        }
    }

    private void ToggleHintOfGroup(List<Vector2Int> group, bool isHint)
    {
        foreach (var pos in group)
        {
            if (grid[pos.x, pos.y] != null)
            {
                grid[pos.x, pos.y].SetRocketHint(isHint);
            }
        }
    }

    public void Rocket(int x, int y, TileType type)
    {
        if (type == TileType.rocketV || type == TileType.rocketH)
        {
            if (!CheckAdjacentRockets(x, y))
                StartCoroutine(LaunchRocket(x, y, type));
            else
                RocketCombo(x, y);
        }
        else
        {
            Debug.LogError("GridManager: Invalid rocket type");
        }
    }

    private bool CheckAdjacentRockets(int x, int y)
    {
        if (x > 0 && grid[x - 1, y] != null && (grid[x - 1, y].type == TileType.rocketV || grid[x - 1, y].type == TileType.rocketH))
        {
            return true;
        }
        if (x < width - 1 && grid[x + 1, y] != null && (grid[x + 1, y].type == TileType.rocketV || grid[x + 1, y].type == TileType.rocketH))
        {
            return true;
        }
        if (y > 0 && grid[x, y - 1] != null && (grid[x, y - 1].type == TileType.rocketV || grid[x, y - 1].type == TileType.rocketH))
        {
            return true;
        }
        if (y < height - 1 && grid[x, y + 1] != null && (grid[x, y + 1].type == TileType.rocketV || grid[x, y + 1].type == TileType.rocketH))
        {
            return true;
        }
        return false;
    }

    private void RocketCombo(int x, int y)
    {
        for (int i = -1; i < 2; i++)
        {
            if (IsValidCoordinate(x, y + i))
            {
                StartCoroutine(LaunchRocket(x, y + i, TileType.rocketH));
            }

            if (IsValidCoordinate(x + i, y))
            {
                StartCoroutine(LaunchRocket(x + i, y, TileType.rocketV));
            }
        }


    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private IEnumerator LaunchRocket(int x, int y, TileType type)
    {
        yield return new WaitForEndOfFrame();
        Vector2 dir1 = Vector2.zero;
        Vector2 dir2 = Vector2.zero;
        if (type == TileType.rocketV)
        {
            dir1 = Vector2.up;
            dir2 = Vector2.down;
        }
        else if (type == TileType.rocketH)
        {
            dir1 = Vector2.right;
            dir2 = Vector2.left;
        }
        else
        {
            yield break;
        }

        GameObject rocket1 = Instantiate(rocketPrefab, gridParent);
        GameObject rocket2 = Instantiate(rocketPrefab, gridParent);

        rocket1.transform.localPosition = new Vector3(x, y);
        rocket2.transform.localPosition = new Vector3(x, y);

        DestroyTile(x, y);

        rocket1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetTileSprite(type, 1);
        rocket2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetTileSprite(type, 2);

        Vector2 target1 = (Vector2)rocket1.transform.localPosition + dir1 * height * 2;
        Vector2 target2 = (Vector2)rocket2.transform.localPosition + dir2 * height * 2;

        Sequence seq1 = DOTween.Sequence();
        Sequence seq2 = DOTween.Sequence();

        seq1.Append(rocket1.transform.DOLocalMove(target1, rocketTime).SetEase(Ease.Linear));
        seq2.Append(rocket2.transform.DOLocalMove(target2, rocketTime).SetEase(Ease.Linear));

        List<Vector2Int> rocket1Visited = new();
        List<Vector2Int> rocket2Visited = new();

        seq1.OnUpdate(() =>
        {
            RocketOnUpdate(rocket1, rocket1Visited);
        });

        seq2.OnUpdate(() =>
        {
            RocketOnUpdate(rocket2, rocket2Visited);
        });

        yield return seq1.WaitForCompletion();
        if (seq2.active)
            yield return seq2.WaitForCompletion();

        Destroy(rocket1, 2f);
        Destroy(rocket2, 2f);

        List<int> effectedColumns = new();
        if (dir1 == Vector2.up || dir1 == Vector2.down)
        {
            effectedColumns.Add(x);
        }
        else if (dir1 == Vector2.left || dir1 == Vector2.right)
        {
            for (int i = 0; i < width; i++)
            {
                effectedColumns.Add(i);
            }
        }

        DropTiles(effectedColumns);
    }

    private void RocketOnUpdate(GameObject rocketInstance, List<Vector2Int> visited)
    {
        Vector2 pos = rocketInstance.transform.localPosition;
        int gridX = Mathf.RoundToInt(pos.x);
        int gridY = Mathf.RoundToInt(pos.y);

        if (visited.Contains(new Vector2Int(gridX, gridY)))
        {
            return;
        }
        visited.Add(new Vector2Int(gridX, gridY));

        if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height && grid[gridX, gridY] != null)
        {
            if (grid[gridX, gridY].type == TileType.rocketV || grid[gridX, gridY].type == TileType.rocketH)
            {
                Rocket(gridX, gridY, grid[gridX, gridY].type);
            }
            else if (grid[gridX, gridY].isObstacle)
            {
                if (grid[gridX, gridY].Damage())
                {
                    DestroyTile(gridX, gridY);
                }
            }
            else
                DestroyTile(gridX, gridY); // TODO buraya obstacle için farklı mantık ekle
        }
    }

    private void DropTiles(List<int> effectedColumns)
    {
        foreach (int x in effectedColumns)
        {
            int emptyCount = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                if (grid[x, y] == null)
                {
                    emptyCount++;
                }
                else if (grid[x, y].type == TileType.stone)
                {
                    break;
                }
            }

            if (emptyCount == 0)
            {
                continue;
            }

            for (int i = 0; i < emptyCount; i++)
            {
                SpawnTile(x, height + i, TileType.rand);
            }

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int k = y + 1; k < height + emptyCount; k++)
                    {
                        if (grid[x, k] != null)
                        {
                            if (grid[x, k].type == TileType.stone)
                                break;
                            grid[x, y] = grid[x, k];
                            grid[x, k] = null;
                            grid[x, y].transform.DOLocalMove(new Vector2(x, y), 0.3f);
                            break;
                        }
                    }
                }
            }
        }
        FindAllLargeGroups();
    }
}
