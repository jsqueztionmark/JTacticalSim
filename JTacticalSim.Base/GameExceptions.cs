using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API
{
	public abstract class BaseGameException : Exception
	{
		protected BaseGameException()
		{}

		protected BaseGameException(string message)
			: base(message)
		{}

		protected BaseGameException(string message, Exception innerException)
			: base(message, innerException)
		{}
	}

	public class ComponentNotFoundException : BaseGameException
	{
		public ComponentNotFoundException()
		{ }

		public ComponentNotFoundException(string message)
			: base(message)
		{ }

		public ComponentNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}

	public class RulesViolationException : BaseGameException
	{
		public RulesViolationException()
		{ }

		public RulesViolationException(string message)
			: base(message)
		{ }

		public RulesViolationException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}

	public class DTONotFoundException : BaseGameException
	{
		public DTONotFoundException()
		{ }

		public DTONotFoundException(string message)
			: base(message)
		{ }

		public DTONotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}
