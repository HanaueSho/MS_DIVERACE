/*
    Player_Movement
    20261004  hanaue sho
 */
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    // --------------------------------------------------
    // ----- CharacterController -----
    // --------------------------------------------------
    private Player_CharacterController _characterController;

    // --------------------------------------------------
    // ----- Fall Parameter -----
    // --------------------------------------------------
    [Header("Fall Parameter")]
    [SerializeField] private float _gravityAcceleration = 30.0f;
    // 姿勢ごとの自然な終端落下速度
    [SerializeField] private float _horizontalTerminalFallSpeed = 20.0f;
    [SerializeField] private float _verticalTerminalFallSpeed = 50.0f;

    // --------------------------------------------------
    // ----- HorizontalMove Parameter -----
    // --------------------------------------------------
    [Header("HorizontalMove Parameter")]
    [SerializeField] private float _forwardAcceleration = 20.0f;
    [SerializeField] private float _sideAcceleration = 15.0f;
    [SerializeField] private float _backAcceleration = 10.0f;

    // --------------------------------------------------
    // ----- HorizontalControl Parameter -----
    // --------------------------------------------------
    [Header("HorizontalControl Parameter")]
    // 姿勢に寄る水平走査性能
    [SerializeField] private float _horizontalPostureControl = 1.0f;
    [SerializeField] private float _verticalPostureControl = 0.4f;
    // 水平方向の抵抗
    [SerializeField] private float _horizontalDrag = 2.0f;

    // --------------------------------------------------
    // ----- Rotation Parameter -----
    // --------------------------------------------------
    [Header("Rotation Parameter")]
    [SerializeField] private float _yawSpeed = 180.0f;

    // --------------------------------------------------
    // ----- Runtime Parameter -----
    // --------------------------------------------------
    private Vector3 _currentVelocity;
    public Vector3 CurrentVelocity => _currentVelocity;

    // --------------------------------------------------
    // ----- Unity Events -----
    // --------------------------------------------------
    private void Awake()
    {
        _characterController = GetComponent<Player_CharacterController>();
    }

    // --------------------------------------------------
    // ----- Move -----
    // --------------------------------------------------
    public void Move(Vector2 input,float posture, float deltaTime)
    {
        posture = Mathf.Clamp01(posture);

        // 入力値の大きさを１以下にする
        input = Vector2.ClampMagnitude(input, 1.0f);

        // 1. 水平方向の加速度
        Vector3 horizontalAcceleration = CalculateHorizontalAcceleration(input, posture);

        // 2. 水平方向の抵抗
        Vector3 horizontalDragAcceleration = CalculateHorizontalDrag();

        // 3. 落下方向の加速度
        float verticalAcceleration = CalculateVerticalAcceleration(posture);

        // 4. 速度更新
        _currentVelocity += horizontalAcceleration * deltaTime;
        _currentVelocity += horizontalDragAcceleration * deltaTime;
        _currentVelocity.y += verticalAcceleration * deltaTime;

        // 5. 位置更新
        transform.position += _currentVelocity * deltaTime;
    }

    // --------------------------------------------------
    // ----- RotateYaw -----
    // --------------------------------------------------
    public void RotateYaw(float input, float deltaTime)
    {
        float angle = input * _yawSpeed * deltaTime;
        transform.Rotate(0.0f, angle, 0.0f, Space.World);
    }


    // --------------------------------------------------
    // ----- Calculate -----
    // --------------------------------------------------
    private Vector3 CalculateHorizontalAcceleration(Vector2 input, float posture)
    {
        // Y軸回転のみを想定した前方向、右方向
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0.0f;
        right.y = 0.0f;
        forward.Normalize();
        right.Normalize();

        // ----- 前方向 -----
        float longitudinalAcceleration;
        if (input.y >= 0.0f)
        {
            // 頭方向
            longitudinalAcceleration = input.y * _forwardAcceleration;
        }
        else
        {
            // 足方向
            longitudinalAcceleration = input.y * _backAcceleration;
        }

        // ----- 左右方向 -----
        float lateralAcceleration = input.x * _sideAcceleration;

        // ----- 姿勢による操作性能 -----
        float postureControl = Mathf.Lerp(_horizontalPostureControl, _verticalPostureControl, posture);

        // ----- 最終算出 -----
        Vector3 acceleration = forward * longitudinalAcceleration + right * lateralAcceleration;
        acceleration *= postureControl;

        return acceleration;
    }
    private Vector3 CalculateHorizontalDrag()
    {
        Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0.0f, _currentVelocity.z);
        // 速度とは逆方向に抵抗
        return -horizontalVelocity * _horizontalDrag;
    }
    private float CalculateVerticalAcceleration(float posture)
    {
        // 姿勢に応じた終端速度
        float terminalFallSpeed = Mathf.Lerp(_horizontalTerminalFallSpeed, _verticalTerminalFallSpeed, posture);

        // 終端速度から抵抗係数を逆算
        float verticalDrag = _gravityAcceleration / terminalFallSpeed;

        // 重力
        float gravityAcceleration = -_gravityAcceleration;

        // 現在速度に対する抵抗
        float dragAcceleration = -_currentVelocity.y * verticalDrag;

        return gravityAcceleration + dragAcceleration;
    }

}
