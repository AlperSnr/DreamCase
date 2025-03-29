using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween kütüphanesi

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public int width = 8;
    public int height = 8;
    public GameObject[] blockPrefabs; // Farklı blok türlerini buraya ekleyebilirsin.
    private Tile[,] grid; // Blokları saklamak için 2D dizi

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

        if (matchedBlocks.Count >= 2)
        {
            foreach (var pos in matchedBlocks)
            {
                Destroy(grid[pos.x, pos.y].gameObject);
                grid[pos.x, pos.y] = null;
                if (!effectedColumns.Contains(pos.x))
                {
                    effectedColumns.Add(pos.x);
                }
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
