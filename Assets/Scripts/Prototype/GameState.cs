using Level;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Camera camera;

	[SerializeField]
	private Map levelInstance;

	[SerializeField]
	private Character playerCharacter;

	[SerializeField]
	private Exit exit;

	[SerializeField]
	[Range(1, 100)]
	private int level = 1;

	[SerializeField]
	private Text dungeonLevelLabel, stepsLeftLabel, stepsTakenLabel;

	[Serializable]
	public class Level
	{
		[SerializeField]
		[Range(1, 100)]
		private int id = 1;

		public int Id { get { return id; } }

		[SerializeField]
		private int stepsTaken = 0;

		public int StepsTaken { get { return stepsTaken; } }

		[SerializeField]
		private int maximumSteps;

		public int MaximumSteps { get { return maximumSteps; } }

		public int StepsLeft { get { return maximumSteps - stepsTaken; } }

		public Level(int id)
		{
			this.id = id;
		}

		public void StepTaken()
		{
			++stepsTaken;
		}

		public void SetSize(out int width, out int height)
		{
			height = width = id + 9;
		}

		public void SetMaximumSteps(int level, RoomMap.Room[] rooms, Vector2 tileMapOrigin)
		{
			maximumSteps = stepsTaken = 0;
			foreach (RoomMap.Room room in rooms)
			{
				maximumSteps += (int)Vector2.Distance(room.Center, tileMapOrigin);
			}

			maximumSteps = Mathf.Clamp(maximumSteps / level * rooms.Length, level + rooms.Length, maximumSteps + level + rooms.Length);
		}
	}

	private Stack<Level> levels = new Stack<Level>();

	[SerializeField]
	private Level currentLevel;

	private void Awake()
	{
		camera = FindObjectOfType<Camera>();
		Debug.Assert(camera);

		levelInstance = FindObjectOfType<Map>();
		Debug.Assert(levelInstance);

		playerCharacter = FindObjectOfType<Character>();
		Debug.Assert(playerCharacter);

		exit = FindObjectOfType<Exit>();
		Debug.Assert(exit);

		levelInstance.Built += OnTileMapBuilt;
		levelInstance.GetComponent<RoomMap>().Built += OnRoomMapBuilt;
		playerCharacter.StepTaken += OnStepTaken;
		exit.Reached += OnExitReached;

		Debug.Assert(dungeonLevelLabel);
		Debug.Assert(stepsLeftLabel);
		Debug.Assert(stepsTakenLabel);
	}

	private void Start()
	{
		BuildLevel();
		UpdateUI();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ResetLevel();
			return;
		}

		if (currentLevel.StepsLeft == 0)
		{
			ResetLevel();
		}

		UpdateUI();
	}

	private void LateUpdate()
	{
		SetCameraPosition(playerCharacter.transform.position);
	}

	private void UpdateUI()
	{
		SetDungeonLevelLabel(currentLevel.Id);
		SetStepsLeftLabel(currentLevel.StepsLeft);
		SetStepsTakenLabel(playerCharacter.Steps);
	}

	private void SetCameraPosition(Vector3 position)
	{
		camera.transform.position = Vector3.back * 10f + position;
	}

	private void SetDungeonLevelLabel(int level)
	{
		dungeonLevelLabel.text = "Dungeon Level: " + level;
	}

	private void SetStepsLeftLabel(int stepsLeft)
	{
		stepsLeftLabel.text = "Steps Left: " + stepsLeft;
	}

	private void SetStepsTakenLabel(int steps)
	{
		stepsTakenLabel.text = "Steps Taken: " + steps;
	}

	private void BuildLevel()
	{
		if (level > levels.Count)
		{
			levels.Push(currentLevel = new Level(level));
		}

		int width, height;
		currentLevel.SetSize(out width, out height);
		camera.orthographicSize = Mathf.Sqrt(Mathf.Max(width, height));
		StartCoroutine(BuildNewTileMap(width, height));
	}

	private IEnumerator BuildNewTileMap(int width, int height)
	{
		DisableSceneObjects();

		yield return 0;
		var levelParams = new LevelParams();
		levelParams.Width = width;
		levelParams.Height = height;
		IMapParams mapParams = levelParams;
		levelInstance.Build(ref mapParams);
	}

	private void DisableSceneObjects()
	{
		playerCharacter.Disable();
		exit.Disable();
	}

	private void EnableSceneObjects()
	{
		playerCharacter.Enable();
		exit.Enable();
	}

	#region Callbacks

	private void OnStepTaken()
	{
		currentLevel.StepTaken();
	}

	private void OnExitReached()
	{
		++level;
		BuildLevel();
	}

	public void OnTileMapBuilt(IMapParams mapParams)
	{
	}

	public void OnRoomMapBuilt(IRoomMapParams roomMapParams)
	{
		StartCoroutine(PopulateTileMap(roomMapParams));
	}

	#endregion Callbacks

	#region Populate Tile Map

	private IEnumerator PopulateTileMap(IRoomMapParams roomMapParams)
	{
		bool populated = Populate(roomMapParams);
		yield return 0;

		if (!populated)
		{
			ResetLevel();
		}
		else
		{
			EnableSceneObjects();
			currentLevel.SetMaximumSteps(level, roomMapParams.Rooms, levelInstance.WorldPosition);
		}
	}

	private void ResetLevel()
	{
		levels.Pop();
		BuildLevel();
	}

	private bool Populate(IRoomMapParams roomMapParams)
	{
		bool playerSet = false, exitSet = false;
		for (int i = 0; i < roomMapParams.Rooms.Length; i++)
		{
			RoomMap.Room room = roomMapParams.Rooms[i];
			for (int x = 0; x < room.Width; x++)
			{
				for (int y = 0; y < room.Height; y++)
				{
					if (levelInstance.Tiles[room.Left + x, room.Top + y].Type == TileType.Floor)
					{
						Vector2 tilePosition = new Vector2(room.Left + x, room.Top + y) + Vector2.one * 0.5f;
						switch (roomMapParams.Rooms.Length)
						{
							case 1:
								if (!playerSet)
								{
									SetPlayerPosition(tilePosition);
									playerSet = true;
								}
								else if (!exitSet)
								{
									SetExitPosition(tilePosition);
									exitSet = true;
								}

								break;

							default:
								if (i == 0)
								{
									SetPlayerPosition(tilePosition);
									playerSet = true;
								}
								else if (i == roomMapParams.Rooms.Length - 1)
								{
									SetExitPosition(tilePosition);
									exitSet = true;
								}
								break;
						}
						if (playerSet && exitSet)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private void SetPlayerPosition(Vector2 position)
	{
		playerCharacter.transform.position = position;
		SetCameraPosition(position);
	}

	private void SetExitPosition(Vector2 position)
	{
		exit.transform.position = position;
	}

	#endregion Populate Tile Map

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		levelInstance.Built -= OnTileMapBuilt;
		playerCharacter.StepTaken += OnStepTaken;
		exit.Reached -= OnExitReached;
	}
}