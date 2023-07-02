using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.Service
{
	public static class Extension
	{
		public static void Sort(this Queue<INode> queue)
		{
			if (!queue.Any())
				return;

			var tmp = queue.ToList();
			queue.Clear();
			tmp.Sort(NodeComparer);
			tmp.ForEach(queue.Enqueue);
		}

		public static void ReverseSort(this Queue<INode> queue)
		{
			if (!queue.Any())
				return;

			var tmp = queue.ToList();
			queue.Clear();
			tmp.Sort(ReverseNodeComparer);
			tmp.ForEach(queue.Enqueue);
		}

		private static int NodeComparer(INode n1, INode n2)
		{
			if (n1 == null || n2 == null)
				return -1;
			return Convert.ToDouble(n1.H).CompareTo(Convert.ToDouble(n2.H));
		}

		private static int ReverseNodeComparer(INode n1, INode n2)
		{
			if (n1 == null || n2 == null)
				return -1;
			return Convert.ToDouble(n2.H).CompareTo(Convert.ToDouble(n1.H));
		}
	}
}
