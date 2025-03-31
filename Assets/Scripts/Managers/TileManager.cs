using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private List<TileData> tileDataList;

    public static TileManager instance;

    private Dictionary<TileType, TileData> tileDataDictionary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        tileDataDictionary = new Dictionary<TileType, TileData>();

        foreach (var tileData in tileDataList)
        {
            if (!tileDataDictionary.ContainsKey(tileData.type))
            {
                tileDataDictionary[tileData.type] = tileData;
            }
        }
    }

    public Sprite GetTileSprite(TileType type,int index)
    {
        if (tileDataDictionary.TryGetValue(type, out TileData tileData))
        {
            if(index < tileData.sprite.Length)
            {
                return tileData.sprite[index];
            }
            else
            {
                Debug.LogError("TileManager: Tile sprite index out of range for type: " + type);
                return null;
            }
        }
        else
        {
            Debug.LogError("TileManager: Tile sprite not found for type: " + type);
            return null;
        }
    }

    public int GetHealth(TileType type)
    {
        if (tileDataDictionary.TryGetValue(type, out TileData tileData))
        {
            return tileData.health;
        }
        else
        {
            return 0;
        }
    }
}
