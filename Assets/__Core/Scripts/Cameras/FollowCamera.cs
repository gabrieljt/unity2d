using UnityEngine;

[RequireComponent(
	typeof(Camera)
)]
public class FollowCamera : MonoBehaviour
{
	public Transform target;

	private void Awake()
	{
		var camera = GetComponent<Camera>();
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