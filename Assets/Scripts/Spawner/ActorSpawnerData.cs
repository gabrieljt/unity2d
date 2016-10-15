using System;
using UnityEngine;

[Serializable]
public class ActorSpawnerData
{
	[SerializeField]
	private ActorType type;

	public ActorType Type { get { return type; } }

	[SerializeField]
	[Range(1, 99)]
	private int quantity;

	public int Quantity { get { return quantity; } }

	public ActorSpawner[] spawners = new ActorSpawner[0];

	public ActorSpawnerData(ActorType type, int quantity)
	{
		this.type = type;
		this.quantity = quantity;
	}
}