using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.Data.DTO
{
	public interface ITextDisplayableDTO : IBaseGameComponentDTO
	{
		string TextDisplayZ1 { get; set; }
		string TextDisplayZ2 { get; set; }
		string TextDisplayZ3 { get; set; }
		string TextDisplayZ4 { get; set; }
	}
}
