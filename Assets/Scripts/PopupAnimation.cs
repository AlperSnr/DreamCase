using UnityEngine;
using DG.Tweening;

public class PopupAnimation : MonoBehaviour
{
    [SerializeField] private Transform star;


    private void Start()
    {
        star.DOScale(Vector3.one * 1.5f, 0.5f).SetLoops(3, LoopType.Yoyo);
        star.DORotate(new Vector3(0, 0, -360), 1.5f,RotateMode.LocalAxisAdd);
    }
}
