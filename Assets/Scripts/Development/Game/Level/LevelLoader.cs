using Game.Actor;
using Game.Level.Tiled;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(LevelLoader))]
	public class LevelLoaderInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Create Level"))
			{
				var levelLoader = (LevelLoader)target;
				GameObject loadedLevel;
				if (LevelLoader.LoadLevelStatus.Created == levelLoader.CreateLevel(
					levelLoader.Index, out loadedLevel, false)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Recreate Level"))
			{
				var levelLoader = (LevelLoader)target;

				GameObject loadedLevel;
				if (LevelLoader.LoadLevelStatus.Created == levelLoader.CreateLevel(
					levelLoader.Index, out loadedLevel, true)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Load Level"))
			{
				var levelLoader = (LevelLoader)target;
				levelLoader.LoadLevel(levelLoader.Index);
			}

			if (GUILayout.Button("Dispose"))
			{
				var levelLoader = (LevelLoader)target;

				levelLoader.Dispose();
			}
		}
	}

#endif

	public class LevelLoader : MonoBehaviour, IDestroyable, IDisposable
	{
		public enum LoadLevelStatus
		{
			Created,
			Loaded,
			Failed,
			Destroyed,
		}

		[SerializeField]
		private GameObject levelPrefab;

		public GameObject LevelPrefab { get { return levelPrefab; } }

		[SerializeField]
		private GameObject activeLevel;

		[SerializeField]
		private int index;

		public int Index { get { return index; } }

		[SerializeField]
		private int[] levelIndexes;

		private static Dictionary<int, GameObject> levels = new Dictionary<int, GameObject>();

		public Action<int, GameObject> LevelLoaded = delegate { };

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		private void Awake()
		{
			Debug.Assert(levelPrefab);
			// TODO: LevelState
			Debug.Assert(levelPrefab.GetComponent<Map>());

			gameObject.isStatic = true;
		}

		private void Start()
		{
			Clear();
		}

		public LoadLevelStatus CreateLevel(int index, out GameObject loadedLevel, bool overwrite = false)
		{
			var status = LoadLevelStatus.Failed;
			loadedLevel = null;

			if (!levels.ContainsKey(index))
			{
				SetStatusCreated(index, out loadedLevel, ref status);
			}
			else if (overwrite)
			{
				DestroyLevel(index);
				SetStatusCreated(index, out loadedLevel, ref status);
			}

			Debug.LogWarning(GetType() + " " + status);
			return status;
		}

		private void SetStatusCreated(int index, out GameObject loadedLevel, ref LoadLevelStatus status)
		{
			loadedLevel = Instantiate(levelPrefab);
			loadedLevel.GetComponent<MapDungeonLevel>().Build();
			loadedLevel.transform.SetParent(transform);
			loadedLevel.name = "Level " + index;
			levels[index] = loadedLevel;
			levelIndexes = new int[levels.Count];
			levels.Keys.CopyTo(levelIndexes, 0);
			status = LoadLevelStatus.Created;
		}

		private LoadLevelStatus DestroyLevel(int index)
		{
			var status = LoadLevelStatus.Failed;

			if (levels.ContainsKey(index))
			{
				SetStatusDestroyed(index, ref status);
			}

			Debug.LogWarning(GetType() + " " + status);
			return status;
		}

		private void SetStatusDestroyed(int index, ref LoadLevelStatus status)
		{
			Destroy(levels[index].gameObject);
			levels.Remove(index);
			status = LoadLevelStatus.Destroyed;
		}

		public LoadLevelStatus LoadLevel(int index)
		{
			var status = LoadLevelStatus.Failed;

			if (levels.ContainsKey(index))
			{
				SetStatusLoaded(index, ref status);
			}

			if (status == LoadLevelStatus.Loaded)
			{
				LevelLoaded(index, activeLevel);
			}

			Debug.LogWarning(GetType() + " " + status);
			return status;
		}

		private void SetStatusLoaded(int index, ref LoadLevelStatus status)
		{
			this.index = index;

			if (activeLevel != null)
			{
				activeLevel.SetActive(false);
			}

			activeLevel = levels[index];
			activeLevel.SetActive(true);
			status = LoadLevelStatus.Loaded;
		}

		public void Dispose()
		{
			Clear();
		}

		private void Clear()
		{
			var sceneLevelInstances = FindObjectsOfType<MapDungeonLevel>();

			for (int i = 0; i < sceneLevelInstances.Length; i++)
			{
				Destroy(sceneLevelInstances[i].gameObject);
			}

			levels.Clear();
			levelIndexes = new int[0];

			var actors = FindObjectsOfType<AActor>();
			for (int i = 0; i < actors.Length; i++)
			{
				Destroy(actors[i].gameObject);
			}
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}