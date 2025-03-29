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
        GridManager.instance.CheckMatches((int)transform.position.x, (int)transform.position.y);
    }
}

