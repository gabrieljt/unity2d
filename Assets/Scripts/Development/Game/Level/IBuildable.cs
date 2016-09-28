using System;

namespace Game.Level
{
	public interface IBuildable
	{
		void Build();

		Action Built { get; set; }
	}
}