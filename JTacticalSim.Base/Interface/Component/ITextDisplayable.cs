using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface ITextDisplayable : IBaseComponent
	{
		string TextDisplayZ1 { get; set; }
		string TextDisplayZ2 { get; set; }
		string TextDisplayZ3 { get; set; }
		string TextDisplayZ4 { get; set; }
	}
}
