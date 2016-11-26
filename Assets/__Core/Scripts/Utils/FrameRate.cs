using UnityEngine;

public class FrameRate : MonoBehaviour
{
	[SerializeField]
	[Range(0, 60)]
	private int frameRate = 0; // Default value

	private int defaultVSyncCount, defaultFrameRate;

	private void Awake()
	{
		gameObject.isStatic = true;

		defaultVSyncCount = QualitySettings.vSyncCount;
		frameRate = defaultFrameRate = Application.targetFrameRate;
	}

	private void OnValidate()
	{
		if (frameRate > 0)
		{
			SetFrameRate(0, frameRate); // VSync must be disabled
		}
		else
		{
			SetFrameRate(defaultVSyncCount, defaultFrameRate);
		}
	}

	private void SetFrameRate(int vSyncCount, int frameRate)
	{
		QualitySettings.vSyncCount = vSyncCount;
		Application.targetFrameRate = frameRate;
	}
}