/*
    PlayerMovement_ParameterData
    20261008  hanaue sho
    プレイヤーキャラクターの基礎パラメータデータ
 */
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerMovement_ParameterData", menuName = "Player/Movement Parameter Data")]
public class PlayerMovement_ParameterData : ScriptableObject
{
    // --------------------------------------------------
    // ----- Fall -----
    // --------------------------------------------------
    [Header("Fall")]
    [Tooltip("重力加速度")]
    public float GravityAcceleration = 30.0f;

    [Tooltip("水平方向終端速度")]
    public float HorizontalTerminalFallSpeed = 20.0f;
    [Tooltip("鉛直方向終端速度")]
    public float VerticalTerminalFallSpeed = 50.0f;

    // --------------------------------------------------
    // ----- HorizontalMove -----
    // --------------------------------------------------
    [Header("HorizontalMove")]
    [Tooltip("水平頭方向加速度")]
    public float ForwardAcceleration = 20.0f;
    [Tooltip("水平左右方向加速度")]
    public float SideAcceleration = 15.0f;
    [Tooltip("水平足方向加速度")]
    public float BackAcceleration = 10.0f;

    // --------------------------------------------------
    // ----- Resistance -----
    // --------------------------------------------------
    [Header("Resistance")]
    [Tooltip("水平方向抗力")]
    public float HorizontalDrag = 2.0f;
    
    // --------------------------------------------------
    // ----- Rotation -----
    // --------------------------------------------------
    [Header("Rotation")]
    [Tooltip("姿勢回転速度")]
    public float YawSpeed = 90.0f;

    // --------------------------------------------------
    // ----- PostureControl -----
    // --------------------------------------------------
    [Header("PostureControl")]
    [Tooltip("姿勢による制御のしやすさ")]
    public float HorizontalPostureControl = 1.0f;
    public float VerticalPostureControl = 0.4f;

}
