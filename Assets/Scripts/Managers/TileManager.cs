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
        return tileSprites[(int)type];
    }
}
