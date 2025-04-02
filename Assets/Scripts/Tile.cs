using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public static string poolingTag = "pool.tile";
    public static string particleTag = "pool.tileParticle";
    public TileType type;

    static bool particlePoolCreated = false;

    private SpriteRenderer spriteRenderer;
    public bool isObstacle = false;
    private int health = 0;
    private bool isHint = false;
    private TileManager tileManager => TileManager.instance;

    private void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(TileType newType)
    {
        type = newType;
        spriteRenderer.sprite = tileManager.GetTileSprite(type, 0);

        isObstacle = type == TileType.box || type == TileType.stone || type == TileType.vase;

        if (isObstacle)
        {
            health = TileManager.instance.GetHealth(type);
        }
    }

    public void SetRocketHint(bool isHint)
    {
        if (this.isHint == isHint) return;

        this.isHint = isHint;
        if (isHint)
        {
            spriteRenderer.sprite = tileManager.GetTileSprite(type, 1);
        }
        else
        {
            spriteRenderer.sprite = tileManager.GetTileSprite(type, 0);
        }
    }

    public bool Damage()
    {
        if (!isObstacle) return false;

        health--;

        if (type == TileType.vase)
        {
            spriteRenderer.sprite = tileManager.GetTileSprite(type, 1);
        }

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

    public void PlayParticle()
    {
        GameObject particle = PoolingManager.instance.GetFromPool(particleTag);
        particle.GetComponent<ParticleSystemRenderer>().material = tileManager.GetParticleMaterial(type);
        particle.transform.position = transform.position;
        DOVirtual.DelayedCall(2f, () =>
        {
            PoolingManager.instance.ReturnToPool(particleTag, particle);
        });
    }

    private void OnDisable()
    {
        isHint = false;
    }


}

