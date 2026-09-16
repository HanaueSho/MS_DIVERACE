using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLevelStreamer : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private Transform player;

    [Header("Test Scenes")]
    [SerializeField]
    private string level5Scene = "Test_Scene_Level_05";

    [SerializeField]
    private string level4Scene = "Test_Scene_Level_04";

    [Header("Level Settings")]
    [SerializeField]
    private float level5CenterY = 150.0f;

    [SerializeField]
    private float halfLevelHeight = 150.0f;

    [Header("Unload")]
    [SerializeField]
    private float unloadMargin = 30.0f;

    private bool enteredLevel4 = false;

    private float SeamWorldY
    {
        get
        {
            return level5CenterY - halfLevelHeight;
        }
    }

    private IEnumerator Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "TestLevelStreamer : Player ‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB"
            );

            yield break;
        }

        // Player ‘ªŠJn‚“x
        Vector3 playerPosition = player.position;
        playerPosition.y = 300.0f;
        player.position = playerPosition;


        // Level5 Ú“ü
        yield return LoadLevel(
            level5Scene,
            level5CenterY
        );


        // Level4 ’†SˆÊ’u
        //
        // Level5 Bottom = 200 - 150 = 50
        //
        // Level4 Top •K{ = 50
        //
        // Level4 Center = 50 - 150 = -100
        float level4CenterY =
            level5CenterY -
            (halfLevelHeight * 2.0f);


        // İŠß‰Æi“ü Level4 ‘OA—aæÚ“ü
        yield return LoadLevel(
            level4Scene,
            level4CenterY
        );


        Debug.Log(
            $"Scene Streaming Ready. Seam Y = {SeamWorldY}"
        );
    }


    private void Update()
    {
        if (player == null)
            return;


        // Šß‰Æ›ßãSœn Level5 i“ü Level4
        if (!enteredLevel4 &&
            player.position.y <= SeamWorldY)
        {
            enteredLevel4 = true;

            Debug.Log(
                "Level5 -> Level4"
            );

            StartCoroutine(
                UnloadLevel5WhenSafe()
            );
        }
    }


    private IEnumerator LoadLevel(
        string sceneName,
        float centerY
    )
    {
        Scene scene =
            SceneManager.GetSceneByName(sceneName);


        // ŠÒŸ“—L Load
        if (!scene.isLoaded)
        {
            Debug.Log(
                $"Load Scene : {sceneName}"
            );

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(
                    sceneName,
                    LoadSceneMode.Additive
                );

            while (!operation.isDone)
            {
                yield return null;
            }
        }


        // Ú“üŠ®¬Œãæ“¾ Scene
        scene =
            SceneManager.GetSceneByName(sceneName);


        GameObject[] roots =
            scene.GetRootGameObjects();


        foreach (GameObject root in roots)
        {
            if (root.name != "LevelRoot")
                continue;


            Vector3 position =
                root.transform.position;

            position.y = centerY;

            root.transform.position =
                position;


            Debug.Log(
                $"{sceneName} Center Y = {centerY}"
            );

            yield break;
        }


        Debug.LogWarning(
            $"{sceneName} ‚É LevelRoot ‚ª‚ ‚è‚Ü‚¹‚ñB"
        );
    }


    private IEnumerator UnloadLevel5WhenSafe()
    {
        // „Œ×‰ßŒğŠEêy•s—v—§™ˆœC
        // Ä‰º~ˆêêyË‰µÚ Level5
        float unloadY =
            SeamWorldY - unloadMargin;


        while (player.position.y > unloadY)
        {
            yield return null;
        }


        Scene scene =
            SceneManager.GetSceneByName(level5Scene);


        if (scene.isLoaded)
        {
            Debug.Log(
                $"Unload Scene : {level5Scene}"
            );

            yield return
                SceneManager.UnloadSceneAsync(
                    level5Scene
                );
        }
    }
}