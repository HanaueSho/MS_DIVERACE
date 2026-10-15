using UnityEngine;
using UnityEngine.InputSystem;

public class HapticsHorizontalTest : MonoBehaviour
{
    [SerializeField] private HapticsManager hapticsManager;

    [SerializeField, Range(0.01f, 1f)]
    private float intensity = 0.8f;

    private void Update()
    {
        if (Keyboard.current == null || hapticsManager == null)
            return;

        if (Keyboard.current.aKey.isPressed)
        {
            hapticsManager.RequestHaptics(HapticType.HorizontalMovement,
                intensity, direction: -1f);
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            hapticsManager.RequestHaptics(HapticType.HorizontalMovement,
                intensity, direction: 1f);
        }
        else
        {
            hapticsManager.StopHorizontalHaptics();
        }
    }
}
