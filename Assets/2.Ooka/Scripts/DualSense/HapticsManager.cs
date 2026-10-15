using UnityEngine;
using GamepadCore.Unity;

public class HapticsManager : MonoBehaviour
{
    private const int DeviceIndex = 0;
    private const int SampleRate = 48000;

    [Header("落下ハプティクス")]
    // [SerializeField] private float maxFallSpeed = 50f;
    [SerializeField] private float minAmplitude = 0.01f;
    [SerializeField] private float maxAmplitude = 0.3f;
    [SerializeField] private float noiseGain = 2.5f;
    [SerializeField] private float clipDuration = 0.5f;

    private bool isPaused;
    private float fallSpeedRatio;
    private float playbackTimer;

    /// <summary>
    /// 外部から振動をリクエストするための入口
    /// </summary>
    public void RequestHaptics(HapticType type, float intensity, float frequencyHz = 50f,
        HapticWaveform waveform = HapticWaveform.Noise)
    {
        intensity = Mathf.Clamp01(intensity);
        frequencyHz = Mathf.Clamp(frequencyHz, 1f, 1000f);

        switch (type)
        {
            case HapticType.Falling:
                fallSpeedRatio = intensity;

                // 現段階では落下はノイズ波形を維持。
                // 周波数・波形の再生処理は次の段階で接続する。
                break;

            default:
                Debug.Log($"未実装の振動リクエスト: {type}");
                break;
        }
    }

    private void Update()
    {
        if (isPaused || fallSpeedRatio <= 0f)
            return;

        playbackTimer -= Time.unscaledDeltaTime;

        if (playbackTimer <= 0f)
        {
            PlayFallNoise();
            playbackTimer = clipDuration;
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        playbackTimer = 0f;
    }

    public void StopFallingHaptics()
    {
        fallSpeedRatio = 0f;
        playbackTimer = 0f;
    }

    private void PlayFallNoise()
    {
        if (!GamepadCoreApi.IsInitialized || GamepadCoreApi.ConnectedCount <= DeviceIndex)
            return;

        float amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, fallSpeedRatio);

        int frameCount = Mathf.RoundToInt(SampleRate * clipDuration);

        short[] samples = new short[frameCount * 2];

        for (int i = 0; i < frameCount; i++)
        {
            float noise = Random.Range(-1f, 1f);
            float output = noise * amplitude * noiseGain;

            short sample = (short)(Mathf.Clamp(output, -1f, 1f) * short.MaxValue);

            samples[i * 2] = sample;
            samples[i * 2 + 1] = sample;
        }

        int result = GamepadCoreApi.PlayPcm16(DeviceIndex, samples);

        if (result != 1)
            Debug.LogWarning($"PCM振動の再生要求に失敗: {result}");
    }
}
