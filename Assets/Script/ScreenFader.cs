using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Range(0f, 1f)] public float initialAlpha = 0f; // ����һ������ʱ�Ƿ����
    [Tooltip("��Ļ�����ϲ������˳��")]
    public int sortingOrder = 32760;

    Canvas _canvas;
    Image _black;
    Coroutine _running;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ׼��һ��������Canvas + ȫ����Ļ
        _canvas = new GameObject("ScreenFaderCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;
        DontDestroyOnLoad(_canvas.gameObject);

        var go = new GameObject("Black", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(_canvas.transform, false);

        _black = go.GetComponent<Image>();
        _black.color = new Color(0, 0, 0, Mathf.Clamp01(initialAlpha));
        _black.raycastTarget = false; // ��Ҫ������Ļ����л�ʱ��ʱ��

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void SetSortingOrder(int order) => _canvas.sortingOrder = order;

    public Coroutine Fade(float from, float to, float duration, bool blockRaycast = true)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeCo(from, to, duration, blockRaycast));
        return _running;
    }

    IEnumerator FadeCo(float from, float to, float duration, bool blockRaycast)
    {
        _black.raycastTarget = blockRaycast;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration < 1e-5f ? 1f : Mathf.Clamp01(t / duration);
            float a = Mathf.Lerp(from, to, k);
            var c = _black.color;
            c.a = a;
            _black.color = c;
            yield return null;
        }
        var final = _black.color; final.a = to; _black.color = final;
        _black.raycastTarget = blockRaycast && to > 0.001f; // ֻ�к���ʱ�������
        _running = null;
    }

    /// <summary>
    /// ���ڡ����س���������
    /// </summary>
    public void FadeToScene(string sceneName, float flashOut = 0.18f, float fadeIn = 0.35f)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ScreenFader] sceneName Ϊ��");
            return;
        }
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeToSceneCo(sceneName, flashOut, fadeIn));
    }

    IEnumerator FadeToSceneCo(string sceneName, float flashOut, float fadeIn)
    {
        float from = _black.color.a;
        // �������ڵ� 1
        yield return FadeCo(from, 1f, Mathf.Max(0f, flashOut), blockRaycast: true);

        // �첽���أ������У�
        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op != null) while (!op.isDone) yield return null;

        // �������л����Ե�һ֡ȷ�������ȶ��ٵ���
        yield return null;

        yield return FadeCo(1f, 0f, Mathf.Max(0f, fadeIn), blockRaycast: false);
    }
}
