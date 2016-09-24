using System;
using UnityEngine;

namespace Game
{
	public interface IDestroyable : IDisposable
	{
		void OnDestroy();

		Action<MonoBehaviour> Destroyed { get; set; }
	}
}