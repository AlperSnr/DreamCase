using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public TileType type;

    public void SetType(TileType newType)
    {
        type = newType;
        GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetTileSprite(type);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (type)
        {
            case TileType.rocketV:
            case TileType.rocketH:
                GridManager.instance.Rocket((int)transform.position.x, (int)transform.position.y, type);
                break;
            default:
                GridManager.instance.CheckMatches((int)transform.position.x, (int)transform.position.y);
                break;
        }
    }
}

