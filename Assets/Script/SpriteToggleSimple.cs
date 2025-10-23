using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class SpriteCrossfadeTogglePro : MonoBehaviour, IPointerClickHandler
{
    [Header("互相切换的两张图")]
    public Sprite spriteA;
    public Sprite spriteB;

    [Header("初始显示")]
    public bool startWithA = true;

    [Header("过渡设置")]
    [Range(0.05f, 2f)] public float duration = 0.45f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool preserveAspect = true;          // 比例不变
    public bool blockInputWhileFading = true;   // 过渡中屏蔽点击
    public bool useUnscaledTime = true;         // UI 常用不受 TimeScale

    private Image _baseImg;         // 底图：始终显示“当前图”
    private Image _overlayImg;      // 前景：用于淡入“下一张”
    private CanvasGroup _overlayCg; // 控制 overlay 透明度
    private bool _showingA;
    private bool _isFading;
    private Coroutine _co;

    void Awake()
    {
        _baseImg = GetComponent<Image>();
        _baseImg.preserveAspect = preserveAspect;

        // —— 预建前景层（避免点击时创建GC抖动）——
        var overlayGO = new GameObject("CrossfadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        overlayGO.transform.SetParent(transform, false);

        var rt = (RectTransform)overlayGO.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        _overlayImg = overlayGO.GetComponent<Image>();
        _overlayImg.preserveAspect = preserveAspect;
        _overlayImg.raycastTarget = blockInputWhileFading; // 需要屏蔽就让它吃事件

        _overlayCg = overlayGO.GetComponent<CanvasGroup>();
        _overlayCg.alpha = 0f; // 初始透明

        // —— 初始化当前图 —— 
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
            Debug.LogWarning("[SpriteCrossfadeTogglePro] 目标 Sprite 为空，请设置 spriteA/spriteB。");
            return;
        }

        // 预设要淡入的那张图
        _overlayImg.sprite = next;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FadeRoutine(next));
    }

    IEnumerator FadeRoutine(Sprite nextSprite)
    {
        _isFading = true;

        // 过渡开始：让 overlay 捕获点击（可选），底图保持不动，仅做很轻微的“让位”视觉
        _overlayImg.raycastTarget = blockInputWhileFading;
        float t = 0f;
        float baseA0 = _baseImg.color.a;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float a = curve.Evaluate(k); // 0→1 的S曲线

            // 前景从 0 → 1
            _overlayCg.alpha = a;

            // 底图仅轻微降低 2% 透明度中的 30%（几乎无感，但会更“顺”）
            float baseA = Mathf.Lerp(baseA0, baseA0 * 0.98f, a * 0.3f);
            var bc = _baseImg.color; bc.a = baseA; _baseImg.color = bc;

            yield return null;
        }

        // 收尾：底图换成新图，颜色还原；overlay 复位到透明备用（不销毁）
        _baseImg.sprite = nextSprite;
        var back = _baseImg.color; back.a = baseA0; _baseImg.color = back;

        _overlayCg.alpha = 0f;             // 复位透明
        _overlayImg.raycastTarget = false; // 解锁点击

        _showingA = !_showingA;
        _isFading = false;
        _co = null;
    }
}
