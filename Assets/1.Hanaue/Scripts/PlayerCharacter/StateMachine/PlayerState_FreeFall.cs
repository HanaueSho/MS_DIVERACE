/*
    PlayerState_FreeFall
    20261004  hanaue sho
 */
using UnityEngine;

public class PlayerState_FreeFall : PlayerState_Base
{
    // --------------------------------------------------
    // ----- Constructor -----
    // --------------------------------------------------
    public PlayerState_FreeFall(Player_CharacterController controller) : base(controller)
    {
    }


    // --------------------------------------------------
    // ----- Lifecycle -----
    // --------------------------------------------------
    public override void Enter() { }
    public override void Update() 
    {
        // 入力を取得
        Vector2 moveInput = _controller.InputHandler.MoveInput;

        // 移動
        _controller.Movement.Move(moveInput, Time.deltaTime);
    }
    public override void Exit() { }
}
