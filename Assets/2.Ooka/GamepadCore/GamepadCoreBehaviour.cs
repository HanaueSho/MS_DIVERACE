using GamepadCore.Unity;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class GamepadCoreBehaviour : MonoBehaviour
{
    private void Awake()
    {
        if (!GamepadCoreApi.Initialize())
            Debug.LogError("GamepadCore initialization failed: " + GamepadCoreApi.LastError);
    }

    private void Update()
    {
        GamepadCoreApi.Update(Time.unscaledDeltaTime);
    }

    private void OnApplicationQuit()
    {
        GamepadCoreApi.Shutdown();
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) GamepadCoreApi.Shutdown();
    }
}
