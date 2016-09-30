using System;
using UnityEngine;

namespace Game.Interfaces
{
	public interface IDestroyable
	{
		void OnDestroy();

		Action<MonoBehaviour> Destroyed { get; set; }
	}
}