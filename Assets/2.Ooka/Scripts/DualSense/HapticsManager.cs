using UnityEngine;
using GamepadCore.Unity;

public class HapticsManager : MonoBehaviour
{
    private const int DeviceIndex = 0;
    private const int SampleRate = 48000;

    [Header("落下ハプティクス")]
    [SerializeField] private float minAmplitude = 0.05f;
    [SerializeField] private float maxAmplitude = 0.7f;
    [SerializeField] private float noiseGain = 2.5f;
    [SerializeField] private float clipDuration = 0.5f;

    [Header("横移動ハプティクス")]
    [SerializeField] private float minHorizontalAmplitude = 0.15f;
    [SerializeField] private float maxHorizontalAmplitude = 0.6f;
    [SerializeField] private float horizontalFrequencyHz = 50f;

    private bool isHorizontalActive;
    private float horizontalIntensity;
    private float horizontalDirection;

    private bool isPaused;
    private bool isFallingActive;

    private float fallSpeedRatio;
    private float currentFrequencyHz = 50f;
    private float playbackTimer;

    private HapticWaveform currentWaveform = HapticWaveform.Noise;

    /// <summary>
    /// ハプティクスの再生を要求する。
    /// intensity は 0～1 の範囲で指定する。
    /// </summary>
    public void RequestHaptics(HapticType type, float intensity, float frequencyHz = 50f,
        HapticWaveform waveform = HapticWaveform.Noise, float direction = 0f)
    {
        intensity = Mathf.Clamp01(intensity);
        frequencyHz = Mathf.Clamp(frequencyHz, 1f, 1000f);

        switch (type)
        {
            case HapticType.Falling:
                fallSpeedRatio = intensity;
                currentFrequencyHz = frequencyHz;
                currentWaveform = waveform;
                isFallingActive = intensity > 0f;
                break;

            case HapticType.HorizontalMovement:
                horizontalIntensity = intensity;
                horizontalDirection = Mathf.Clamp(direction, -1f, 1f);
                isHorizontalActive = intensity > 0f;
                break;

            default:
                Debug.Log($"未実装の振動リクエスト: {type}");
                break;
        }
    }

    private void Update()
    {
        if (isPaused || (!isFallingActive && !isHorizontalActive))
        {
            return;
        }

        playbackTimer -= Time.unscaledDeltaTime;

        if (playbackTimer <= 0f)
        {
            PlayCombinedHaptics();
            playbackTimer = clipDuration;
        }
    }

    /// <summary>
    /// ゲームの一時停止・再開を反映する。
    /// </summary>
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        playbackTimer = 0f;
    }

    /// <summary>
    /// 落下振動の再生要求を終了する。
    /// </summary>
    public void StopFallingHaptics()
    {
        isFallingActive = false;
        fallSpeedRatio = 0f;
        playbackTimer = 0f;
    }

    public void StopHorizontalHaptics()
    {
        isHorizontalActive = false;
        horizontalIntensity = 0f;
        horizontalDirection = 0f;
        playbackTimer = 0f;
    }

    private void PlayCombinedHaptics()
    {
        if (!GamepadCoreApi.IsInitialized || GamepadCoreApi.ConnectedCount <= DeviceIndex)
        {
            return;
        }

        int frameCount = Mathf.RoundToInt(SampleRate * clipDuration);
        short[] samples = new short[frameCount * 2];

        float fallAmplitude = Mathf.Lerp(minAmplitude, maxAmplitude, fallSpeedRatio);

        float horizontalAmplitude = Mathf.Lerp(minHorizontalAmplitude,
            maxHorizontalAmplitude, horizontalIntensity);

        for (int i = 0; i < frameCount; i++)
        {
            float time = (float)i / SampleRate;

            float left = 0f;
            float right = 0f;

            // 落下振動：左右同じノイズまたは指定波形
            if (isFallingActive)
            {
                float phase = 2f * Mathf.PI * currentFrequencyHz * time;

                float wave = GetWave(phase, currentWaveform);
                float gain = currentWaveform == HapticWaveform.Noise ? noiseGain : 1f;

                float fall = wave * fallAmplitude * gain;

                left += fall;
                right += fall;
            }

            // 横移動振動：方向に応じて左右の強さを変える
            if (isHorizontalActive)
            {
                float phase = 2f * Mathf.PI * horizontalFrequencyHz * time;

                float wave = GetWave(phase, HapticWaveform.Triangle);
                float horizontal = wave * horizontalAmplitude;

                float secondaryWeight = 0.35f;

                float leftWeight = horizontalDirection < 0f ? 1f : secondaryWeight;

                float rightWeight = horizontalDirection > 0f ? 1f : secondaryWeight;

                left += horizontal * leftWeight;
                right += horizontal * rightWeight;
            }

            samples[i * 2] = ToPcmSample(left);
            samples[i * 2 + 1] = ToPcmSample(right);
        }

        int result = GamepadCoreApi.PlayPcm16(DeviceIndex, samples);

        if (result != 1)
        {
            Debug.LogWarning($"PCM振動の再生要求に失敗: {result}");
        }
    }

    private float GetWave(float phase, HapticWaveform waveform)
    {
        switch (waveform)
        {
            case HapticWaveform.Sine:
                return Mathf.Sin(phase);

            case HapticWaveform.Triangle:
                return 2f / Mathf.PI * Mathf.Asin(Mathf.Sin(phase));

            case HapticWaveform.Square:
                return Mathf.Sin(phase) >= 0f ? 1f : -1f;

            case HapticWaveform.Noise:
            default:
                return Random.Range(-1f, 1f);
        }
    }

    private short ToPcmSample(float value)
    {
        value = Mathf.Clamp(value, -1f, 1f);
        return (short)(value * short.MaxValue);
    }
}
