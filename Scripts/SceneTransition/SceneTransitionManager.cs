using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI 引用")]
    public CanvasGroup fadePanel;      // 挂载 FadePanel 的 CanvasGroup
    public float fadeDuration = 0.5f;  // 淡入淡出时间

    private bool isTransitioning = false;

    void Awake()
    {
        // 单例 + 跨场景不销毁
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始确保是透明的
        if (fadePanel != null)
            fadePanel.alpha = 0f;
    }

    /// <summary>
    /// 外部调用：带转场切换场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // 1. 黑屏淡入（遮住画面）
        yield return StartCoroutine(Fade(1f));

        // 2. 异步加载场景（避免卡顿）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 真正切换
        asyncLoad.allowSceneActivation = true;
        yield return new WaitForSeconds(0.1f); // 等一帧确保场景初始化

        // 3. 黑屏淡出（显示新场景）
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanel.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // 用 unscaled 避免 Time.timeScale=0 时卡住
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }
}