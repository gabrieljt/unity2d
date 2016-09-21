using UnityEngine;

namespace Level
{
	using System;
	using System.Collections.Generic;

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
				LevelLoader levelLoader = (LevelLoader)target;

				var levelParameters = levelLoader.levelInstanceParameters;

				LevelInstance loadedLevel;
				if (LevelLoader.LoadLevelStatus.Created == levelLoader.CreateLevel(
					levelLoader.Index, ref levelParameters, out loadedLevel, false)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Recreate Level"))
			{
				LevelLoader levelLoader = (LevelLoader)target;

				var levelParameters = levelLoader.levelInstanceParameters;

				LevelInstance loadedLevel;
				if (LevelLoader.LoadLevelStatus.Created == levelLoader.CreateLevel(
					levelLoader.Index, ref levelParameters, out loadedLevel, true)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Load Level"))
			{
				LevelLoader levelLoader = (LevelLoader)target;

				var levelParameters = levelLoader.levelInstanceParameters;

				levelLoader.LoadLevel(levelLoader.Index);
			}

			if (GUILayout.Button("Dispose"))
			{
				LevelLoader levelLoader = (LevelLoader)target;

				levelLoader.Dispose();
			}
		}
	}

#endif

	public class LevelLoader : MonoBehaviour, IDisposable
	{
		public enum LoadLevelStatus
		{
			Created,
			Loaded,
			Failed,
			Destroyed,
		}

		[SerializeField]
		private GameObject levelInstancePrefab;

		public GameObject LevelInstancePrefab { get { return levelInstancePrefab; } }

		[SerializeField]
		private LevelInstance loadedLevel;

		[SerializeField]
		private int index;

		public int Index { get { return index; } }

		public LevelInstanceParameters levelInstanceParameters;

		[SerializeField]
		private int[] levelIndexes;

		private static Dictionary<int, LevelInstance> levelInstances = new Dictionary<int, LevelInstance>();

		public Action<int, LevelInstance, LevelInstanceParameters> LevelLoaded = delegate { };

		private void Awake()
		{
			Debug.Assert(levelInstancePrefab);
			Debug.Assert(levelInstancePrefab.GetComponent<LevelInstance>());
		}

		private void Start()
		{
			Clear();
		}

		public LoadLevelStatus CreateLevel(int index, ref LevelInstanceParameters levelInstanceParameters, out LevelInstance loadedLevel, bool overwrite = false)
		{
			LoadLevelStatus status = LoadLevelStatus.Failed;
			loadedLevel = null;

			if (!levelInstances.ContainsKey(index))
			{
				levelInstanceParameters = SetStatusCreated(index, levelInstanceParameters, out loadedLevel, ref status);
			}
			else if (overwrite)
			{
				DestroyLevel(index);
				levelInstanceParameters = SetStatusCreated(index, levelInstanceParameters, out loadedLevel, ref status);
			}

			this.levelInstanceParameters = levelInstanceParameters;

			Debug.LogWarning(GetType() + "LevelLoader.CreateLevel.status: " + status);
			return status;
		}

		private LevelInstanceParameters SetStatusCreated(int index, LevelInstanceParameters levelParameters, out LevelInstance loadedLevel, ref LoadLevelStatus status)
		{
			loadedLevel = Instantiate(levelInstancePrefab).GetComponent<LevelInstance>();
			loadedLevel.Build(ref levelParameters);
			loadedLevel.transform.SetParent(transform, true);
			loadedLevel.name = "Level " + index;
			levelInstances[index] = loadedLevel;
			levelIndexes = new int[levelInstances.Count];
			levelInstances.Keys.CopyTo(levelIndexes, 0);
			status = LoadLevelStatus.Created;
			return levelParameters;
		}

		private LoadLevelStatus DestroyLevel(int index)
		{
			var status = LoadLevelStatus.Failed;

			if (levelInstances.ContainsKey(index))
			{
				SetStatusDestroyed(index, ref status);
			}

			Debug.LogWarning(GetType() + "LevelLoader.DestroyLevel.status: " + status);
			return status;
		}

		private void SetStatusDestroyed(int index, ref LoadLevelStatus status)
		{
			DestroyImmediate(levelInstances[index].gameObject);
			levelInstances.Remove(index);
			status = LoadLevelStatus.Destroyed;
		}

		public LoadLevelStatus LoadLevel(int index)
		{
			LoadLevelStatus status = LoadLevelStatus.Failed;

			if (levelInstances.ContainsKey(index))
			{
				levelInstanceParameters = SetStatusLoaded(index, ref status);
			}

			if (status == LoadLevelStatus.Loaded)
			{
				LevelLoaded(index, loadedLevel, levelInstanceParameters);
			}

			Debug.LogWarning(GetType() + "LevelLoader.LoadLevel.status: " + status);
			return status;
		}

		private LevelInstanceParameters SetStatusLoaded(int index, ref LoadLevelStatus status)
		{
			this.index = index;

			if (loadedLevel != null)
			{
				loadedLevel.gameObject.SetActive(false);
			}

			loadedLevel = levelInstances[index];
			loadedLevel.gameObject.SetActive(true);
			status = LoadLevelStatus.Loaded;

			var levelInstanceParameters = new LevelInstanceParameters();
			levelInstanceParameters.width = loadedLevel.Width;
			levelInstanceParameters.height = loadedLevel.Height;
			levelInstanceParameters.rooms = loadedLevel.Rooms;
			levelInstanceParameters.tiles = loadedLevel.Tiles;

			return levelInstanceParameters;
		}

		public void Dispose()
		{
			Clear();
		}

		private void Clear()
		{
			var sceneLevelInstances = FindObjectsOfType<LevelInstance>();

			for (int i = 0; i < sceneLevelInstances.Length; i++)
			{
				DestroyImmediate(sceneLevelInstances[i].gameObject);
			}

			levelInstances.Clear();
			levelIndexes = new int[0];
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}