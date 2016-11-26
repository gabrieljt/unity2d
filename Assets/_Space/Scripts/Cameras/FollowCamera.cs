using UnityEngine;

[RequireComponent(
	typeof(Camera)
)]
public class FollowCamera : MonoBehaviour
{
	public static FollowCamera Instance
	{
		get
		{
			return FindObjectOfType<FollowCamera>();
		}
	}

	public Transform target;

	[SerializeField]
	private float smoothTime = 0.3f;

	[SerializeField]
	private Vector3 velocity = Vector2.zero;

	[SerializeField]
	private Camera camera;

	private void Awake()
	{
		camera = GetComponent<Camera>();
		camera.orthographic = true;
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.backgroundColor = Color.black;

		if (!target)
		{
			target = transform;
		}
	}

	private void LateUpdate()
	{
		transform.position = target.TransformPoint(new Vector3(0, 0, transform.position.z));
	}
}