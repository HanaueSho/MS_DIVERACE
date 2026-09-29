using UnityEngine;

public class LevelSceneInfo : MonoBehaviour
{
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;

    public Transform TopPoint => topPoint;
    public Transform BottomPoint => bottomPoint;
}