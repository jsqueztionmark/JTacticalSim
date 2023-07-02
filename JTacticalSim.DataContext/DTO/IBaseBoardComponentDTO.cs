using System;
using System.Collections.Generic;
using System.Linq;
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
