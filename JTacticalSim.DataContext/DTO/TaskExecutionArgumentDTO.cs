using System;
using System.Text;

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
