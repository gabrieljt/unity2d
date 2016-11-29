using UnityEngine;

[RequireComponent(
	typeof(ParticleSystem),
	typeof(AudioSource)
)]
public class MeteorExplosion : MonoBehaviour
{
	private void Start()
	{
		GetComponent<AudioSource>().Play();
		Destroy(gameObject, GetComponent<ParticleSystem>().duration);
	}
}