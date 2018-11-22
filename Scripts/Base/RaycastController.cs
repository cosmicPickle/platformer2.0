using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RaycastController : MonoBehaviour
{
    public const float skinWidth = .03f;
    const float dstBetweenRays = .25f;

    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public int verticalRayCount;

    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public Collider2D ctrlCollider;
    public RaycastOrigins origins;

    protected virtual void Awake()
    {
        ctrlCollider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        CalculateRaySpacing();
    }


    public void UpdateRaycastOrigins()
    {
        Bounds bounds = ctrlCollider.bounds;
        bounds.Expand(skinWidth * -2);

        origins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        origins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        origins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        origins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing()
    {
        Bounds bounds = ctrlCollider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;


        horizontalRayCount = Mathf.Clamp(Mathf.RoundToInt(boundsHeight / dstBetweenRays), 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(Mathf.RoundToInt(boundsWidth / dstBetweenRays), 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
