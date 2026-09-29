using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLevelStreamer : MonoBehaviour
{
    [System.Serializable]
    public class LevelScene
    {
        public string sceneName;
    }

    [SerializeField] private Transform player;

    [SerializeField]
    private LevelScene[] levelScenes =
    {
        new LevelScene { sceneName = "Scene_Level_05" },
        new LevelScene { sceneName = "Scene_Level_04" },
        new LevelScene { sceneName = "Scene_Level_03" },
        new LevelScene { sceneName = "Scene_Level_02" },
        new LevelScene { sceneName = "Scene_Level_01" }
    };

    [Range(0.0f, 1.0f)]
    [SerializeField] private float unloadPreviousProgress = 0.5f;

    [SerializeField] private bool enableDebugLog = true;

    private int currentLevelIndex;
    private bool previousUnloaded = true;
    private bool isStreaming;

    private IEnumerator Start()
    {
        if (player == null)
        {
            Debug.LogError("Player is not assigned.");
            enabled = false;
            yield break;
        }

        if (levelScenes == null || levelScenes.Length == 0)
        {
            Debug.LogError("Level scenes are not assigned.");
            enabled = false;
            yield break;
        }

        isStreaming = true;

        yield return LoadAndPlaceLevel(0, Vector3.zero);

        LevelSceneInfo firstLevel = GetLevelInfo(0);

        if (firstLevel == null || !firstLevel.IsValid())
        {
            Debug.LogError("First LevelSceneInfo is invalid.");
            enabled = false;
            yield break;
        }

        if (levelScenes.Length > 1)
            yield return LoadAndPlaceLevel(1, firstLevel.BottomPoint.position);

        isStreaming = false;
    }

    private void Update()
    {
        if (isStreaming || player == null)
            return;

        LevelSceneInfo currentLevel = GetLevelInfo(currentLevelIndex);

        if (currentLevel == null || !currentLevel.IsValid())
            return;

        if (currentLevelIndex < levelScenes.Length - 1 &&
            player.position.y <= currentLevel.BottomPoint.position.y)
        {
            StartCoroutine(MoveToNextLevel());
            return;
        }

        if (!previousUnloaded &&
            currentLevelIndex > 0 &&
            GetLevelProgress(currentLevel) >= unloadPreviousProgress)
        {
            StartCoroutine(UnloadPreviousLevel());
        }
    }

    private IEnumerator MoveToNextLevel()
    {
        if (isStreaming)
            yield break;

        isStreaming = true;
        currentLevelIndex++;
        previousUnloaded = false;

        LevelSceneInfo currentLevel = GetLevelInfo(currentLevelIndex);

        if (currentLevel == null || !currentLevel.IsValid())
        {
            Debug.LogError($"Level info is invalid : {currentLevelIndex}");
            isStreaming = false;
            yield break;
        }

        int nextIndex = currentLevelIndex + 1;

        if (nextIndex < levelScenes.Length)
            yield return LoadAndPlaceLevel(nextIndex, currentLevel.BottomPoint.position);

        for (int i = 0; i < currentLevelIndex - 1; i++)
        {
            if (IsSceneLoaded(i))
                yield return UnloadLevel(i);
        }

        Log($"Current Level : {levelScenes[currentLevelIndex].sceneName}");
        isStreaming = false;
    }

    private IEnumerator UnloadPreviousLevel()
    {
        if (isStreaming || previousUnloaded)
            yield break;

        int previousIndex = currentLevelIndex - 1;

        if (previousIndex < 0)
        {
            previousUnloaded = true;
            yield break;
        }

        isStreaming = true;
        yield return UnloadLevel(previousIndex);
        previousUnloaded = true;
        isStreaming = false;
    }

    private IEnumerator LoadAndPlaceLevel(int index, Vector3 targetTopPosition)
    {
        if (index < 0 || index >= levelScenes.Length)
            yield break;

        string sceneName = levelScenes[index].sceneName;

        if (string.IsNullOrEmpty(sceneName))
            yield break;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not in the Build Profile.");
            yield break;
        }

        if (!IsSceneLoaded(index))
        {
            Log($"Load Scene : {sceneName}");

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (operation == null)
                yield break;

            yield return operation;
        }

        LevelSceneInfo info = GetLevelInfo(index);

        if (info == null || !info.IsValid())
        {
            Debug.LogError($"LevelSceneInfo is invalid : {sceneName}");
            yield break;
        }

        Vector3 offset = targetTopPosition - info.TopPoint.position;
        info.transform.position += offset;

        Log(
            $"{sceneName} | Top Y = {info.TopPoint.position.y} | " +
            $"Bottom Y = {info.BottomPoint.position.y}"
        );
    }

    private IEnumerator UnloadLevel(int index)
    {
        if (!IsSceneLoaded(index))
            yield break;

        string sceneName = levelScenes[index].sceneName;

        Log($"Unload Scene : {sceneName}");

        AsyncOperation operation =
            SceneManager.UnloadSceneAsync(sceneName);

        if (operation != null)
            yield return operation;
    }

    private float GetLevelProgress(LevelSceneInfo level)
    {
        float topY = level.TopPoint.position.y;
        float bottomY = level.BottomPoint.position.y;

        if (Mathf.Approximately(topY, bottomY))
            return 0.0f;

        return Mathf.InverseLerp(
            topY,
            bottomY,
            player.position.y
        );
    }

    private LevelSceneInfo GetLevelInfo(int index)
    {
        if (!IsSceneLoaded(index))
            return null;

        Scene scene =
            SceneManager.GetSceneByName(levelScenes[index].sceneName);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            LevelSceneInfo info =
                root.GetComponentInChildren<LevelSceneInfo>(true);

            if (info != null)
                return info;
        }

        return null;
    }

    private bool IsSceneLoaded(int index)
    {
        if (index < 0 || index >= levelScenes.Length)
            return false;

        Scene scene =
            SceneManager.GetSceneByName(levelScenes[index].sceneName);

        return scene.IsValid() && scene.isLoaded;
    }

    private void Log(string message)
    {
        if (enableDebugLog)
            Debug.Log(message);
    }
}