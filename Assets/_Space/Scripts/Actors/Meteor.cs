using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(MeteorMovement)
)]
public class Meteor : AActor
{
	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private MeteorMovement movement;

	private void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();
		movement = GetComponent<MeteorMovement>();
	}

	public override void Dispose()
	{
	}
}