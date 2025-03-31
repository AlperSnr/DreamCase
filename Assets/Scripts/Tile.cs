using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public static string poolingTag = "pool.tile";
    public TileType type;

    public bool isObstacle = false;
    private int health = 0;

    public void SetType(TileType newType)
    {
        type = newType;
        GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetTileSprite(type, 0);

        isObstacle = type == TileType.box || type == TileType.stone || type == TileType.vase;

        if (isObstacle)
        {
            health = TileManager.instance.GetHealth(type);
        }
    }

    public bool Damage()
    {
        if (!isObstacle) return false;

        health--;

        if (health <= 0)
        {
            return true;
        }
        return false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isObstacle) return;

        switch (type)
        {
            case TileType.rocketV:
            case TileType.rocketH:
                GridManager.instance.Rocket((int)transform.localPosition.x, (int)transform.localPosition.y, type);
                break;
            default:
                GridManager.instance.CheckMatches((int)transform.localPosition.x, (int)transform.localPosition.y);
                break;
        }
    }


}

