using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.Game;
using JTacticalSim.Component.World;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.Data;

namespace JTacticalSim.LINQPad.Plugins
{
	public static class ScriptTestUnits
	{
		public static IUnit MEDSP_Tanks { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("medsp_tanks"); } }
		public static IUnit Tenth_Medical { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("tenth_medical"); } }
		public static IUnit FighterSquad_Z { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("fightersquad_z"); } }
		public static IUnit TestCavB { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("testcavb"); } }
		public static IUnit LOADME_HARMOR { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadme_harmor"); } }
		public static IUnit LOADME_LINFANTRY { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadme_linfantry"); } }
		public static IUnit LOADME_MINFANTRY { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadme_minfantry"); } }
		public static IUnit LOADME_FIGHTER { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadme_fighter"); } }
		public static IUnit LOADME_BOMBER { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadme_bomber"); } }
		public static IUnit LOADTO_HELO { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadto_helo"); } }
		public static IUnit LOADTO_TRANSPORT { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("loadto_transport"); } }
		public static IUnit LI_298 { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("li_298"); } }
		public static IUnit MI_HQ { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("mi_hq"); } }
		public static IUnit Navy_HQ { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("navy_hq"); } }
		public static IUnit Trans_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("trans_a"); } }
		public static IUnit Btl101 { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("101btl"); } }
		public static IUnit A_Tank { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("a_tank"); } }
		public static IUnit C_Tank { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("c_tank"); } }
		public static IUnit A_Infantry { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("a_infantry"); } }
		public static IUnit B_Infantry { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("b_infantry"); } }
		public static IUnit C_Infantry { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("c_infantry"); } }
		public static IUnit D_Infantry { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("d_infantry"); } }
		public static IUnit NinthMedical { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("9thmedical"); } }
		public static IUnit MedVac_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("medvac_a"); } }
		public static IUnit HellCats { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("hellcats"); } }
		public static IUnit B_Tank { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("b_tank"); } }
		public static IUnit BigGuns { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("bigguns"); } }
		public static IUnit Sam_Site_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("sam_site_a"); } }
		public static IUnit TestCav { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("testcav"); } }
		public static IUnit ScoutCavA { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("scoutCavA"); } }
		public static IUnit Snipers { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("snipers"); } }
		public static IUnit ANTISUB_FIGHTER { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("antisub_fighter"); } }

		public static IUnit SubSquad_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("subsquad_a"); } }
		public static IUnit ChopperSquad_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("choppersquad_a"); } }
		public static IUnit FighterSquad_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("fightersquad_a"); } }
		public static IUnit BomberSquad_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("bombersquad_a"); } }
		public static IUnit Nuc_Sub_A { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("nuc_sub_a"); } }
		public static IUnit Fadaykin_SF { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("fadaykin_sf"); } }
		public static IUnit A_SandyTanks { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("a_sandytanks"); } }
		public static IUnit Sandy_MLRS { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("sandy_mlrs"); } }
		public static IUnit Sam_Site { get { return Game.Instance.JTSServices.GenericComponentService.GetByName<Unit>("sam_site"); } }
	}
}
