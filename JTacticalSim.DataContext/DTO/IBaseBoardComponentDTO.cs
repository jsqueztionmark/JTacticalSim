using System;
using System.Text;
using JTacticalSim.Data.DTO;

namespace JTacticalSim.Data.DTO
{
	public interface IBaseBoardComponentDTO : IBaseGameComponentDTO
	{
		CoordinateDTO Location { get; set; }
		int SubNodeLocation { get; set; }
		int Country { get; set; }
	}
}
