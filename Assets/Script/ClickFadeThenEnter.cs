using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ClickFadeThenEnter : MonoBehaviour, IPointerClickHandler
{
    [Header("叠加出现的图片")]
    public Sprite overlaySprite;

    [Header("叠图淡入设置")]
    [Range(0.05f, 3f)] public float overlayFadeDuration = 0.6f;
    public AnimationCurve overlayFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("场景跳转（Build Settings 中的名称）")]
    public string sceneToLoad = "1";

    [Header("闪黑参数")]
    [Tooltip("点击进入游戏时先快速变黑的时长")]
    public float flashOut = 0.18f;
    [Tooltip("加载后从黑幕淡入的时长")]
    public float fadeIn = 0.35f;

    [Header("其他")]
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
            Debug.LogError("[ClickFadeThenEnter] sceneToLoad 为空，请在 Inspector 填正确的场景名。");
            return;
        }

        // 调 ScreenFader 的“闪黑切场景”
        if (ScreenFader.Instance == null)
        {
            // 若你没在场景里放 ScreenFader，这里自动创建一个
            var go = new GameObject("~ScreenFaderAuto~");
            go.AddComponent<ScreenFader>();
        }
        ScreenFader.Instance.FadeToScene(sceneToLoad, flashOut, fadeIn);
    }
}
