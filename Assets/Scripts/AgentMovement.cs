using UnityEngine;
using System.Collections;


public class AgentMovement : MonoBehaviour
{
    private bool isMoving;
    private float timeToMove = 0.2f;

    public bool IsMoving => isMoving;

    public void Move(Vector3 direction)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveAgent(direction));
        }
    }

    private IEnumerator MoveAgent(Vector3 direction)
    {
        isMoving = true;

        Vector3 origPos = transform.position;
        Vector3 targetPos = origPos + direction;

        float elapsedTime = 0f;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(
                origPos,
                targetPos,
                elapsedTime / timeToMove
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }
}
