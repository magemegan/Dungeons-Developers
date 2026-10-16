using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementTemplate : MonoBehaviour
{
    private bool isMoving;
    private Vector3 origPos, targetPos;
    private float timeToMove = 0.2f;

    void Update()
    {
        if (Keyboard.current.wKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3.up));

        if (Keyboard.current.aKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3.left));

        if (Keyboard.current.sKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3.down));

        if (Keyboard.current.dKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3.right));
    }
    

    private IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;

        float elapsedTime = 0;

        origPos = transform.position;
        targetPos = origPos + direction;

        while(elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }
}
