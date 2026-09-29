using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// レイヤー間のシーン自動切替を管理する。Scene_Level_Main に配置する想定。
/// スカイシャフトのExit通過で次シーンをロードし、Entry通過で前シーンをアンロードする。
/// </summary>
public class LayerSceneManager : MonoBehaviour
{
    public static LayerSceneManager Instance { get; private set; }

    [Header("上から下への層シーン名の並び（例: Level_05, Level_04, ..., Level_01）")]
    [SerializeField] private List<string> layerSceneNames;

    [Header("プレイヤー参照")]
    [SerializeField] private Transform player;

    [Header("演出（未接続でもエラーにならない。仮のフェードのみ実行）")]
    [SerializeField] private CanvasGroup whiteoutOverlay;
    [SerializeField] private float fadeDuration = 0.3f;

    private int _currentLayerIndex;
    private string _currentSceneName;
    private string _previousSceneName;
    private SkyShaftTrigger _pendingEntryPoint;
    private Transform _pendingStageRoot;
    private Vector3 _exitAnchorPosition; // Exit通過の瞬間のプレイヤー位置。次ステージの接続基準にする
    private bool _isTransitioning;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 仮の起動処理。本来はGameFlowManagerのレベル開始処理から呼ぶ想定だが、
        // まだ未実装のため、検証用にここから直接呼んでいる。
        StartCoroutine(LoadInitialLayer());
    }

    /// <summary>
    /// レベル開始時に最初の層をロードする。GameFlowManager側の開始処理から呼ぶ想定。
    /// </summary>
    public IEnumerator LoadInitialLayer()
    {
        _currentLayerIndex = 0;
        _currentSceneName = layerSceneNames[_currentLayerIndex];
        yield return SceneManager.LoadSceneAsync(_currentSceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// ExitロールのSkyShaftTriggerから呼ばれる。
    /// </summary>
    public void BeginTransition()
    {
        if (_isTransitioning) return;

        if (_currentLayerIndex + 1 >= layerSceneNames.Count)
        {
            Debug.Log("[LayerSceneManager] 最終層のため次のロードは行いません");
            return;
        }

        _isTransitioning = true;
        _pendingEntryPoint = null; // 前回の登録が残らないようにリセット
        _pendingStageRoot = null;
        _exitAnchorPosition = player.position; // この瞬間の位置を次ステージの接続先にする
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        string nextSceneName = layerSceneNames[_currentLayerIndex + 1];
        Debug.Log($"[LayerSceneManager] {nextSceneName} のプリロード開始");

        var loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        yield return Fade(1f); // ホワイトアウト（見えなくなるまでフェード）

        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }

        loadOp.allowSceneActivation = true;
        yield return null; // 活性化・Awake実行を1フレーム待つ

        AlignStageToPlayer(nextSceneName);

        yield return Fade(0f); // ホワイトイン

        _previousSceneName = _currentSceneName;
        _currentSceneName = nextSceneName;
        _currentLayerIndex++;
        _isTransitioning = false;
    }

    /// <summary>
    /// 新ステージのEntryPointが、Exit通過時に記録したプレイヤー位置にぴったり重なるよう
    /// StageRootごとステージ全体をオフセットする。プレイヤー自身は動かさない。
    /// </summary>
    private void AlignStageToPlayer(string sceneName)
    {
        if (_pendingStageRoot == null || _pendingEntryPoint == null)
        {
            // マップ未完成でStageRoot/EntryPointがまだ置かれていない場合の仮対応。
            // 位置合わせをスキップするだけで、ロード/アンロード自体の検証は続行できる。
            Debug.LogWarning($"[LayerSceneManager] {sceneName} にStageRootまたはEntryPointが見つかりません。位置合わせをスキップします");
            return;
        }

        Vector3 rawEntryWorldPos = _pendingEntryPoint.transform.position; // 未移動時点のワールド座標
        Vector3 offset = _exitAnchorPosition - rawEntryWorldPos;
        _pendingStageRoot.position += offset;

        Debug.Log($"[LayerSceneManager] {sceneName} を {offset} だけオフセットして接続しました");
    }

    /// <summary>
    /// EntryロールのSkyShaftTriggerが、自分のシーンのAwakeで登録する。
    /// </summary>
    public void RegisterEntryPoint(SkyShaftTrigger entryPoint)
    {
        _pendingEntryPoint = entryPoint;
    }

    /// <summary>
    /// StageRootが、自分のシーンのAwakeで登録する。
    /// </summary>
    public void RegisterStageRoot(Transform stageRoot)
    {
        _pendingStageRoot = stageRoot;
    }

    /// <summary>
    /// EntryロールのSkyShaftTriggerから、プレイヤーが通過した際に呼ばれる。
    /// </summary>
    public void CompleteTransition()
    {
        if (string.IsNullOrEmpty(_previousSceneName)) return;

        string target = _previousSceneName;
        _previousSceneName = null;
        StartCoroutine(UnloadPrevious(target));
    }

    private IEnumerator UnloadPrevious(string sceneName)
    {
        Debug.Log($"[LayerSceneManager] {sceneName} のアンロード開始");
        yield return SceneManager.UnloadSceneAsync(sceneName);
        Debug.Log($"[LayerSceneManager] {sceneName} のアンロード完了");
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (whiteoutOverlay == null) yield break; // 演出未実装でも安全に動く仮対応

        float start = whiteoutOverlay.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            whiteoutOverlay.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            yield return null;
        }
        whiteoutOverlay.alpha = targetAlpha;
    }
}
