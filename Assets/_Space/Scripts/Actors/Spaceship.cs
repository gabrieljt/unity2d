using UnityEngine;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(SpaceshipInputDequeuer),
    typeof(SpaceshipMovement)
)]
[RequireComponent(
    typeof(StepCounter)
)]
public class Spaceship : AActor
{
    [SerializeField]
    private SpriteRenderer renderer;

    [SerializeField]
    private SpaceshipInputDequeuer inputDequeuer;

    [SerializeField]
    private SpaceshipMovement movement;


    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();

        inputDequeuer = GetComponent<SpaceshipInputDequeuer>();
        inputDequeuer.InputsDequeued += OnInputsDequeued;

        movement = GetComponent<SpaceshipMovement>();
        movement.Moving += OnMoving;

    }

    private void OnInputsDequeued(Vector2 direction)
    {
        movement.Move(direction);
    }

    private void OnMoving(Vector2 direction)
    {
        if (direction != Vector2.zero && direction.x != 0)
        {
            renderer.flipX = direction.x > 0f;
        }
    }

    public override void Dispose()
    {
        inputDequeuer.InputsDequeued -= OnInputsDequeued;
        movement.Moving -= OnMoving;
    }
}