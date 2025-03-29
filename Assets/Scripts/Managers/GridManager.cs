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
    private Tile[,] grid;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        grid = new Tile[width, height];
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
        //TODO: Başka bir rocket varsa farklı sonuç ekle
        if (type == TileType.rocketV)
        {
            for (int i = 0; i < height; i++)
            {
                Destroy(grid[x, i].gameObject);
                grid[x, i] = null;
            }
            DropBlocks(new List<int>() { x });
        }
        else if (type == TileType.rocketH)
        {
            List<int> effectedColumns = new();
            for (int i = 0; i < width; i++)
            {
                Destroy(grid[i, y].gameObject);
                grid[i, y] = null;
                effectedColumns.Add(i);
            }

            DropBlocks(effectedColumns);
        }

    }

    private void DropBlocks(List<int> effectedColumns)
    {
        foreach (int x in effectedColumns)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (grid[x, k] != null)
                        {
                            grid[x, y] = grid[x, k];
                            grid[x, k] = null;
                            grid[x, y].transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
                            break;
                        }
                    }
                }
            }

            // Yeni bloklar oluştur
            for (int y = height - 1; y >= 0; y--)
            {
                if (grid[x, y] == null)
                {
                    SpawnBlock(x, y);
                }
            }
        }
    }
}
