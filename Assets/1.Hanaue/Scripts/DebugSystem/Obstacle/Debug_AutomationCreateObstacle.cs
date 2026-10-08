/*
    Debug_AutomationCreateObstacle.cs
    20261008  hanaue sho
    オブジェクトプレファブ、垂直距離、間隔、個数、水平範囲を指定してランダム配置
 */
using UnityEngine;

public class Debug_AutomationCreateObstacle : MonoBehaviour
{
    // --------------------------------------------------
    // ----- Obstacle Prefab -----
    // --------------------------------------------------
    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject _obstaclePrefab;

    // --------------------------------------------------
    // ----- Parameters -----
    // --------------------------------------------------
    [Header("Parameters")]
    [SerializeField] private float _verticalDistance = 100.0f;
    [SerializeField] private float _radius = 20.0f;
    [SerializeField] private float _intervalDistance = 10.0f;
    [SerializeField] private int _obstacleCount = 10;

    // --------------------------------------------------
    // ----- Random -----
    // --------------------------------------------------
    [Header("Random")]
    [SerializeField] private int _seed = 0;


    // --------------------------------------------------
    // ----- Unity Events -----
    // --------------------------------------------------
    private void Start()
    {
        // チェック
        if (_obstaclePrefab == null)
        {
            return;
        }
        if (_obstacleCount == 0)
        {
            return;
        }

        // ランダムシード設定
        Random.InitState(_seed);

        // 生成処理
        for (
             float verticalDistance = _intervalDistance;
             verticalDistance <= _verticalDistance;
             verticalDistance += _intervalDistance
            )
        {
            // 1層に配置する障害物
            for (int i = 0; i < _obstacleCount; i++)
            {
                // XZ平面上の半径内からランダムな位置を取得
                Vector2 randomPosition = Random.insideUnitCircle * _radius;

                // ローカル座標で生成位置を決定
                Vector3 localPosition = new Vector3(
                    randomPosition.x,
                    -verticalDistance,
                    randomPosition.y
                );

                // プレファブ生成
                GameObject obstacle = Instantiate(
                    _obstaclePrefab,
                    transform
                );

                // 親オブジェクトを基準とした位置に設定
                obstacle.transform.localPosition = localPosition;
                obstacle.transform.localRotation = _obstaclePrefab.transform.localRotation;
            }
        }



    }
}
