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

    private EncounterManager encounterManager;
    private bool isMoving = false;

    private void Start()
    {
        encounterManager = EncounterManager.Instance;
        isMoving = false;

        Debug.Log("Movement iniciado. EncounterManager: " +
            (encounterManager != null ? "Encontrado" : "NO encontrado"));
    }

    private void OnEnable()
    {
        isMoving = false;
    }

    private void Update()
    {
        // Recuperar la referencia si todavía no existe.
        if (encounterManager == null)
            encounterManager = EncounterManager.Instance;

        if (isMoving)
            return;

        if (encounterManager != null &&
            encounterManager.IsEncounterStarted)
        {
            return;
        }

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

    public void UnlockMovement()
    {
        StopAllCoroutines();

        isMoving = false;
        encounterManager = EncounterManager.Instance;

        Debug.Log("MOVIMIENTO DESBLOQUEADO");
    }

    private void TryMove(Vector3 direction)
    {
        Vector3 worldDirection = transform.rotation * direction;

        Vector3 targetPosition =
            transform.position + worldDirection * moveDistance;

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

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

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

        if (encounterManager != null)
            encounterManager.RegisterStep();

        yield return new WaitForSeconds(actionDelay);

        isMoving = false;
    }

    private IEnumerator Rotate(float angle)
    {
        isMoving = true;

        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation =
            startRotation * Quaternion.Euler(0f, angle, 0f);

        float elapsed = 0f;

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

        yield return new WaitForSeconds(actionDelay);

        isMoving = false;
    }
}