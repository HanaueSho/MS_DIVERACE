/*
    Player_CollisionDetector
    20261007  hanaue sho
 */
using UnityEngine;

public enum PlayerImpactType
{
    None,
    Top,
    Side,
}
public struct PlayerImpactInfo
{
    public PlayerImpactType ImpactType;

    public Vector3 Point;
    public Vector3 Normal;

    public Collider Collider;

    public float Distance;
}

public class Player_CollisionDetector : MonoBehaviour
{
    // --------------------------------------------------
    // ----- Collider -----
    // --------------------------------------------------
    [Header("Collider")]
    [SerializeField] private BoxCollider _boxCollider;

    // --------------------------------------------------
    // ----- Collision Parameter -----
    // --------------------------------------------------
    [Header("Collision Parameter")]
    [SerializeField] private float _skinWidth = 0.02f;
    [SerializeField, Range(-1.0f, 1.0f)] private float _topNormalThreshold = 0.6f;


    // --------------------------------------------------
    // ----- Unity Events -----
    // --------------------------------------------------
    private void Awake()
    {
        if (_boxCollider == null)
        {
            _boxCollider = GetComponentInChildren<BoxCollider>();
        }
        if (_boxCollider == null)
        {
            Debug.LogError("[Error] BoxCollider Not Found !!");
        }
    }

    // --------------------------------------------------
    // ----- Check Collision -----
    // --------------------------------------------------
    public bool CheckCollision(Vector3 displacement, out PlayerImpactInfo impactInfo)
    {
        impactInfo = default;

        float distance = displacement.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            return false;
        }

        Vector3 direction = displacement.normalized;

        // BoxCollider の中心をワールド座標へ変換
        Vector3 center = _boxCollider.transform.TransformPoint(_boxCollider.center);

        // Scale 込みの HalfExtents
        Vector3 lossyScale = _boxCollider.transform.lossyScale;
        Vector3 halfExtents = _boxCollider.size * 0.5f;
        halfExtents = Vector3.Scale(halfExtents, new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z)));

        Quaternion orientation = _boxCollider.transform.rotation;

        // BoxCast
        RaycastHit[] hits = Physics.BoxCastAll(
                                               center,
                                               halfExtents,
                                               direction,
                                               orientation,
                                               distance + _skinWidth,
                                               ~0,
                                               QueryTriggerInteraction.Ignore
                                               );
        RaycastHit? nearestHit = null;
        float nearestDistance = float.MaxValue;
        
        foreach (RaycastHit hit in hits)
        {

            // 自分自身は無視
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (!IsTargetCollision(hit.collider))
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestHit = hit;
            }
        }
        if (!nearestHit.HasValue)
        {
            return false;
        }
        RaycastHit targetHit = nearestHit.Value;

        // PlayerImpactType の構築
        impactInfo = new PlayerImpactInfo
        {
            ImpactType = JudgeImpactType(targetHit.normal),

            Point = targetHit.point,
            Normal = targetHit.normal,

            Collider = targetHit.collider,

            Distance = targetHit.distance,
        };

        return true;
    }
    private bool IsTargetCollision(Collider target)
    {
        return
            target.CompareTag("Obstacle") ||
            target.CompareTag("Terrain");
    }
    private PlayerImpactType JudgeImpactType(Vector3 normal)
    {
        float upDot = Vector3.Dot(normal.normalized, Vector3.up);
        if (upDot >= _topNormalThreshold)
        {
            return PlayerImpactType.Top;
        }

        return PlayerImpactType.Side;
    }

    // --------------------------------------------------
    // ----- ResolvePenetration -----
    // --------------------------------------------------
    public bool ResolvePenetration()
    {
        // BoxCollider の中心をワールド座標へ変換
        Vector3 center = _boxCollider.transform.TransformPoint(_boxCollider.center);

        // Scale 込みの HalfExtents
        Vector3 lossyScale = _boxCollider.transform.lossyScale;
        Vector3 halfExtents = _boxCollider.size * 0.5f;
        halfExtents = Vector3.Scale(halfExtents, new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z)));
        Quaternion orientation = _boxCollider.transform.rotation;

        // 現在の BoxCollider と重なっている Collider を取得
        Collider[] overlaps = Physics.OverlapBox(center, halfExtents, orientation, ~0, QueryTriggerInteraction.Ignore);

        bool resolved = false;
        foreach (Collider other in overlaps)
        {
            if (other.transform.IsChildOf(transform))
            {
                continue;
            }

            if (!IsTargetCollision(other))
            {
                continue;
            }

            // めり込み量と押し戻し方向を取得
            bool isPenetrating = Physics.ComputePenetration(
                                                            _boxCollider,
                                                            _boxCollider.transform.position,
                                                            _boxCollider.transform.rotation,
                                                            other,
                                                            other.transform.position,
                                                            other.transform.rotation,
                                                            out Vector3 pushDirection,
                                                            out float pushDistance
                                                           );
            if (!isPenetrating)
            {
                continue;
            }

            // 最低限めり込みを解消するだけ押し戻す
            transform.position += pushDirection * (pushDistance + _skinWidth);
            resolved = true;
        }
        return resolved;
    }

}
