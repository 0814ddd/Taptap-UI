using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SpriteToggleSimple : MonoBehaviour, IPointerClickHandler
{
    [Header("����Ҫ�����л���ͼƬ")]
    public Sprite spriteA;
    public Sprite spriteB;

    [Header("��ʼ��ʾ��һ��")]
    public bool startWithA = true;

    [Header("��ʾ����")]
    public bool preserveAspect = true;   // �������䣨����������Ӧ��

    private Image _img;
    private bool _showingA;

    void Awake()
    {
        _img = GetComponent<Image>();
        _img.preserveAspect = preserveAspect;

        // ��ʼ����ǰ��ʾ
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
        // ���ֱ������ò���
        _img.preserveAspect = preserveAspect;
    }
}
