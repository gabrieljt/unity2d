using UnityEngine;

[RequireComponent(
	typeof(ParticleSystem),
	typeof(AudioSource)
)]
public class MeteorExplosion : MonoBehaviour
{
	private void Start()
	{
		Destroy(gameObject, GetComponent<ParticleSystem>().duration);
	}
}