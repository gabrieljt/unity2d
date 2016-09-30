using System;
using UnityEngine;

namespace Game.Actor
{
	[Serializable]
	public class ActorSpawnerData
	{
		[SerializeField]
		private ActorType actorType;

		public ActorType ActorType { get { return actorType; } }

		[SerializeField]
		[Range(1, 99)]
		private int quantity;

		public int Quantity { get { return quantity; } }

		public ActorSpawner[] actorSpawners = new ActorSpawner[0];
		private ActorType player;
		private int v;

		public ActorSpawnerData(ActorType actorType, int quantity)
		{
			this.actorType = actorType;
			this.quantity = quantity;
		}
	}
}