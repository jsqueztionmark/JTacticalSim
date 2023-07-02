using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public class ControlStack
	{
		public Stack<IConsoleControl> Controls { get; private set; }
		
		public ControlStack()
		{
			Controls = new Stack<IConsoleControl>();	
		}

		/// <summary>
		/// Checks if the control is already in the stack and adds if not
		/// </summary>
		/// <param name="control"></param>
		public void Push(IConsoleControl control)
		{
			if(Controls.Contains(control)) return;
			Controls.Push(control);
			ResetFocus();
		}

		public IConsoleControl Peek()
		{
			if(!Controls.Any()) return null;
			var retVal = Controls.Peek();
			return retVal;
		}

		public IConsoleControl Pop()
		{
			if(!Controls.Any()) return null;
			var retVal = Controls.Pop();
			ResetFocus();
			return retVal;
		}

		public void RenderControls()
		{
			if (!Controls.Any()) return;

			foreach (var control in Controls.Reverse())
				control.ClearAndRedraw();
		}

		public void ClearControls()
		{
			Controls.Clear();
		}

		private void ResetFocus()
		{
			foreach (var control in Controls.Reverse().Where(control => Controls.Any()))
			{
				// Set focus to the top control in the stack
				control.HasFocus = control.Equals(Controls.Peek());
			}
		}
	}
}
