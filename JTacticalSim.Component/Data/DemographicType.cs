﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;

namespace JTacticalSim.Component.Data
{
	public class DemographicType : GameComponentBase, IDemographicType
	{
		public int DisplayOrder { get; set; }
	}
}