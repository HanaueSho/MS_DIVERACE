using UnityEngine;

/// <summary>
/// 各層シーンの最上位に1つだけ配置するマーカー。
/// シーン内の地形・ギミック・SkyShaftTrigger等は、すべてこの下の子として配置する。
/// このTransformを動かすことで、ステージ全体をワールド空間上で一括平行移動できる。
/// </summary>
public class StageRoot : MonoBehaviour
{
    private void Awake()
    {
        if (LayerSceneManager.Instance == null)
        {
            Debug.LogWarning($"[StageRoot] {name} : LayerSceneManagerが見つかりません");
            return;
        }

        LayerSceneManager.Instance.RegisterStageRoot(transform);
    }
}
