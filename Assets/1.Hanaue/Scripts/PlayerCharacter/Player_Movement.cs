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
    // ----- Parameter Property -----
    // --------------------------------------------------
    [SerializeField] private float _moveSpeed = 5.0f;


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
    public void Move(Vector2 input, float deltaTime)
    {
        Vector3 moveDirection = new Vector3(input.x, 0.0f, input.y);
        transform.position += moveDirection * _moveSpeed * deltaTime;
    }

}
