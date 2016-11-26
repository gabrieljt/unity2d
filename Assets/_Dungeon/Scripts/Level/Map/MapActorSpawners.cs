using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(
	typeof(Map)
)]
public class MapActorSpawners : ALevelComponent
{
	public ActorSpawnerData[] spawnersData = new ActorSpawnerData[0];

	public Dictionary<ActorType, List<AActor>> actorsContainers = new Dictionary<ActorType, List<AActor>>();

	public int SpawnersToSet
	{
		get
		{
			var spawnersToSet = 0;
			foreach (var spawnerDatum in spawnersData)
			{
				spawnersToSet += spawnerDatum.Quantity;
			}

			return spawnersToSet;
		}
	}

	private List<KeyValuePair<Tile, Vector2>> availableTiles = new List<KeyValuePair<Tile, Vector2>>();

	[SerializeField]
	private Map map;

	private void Awake()
	{
		map = GetComponent<Map>();
	}

	public override void Build()
	{
		if (actorsContainers.Count == 0)
		{
			SetActorsContainers();
		}
		BuildSpawners();

		StartCoroutine(SetSpawnersPositionsCoroutine());
	}

	private void SetActorsContainers()
	{
		Debug.Assert(spawnersData.Length > 0);
		if (spawnersData.Length > 0)
		{
			foreach (var spawnerDatum in spawnersData)
			{
				spawnerDatum.spawners = new ActorSpawner[spawnerDatum.Quantity];
				actorsContainers[spawnerDatum.Type] = new List<AActor>(spawnerDatum.Quantity);
			}
		}
	}

	private void BuildSpawners()
	{
		foreach (var spawnerDatum in spawnersData)
		{
			for (int i = 0; i < spawnerDatum.Quantity; i++)
			{
				spawnerDatum.spawners[i] = BuildSpawner(spawnerDatum.Type);
			}
		}
	}

	private ActorSpawner BuildSpawner(ActorType type)
	{
		var spawner = gameObject.AddComponent<ActorSpawner>();
		spawner.type = type;
		spawner.Performed += OnSpawnerPerformed;
		spawner.enabled = false;
		return spawner;
	}

	private void OnSpawnerPerformed(ActorSpawner spawner, AActor actor)
	{
		spawner.Performed -= OnSpawnerPerformed;

		actor.Destroyed += OnActorDestroyed;
		actor.transform.SetParent(transform);
		actorsContainers[spawner.type].Add(actor);
	}

	private void OnActorDestroyed(IDestroyable destroyedComponent)
	{
		var destroyedActor = destroyedComponent as AActor;
		var actorType = destroyedActor.tag.ToEnum<ActorType>();
		var actor = actorsContainers[actorType].Find(containedActor => containedActor == (destroyedActor));
		Debug.Assert(actor);
		actor.Destroyed -= OnActorDestroyed;
		actorsContainers[actorType].Remove(destroyedActor as AActor);
	}

	private IEnumerator SetSpawnersPositionsCoroutine()
	{
		var spawnersToSet = 0;
		SetSpawnersPositions(ref spawnersToSet);

		yield return 0;

		if (spawnersToSet == SpawnersToSet)
		{
			EnableSpawners();
			Built(GetType());
		}
		else
		{
			Dispose();
			Build();
		}
	}

	private void SetSpawnersPositions(ref int spawnersToSet)
	{
		availableTiles = map.GetTilesWithMapIndexes(TileType.Floor);

		if (availableTiles.Count < SpawnersToSet)
		{
			var message = GetType() + " tilesAvailableToSpawn.Count < ActorSpawnersToSet";
			Debug.LogError(message);
			throw new Exception(message);
		}
		else
		{
			availableTiles = availableTiles.OrderBy(avialbleTile => Guid.NewGuid()).Take(SpawnersToSet).ToList();

			foreach (var spawnerDatum in spawnersData)
			{
				for (int i = 0; i < spawnerDatum.Quantity; i++)
				{
					var randomTileWithIndex = availableTiles[UnityEngine.Random.Range(0, availableTiles.Count)];
					spawnerDatum.spawners[i].position = randomTileWithIndex.Value + Vector2.one * 0.5f;
					availableTiles.Remove(randomTileWithIndex);
					++spawnersToSet;
				}
			}
		}
	}

	private void EnableSpawners()
	{
		foreach (var spawnerDatum in spawnersData)
		{
			foreach (var spawner in spawnerDatum.spawners)
			{
				spawner.enabled = true;
			}
		}
	}

	public void DestroySpawners()
	{
		var spawners = GetComponents<ActorSpawner>();
		for (int i = 0; i < spawners.Length; i++)
		{
			Destroy(spawners[i]);
		}
	}

	private void ClearActorsContainer()
	{
		foreach (var actorsContainer in actorsContainers.Values)
		{
			foreach (var actor in actorsContainer)
			{
				actor.Destroyed -= OnActorDestroyed;
				Destroy(actor.gameObject);
			}
			actorsContainer.Clear();
		}
	}

	public override void Dispose()
	{
		StopCoroutine(SetSpawnersPositionsCoroutine());
		DestroySpawners();
		ClearActorsContainer();
		actorsContainers.Clear();
		availableTiles.Clear();
	}
}