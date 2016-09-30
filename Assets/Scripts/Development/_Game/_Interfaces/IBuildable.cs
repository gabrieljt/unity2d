using System;

namespace Game.Interfaces
{
	public interface IBuildable
	{
		void Build();

		Action<Type> Built { get; set; }
	}
}