using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTacticalSim.DataContext.DTO
{
	public class TaskExecutionArgumentDTO
	{
		public string Type { get; set; }
		public string Assembly { get; set; }
		public string Name { get; set; }
		public IEnumerable<string> Value { get; set; }
	}
}
