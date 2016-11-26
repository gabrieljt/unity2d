using System;

public interface IDestroyable
{
	void OnDestroy();

	Action<IDestroyable> Destroyed { get; set; }
}