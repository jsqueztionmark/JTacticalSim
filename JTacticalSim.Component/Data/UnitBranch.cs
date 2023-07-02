﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.Data
{
	public class UnitBranch : GameComponentBase, IUnitBranch
	{
		public string SpriteName { get { return "branch_{0}".F(this.Name); }}
	}
}
