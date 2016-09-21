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
					levelLoader.Index, ref levelParameters, out loadedLevel)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
			}

			if (GUILayout.Button("Load Level"))
			{
				LevelLoader levelLoader = (LevelLoader)target;

				var levelParameters = levelLoader.levelInstanceParameters;

				LevelInstance loadedLevel;
				if (LevelLoader.LoadLevelStatus.Loaded == levelLoader.LoadLevel(
					levelLoader.Index)
				)
				{
					levelLoader.LoadLevel(levelLoader.Index);
				}
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
			Failed
		}

		[SerializeField]
		private GameObject levelInstancePrefab;

		public GameObject LevelInstancePrefab { get { return levelInstancePrefab; } }

		[SerializeField]
		private int index;

		public int Index { get { return index; } }

		[SerializeField]
		private LevelInstance loadedLevel;

		public LevelInstanceParameters levelInstanceParameters;

		private static Dictionary<int, LevelInstance> levelInstances = new Dictionary<int, LevelInstance>();

		[SerializeField]
		private int[] levelIndexes;

		public Action LevelLoaded = delegate { };

		private void Awake()
		{
			Debug.Assert(levelInstancePrefab);
			Debug.Assert(levelInstancePrefab.GetComponent<LevelInstance>());
		}

		public LoadLevelStatus CreateLevel(int index, ref LevelInstanceParameters levelParameters, out LevelInstance loadedLevel, bool overwrite = false)
		{
			LoadLevelStatus status = LoadLevelStatus.Failed;
			loadedLevel = null;

			if (overwrite || !levelInstances.ContainsKey(index))
			{
				levelParameters = SetStatusCreated(index, levelParameters, out loadedLevel, out status);
			}

			Debug.LogWarning(GetType() + "LevelLoader.CreateLevel.status: " + status);
			return status;
		}

		private LevelInstanceParameters SetStatusCreated(int index, LevelInstanceParameters levelParameters, out LevelInstance loadedLevel, out LoadLevelStatus status)
		{
			loadedLevel = Instantiate(levelInstancePrefab).GetComponent<LevelInstance>();
			loadedLevel.Build(ref levelParameters);
			levelInstances[index] = loadedLevel;
			levelIndexes = new int[levelInstances.Count];
			levelInstances.Keys.CopyTo(levelIndexes, 0);
			status = LoadLevelStatus.Created;
			return levelParameters;
		}

		public LoadLevelStatus LoadLevel(int index)
		{
			LoadLevelStatus status = LoadLevelStatus.Failed;

			if (levelInstances.ContainsKey(index))
			{
				status = SetStatusLoaded(index);
			}

			Debug.LogWarning(GetType() + "LevelLoader.CreateLevel.status: " + status);			
			return status;
		}

		private LoadLevelStatus SetStatusLoaded(int index)
		{
			this.index = index;
			LoadLevelStatus status;

			if (loadedLevel != null)
			{
				loadedLevel.gameObject.SetActive(false);
			}

			loadedLevel = levelInstances[index];
			loadedLevel.gameObject.SetActive(true);
			status = LoadLevelStatus.Loaded;

			LevelLoaded();

			return status;
		}

		public void Dispose()
		{
			foreach (LevelInstance levelInstance in levelInstances.Values)
			{
				DestroyImmediate(levelInstance.gameObject);
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