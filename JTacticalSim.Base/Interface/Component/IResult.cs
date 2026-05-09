using System;

namespace JTacticalSim.API.Component
{
	public interface IResult<TResult, TObject> : IBaseResult
	{
		TResult Result { get; set; }

		List<TObject> FailedObjects { get; set; }
		List<TObject> SuccessfulObjects { get; set; }
	}
}
