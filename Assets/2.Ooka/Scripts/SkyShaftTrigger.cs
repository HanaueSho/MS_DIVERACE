using UnityEngine;

/// <summary>
/// スカイシャフトに配置する検知用トリガー。
///
/// Role.Exit  : 現在レイヤーの退出点。プレイヤーが触れたら次レイヤーへの遷移を開始する
/// Role.Entry : 新レイヤーの進入点。ロード後のプレイヤー配置先になり、
///              プレイヤーがここを通過(退出)すると旧レイヤーのアンロードが走る
///
/// 各層のマップが未完成のうちは、このコンポーネントを付けた空のGameObjectを
/// 仮の位置に置いておけばよい。位置調整はTransformを動かすだけでよく、
/// コードの変更は不要。
/// </summary>
[RequireComponent(typeof(Collider))]
public class SkyShaftTrigger : MonoBehaviour
{
    public enum Role { Exit, Entry }

    [SerializeField] private Role role;
    [SerializeField] private string playerTag = "Player";

    private bool _hasExited; // Exit側の多重発火防止

    private void Awake()
    {
        if (role != Role.Entry) return;

        if (LayerSceneManager.Instance == null)
        {
            Debug.LogWarning($"[SkyShaftTrigger] {name} : LayerSceneManagerが見つかりません（Scene_Level_Mainがロードされているか確認）");
            return;
        }

        // 自分のシーンがロード・活性化されたタイミングで、配置先として登録する
        LayerSceneManager.Instance.RegisterEntryPoint(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (role != Role.Exit) return;
        if (_hasExited) return;
        if (!other.CompareTag(playerTag)) return;

        _hasExited = true;
        LayerSceneManager.Instance.BeginTransition();
    }

    private void OnTriggerExit(Collider other)
    {
        if (role != Role.Entry) return;
        if (!other.CompareTag(playerTag)) return;

        LayerSceneManager.Instance.CompleteTransition();
    }
}
