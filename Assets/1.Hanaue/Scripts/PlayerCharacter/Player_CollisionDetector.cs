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
    // ----- Collision Parameter -----
    // --------------------------------------------------
    private void Awake()
    {
        if (_boxCollider == null)
        {
            _boxCollider = GetComponentInChildren<BoxCollider>();
        }
        if (_boxCollider == null)
        {
            Debug.LogError("BoxCollider Not Found !!");
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

            Collider = targetHit.collider
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

}
