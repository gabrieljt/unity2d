using System;
using System.Collections;
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
	private int level = 100;

	[SerializeField]
	private Text dungeonLevelLabel, stepsTakenLabel;

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
		SetDungeonLevelLabel(level);
		SetStepsTakenLabel(playerCharacter.Steps);
	}

	private void Start()
	{
		BuildLevel();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			BuildLevel();
		}
	}

	private void LateUpdate()
	{
		SetCameraPosition(playerCharacter.transform.position);
	}

	private void SetCameraPosition(Vector3 position)
	{
		camera.transform.position = Vector3.back * 10f + position;
	}

	private void OnStepTaken()
	{
		SetStepsTakenLabel(playerCharacter.Steps);
	}

	private void SetStepsTakenLabel(int steps)
	{
		stepsTakenLabel.text = "Steps Taken: " + steps;
	}

	private void OnExitReached()
	{
		SetDungeonLevelLabel(++level);
		BuildLevel();
	}

	private void SetDungeonLevelLabel(int level)
	{
		dungeonLevelLabel.text = "Dungeon Level: " + level;
	}

	public void OnTileMapBuilt()
	{
		StartCoroutine(PopulateTileMap());
	}

	#region Populate Tile Map

	private void BuildLevel()
	{
		StartCoroutine(BuildNewTileMap());
	}

	private IEnumerator BuildNewTileMap()
	{
		DisableSceneObjects();

		yield return new WaitForEndOfFrame();
		tileMap.Build();
	}

	private void DisableSceneObjects()
	{
		playerCharacter.HaltMovement();
		playerCharacter.Inputs.Clear();
		if (Application.isPlaying)
		{
			playerCharacter.Rigidbody2D.isKinematic = true;
		}
		playerCharacter.gameObject.SetActive(false);

		exit.gameObject.SetActive(false);
	}

	private IEnumerator PopulateTileMap()
	{
		SetPlayerPosition();
		SetExitPosition();
		yield return 0;

		if (playerCharacter.transform.position.Equals(exit.transform.position))
		{
			StartCoroutine(BuildNewTileMap());
		}
		else
		{
			EnableSceneObjects();
		}
	}

	private void EnableSceneObjects()
	{
		playerCharacter.gameObject.SetActive(true);
		if (Application.isPlaying)
		{
			playerCharacter.Rigidbody2D.isKinematic = false;
		}

		exit.gameObject.SetActive(true);
	}

	private void SetPlayerPosition()
	{
		Vector2 roomCenter = tileMap.GetRandomRoomCenter() + Vector2.one * 0.5f;
		playerCharacter.transform.position = roomCenter;
		SetCameraPosition(roomCenter);
	}

	private void SetExitPosition()
	{
		Vector2 roomCenter = tileMap.GetRandomRoomCenter() + Vector2.one * 0.5f;
		exit.transform.position = roomCenter;
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