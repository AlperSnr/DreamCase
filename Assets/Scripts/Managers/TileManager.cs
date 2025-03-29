using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private List<Sprite> tileSprites;

    public static TileManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Sprite GetTileSprite(TileType type)
    {
        int index = (int)type;
        if (index >= tileSprites.Count)
        {
            Debug.LogError("TileManager: Tile sprite not found for type: " + type);
            return null;
        }

        return tileSprites[(int)type];
    }
}
