using System;
using System.Collections;
using UnityEngine;

public class GameState : MonoBehaviour, IDisposable
{
	[SerializeField]
	new private Camera camera;

	[SerializeField]
	private TileMap tileMap;

	[SerializeField]
	private Character playerCharacter;

	[SerializeField]
	private Exit exit;

	[SerializeField]
	[Range(1, 10)]
	private int level = 1;

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
		exit.Reached += OnExitReached;
	}

	private void Start()
	{
		BuildLevel();
	}

	private void LateUpdate()
	{
		SetCameraPosition(playerCharacter.transform.position);
	}

	public void SetCameraPosition(Vector3 position)
	{
		camera.transform.position = Vector3.back * 10f + position;
	}

	private void OnExitReached()
	{
		level++;
		BuildLevel();
	}

	private void BuildLevel()
	{
		StartCoroutine(BuildNewTileMap());
	}

	#region Populate Tile Map

	private IEnumerator BuildNewTileMap()
	{
		playerCharacter.HaltMovement();
		playerCharacter.Inputs.Clear();
		if (Application.isPlaying)
		{
			playerCharacter.Rigidbody2D.isKinematic = true;
		}
		playerCharacter.gameObject.SetActive(false);

		exit.gameObject.SetActive(false);
		yield return new WaitForEndOfFrame();

		tileMap.Build();
	}

	public void OnTileMapBuilt()
	{
		StartCoroutine(PopulateTileMap());
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
			playerCharacter.gameObject.SetActive(true);
			if (Application.isPlaying)
			{
				playerCharacter.Rigidbody2D.isKinematic = false;
			}

			exit.gameObject.SetActive(true);
		}
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
	}
}