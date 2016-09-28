using System;

namespace Game.Level
{
	public interface ILevelComponent
	{
		void Build();

		Action Built { get; set; }
	}
}