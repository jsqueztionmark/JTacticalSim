using System;
using System.Text;
using JTacticalSim.API.Game;

namespace JTacticalSim.Data.DTO
{
	public interface IBaseGameComponentDTO
	{
		string Name { get; set; }
		string Description { get; set; }
		int ID { get; set; }
		Guid UID { get; set; }
	}
}
