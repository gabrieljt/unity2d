using UnityEngine;

[RequireComponent(
	typeof(MeshRenderer)
)]
public class FollowUV : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer renderer;

	[SerializeField]
	[Range(1f, 10f)]
	private float parallax = 2f;

	[SerializeField]
	private Vector2 startingOffset = Vector2.zero;

	private void Awake()
	{
		renderer = GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		var material = renderer.material;

		var offset = material.mainTextureOffset;

		offset.x = transform.position.x / transform.localScale.x / parallax;
		offset.y = transform.position.y / transform.localScale.y / parallax;
		offset += startingOffset.normalized;

		material.mainTextureOffset = offset;
	}
}