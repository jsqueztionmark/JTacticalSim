﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IDemographicType : IBaseComponent
	{
		int DisplayOrder { get; set; }
	}
}
