using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;

namespace JTacticalSim.DataContext
{
	[Serializable]
	public class DataResult<TResult, TObject> : IResult<TResult, TObject>
	{
		public List<string> Messages { get; set; }
		public string Message
		{
			get
			{
				var sb = new StringBuilder();
				Messages.ForEach(m => sb.AppendLine(m));
				return sb.ToString();
			}
		}
		public Exception ex { get; set; }
		public ResultStatus Status { get; set; }
		public TResult Result { get; set; }

		public bool IsFailed { get { return Status == ResultStatus.FAILURE || Status == ResultStatus.SOME_FAILURE || Status == ResultStatus.EXCEPTION; }}

		public List<TObject> FailedObjects { get; set; }
		public List<TObject> SuccessfulObjects { get; set; }

		public DataResult()
		{
			FailedObjects = new List<TObject>();
			Messages = new List<string>();
			SuccessfulObjects = new List<TObject>();
			Status = ResultStatus.OTHER;
		}
	}
}
