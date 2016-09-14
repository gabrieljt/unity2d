using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMap))]
public class TileMapInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Build"))
		{
			TileMap tileMap = (TileMap)target;
			tileMap.Build();
		}
	}
}

[ExecuteInEditMode]
[RequireComponent(
	typeof(MeshFilter),
	typeof(MeshRenderer)
)]
public class TileMap : MonoBehaviour
{
	[SerializeField]
	[Range(1, 128)]
	private int width;

	[SerializeField]
	[Range(1, 128)]
	private int height;

	[SerializeField]
	private int tileResolution = 16;

	[SerializeField]
	private Texture2D tilesTexture;

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private void Awake()
	{
		Debug.Assert(tilesTexture);

		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void Start()
	{
		Build();
	}

	public void Build()
	{
		// TODO: build logical map
		SetOrigin();
		BuildMesh();
		BuildTexture();
	}

	private void SetOrigin()
	{
		transform.position = new Vector3(-(width / 2f), height / 2f, 0f);
		transform.localEulerAngles = new Vector3(270f, 0f, 0f);
	}

	private void BuildMesh()
	{
		#region Mesh Data

		int totalTiles = width * height;
		int totalTriangles = totalTiles * 2;
		int[] triangles = new int[totalTriangles * 3];

		int verticesWidth = width + 1;
		int verticesHeight = height + 1;
		int totalVertices = verticesWidth * verticesHeight;
		Vector3[] vertices = new Vector3[totalVertices];
		Vector3[] normals = new Vector3[totalVertices];
		Vector2[] uv = new Vector2[totalVertices];

		#endregion Mesh Data

		#region Vertices

		int x, z;
		for (z = 0; z < verticesHeight; z++)
		{
			for (x = 0; x < verticesWidth; x++)
			{
				vertices[z * verticesWidth + x] = new Vector3(x, 0, -z);
				normals[z * verticesWidth + x] = Vector3.up;
				uv[z * verticesWidth + x] = new Vector2((float)x / width, 1f - (float)z / height);
			}
		}

		#endregion Vertices

		#region Triangles

		for (z = 0; z < height; z++)
		{
			for (x = 0; x < width; x++)
			{
				int squareIndex = z * width + x;
				int triOffset = squareIndex * 6;
				triangles[triOffset + 0] = z * verticesWidth + x + 0;
				triangles[triOffset + 2] = z * verticesWidth + x + verticesWidth + 0;
				triangles[triOffset + 1] = z * verticesWidth + x + verticesWidth + 1;

				triangles[triOffset + 3] = z * verticesWidth + x + 0;
				triangles[triOffset + 5] = z * verticesWidth + x + verticesWidth + 1;
				triangles[triOffset + 4] = z * verticesWidth + x + 1;
			}
		}

		#endregion Triangles

		#region Mesh Setup

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;

		meshFilter.mesh = mesh;

		#endregion Mesh Setup
	}

	private Color[][] GetPixelsFromTexture()
	{
		int tilesPerRow = tilesTexture.width / tileResolution;
		int rows = tilesTexture.height / tileResolution;

		Color[][] tilesPixels = new Color[tilesPerRow * rows][];

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < tilesPerRow; x++)
			{
				tilesPixels[y * tilesPerRow + x] = tilesTexture.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
			}
		}

		return tilesPixels;
	}

	private int GetRandomTileTextureIndex()
	{
		int tilesPerRow = tilesTexture.width / tileResolution;
		int rows = tilesTexture.height / tileResolution;

		return Random.Range(0, tilesPerRow * rows);
	}

	private void BuildTexture()
	{
		int textureWidth = width * tileResolution;
		int textureHeight = height * tileResolution;
		Texture2D texture = new Texture2D(textureWidth, textureHeight);

		Color[][] tilesPixels = GetPixelsFromTexture();

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Color[] pixels = tilesPixels[GetRandomTileTextureIndex()]; // TODO: read tile type from logical map
				texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, pixels);
			}
		}

		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();

		meshRenderer.sharedMaterials[0].mainTexture = texture;
	}
}