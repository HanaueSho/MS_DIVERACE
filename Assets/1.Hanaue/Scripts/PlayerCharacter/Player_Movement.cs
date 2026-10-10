/*
    Player_Movement
    20261004  hanaue sho
    UpdateVelocity => CheckCollision => ApplyVelocity
 */
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    // --------------------------------------------------
    // ----- CharacterController -----
    // --------------------------------------------------
    private Player_CharacterController _characterController;

    // --------------------------------------------------
    // ----- Parameter -----
    // --------------------------------------------------
    [Header("Movement Parameter")]
    [SerializeField] private PlayerMovement_ParameterData _parameterData;

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
    // ----- UpdateVelocity -----
    // --------------------------------------------------
    public void UpdateVelocity(Vector2 input,float posture, PlayerMovement_ControlData controlData, float deltaTime)
    {
        posture = Mathf.Clamp01(posture);
        // 入力値の大きさを１以下にする
        input = Vector2.ClampMagnitude(input, 1.0f);

        // 1. 水平方向の加速度
        if (controlData.EnableHorizontalMove)
        {
            Vector3 horizontalAcceleration = CalculateHorizontalAcceleration(input, posture, controlData);

            _currentVelocity += horizontalAcceleration * deltaTime;
        }

        // 2. 水平方向の抵抗
        Vector3 horizontalDragAcceleration = CalculateHorizontalDrag();
        _currentVelocity += horizontalDragAcceleration * deltaTime;

        // 3. 落下方向の加速度
        if (controlData.EnableFall)
        {
            float verticalAcceleration = CalculateVerticalAcceleration(posture, controlData);
            _currentVelocity.y += verticalAcceleration * deltaTime;
        }
    }

    // --------------------------------------------------
    // ----- Get Displacement -----
    // --------------------------------------------------
    public Vector3 GetDisplacement(float deltaTime)
    {
        return _currentVelocity * deltaTime;
    }

    // --------------------------------------------------
    // ----- Aplly Movement -----
    // --------------------------------------------------
    public void ApplyMovement(Vector3 displacement)
    {
        transform.position += displacement;
    }

    // --------------------------------------------------
    // ----- RotateYaw -----
    // --------------------------------------------------
    public void RotateYaw(float input, PlayerMovement_ControlData controlData, float deltaTime)
    {
        if (!controlData.EnableYawRotation)
        {
            return;
        }

        float angle = input * _parameterData.YawSpeed * deltaTime;
        transform.Rotate(0.0f, angle, 0.0f, Space.World);
    }


    // --------------------------------------------------
    // ----- Calculate -----
    // --------------------------------------------------
    private Vector3 CalculateHorizontalAcceleration(Vector2 input, float posture, PlayerMovement_ControlData controlData)
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
            longitudinalAcceleration = input.y * _parameterData.ForwardAcceleration * controlData.ForwardControlMultiplier;
        }
        else
        {
            // 足方向
            longitudinalAcceleration = input.y * _parameterData.BackAcceleration * controlData.BackControlMultiplier;
        }

        // ----- 左右方向 -----
        float lateralAcceleration = input.x * _parameterData.SideAcceleration * controlData.SideControlMultiplier;

        // ----- 姿勢による操作性能 -----
        float postureControl = Mathf.Lerp(_parameterData.HorizontalPostureControl, _parameterData.VerticalPostureControl, posture);

        // ----- 最終算出 -----
        Vector3 acceleration = forward * longitudinalAcceleration + right * lateralAcceleration;
        acceleration *= postureControl * controlData.HorizontalControlMultiplier;

        return acceleration;
    }
    private Vector3 CalculateHorizontalDrag()
    {
        Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0.0f, _currentVelocity.z);
        // 速度とは逆方向に抵抗
        return -horizontalVelocity * _parameterData.HorizontalDrag;
    }
    private float CalculateVerticalAcceleration(float posture, PlayerMovement_ControlData controlData)
    {
        // 姿勢に応じた終端速度
        float terminalFallSpeed = Mathf.Lerp(_parameterData.HorizontalTerminalFallSpeed, _parameterData.VerticalTerminalFallSpeed, posture);
        terminalFallSpeed *= controlData.TerminalFallSpeedMultiplier;

        // 終端速度から抵抗係数を逆算
        float verticalDrag = _parameterData.GravityAcceleration / terminalFallSpeed;

        // 重力
        float gravityAcceleration = -_parameterData.GravityAcceleration;

        // 現在速度に対する抵抗
        float dragAcceleration = -_currentVelocity.y * verticalDrag;

        return gravityAcceleration + dragAcceleration;
    }

    // --------------------------------------------------
    // ----- Public Events -----
    // --------------------------------------------------
    public void SetVelocity(Vector3 velocity)
    {
        _currentVelocity = velocity;
    }
    public void ApplyImpulse(Vector3 velocity)
    {
        _currentVelocity += velocity;
    }

}
