using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using JTacticalSim.API;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class UnitTaskDTO : BaseGameComponentDTO
	{
		[DataMember]
		public int UnitTaskType { get; set; }

		[DataMember]
		public int Mission { get; set; }

		[DataMember]
		public IEnumerable<TaskExecutionArgument> Args { get; private set; }

		[DataMember]
		public int TurnsToComplete { get; set; }
	}
}
