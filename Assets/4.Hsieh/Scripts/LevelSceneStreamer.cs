using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneStreamer : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private Transform player;

    [Header("Level Scenes")]
    [SerializeField]
    private string[] levelScenes =
    {
        "Scene_Level_01",
        "Scene_Level_02",
        "Scene_Level_03",
        "Scene_Level_04",
        "Scene_Level_05"
    };

    [Header("Level Settings")]
    [SerializeField]
    private float levelStartY = 300.0f;

    [SerializeField]
    private float levelHeight = 300.0f;

    // 1 = 前後各 1 層
    // 最大同時存在 3 個 Level Scene
    [SerializeField]
    private int preloadRadius = 1;

    [Header("Optional Level Positioning")]
    [SerializeField]
    private string levelRootName = "LevelRoot";


    // 玩家目前要求的 Level
    private int requestedLevel = 0;

    // 已經完成串流處理的 Level
    private int appliedLevel = -1;

    private bool isRefreshing = false;


    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("LevelSceneStreamer : Player が設定されていません。");
            enabled = false;
            return;
        }

        if (levelScenes == null || levelScenes.Length == 0)
        {
            Debug.LogError("LevelSceneStreamer : Level Scene が設定されていません。");
            enabled = false;
            return;
        }

        requestedLevel = GetLevelIndex(player.position.y);

        StartCoroutine(RefreshLoop());
    }


    private void Update()
    {
        int detectedLevel = GetLevelIndex(player.position.y);

        // Level 沒變就不需要做任何事情
        if (detectedLevel == requestedLevel)
            return;

        requestedLevel = detectedLevel;

        Debug.Log(
            $"Current Level : {requestedLevel + 1}"
        );

        if (!isRefreshing)
        {
            StartCoroutine(RefreshLoop());
        }
    }


    /// <summary>
    /// 玩家快速下降時，
    /// 即使載入途中又跨過下一層，
    /// 也會繼續更新到最新 Level。
    /// </summary>
    private IEnumerator RefreshLoop()
    {
        isRefreshing = true;

        while (appliedLevel != requestedLevel)
        {
            int targetLevel = requestedLevel;

            yield return RefreshStreamingWindow(targetLevel);

            appliedLevel = targetLevel;
        }

        isRefreshing = false;
    }


    /// <summary>
    /// 維持：
    ///
    /// Previous
    /// Current
    /// Next
    ///
    /// 三個 Scene。
    /// </summary>
    private IEnumerator RefreshStreamingWindow(int centerLevel)
    {
        HashSet<int> requiredLevels =
            new HashSet<int>();


        // 要留下哪些 Level
        for (int i = centerLevel - preloadRadius;
             i <= centerLevel + preloadRadius;
             i++)
        {
            if (i < 0 || i >= levelScenes.Length)
                continue;

            requiredLevels.Add(i);
        }


        // ==============================
        // ① 先 Load 必要 Scene
        // ==============================

        foreach (int levelIndex in requiredLevels)
        {
            string sceneName =
                levelScenes[levelIndex];

            if (IsSceneLoaded(sceneName))
                continue;


            Debug.Log($"Load Scene : {sceneName}");

            AsyncOperation loadOperation =
                SceneManager.LoadSceneAsync(
                    sceneName,
                    LoadSceneMode.Additive
                );


            if (loadOperation != null)
            {
                yield return loadOperation;

                PositionLevelScene(levelIndex);
            }
        }


        // ==============================
        // ② 再 Unload 太遠的 Scene
        // ==============================

        for (int i = 0;
             i < levelScenes.Length;
             i++)
        {
            if (requiredLevels.Contains(i))
                continue;


            string sceneName =
                levelScenes[i];

            if (!IsSceneLoaded(sceneName))
                continue;


            Debug.Log($"Unload Scene : {sceneName}");

            AsyncOperation unloadOperation =
                SceneManager.UnloadSceneAsync(
                    sceneName
                );


            if (unloadOperation != null)
            {
                yield return unloadOperation;
            }
        }
    }


    /// <summary>
    /// 根據玩家 Y 算目前在哪一層。
    /// </summary>
    private int GetLevelIndex(float playerY)
    {
        float fallDistance =
            levelStartY - playerY;

        int levelIndex =
            Mathf.FloorToInt(
                fallDistance / levelHeight
            );


        return Mathf.Clamp(
            levelIndex,
            0,
            levelScenes.Length - 1
        );
    }


    /// <summary>
    /// Scene 是否已載入。
    /// </summary>
    private bool IsSceneLoaded(string sceneName)
    {
        Scene scene =
            SceneManager.GetSceneByName(sceneName);

        return scene.IsValid() &&
               scene.isLoaded;
    }


    /// <summary>
    /// 將每個 LevelRoot 放到對應高度。
    ///
    /// Level01 = Y 1000
    /// Level02 = Y 800
    /// Level03 = Y 600
    /// ...
    /// </summary>
    private void PositionLevelScene(int levelIndex)
    {
        string sceneName =
            levelScenes[levelIndex];

        Scene scene =
            SceneManager.GetSceneByName(sceneName);


        if (!scene.IsValid())
            return;


        GameObject[] rootObjects =
            scene.GetRootGameObjects();


        foreach (GameObject root in rootObjects)
        {
            if (root.name != levelRootName)
                continue;


            Vector3 position =
                root.transform.position;

            position.y =
                levelStartY -
                (levelHeight * levelIndex);

            root.transform.position =
                position;


            Debug.Log(
                $"{sceneName} Position Y = {position.y}"
            );

            return;
        }


        Debug.LogWarning(
            $"{sceneName} に {levelRootName} がありません。"
        );
    }
}