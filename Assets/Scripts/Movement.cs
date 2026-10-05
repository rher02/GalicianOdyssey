using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveDuration = 0.25f;

    [Header("Rotation")]
    [SerializeField] private float rotationDuration = 0.25f;

    [Header("Timing")]
    [SerializeField] private float actionDelay = 0.08f;

    [Header("Collision")]
    [SerializeField] private LayerMask wallLayer;

    private bool isMoving = false;

    void Update()
    {
        if (isMoving)
            return;

        if (Input.GetKey(KeyCode.W))
        {
            TryMove(Vector3.forward);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            TryMove(Vector3.back);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            TryMove(Vector3.left);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            TryMove(Vector3.right);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            StartCoroutine(Rotate(-90f));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            StartCoroutine(Rotate(90f));
        }
    }

    void TryMove(Vector3 direction)
    {
        Vector3 worldDirection =
            transform.rotation * direction;

        Vector3 targetPosition =
            transform.position +
            worldDirection * moveDistance;

        // Check for a wall.
        if (Physics.Raycast(
            transform.position,
            worldDirection,
            moveDistance,
            wallLayer))
        {
            return;
        }

        StartCoroutine(MoveTo(targetPosition));
    }

    IEnumerator MoveTo(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;

        float elapsed = 0f;

        // Smooth movement.
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        transform.position = targetPosition;

        // Small pause before the next action.
        yield return new WaitForSeconds(actionDelay);

        isMoving = false;
    }

    IEnumerator Rotate(float angle)
    {
        isMoving = true;

        Quaternion startRotation =
            transform.rotation;

        Quaternion targetRotation =
            startRotation *
            Quaternion.Euler(0f, angle, 0f);

        float elapsed = 0f;

        // Smooth rotation.
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / rotationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        transform.rotation = targetRotation;

        // Small pause before the next action.
        yield return new WaitForSeconds(actionDelay);

        isMoving = false;
    }
}