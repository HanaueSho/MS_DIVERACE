/*
    Player_StateMachine
    20261004  hanaue sho
 */
using UnityEngine;


public class Player_StateMachine : MonoBehaviour
{
    // --------------------------------------------------
    // ----- CharacterController -----
    // --------------------------------------------------
    private Player_CharacterController _characterController;

    // --------------------------------------------------
    // ----- States -----
    // --------------------------------------------------
    private PlayerState_FreeFall _stateFreeFall;
    private PlayerState_Damage _stateDamage;
    public PlayerState_FreeFall StateFreeFall => _stateFreeFall;
    public PlayerState_Damage StateDamage => _stateDamage;

    // --------------------------------------------------
    // ----- CurrentState -----
    // --------------------------------------------------
    private PlayerState_Base _currentState;
    public PlayerState_Base CurrentState => _currentState;

    // --------------------------------------------------
    // ----- ControlData -----
    // --------------------------------------------------
    [Header("Control Data")]
    [Tooltip("FreeFallState ControlData")]
    [SerializeField] private PlayerMovement_ControlData _freeFallControlData;
    [Tooltip("DamageState ControlData")]
    [SerializeField] private PlayerMovement_ControlData _damageControlData;


    // --------------------------------------------------
    // ----- Unity Events -----
    // --------------------------------------------------
    private void Start()
    {
        _characterController = GetComponent<Player_CharacterController>();
        _stateFreeFall = new PlayerState_FreeFall(_characterController, _freeFallControlData);
        _stateDamage = new PlayerState_Damage(_characterController, _damageControlData);

        ChangeState(_stateFreeFall);
    }
    private void Update()
    {
        // ControllerMode が Manual かチェック
        if ( _characterController.CurrentControllerMode != ControllerMode.Manual)
        {
            return;
        }

        _currentState?.Update();
    }

    // --------------------------------------------------
    // ----- Change State -----
    // --------------------------------------------------
    public void ChangeState(PlayerState_Base newState)
    {
        _currentState?.Exit();

        _currentState = newState;

        _currentState?.Enter();
    }


}
