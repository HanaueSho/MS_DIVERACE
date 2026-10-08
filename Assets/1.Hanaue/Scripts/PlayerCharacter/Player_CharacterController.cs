/*
    Player_CharacterController
    20261004  hanaue sho
    PlayerCharacter のコンポーネントをまとめるもの
    外部からの窓口
 */
using UnityEngine;


public enum ControllerMode
{
    Manual,   // 操作
    Cutscene, // カットシーン用
}

public class Player_CharacterController : MonoBehaviour
{
    // --------------------------------------------------
    // ----- Components -----
    // --------------------------------------------------
    [Header("Components")]
    [SerializeField] private Player_InputHandler _input;
    [SerializeField] private Player_Movement _movement;
    [SerializeField] private Player_StateMachine _stateMachine;
    [SerializeField] private Player_CollisionDetector _collisionDetector;

    // --------------------------------------------------
    // ----- Public Property -----
    // --------------------------------------------------
    public Player_InputHandler InputHandler => _input;
    public Player_Movement Movement => _movement;
    public Player_CollisionDetector CollisionDetector => _collisionDetector;

    // --------------------------------------------------
    // ----- ControllerMode -----
    // --------------------------------------------------
    [SerializeField] private ControllerMode _currentControllerMode = ControllerMode.Manual;
    public ControllerMode CurrentControllerMode => _currentControllerMode;


    // --------------------------------------------------
    // ----- Unity Events -----
    // --------------------------------------------------
    void Awake()
    {
        _input = GetComponent<Player_InputHandler>();
        _movement = GetComponent<Player_Movement>();
        _stateMachine = GetComponent<Player_StateMachine>();
        _collisionDetector = GetComponent<Player_CollisionDetector>();
    }
    void Update()
    {
        
    }
}
