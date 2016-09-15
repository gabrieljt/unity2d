using System;
using UnityEngine;

public class GameState : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Character playerCharacter;

	[SerializeField]
	new private Camera camera;

	[SerializeField]
	private TileMap tileMap;

	private void Awake()
	{
		tileMap = FindObjectOfType<TileMap>();
		Debug.Assert(tileMap);

		playerCharacter = FindObjectOfType<Character>();
		Debug.Assert(playerCharacter);

		camera = FindObjectOfType<Camera>();
		Debug.Assert(camera);

		tileMap.Built += OnTileMapBuilt;
	}

	private void OnTileMapBuilt()
	{
		playerCharacter.transform.position = tileMap.Origin;

		SetCameraPosition(playerCharacter.transform.position);
	}

	private void SetCameraPosition(Vector3 position)
	{
		camera.transform.position = Vector3.back * 10f + position;
	}

	private void LateUpdate()
	{
		SetCameraPosition(playerCharacter.transform.position);
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		tileMap.Built -= OnTileMapBuilt;
	}
}