using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SpriteToggleSimple : MonoBehaviour, IPointerClickHandler
{
    [Header("两张要互相切换的图片")]
    public Sprite spriteA;
    public Sprite spriteB;

    [Header("初始显示哪一张")]
    public bool startWithA = true;

    [Header("显示设置")]
    public bool preserveAspect = true;   // 比例不变（按容器自适应）

    private Image _img;
    private bool _showingA;

    void Awake()
    {
        _img = GetComponent<Image>();
        _img.preserveAspect = preserveAspect;

        // 初始化当前显示
        if (_img.sprite == null)
        {
            _img.sprite = startWithA ? spriteA : spriteB;
            _showingA = startWithA;
        }
        else
        {
            if (_img.sprite == spriteA) _showingA = true;
            else if (_img.sprite == spriteB) _showingA = false;
            else
            {
                _showingA = startWithA;
                _img.sprite = _showingA ? spriteA : spriteB;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (spriteA == null || spriteB == null) return;

        _showingA = !_showingA;
        _img.sprite = _showingA ? spriteA : spriteB;
        // 保持比例设置不变
        _img.preserveAspect = preserveAspect;
    }
}
