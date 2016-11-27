using UnityEngine;

[RequireComponent(
	typeof(ParticleSystem),
	typeof(AudioSource)
)]
public class MeteorExplosion : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem particleSystem;

	[SerializeField]
	private AudioSource audioSource;

	private void Awake()
	{
		particleSystem = GetComponent<ParticleSystem>();
		audioSource = GetComponent<AudioSource>();
	}

	private void Start()
	{
		audioSource.Play();
		Destroy(gameObject, particleSystem.duration);
	}
}