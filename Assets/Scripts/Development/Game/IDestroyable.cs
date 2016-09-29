using System;
using UnityEngine;

namespace Game
{
	public interface IDestroyable
	{
		void OnDestroy();

		Action<MonoBehaviour> Destroyed { get; set; }
	}
}