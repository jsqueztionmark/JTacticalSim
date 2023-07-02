using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public enum PromptType
	{
		NONE,
		PRESS_ANY_KEY,
		YES_NO,
		YES_NO_CANCEL,
		SELECT_ITEM,
		OK_CANCEL,
		CTLX_ONLY
	}

	public enum BoxDisplayType : int
	{
		DISPLAY,
		TEXT,
		CMD,
		INFO,
		WARNING,
		ERROR
	}

	public enum CycleDirection
	{
		IN,
		OUT,
		LEFT,
		RIGHT,
		UP,
		DOWN
	}
}
