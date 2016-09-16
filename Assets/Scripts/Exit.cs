using System;
using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(CircleCollider2D)
)]
public class Exit : MonoBehaviour
{
	private CircleCollider2D circleCollider2D;

	public Action Reached = delegate { };

	private void Awake()
	{
		circleCollider2D = GetComponent<CircleCollider2D>();
		circleCollider2D.isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<Character>())
		{
			Debug.LogWarning("Exit Reached");
			Reached();
		}
	}
}