using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.AI;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.AI
{
	public abstract class AIStrategyComponentContainer<T, TChild> : GameComponentBase, IAIStrategyObjectContainer<TChild>
	{
		protected abstract string ComponentTypeName { get; }
		protected abstract string ChildTypeName { get; } 
		protected List<TChild> ChildComponents { get; set; }

		protected AIStrategyComponentContainer()
		{
			ChildComponents = new List<TChild>();
		}

		public virtual IResult<TChild, TChild> AddChildComponent(TChild component)
		{
			var r = new OperationResult<TChild, TChild> { Status = ResultStatus.SUCCESS, Result = component };

			if (ChildComponents.Contains(component))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} already contains {1}. Can not add.".F(ComponentTypeName, ChildTypeName));
				r.FailedObjects.Add(component);
				return r;
			}

			try
			{
				ChildComponents.Add(component);
				r.Messages.Add("{0} Added.".F(ChildTypeName));
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.FailedObjects.Add(component);
				return r;
			}

			return r;
		}

		public virtual IResult<TChild, TChild> AddChildComponents(IEnumerable<TChild> components)
		{
			var r = new OperationResult<TChild, TChild> { Status = ResultStatus.SUCCESS};

			var add = components.Where(c => !ChildComponents.Contains(c)).Select(c => c);

			if (!add.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} already contains all {1}s. Can not add.".F(ComponentTypeName, ChildTypeName));
				r.FailedObjects.AddRange(components);
				return r;
			}

			try
			{
				ChildComponents.AddRange(add);
				r.Messages.Add("{0} Added.".F(ChildTypeName));
				r.SuccessfulObjects.AddRange(add);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.FailedObjects.AddRange(components);
				return r;
			}

			if (add.Any() && add.Count() != components.Count())
			{
				r.Status = ResultStatus.SOME_FAILURE;
				r.Messages.Add("Some {0}s could not be added.".F(ChildTypeName));
				r.FailedObjects.AddRange(components.Where(c => ChildComponents.Contains(c)).Select(c => c));
				return r;
			}

			return r;
		}

		public virtual IResult<TChild, TChild> RemoveChildComponent(TChild component)
		{
			var r = new OperationResult<TChild, TChild> { Status = ResultStatus.SUCCESS, Result = component };

			if (!ChildComponents.Contains(component))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} does not contain {1}. Can not remove.".F(ComponentTypeName, ChildTypeName));
				r.FailedObjects.Add(component);
				return r;
			}

			try
			{
				ChildComponents.Remove(component);
				r.Messages.Add("{0} Removed.".F(ChildTypeName));
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.FailedObjects.Add(component);
				return r;
			}

			return r;
		}

		public virtual List<TChild> GetChildComponents() 
		{
			return ChildComponents; 
		}
	}
}
