using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEditor.PlayerSettings;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    private int width;
    private int height;
    public GameObject tilePrefab;
    public GameObject rocketPrefab; // Roket prefabı
    private Tile[,] grid;
    private float rocketTime = 0.4f;
    private Level curLevel;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Level curLevel = JsonUtility.FromJson<Level>(Resources.Load<TextAsset>("Levels/level_01").text);
        width = curLevel.grid_width;
        height = curLevel.grid_height;
        grid = new Tile[width, height * 2];
        InitializeGrid();
    }

    void InitializeGrid()
    {
        PoolingManager.instance.CreatePool(Tile.poolingTag, tilePrefab, width * height);
        Level curLevel = JsonUtility.FromJson<Level>(Resources.Load<TextAsset>("Levels/level_01").text);
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
                    break;
                case "s":
                    type = TileType.stone;
                    break;
                case "v1":
                    type = TileType.vase;
                    break;

            }
            SpawnTile(counter % curLevel.grid_width, counter / curLevel.grid_width, type);
            counter++;
        }
    }

    void SpawnTile(int x, int y, TileType type)
    {
        GameObject block = PoolingManager.instance.GetFromPool(Tile.poolingTag, new Vector2(x, height + 1), Quaternion.identity);
        block.transform.parent = transform;
        grid[x, y] = block.GetComponent<Tile>();

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

        block.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
    }

    public void CheckMatches(int x, int y)
    {
        if (grid[x, y] == null) return;
        List<Vector2Int> matchedBlocks = new();
        List<Vector2Int> obstacles = new();

        FindMatches(x, y, grid[x, y].type, matchedBlocks, obstacles);

        if (matchedBlocks.Count < 2) return;

        List<int> effectedColumns = new();
        //TODO: Lower move count

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
            DestroyTile(pos);
            if (!effectedColumns.Contains(pos.x))
            {
                effectedColumns.Add(pos.x);
            }
        }

        foreach (var pos in obstacles)
        {
            if (grid[pos.x, pos.y].Damage())
            {
                DestroyTile(pos);
                if (!effectedColumns.Contains(pos.x))
                {
                    effectedColumns.Add(pos.x);
                }
            }
        }

        DropBlocks(effectedColumns);
    }

    private void DestroyTile(Vector2Int pos)
    {
        PoolingManager.instance.ReturnToPool(Tile.poolingTag, grid[pos.x, pos.y].gameObject);
        grid[pos.x, pos.y] = null;
    }

    private void FindMatches(int x, int y, TileType type, List<Vector2Int> matchedBlocks, List<Vector2Int> obstacles)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
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

    public void Rocket(int x, int y, TileType type)
    {
        if (type == TileType.rocketV || type == TileType.rocketH)
        {
            StartCoroutine(LaunchRocket(x, y, type));
        }
        else
        {
            Debug.LogError("GridManager: Invalid rocket type");
        }
    }

    private IEnumerator LaunchRocket(int x, int y, TileType type)
    {
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

        GameObject rocket1 = Instantiate(rocketPrefab, grid[x, y].transform.position, Quaternion.identity);
        GameObject rocket2 = Instantiate(rocketPrefab, grid[x, y].transform.position, Quaternion.identity);

        //Destroy(grid[x, y].gameObject);
        DestroyTile(new Vector2Int(x, y));

        rocket1.transform.parent = transform;
        rocket2.transform.parent = transform;

        rocket1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetTileSprite(type, 1);
        rocket2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetTileSprite(type, 2);

        Vector2 target1 = new Vector2(x, y) + dir1 * height;
        Vector2 target2 = new Vector2(x, y) + dir2 * height;

        Sequence seq1 = DOTween.Sequence();
        Sequence seq2 = DOTween.Sequence();

        seq1.Append(rocket1.transform.DOMove(target1, rocketTime).SetEase(Ease.Linear));
        seq2.Append(rocket2.transform.DOMove(target2, rocketTime).SetEase(Ease.Linear));

        seq1.OnUpdate(() =>
        {
            RocketOnUpdate(rocket1);
        });

        seq2.OnUpdate(() =>
        {
            RocketOnUpdate(rocket2);
        });

        yield return seq1.WaitForCompletion();
        if (seq2.active)
            yield return seq2.WaitForCompletion();

        Destroy(rocket1);
        Destroy(rocket2);

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

        DropBlocks(effectedColumns);
    }

    private void RocketOnUpdate(GameObject rocketInstance)
    {
        Vector2 pos = rocketInstance.transform.position;
        int gridX = Mathf.RoundToInt(pos.x);
        int gridY = Mathf.RoundToInt(pos.y);
        if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height && grid[gridX, gridY] != null)
        {
            if (grid[gridX, gridY].type == TileType.rocketV || grid[gridX, gridY].type == TileType.rocketH)
            {
                Rocket(gridX, gridY, grid[gridX, gridY].type);
            }
            else
            {
                DestroyTile(new Vector2Int(gridX, gridY));
            }
        }
    }

    private void DropBlocks(List<int> effectedColumns)
    {
        foreach (int x in effectedColumns)
        {
            int emptyCount = 0;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    emptyCount++;
                }
            }

            for (int i = 0; i < emptyCount; i++)
            {
                //GameObject block = PoolingManager.instance.GetFromPool(Tile.poolingTag, new Vector2(x, height + i + 1), Quaternion.identity);
                //block.transform.parent = transform;
                //grid[x, height + i] = block.GetComponent<Tile>();
                //int randomType = Random.Range(0, 4); //TODO: Bu değerler enumdan alınabilir.
                //grid[x, height + i].SetType((TileType)randomType);
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
                            grid[x, y].transform.DOMove(new Vector2(x, y), 0.3f);
                            break;
                        }
                    }
                }
            }
        }
    }
}
