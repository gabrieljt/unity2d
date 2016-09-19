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
	private TileMap tileMap;

	[SerializeField]
	private Character playerCharacter;

	[SerializeField]
	private Exit exit;

	[SerializeField]
	[Range(1, 100)]
	private int level = 1;

	[SerializeField]
	private Text dungeonLevelLabel, stepsTakenLabel;

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
			width = id + 9;
			height = width;
		}

		public void SetMaximumSteps(Vector2 startPosition, Vector2 endPosition)
		{
			maximumSteps = (int)Vector2.Distance(startPosition, endPosition) + id;
		}
	}

	private Stack<Level> levels = new Stack<Level>();

	[SerializeField]
	private Level currentLevel;

	private void Awake()
	{
		camera = FindObjectOfType<Camera>();
		Debug.Assert(camera);

		tileMap = FindObjectOfType<TileMap>();
		Debug.Assert(tileMap);

		playerCharacter = FindObjectOfType<Character>();
		Debug.Assert(playerCharacter);

		exit = FindObjectOfType<Exit>();
		Debug.Assert(exit);

		tileMap.Built += OnTileMapBuilt;
		playerCharacter.StepTaken += OnStepTaken;
		exit.Reached += OnExitReached;

		Debug.Assert(dungeonLevelLabel);
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
		StartCoroutine(BuildNewTileMap(width, height));
	}

	private IEnumerator BuildNewTileMap(int width, int height)
	{
		DisableSceneObjects();

		yield return new WaitForEndOfFrame();
		tileMap.Build(width, height);
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

	public void OnTileMapBuilt()
	{
		StartCoroutine(PopulateTileMap());
	}

	#endregion Callbacks

	#region Populate Tile Map

	private IEnumerator PopulateTileMap()
	{
		bool populated = Populate();
		yield return 0;

		if (!populated)
		{
			ResetLevel();
		}
		else
		{
			EnableSceneObjects();
			currentLevel.SetMaximumSteps(playerCharacter.transform.position, exit.transform.position);
		}
	}

	private void ResetLevel()
	{
		levels.Pop();
		BuildLevel();
	}

	private bool Populate()
	{
		bool playerSet = false, exitSet = false;
		for (int i = 0; i < tileMap.Rooms.Count; i++)
		{
			TileMap.Room room = tileMap.Rooms[i];
			for (int x = 0; x < room.Width; x++)
			{
				for (int y = 0; y < room.Height; y++)
				{
					if (tileMap.Tiles[room.Left + x, room.Top + y].Type == TileType.Floor)
					{
						Vector2 tilePosition = new Vector2(room.Left + x, room.Top + y) + Vector2.one * 0.5f;
						switch (tileMap.Rooms.Count)
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
								else if (i == tileMap.Rooms.Count - 1)
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
		tileMap.Built -= OnTileMapBuilt;
		playerCharacter.StepTaken += OnStepTaken;
		exit.Reached -= OnExitReached;
	}
}