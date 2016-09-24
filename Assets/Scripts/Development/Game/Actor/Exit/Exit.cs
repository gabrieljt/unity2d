using System;
using UnityEngine;

namespace Game.Actor
{
	[RequireComponent(
		typeof(SpriteRenderer),
		typeof(CircleCollider2D)
	)]
	public class Exit : AActor
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

		public override void Enable()
		{
			gameObject.SetActive(true);
		}

		public override void Disable()
		{
			gameObject.SetActive(false);
		}

		public override void Dispose()
		{
		}
	}
}