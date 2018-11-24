using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : RaycastController 
{

    [Header("General Settings")]
    public float movementSpeed;
    public float faceDirection;
    public float maxSlopeAngle = 70;

    [HideInInspector]
    public float movementDirection;

    [Header("Jump Settings")]
    public float maxNumberAerialJumps = 1;
    public float maxJumpForce;
    public float minJumpForce;
    private float aerialJumps;

    [Header("Wall slide Settings")]
    public float wallSlideSpeedMax = 5;
    public float wallStickTme = .25f;
    private float timeToWallUnstick;

    [Header("Wall jump settings")]
    public Vector2 wallLeap;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallFall;
    

    [Header("Dash Settings")]
    public float dashForce;
    public float dashDuration;
    public float dashCooldown = .2f;
    [HideInInspector]
    public float lastDash;
    
    public CollisionInfo collisions;

    Rigidbody2D rb2d;
    Vector2 currentVelocity;

    

    protected override void Awake()
    {
        base.Awake();
        collisions = new CollisionInfo();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        collisions.Reset();
        UpdateRaycastOrigins();

        currentVelocity = rb2d.velocity;
        UpdateCollisions();

        if(rb2d.IsSleeping())
        {
            rb2d.WakeUp();
        }
    }

    public Vector2 HandleWallSliding(float directionX)
    {
        float wallDirX = (collisions.left) ? -1 : 1;
        Vector2 newVelocity = new Vector2(0, -wallSlideSpeedMax);

        if ((collisions.left || collisions.right) && !collisions.below)
        {
            if (timeToWallUnstick > 0)
            {
                if (directionX != wallDirX && directionX != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTme;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTme;
                return new Vector2(-wallDirX * wallFall.x, wallFall.y);
            }
        }

        return newVelocity;
    } 

    public Vector2 GetWallJumpVelocity()
    {
        float wallDirection = collisions.left ? -1 : 1;
        if(collisions.left || collisions.right && !collisions.below)
        {
            Vector2 force = Vector2.zero;

            if(movementDirection == 0)
            {
                force = wallJumpOff;
            }
            else if(movementDirection == wallDirection)
            {
                force = wallJumpClimb;
            }
            else
            {
                force = wallLeap;
            }

            force.x *= -wallDirection;
            return force;
        }

        

        return Vector2.zero;
    }

    public Vector2 GetJumpVelocity()
    {
        if (collisions.below)
        {
            return Vector2.up * maxJumpForce;
        }

        if (!collisions.left && !collisions.right && !collisions.below)
        {
            if (aerialJumps < maxNumberAerialJumps)
            {
                aerialJumps++;
                return Vector2.up * maxJumpForce;
            }
        }

        return Vector2.zero;
    }

    private void UpdateCollisions()
    {
        DetectHorizontalCollisions(-1);
        DetectHorizontalCollisions(1);
        DetectVerticalCollisions(-1);
        DetectVerticalCollisions(1);
    }

    private void DetectHorizontalCollisions(float directionX)
    {
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 raycastOrigin = directionX < 0 ? origins.bottomLeft : origins.bottomRight;
            raycastOrigin += i * horizontalRaySpacing * Vector2.up;

            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.right * directionX, 2 * skinWidth, Physics2D.GetLayerCollisionMask(gameObject.layer));

            if (hit)
            {
                collisions.left = directionX < 0;
                collisions.right = directionX > 0;
                aerialJumps = 0;
            }
        }
    }

    private void DetectVerticalCollisions(float directionY)
    {
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 raycastOrigin = directionY < 0 ? origins.bottomLeft : origins.topLeft;
            raycastOrigin += i * verticalRaySpacing * Vector2.right;

            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.up * directionY, 2 * skinWidth, Physics2D.GetLayerCollisionMask(gameObject.layer));

            if (hit)
            {
                
                if (directionY > 0)
                {
                    collisions.above = true;
                }

                if(directionY < 0)
                {
                    
                    float normalAngle = 90 - Vector2.Angle(hit.normal, Vector2.left);
                    if (normalAngle >= 0 && normalAngle < maxSlopeAngle)
                    {
                        collisions.below = true;
                        aerialJumps = 0;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (collisions.above)
        {
            Gizmos.DrawLine(origins.topLeft, origins.topRight);
        }

        if (collisions.below)
        {
            Gizmos.DrawLine(origins.bottomLeft, origins.bottomRight);
        }

        if (collisions.left)
        {
            Gizmos.DrawLine(origins.bottomLeft, origins.topLeft);
        }

        if (collisions.right)
        {
            Gizmos.DrawLine(origins.bottomRight, origins.topRight);
        }
    }

    public struct CollisionInfo
    {
        public bool below, above;
        public bool left, right;

        public void Reset()
        {
            below = above = false;
            left = right = false;
        }

        
    }
}
