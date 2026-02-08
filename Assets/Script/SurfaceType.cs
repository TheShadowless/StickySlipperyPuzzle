using UnityEngine;

public enum SurfaceKind
{
    Normal,
    Slippery,
    Sticky
}

[RequireComponent(typeof(Collider2D))]
public class SurfaceType : MonoBehaviour
{
    [Header("เลือกประเภทของพื้น")]
    public SurfaceKind surfaceKind = SurfaceKind.Normal;
}

