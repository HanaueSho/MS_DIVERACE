using UnityEngine;
using UnityEngine.InputSystem; // InputSystem本体(Gamepadなどの基本入力、全ての入力デバイスを統一的に扱う)
using UnityEngine.InputSystem.DualShock; // PlayStation系コントローラの拡張

public class ControllerReference : MonoBehaviour
{
    // Gamepadクラス(Xbox / PlayStationなど共通のゲームパッド抽象クラス)
    private Gamepad gamepad;

    void Start()
    {
        
    }

    void Update()
    {
        // 現在接続されているゲームパッドを取得
        // Gamepadクラスを使うことで、XboxやSwitch Proコンでも動くようになる
        gamepad = Gamepad.current;
        if (gamepad == null) return;

        // =========================
        // ■ スティック入力（Vector2）
        // =========================
        Vector2 leftStick = gamepad.leftStick.ReadValue();   // 左スティック
        Vector2 rightStick = gamepad.rightStick.ReadValue(); // 右スティック

        // =========================
        // ■ 十字キー（Vector2）
        // =========================
        Vector2 dpad = gamepad.dpad.ReadValue();

        // =========================
        // ■ トリガー入力（float: 0.0 ～ 1.0）
        // =========================
        float l2 = gamepad.leftTrigger.ReadValue();  // L2
        float r2 = gamepad.rightTrigger.ReadValue(); // R2

        // =========================
        // ■ ボタン入力（bool推奨）
        // =========================
        bool cross = gamepad.buttonSouth.isPressed;      // ×
        bool circle = gamepad.buttonEast.isPressed;      // ○
        bool square = gamepad.buttonWest.isPressed;      // □
        bool triangle = gamepad.buttonNorth.isPressed;   // △

        bool l1 = gamepad.leftShoulder.isPressed;        //L1
        bool r1 = gamepad.rightShoulder.isPressed;       //R1

        bool l3 = gamepad.leftStickButton.isPressed;     //L3
        bool r3 = gamepad.rightStickButton.isPressed;    //R3

        bool share = gamepad.selectButton.isPressed;     //Share
        bool options = gamepad.startButton.isPressed;    //Option

        // =========================
        // ■ DualSense特有の処理
        // =========================
        // DualSenseとして扱う(PS5コントローラ専用機能を使う場合)
        var ds = Gamepad.current as DualSenseGamepadHID;
        if (ds == null) return;

        // =========================
        // ■ ボタン入力（bool推奨）
        // =========================

        bool touchpad = ds.touchpadButton.isPressed;    //タッチパッド
        bool psButton = ds.playStationButton.isPressed; //PSボタン

        // =========================
        // ■ コントローラー振動
        // =========================
        // 特定の入力を取得
        float value = ds.touchpadButton.ReadValue();
        // 低周波と高周波のモーターを回す (0.0 ～ 1.0)
        // 引数１：低周波モーター、引数２：高周波モーター
        ds.SetMotorSpeeds(value * 0.5f, value * 0.2f);

        // =========================
        // ■ ライトバーの色変更
        // =========================
        // 例：スティックの入力方向に応じてライトバーの色を変える
        Vector2 stick = ds.leftStick.ReadValue();
        Color lightColor = new Color(Mathf.Abs(stick.x), Mathf.Abs(stick.y), 1.0f);
        // ライトバーにカラーをセットする
        ds.SetLightBarColor(lightColor);

        // =========================
        // ■ 補足
        // =========================

        // 押された瞬間だけ取りたい場合
        if (ds.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("×ボタン押下");
        }

        // 離された瞬間だけ取りたい場合
        if (ds.buttonSouth.wasReleasedThisFrame)
        {
            Debug.Log("×ボタン離した");
        }
    }
}
