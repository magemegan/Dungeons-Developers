using UnityEngine;

public class AgentSensor : MonoBehaviour
{
    public float detectionDistance = 1f;
    public LayerMask obstacleLayer;

    public bool IsBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            detectionDistance,
            obstacleLayer
        );

        return hit.collider != null;
    }

    void Update()
    {
        Debug.DrawRay(
            transform.position,
            Vector2.up * detectionDistance,
            IsBlocked(Vector2.up) ? Color.red : Color.green
        );

        Debug.DrawRay(
            transform.position,
            Vector2.down * detectionDistance,
            IsBlocked(Vector2.down) ? Color.red : Color.green
        );

        Debug.DrawRay(
            transform.position,
            Vector2.left * detectionDistance,
            IsBlocked(Vector2.left) ? Color.red : Color.green
        );

        Debug.DrawRay(
            transform.position,
            Vector2.right * detectionDistance,
            IsBlocked(Vector2.right) ? Color.red : Color.green
        );
    }
}