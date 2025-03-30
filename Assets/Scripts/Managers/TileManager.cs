using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private List<Sprite> tileSprites;
    [Header("Vertical Rocket Sprites")]
    [SerializeField] private Sprite[] rocketV = new Sprite[2];
    [Header("Horizontal Rocket Sprites")]
    [SerializeField] private Sprite[] rocketH = new Sprite[2];

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

    public Sprite GetRocketSprite(TileType type, int index)
    {
        if (index >= 2) return null;

        if (type == TileType.rocketV)
        {
            return rocketV[index];
        }
        else if (type == TileType.rocketH)
        {
            return rocketH[index];
        }
        else
        {
            Debug.LogError("TileManager: Rocket sprite not found for type: " + type);
            return null;
        }
    }
}
