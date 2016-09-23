using UnityEngine;

namespace TiledLevel
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
				var levelLoader = (LevelLoader)target;
				var levelParams = levelLoader.levelParams;

				GameObject loadedLevel;
				if (LevelLoader.LoadLevelStatus.Created == levelLoader.CreateLevel(
					levelLoader.Index, ref levelParams, out loadedLevel, false)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Recreate Level"))
			{
				var levelLoader = (LevelLoader)target;
				var levelParams = levelLoader.levelParams;

				GameObject loadedLevel;
				if (LevelLoader.LoadLevelStatus.Created == levelLoader.CreateLevel(
					levelLoader.Index, ref levelParams, out loadedLevel, true)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Load Level"))
			{
				var levelLoader = (LevelLoader)target;
				var levelParams = levelLoader.levelParams;
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
		private GameObject levelPrefab;

		public GameObject LevelPrefab { get { return levelPrefab; } }

		[SerializeField]
		private GameObject activeLevel;

		[SerializeField]
		private int index;

		public int Index { get { return index; } }

		public LevelParams levelParams;

		[SerializeField]
		private int[] levelIndexes;

		private static Dictionary<int, GameObject> levels = new Dictionary<int, GameObject>();

		public Action<int, GameObject, LevelParams> LevelLoaded = delegate { };

		private void Awake()
		{
			Debug.Assert(levelPrefab);
			Debug.Assert(levelPrefab.GetComponent<Map>());

			gameObject.isStatic = true;
		}

		private void Start()
		{
			Clear();
		}

		public LoadLevelStatus CreateLevel(int index, ref LevelParams levelParams, out GameObject loadedLevel, bool overwrite = false)
		{
			var status = LoadLevelStatus.Failed;
			loadedLevel = null;

			if (!levels.ContainsKey(index))
			{
				levelParams = SetStatusCreated(index, levelParams, out loadedLevel, ref status);
			}
			else if (overwrite)
			{
				DestroyLevel(index);
				levelParams = SetStatusCreated(index, levelParams, out loadedLevel, ref status);
			}

			this.levelParams = levelParams;

			Debug.LogWarning(GetType() + " " + status);
			return status;
		}

		private LevelParams SetStatusCreated(int index, LevelParams levelParams, out GameObject loadedLevel, ref LoadLevelStatus status)
		{
			loadedLevel = Instantiate(levelPrefab);
			IMapParams mapParams = levelParams;
			loadedLevel.GetComponent<Map>().Build(ref mapParams);
			loadedLevel.transform.SetParent(transform, true);
			loadedLevel.name = "Level " + index;
			levels[index] = loadedLevel;
			levelIndexes = new int[levels.Count];
			levels.Keys.CopyTo(levelIndexes, 0);
			status = LoadLevelStatus.Created;

			return levelParams;
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
			DestroyImmediate(levels[index].gameObject);
			levels.Remove(index);
			status = LoadLevelStatus.Destroyed;
		}

		public LoadLevelStatus LoadLevel(int index)
		{
			var status = LoadLevelStatus.Failed;

			if (levels.ContainsKey(index))
			{
				levelParams = SetStatusLoaded(index, ref status);
			}

			if (status == LoadLevelStatus.Loaded)
			{
				LevelLoaded(index, activeLevel, levelParams);
			}

			Debug.LogWarning(GetType() + " " + status);
			return status;
		}

		private LevelParams SetStatusLoaded(int index, ref LoadLevelStatus status)
		{
			this.index = index;

			if (activeLevel != null)
			{
				activeLevel.SetActive(false);
			}

			activeLevel = levels[index];
			activeLevel.SetActive(true);
			status = LoadLevelStatus.Loaded;

			return new LevelParams(activeLevel);
		}

		public void Dispose()
		{
			Clear();
		}

		private void Clear()
		{
			var sceneLevelInstances = FindObjectsOfType<Map>();

			for (int i = 0; i < sceneLevelInstances.Length; i++)
			{
				DestroyImmediate(sceneLevelInstances[i].gameObject);
			}

			levels.Clear();
			levelIndexes = new int[0];
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}