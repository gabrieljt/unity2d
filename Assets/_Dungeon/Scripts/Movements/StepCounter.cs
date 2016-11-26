using System;
using UnityEngine;

[RequireComponent(
    typeof(IMoveable)
)]
public class StepCounter : MonoBehaviour
{
    [SerializeField]
    private Vector2 previousPosition;

    public Vector2 PreviousPositionDirection { get { return (previousPosition - movement.Position).normalized; } }

    public float DistanceToPreviousPosition { get { return Vector2.Distance(movement.Position, previousPosition); } }

    [SerializeField]
    private float stepSize = 1f;

    public float StepSize { get { return stepSize; } }

    [SerializeField]
    private int steps = 0;

    public Action StepTaken = delegate { };

    [SerializeField]
    private IMoveable movement;

    private bool started;

    private void Awake()
    {
        movement = GetComponent<IMoveable>();
    }

    private void Start()
    {
        started = true;
        previousPosition = movement.Position;
    }

    private void FixedUpdate()
    {
        if (DistanceToPreviousPosition >= stepSize)
        {
            ++steps;
            Debug.DrawRay(movement.Position, PreviousPositionDirection * DistanceToPreviousPosition, Color.red, 3f);
            previousPosition = movement.Position + PreviousPositionDirection * (DistanceToPreviousPosition - stepSize);
            Debug.DrawRay(movement.Position, PreviousPositionDirection * DistanceToPreviousPosition, Color.white, 3f);
            StepTaken();
        }
    }

    private void OnDrawGizmos()
    {
        if (started)
        {
            var gizmosColor = Gizmos.color;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(previousPosition, DistanceToPreviousPosition);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(previousPosition, stepSize);

            Debug.DrawRay(movement.Position, PreviousPositionDirection * DistanceToPreviousPosition, Color.green);

            Gizmos.color = gizmosColor;
        }
    }
}