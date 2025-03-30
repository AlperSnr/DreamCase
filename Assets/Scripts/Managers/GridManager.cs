using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public int width = 8;
    public int height = 8;
    public GameObject[] blockPrefabs;
    public GameObject rocketPrefab; // Roket prefabı
    private Tile[,] grid;
    private float rocketTime = 0.4f;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        grid = new Tile[width, height * 2];
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnBlock(x, y);
            }
        }
    }

    void SpawnBlock(int x, int y)
    {
        if (blockPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        GameObject block = Instantiate(blockPrefabs[randomIndex], new Vector2(x, height + 1), Quaternion.identity);
        block.transform.parent = transform;
        grid[x, y] = block.GetComponent<Tile>();
        int randomType = Random.Range(0, 4); //TODO: Bu değerler enumdan alınabilir.
        grid[x, y].SetType((TileType)randomType);
        block.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
    }

    public void CheckMatches(int x, int y)
    {
        if (grid[x, y] == null) return;
        List<Vector2Int> matchedBlocks = new();

        FindMatches(x, y, grid[x, y].type, matchedBlocks);
        List<int> effectedColumns = new();

        if (matchedBlocks.Count < 2) return;
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
            Destroy(grid[pos.x, pos.y].gameObject);
            grid[pos.x, pos.y] = null;
            if (!effectedColumns.Contains(pos.x))
            {
                effectedColumns.Add(pos.x);
            }
        }

        DropBlocks(effectedColumns);
    }

    private void FindMatches(int x, int y, TileType type, List<Vector2Int> matchedBlocks)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (grid[x, y] == null) return;
        if (grid[x, y].type != type) return;
        if (matchedBlocks.Contains(new Vector2Int(x, y))) return;
        matchedBlocks.Add(new Vector2Int(x, y));
        FindMatches(x + 1, y, type, matchedBlocks);
        FindMatches(x - 1, y, type, matchedBlocks);
        FindMatches(x, y + 1, type, matchedBlocks);
        FindMatches(x, y - 1, type, matchedBlocks);
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

        rocket1.transform.parent = transform;
        rocket2.transform.parent = transform;

        rocket1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetRocketSprite(type, 0);
        rocket2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetRocketSprite(type, 1);

        Vector2 target1 = new Vector2(x, y) + dir1 * height;
        Vector2 target2 = new Vector2(x, y) + dir2 * height;

        Sequence seq1 = DOTween.Sequence();
        Sequence seq2 = DOTween.Sequence();

        seq1.Append(rocket1.transform.DOMove(target1, rocketTime).SetEase(Ease.Linear));
        seq2.Append(rocket2.transform.DOMove(target2, rocketTime).SetEase(Ease.Linear));

        seq1.OnUpdate(() =>
        {
            Vector2 pos = rocket1.transform.position;
            int gridX = Mathf.RoundToInt(pos.x);
            int gridY = Mathf.RoundToInt(pos.y);
            if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height && grid[gridX, gridY] != null)
            {
                Destroy(grid[gridX, gridY].gameObject);
                grid[gridX, gridY] = null;
            }
        });

        seq2.OnUpdate(() =>
        {
            Vector2 pos = rocket2.transform.position;
            int gridX = Mathf.RoundToInt(pos.x);
            int gridY = Mathf.RoundToInt(pos.y);
            if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height && grid[gridX, gridY] != null)
            {
                Destroy(grid[gridX, gridY].gameObject);
                grid[gridX, gridY] = null;
            }
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
                int randomIndex = Random.Range(0, blockPrefabs.Length);
                GameObject block = Instantiate(blockPrefabs[randomIndex], new Vector2(x, height + i + 1), Quaternion.identity);
                block.transform.parent = transform;
                grid[x, height + i] = block.GetComponent<Tile>();
                int randomType = Random.Range(0, 4); //TODO: Bu değerler enumdan alınabilir.
                grid[x, height + i].SetType((TileType)randomType);
            }

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int k = y + 1; k < height + emptyCount; k++)
                    {
                        if (grid[x, k] != null)
                        {
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
