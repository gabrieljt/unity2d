using Game.Actor;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
	using System;
	using System.Collections;
	using System.Linq;

#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonActorSpawner))]
	public class MapDungeonActorSpawnerInspector : ALevelComponentInspector
	{
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapDungeon)
	)]
	public class MapDungeonActorSpawner : ALevelComponent
	{
		public ActorSpawnerData[] actorSpawnersData = new ActorSpawnerData[0];

		public Dictionary<ActorType, List<AActor>> spawnedActors = new Dictionary<ActorType, List<AActor>>();

		public int ActorSpawnersToSet
		{
			get
			{
				var actorSpawnersToSet = 0;
				foreach (var actorSpawnerDatum in actorSpawnersData)
				{
					actorSpawnersToSet += actorSpawnerDatum.Quantity;
				}

				return actorSpawnersToSet;
			}
		}

		private List<KeyValuePair<Tile, Vector2>> tilesAvailableToSpawn = new List<KeyValuePair<Tile, Vector2>>();

		[SerializeField]
		private MapDungeon mapDungeon;

		public MapDungeon MapDungeon { get { return mapDungeon; } }

		private void Awake()
		{
			mapDungeon = GetComponent<MapDungeon>();
		}

		public override void Build()
		{
			if (spawnedActors.Count == 0)
			{
				SetSpawnedActorsLists();
			}
			BuildActorSpawners();

			StartCoroutine(SetActorSpawnersPositionsCoroutine());
		}

		private void SetSpawnedActorsLists()
		{
			Debug.Assert(actorSpawnersData.Length > 0);
			if (actorSpawnersData.Length > 0)
			{
				foreach (var actorSpawnerDatum in actorSpawnersData)
				{
					actorSpawnerDatum.actorSpawners = new ActorSpawner[actorSpawnerDatum.Quantity];
					spawnedActors[actorSpawnerDatum.ActorType] = new List<AActor>(actorSpawnerDatum.Quantity);
				}
			}
		}

		private void BuildActorSpawners()
		{
			foreach (var actorSpawnerDatum in actorSpawnersData)
			{
				for (int i = 0; i < actorSpawnerDatum.Quantity; i++)
				{
					actorSpawnerDatum.actorSpawners[i] = BuildActorSpawner(actorSpawnerDatum.ActorType);
				}
			}
		}

		private ActorSpawner BuildActorSpawner(ActorType actorType)
		{
			var actorSpawner = gameObject.AddComponent<ActorSpawner>();
			actorSpawner.actorType = actorType;
			actorSpawner.Spawned += OnActorSpawned;
			actorSpawner.enabled = false;
			return actorSpawner;
		}

		private void OnActorSpawned(ActorSpawner actorSpawner, AActor spawnedActor)
		{
			Debug.LogWarning(spawnedActor.GetType() + " spawned");

			actorSpawner.Spawned -= OnActorSpawned;

			spawnedActor.transform.SetParent(transform);
			spawnedActors[actorSpawner.actorType].Add(spawnedActor);
		}

		private IEnumerator SetActorSpawnersPositionsCoroutine()
		{
			var actorSpawnersToSet = 0;
			SetActorSpawnersPositions(ref actorSpawnersToSet);

			yield return 0;

			if (actorSpawnersToSet == ActorSpawnersToSet)
			{
				EnableActorSpawners();
				Built(GetType());
			}
			else
			{
				Dispose();
				Build();
			}
		}

		private void SetActorSpawnersPositions(ref int actorSpawnersToSet)
		{
			tilesAvailableToSpawn = mapDungeon.Map.GetTilesOfTypeWithIndex(TileType.Floor);

			if (tilesAvailableToSpawn.Count < ActorSpawnersToSet)
			{
				var message = GetType() + " tilesAvailableToSpawn.Count < ActorSpawnersToSet";
				Debug.LogError(message);
				throw new Exception(message);
			}
			else
			{
				tilesAvailableToSpawn = tilesAvailableToSpawn.OrderBy(emp => Guid.NewGuid()).Take(ActorSpawnersToSet).ToList();

				foreach (var actorSpawnerDatum in actorSpawnersData)
				{
					for (int i = 0; i < actorSpawnerDatum.Quantity; i++)
					{
						var randomTileWithIndex = tilesAvailableToSpawn[UnityEngine.Random.Range(0, tilesAvailableToSpawn.Count)];
						actorSpawnerDatum.actorSpawners[i].position = randomTileWithIndex.Value + Vector2.one * 0.5f;
						tilesAvailableToSpawn.Remove(randomTileWithIndex);
						++actorSpawnersToSet;
					}
				}
			}
		}

		private void EnableActorSpawners()
		{
			foreach (var actorSpawnerDatum in actorSpawnersData)
			{
				foreach (var actorSpawner in actorSpawnerDatum.actorSpawners)
				{
					actorSpawner.enabled = true;
				}
			}
		}

		public void DestroyActorSpawners()
		{
			var actorSpawners = GetComponents<ActorSpawner>();
			for (int i = 0; i < actorSpawners.Length; i++)
			{
#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					Destroy(actorSpawners[i]);
				}
				else
				{
					DestroyImmediate(actorSpawners[i]);
				}
#else
				Destroy(actorSpawners[i]);
#endif
			}
		}

		private void ClearSpawnedActorsLists()
		{
			foreach (var actorsList in spawnedActors.Values)
			{
				foreach (var actor in actorsList)
				{
#if UNITY_EDITOR
					if (Application.isPlaying)
					{
						Destroy(actor.gameObject);
					}
					else
					{
						DestroyImmediate(actor.gameObject);
					}
#else
					Destroy(actor.gameObject);
#endif
				}
				actorsList.Clear();
			}
		}

		public override void Dispose()
		{
			StopCoroutine(SetActorSpawnersPositionsCoroutine());
			DestroyActorSpawners();
			ClearSpawnedActorsLists();
			spawnedActors.Clear();
			tilesAvailableToSpawn.Clear();
		}
	}
}