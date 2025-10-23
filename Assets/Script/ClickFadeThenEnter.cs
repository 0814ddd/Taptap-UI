using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ClickFadeThenEnter : MonoBehaviour, IPointerClickHandler
{
    [Header("���ӳ��ֵ�ͼƬ")]
    public Sprite overlaySprite;

    [Header("��ͼ��������")]
    [Range(0.05f, 3f)] public float overlayFadeDuration = 0.6f;
    public AnimationCurve overlayFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("������ת��Build Settings �е����ƣ�")]
    public string sceneToLoad = "1";

    [Header("���ڲ���")]
    [Tooltip("���������Ϸʱ�ȿ��ٱ�ڵ�ʱ��")]
    public float flashOut = 0.18f;
    [Tooltip("���غ�Ӻ�Ļ�����ʱ��")]
    public float fadeIn = 0.35f;

    [Header("����")]
    public bool matchParent = true;
    public bool preserveAspect = true;

    private enum State { Idle, FadingOverlay, ReadyToEnter }
    private State _state = State.Idle;

    private Image _baseImage;
    private Image _overlay;
    private Coroutine _overlayCo;

    void Awake()
    {
        _baseImage = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (_state)
        {
            case State.Idle:
                StartOverlayFadeIn();
                break;
            case State.ReadyToEnter:
                EnterGame();
                break;
            case State.FadingOverlay:
                break;
        }
    }

    void StartOverlayFadeIn()
    {
        if (_overlay == null)
        {
            var go = new GameObject("OverlayImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);

            _overlay = go.GetComponent<Image>();
            _overlay.sprite = overlaySprite;
            _overlay.preserveAspect = preserveAspect;
            _overlay.raycastTarget = false;
            _overlay.color = new Color(1, 1, 1, 0);

            var rt = (RectTransform)_overlay.transform;
            if (matchParent)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchoredPosition = Vector2.zero;
            }
        }

        if (_overlayCo != null) StopCoroutine(_overlayCo);
        _overlayCo = StartCoroutine(OverlayFadeInCo());
    }

    IEnumerator OverlayFadeInCo()
    {
        _state = State.FadingOverlay;

        float t = 0f;
        while (t < overlayFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / overlayFadeDuration);
            float a = overlayFadeCurve.Evaluate(k);
            var c = _overlay.color; c.a = a; _overlay.color = c;
            yield return null;
        }
        var final = _overlay.color; final.a = 1f; _overlay.color = final;

        _state = State.ReadyToEnter;
        _overlayCo = null;
    }

    void EnterGame()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[ClickFadeThenEnter] sceneToLoad Ϊ�գ����� Inspector ����ȷ�ĳ�������");
            return;
        }

        // �� ScreenFader �ġ������г�����
        if (ScreenFader.Instance == null)
        {
            // ����û�ڳ������ ScreenFader�������Զ�����һ��
            var go = new GameObject("~ScreenFaderAuto~");
            go.AddComponent<ScreenFader>();
        }
        ScreenFader.Instance.FadeToScene(sceneToLoad, flashOut, fadeIn);
    }
}
