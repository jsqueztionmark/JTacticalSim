using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTacticalSim.API.Component
{
	public interface IBaseResult
	{

		/// <summary>
		/// Collection of messages to return to the consumer about this operation.
		/// </summary>
		List<string> Messages { get; set; }
		
		/// <summary>
		/// Returns a concatinated string back of all attached messages.
		/// </summary>
		string Message { get; }

		/// <summary>
		/// Is the status a failure of any kind
		/// </summary>
		bool IsFailed { get; }

		Exception ex { get; set; }
		ResultStatus Status { get; set; }
	}
}
