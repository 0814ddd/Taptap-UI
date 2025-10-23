using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class SpriteCrossfadeTogglePro : MonoBehaviour, IPointerClickHandler
{
    [Header("�����л�������ͼ")]
    public Sprite spriteA;
    public Sprite spriteB;

    [Header("��ʼ��ʾ")]
    public bool startWithA = true;

    [Header("��������")]
    [Range(0.05f, 2f)] public float duration = 0.45f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool preserveAspect = true;          // ��������
    public bool blockInputWhileFading = true;   // ���������ε��
    public bool useUnscaledTime = true;         // UI ���ò��� TimeScale

    private Image _baseImg;         // ��ͼ��ʼ����ʾ����ǰͼ��
    private Image _overlayImg;      // ǰ�������ڵ��롰��һ�š�
    private CanvasGroup _overlayCg; // ���� overlay ͸����
    private bool _showingA;
    private bool _isFading;
    private Coroutine _co;

    void Awake()
    {
        _baseImg = GetComponent<Image>();
        _baseImg.preserveAspect = preserveAspect;

        // ���� Ԥ��ǰ���㣨������ʱ����GC����������
        var overlayGO = new GameObject("CrossfadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        overlayGO.transform.SetParent(transform, false);

        var rt = (RectTransform)overlayGO.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        _overlayImg = overlayGO.GetComponent<Image>();
        _overlayImg.preserveAspect = preserveAspect;
        _overlayImg.raycastTarget = blockInputWhileFading; // ��Ҫ���ξ��������¼�

        _overlayCg = overlayGO.GetComponent<CanvasGroup>();
        _overlayCg.alpha = 0f; // ��ʼ͸��

        // ���� ��ʼ����ǰͼ ���� 
        if (_baseImg.sprite == null)
        {
            _baseImg.sprite = startWithA ? spriteA : spriteB;
            _showingA = startWithA;
        }
        else
        {
            if (_baseImg.sprite == spriteA) _showingA = true;
            else if (_baseImg.sprite == spriteB) _showingA = false;
            else
            {
                _showingA = startWithA;
                _baseImg.sprite = _showingA ? spriteA : spriteB;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isFading) return;

        Sprite next = _showingA ? spriteB : spriteA;
        if (next == null)
        {
            Debug.LogWarning("[SpriteCrossfadeTogglePro] Ŀ�� Sprite Ϊ�գ������� spriteA/spriteB��");
            return;
        }

        // Ԥ��Ҫ���������ͼ
        _overlayImg.sprite = next;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FadeRoutine(next));
    }

    IEnumerator FadeRoutine(Sprite nextSprite)
    {
        _isFading = true;

        // ���ɿ�ʼ���� overlay ����������ѡ������ͼ���ֲ�������������΢�ġ���λ���Ӿ�
        _overlayImg.raycastTarget = blockInputWhileFading;
        float t = 0f;
        float baseA0 = _baseImg.color.a;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float a = curve.Evaluate(k); // 0��1 ��S����

            // ǰ���� 0 �� 1
            _overlayCg.alpha = a;

            // ��ͼ����΢���� 2% ͸�����е� 30%�������޸У��������˳����
            float baseA = Mathf.Lerp(baseA0, baseA0 * 0.98f, a * 0.3f);
            var bc = _baseImg.color; bc.a = baseA; _baseImg.color = bc;

            yield return null;
        }

        // ��β����ͼ������ͼ����ɫ��ԭ��overlay ��λ��͸�����ã������٣�
        _baseImg.sprite = nextSprite;
        var back = _baseImg.color; back.a = baseA0; _baseImg.color = back;

        _overlayCg.alpha = 0f;             // ��λ͸��
        _overlayImg.raycastTarget = false; // �������

        _showingA = !_showingA;
        _isFading = false;
        _co = null;
    }
}
