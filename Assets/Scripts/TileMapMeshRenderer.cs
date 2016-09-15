using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMapMeshRenderer))]
public class TileMapMeshRendererInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Build"))
		{
			TileMapMeshRenderer tileMap = (TileMapMeshRenderer)target;
			tileMap.Render();
		}
	}
}

[RequireComponent(
	typeof(MeshFilter),
	typeof(MeshRenderer)
)]
public class TileMapMeshRenderer : TileMapRenderer
{
	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	protected override void Awake()
	{
		base.Awake();

		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void Start()
	{
		Render();
	}

	public override void Render()
	{
		// TODO: build logical map
		SetOrigin();
		BuildMesh();
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

		#endregion Mesh Setup

		#region MeshComponentsSetup

		meshFilter.mesh = mesh;
		meshRenderer.sharedMaterials[0].mainTexture = BuildTexture();

		#endregion MeshComponentsSetup
	}
}