using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.wKey.isPressed)
        {
            moveDirection += Vector3.forward;
        }

        if (keyboard.sKey.isPressed)
        {
            moveDirection += Vector3.back;
        }

        if (keyboard.aKey.isPressed)
        {
            moveDirection += Vector3.left;
        }

        if (keyboard.dKey.isPressed)
        {
            moveDirection += Vector3.right;
        }

        // ŽÎ‚ßˆÚ“®‚Å‘¬‚­‚È‚ç‚È‚¢‚æ‚¤‚É‚·‚é
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}