using UnityEngine;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(CharacterInputDequeuer),
    typeof(CharacterMovement)
)]
[RequireComponent(
    typeof(StepCounter)
)]
public class Character : AActor
{
    [SerializeField]
    private SpriteRenderer renderer;

    [SerializeField]
    private CharacterInputDequeuer inputDequeuer;

    [SerializeField]
    private CharacterMovement movement;

    [SerializeField]
    private StepCounter stepCounter;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();

        inputDequeuer = GetComponent<CharacterInputDequeuer>();
        inputDequeuer.InputsDequeued += OnInputsDequeued;

        movement = GetComponent<CharacterMovement>();
        movement.Moving += OnMoving;

        stepCounter = GetComponent<StepCounter>();
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

    public override void Enable()
    {
        base.Enable();
        gameObject.SetActive(true);
    }

    public override void Disable()
    {
        base.Disable();
        gameObject.SetActive(false);
    }

    public override void Dispose()
    {
        inputDequeuer.InputsDequeued -= OnInputsDequeued;
        movement.Moving -= OnMoving;
    }
}