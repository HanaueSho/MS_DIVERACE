using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneManager.LoadSceneAsync(Additive) の検証用スクリプト。
/// 空のGameObjectを作って Scene_Level_Main に配置し、このスクリプトをアタッチする。
///
/// 操作方法（Playモード中）：
///   L     : sceneToLoad を追加ロード開始（90%まで進めて待機）
///   Space : ロード待機中のシーンを活性化（allowSceneActivation = true）
///   U     : sceneToUnload をアンロード
/// </summary>
public class AdditiveSceneLoadTester : MonoBehaviour
{
    [Header("ロード対象（Build Settingsに登録済みのシーン名）")]
    [SerializeField] private string sceneToLoad = "Scene_Level_Test1";

    [Header("アンロード対象")]
    [SerializeField] private string sceneToUnload = "Scene_Level_Test1";

    private AsyncOperation _pendingLoad;
    private string _pendingSceneName;

    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            StartCoroutine(LoadAdditive(sceneToLoad));
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && _pendingLoad != null)
        {
            Debug.Log($"[活性化] {_pendingSceneName}");
            _pendingLoad.allowSceneActivation = true;
        }

        if (Keyboard.current.uKey.wasPressedThisFrame && !string.IsNullOrEmpty(sceneToUnload))
        {
            StartCoroutine(UnloadScene(sceneToUnload));
        }
    }

    private IEnumerator LoadAdditive(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"[Load中止] {sceneName} は既にロード済みです");
            yield break;
        }

        Debug.Log($"[Load開始] {sceneName}");
        _pendingSceneName = sceneName;
        _pendingLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _pendingLoad.allowSceneActivation = false;

        // 90%到達＝データ読み込み自体は完了、活性化待ちの状態
        while (_pendingLoad.progress < 0.9f)
        {
            Debug.Log($"[Loading] {sceneName} : {_pendingLoad.progress * 100f:F0}%");
            yield return null;
        }

        Debug.Log($"[Load準備完了] {sceneName} : SPACEキーで活性化してください");
        // ※ 中身の少ないテストシーンだと1フレームで90%に到達することが多い。
        // 実際の進捗バー確認は、オブジェクト数の多い本番シーンで改めて検証する。
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogWarning($"[Unload中止] {sceneName} はロードされていません");
            yield break;
        }

        Debug.Log($"[Unload開始] {sceneName}");
        var op = SceneManager.UnloadSceneAsync(scene);
        yield return op;
        Debug.Log($"[Unload完了] {sceneName} : Hierarchyから消えているか確認");

        if (sceneName == _pendingSceneName)
        {
            _pendingLoad = null;
            _pendingSceneName = null;
        }
    }
}
