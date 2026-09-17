using System.Collections;
using System.Collections.Generic;
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
        new LevelScene { sceneName = "Test_Scene_Level_05" },
        new LevelScene { sceneName = "Test_Scene_Level_04" },
        new LevelScene { sceneName = "Test_Scene_Level_03" },
        new LevelScene { sceneName = "Test_Scene_Level_02" },
        new LevelScene { sceneName = "Test_Scene_Level_01" }
    };

    [SerializeField] private float firstLevelTopY = 300.0f;
    [SerializeField] private float levelHeight = 300.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float unloadPreviousProgress = 0.5f;

    [SerializeField] private string levelRootName = "LevelRoot";

    private int requestedLevelIndex;
    private int appliedLevelIndex = -1;

    private bool requestedKeepPrevious;
    private bool appliedKeepPrevious;

    private bool isRefreshing;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player is not assigned.");
            enabled = false;
            return;
        }

        if (levelScenes == null || levelScenes.Length == 0)
        {
            Debug.LogError("Level scenes are not assigned.");
            enabled = false;
            return;
        }

        requestedLevelIndex = GetLevelIndex(player.position.y);
        requestedKeepPrevious = ShouldKeepPrevious(requestedLevelIndex);

        StartCoroutine(RefreshLoop());
    }

    private void Update()
    {
        if (player == null) return;

        int levelIndex = GetLevelIndex(player.position.y);
        bool keepPrevious = ShouldKeepPrevious(levelIndex);

        if (levelIndex == requestedLevelIndex &&
            keepPrevious == requestedKeepPrevious)
            return;

        requestedLevelIndex = levelIndex;
        requestedKeepPrevious = keepPrevious;

        if (!isRefreshing)
            StartCoroutine(RefreshLoop());
    }

    private IEnumerator RefreshLoop()
    {
        isRefreshing = true;

        while (appliedLevelIndex != requestedLevelIndex ||
               appliedKeepPrevious != requestedKeepPrevious)
        {
            int targetIndex = requestedLevelIndex;
            bool keepPrevious = requestedKeepPrevious;

            yield return RefreshStreamingWindow(targetIndex, keepPrevious);

            appliedLevelIndex = targetIndex;
            appliedKeepPrevious = keepPrevious;
        }

        isRefreshing = false;
    }

    private IEnumerator RefreshStreamingWindow(
        int currentLevelIndex,
        bool keepPrevious)
    {
        HashSet<int> requiredLevels = new HashSet<int>();

        requiredLevels.Add(currentLevelIndex);

        int nextIndex = currentLevelIndex + 1;
        if (nextIndex < levelScenes.Length)
            requiredLevels.Add(nextIndex);

        int previousIndex = currentLevelIndex - 1;

        if (keepPrevious && previousIndex >= 0)
            requiredLevels.Add(previousIndex);

        foreach (int index in requiredLevels)
        {
            string sceneName = levelScenes[index].sceneName;

            if (string.IsNullOrEmpty(sceneName))
                continue;

            if (IsSceneLoaded(sceneName))
            {
                PositionLevelScene(index);
                continue;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"Scene '{sceneName}' is not in the Build Profile."
                );
                continue;
            }

            Debug.Log($"Load Scene : {sceneName}");

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(
                    sceneName,
                    LoadSceneMode.Additive
                );

            if (operation == null)
                continue;

            yield return operation;

            PositionLevelScene(index);
        }

        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (requiredLevels.Contains(i))
                continue;

            string sceneName = levelScenes[i].sceneName;

            if (string.IsNullOrEmpty(sceneName))
                continue;

            if (!IsSceneLoaded(sceneName))
                continue;

            Debug.Log($"Unload Scene : {sceneName}");

            AsyncOperation operation =
                SceneManager.UnloadSceneAsync(sceneName);

            if (operation != null)
                yield return operation;
        }
    }

    private int GetLevelIndex(float playerY)
    {
        float fallDistance = firstLevelTopY - playerY;
        int index = Mathf.FloorToInt(fallDistance / levelHeight);

        return Mathf.Clamp(
            index,
            0,
            levelScenes.Length - 1
        );
    }

    private float GetLevelProgress(int index)
    {
        float topY =
            firstLevelTopY -
            (levelHeight * index);

        float progress =
            (topY - player.position.y) /
            levelHeight;

        return Mathf.Clamp01(progress);
    }

    private bool ShouldKeepPrevious(int index)
    {
        if (index <= 0)
            return false;

        return GetLevelProgress(index) <
               unloadPreviousProgress;
    }

    private float GetLevelCenterY(int index)
    {
        return firstLevelTopY -
               (levelHeight * 0.5f) -
               (levelHeight * index);
    }

    private void PositionLevelScene(int index)
    {
        string sceneName =
            levelScenes[index].sceneName;

        Scene scene =
            SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded)
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != levelRootName)
                continue;

            Vector3 position =
                root.transform.position;

            position.y =
                GetLevelCenterY(index);

            root.transform.position =
                position;

            return;
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        Scene scene =
            SceneManager.GetSceneByName(sceneName);

        return scene.IsValid() && scene.isLoaded;
    }
}