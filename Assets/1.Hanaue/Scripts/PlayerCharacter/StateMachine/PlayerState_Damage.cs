/*
    PlayerState_Damage
    20261010  hanaue sho
    障害物に触れたステート
 */
using UnityEngine;

public class PlayerState_Damage : PlayerState_Base
{
    // --------------------------------------------------
    // ----- ControlData -----
    // --------------------------------------------------
    private readonly PlayerMovement_ControlData _controlData;

    // --------------------------------------------------
    // ----- PlayerImpactInfo -----
    // --------------------------------------------------
    private PlayerImpactInfo _impactInfo;

    // --------------------------------------------------
    // ----- Constructor -----
    // --------------------------------------------------
    public PlayerState_Damage(Player_CharacterController controller, PlayerMovement_ControlData controlData) : base(controller)
    {
        _controlData = controlData;
    }

    // --------------------------------------------------
    // ----- Lifecycle -----
    // --------------------------------------------------
    public override void Enter()
    {
        switch (_impactInfo.ImpactType)
        {
            case PlayerImpactType.Top:
                Debug.Log("Top");
                _controller.Movement.SetVelocity(new Vector3(0.0f, 20.0f, 0.0f));
                break;
            case PlayerImpactType.Side:
                Debug.Log("Side");
                break;
        }
    }
    public override void Update() 
    {
        // ----- 入力を取得 -----
        Vector2 moveInput = _controller.InputHandler.MoveInput;
        float postureInput = _controller.InputHandler.PoseInput;
        float rotateInput = _controller.InputHandler.RotateInput;

        // ----- 速度更新 -----
        _controller.Movement.UpdateVelocity(moveInput, postureInput, _controlData, Time.deltaTime);
        _controller.Movement.RotateYaw(rotateInput, _controlData, Time.deltaTime);

        // ----- 移動予定量 -----
        Vector3 displace = _controller.Movement.GetDisplacement(Time.deltaTime);

        // ----- 衝突判定 -----
        if (_controller.CollisionDetector.CheckCollision(displace, out PlayerImpactInfo inpactInfo))
        {
            Debug.Log("Hit");
        }

        // ----- 通常移動 -----
        _controller.Movement.ApplyMovement(displace);

        // ----- Animation -----
        _controller.Animation.RotateTo(postureInput);
    }
    public override void Exit() { }


    // --------------------------------------------------
    // ----- Setter -----
    // --------------------------------------------------
    public void SetImpactInfo(PlayerImpactInfo impactInfo)
    {
        _impactInfo = impactInfo;
    }


}
