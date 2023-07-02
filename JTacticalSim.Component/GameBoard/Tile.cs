using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.Drawing;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.GameBoard
{
	public class Tile : BoardComponentBase, ITile
	{
		private List<IDemographic> _demographics;

		public List<IDemographic> BaseGeography { get { return _demographics.Where(d => d.IsDemographicType("BaseGeography")).ToList(); }}
		public List<IDemographic> Terrain { get { return _demographics.Where(d => d.IsDemographicType("Terrain")).ToList(); }}
		public List<IDemographic> AllGeography { get { return BaseGeography.Concat(Terrain).ToList(); }}
		public List<IDemographic> Infrastructure { get { return _demographics.Where(d => d.IsDemographicType("Infrastructure")).ToList(); }}
		public List<IDemographic> Flora { get { return _demographics.Where(d => d.IsDemographicType("Flora")).ToList(); } }
		public List<IDemographic> Population { get { return _demographics.Where(d => d.IsDemographicType("Population")).ToList(); } }

		public double NetStealthAdjustment { get; set; }
		public double NetAttackAdjustment { get; set; }
		public double NetDefenceAdjustment { get; set; }
		public int NetMovementAdjustment { get; set; }

		public TileConsoleRenderHelper ConsoleRenderHelper { get; private set; }

		public bool IsPrimeTarget { get; set; }
		public bool IsGeographicChokePoint { get; set; }
		public bool IsDestroyed { get; set; }
		public int VictoryPoints { get; set; }
		public int TotalUnitCount {	get	{ return _componentStacks.Sum(s => s.Value.GetAllUnits().Count); }	}

		/// <summary>
		/// Use Tile_[Location]
		/// </summary>
		public override string SpriteName
		{
			get { return "Tile_{0}".F(Location.ToStringForName()); }
		}

		private Dictionary<Guid, IUnitStack> _componentStacks { get; set; }

		public override ICountry Country
		{
			get	{ return this.GetNode().Country; }
			set { this.GetNode().Country = value; }
		}

		// ----------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Base geography is required for the tile
		/// this paramater can be null for the purposes of conversion
		/// </summary>
		/// <param name="location"></param>
		/// <param name="baseGeography"></param>
		public Tile(ICoordinate location, IDemographic baseGeography)
		{
			ConsoleRenderHelper = new TileConsoleRenderHelper();

			if (baseGeography != null && !baseGeography.IsDemographicType("BaseGeography"))
				throw new RulesViolationException("Tile's baseGeography must be a configured base geography type");

			this._demographics = new List<IDemographic>();			
			if (baseGeography != null) this.AddDemographic(baseGeography);

			this.Location = location;
			this._componentStacks = new Dictionary<Guid, IUnitStack>();
		}
		
	// Moved from extensions ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		public void Render(int zoomLevel)
		{
			TheGame().Renderer.On_TilePreRender(new EventArgs());
			TheGame().Renderer.RenderTile(this, zoomLevel);
			TheGame().Renderer.On_TilePostRender(new EventArgs());

		}

		public void DisplayInfo()
		{
			TheGame().Renderer.DisplayTileInfo(this);
		}

		public StrategicAssessmentInfo GetStrategicValues()
		{
			var r = TheGame().JTSServices.AIService.DetermineTileStrategicValue(this, this.TheGame().GameBoard.StrategicValuesAttributes);
			if (r.Status == ResultStatus.EXCEPTION) throw r.ex;

			return r.Result;
		}

		public StrategicAssessmentRating GetNetStrategicValue()
		{
			var r = TheGame().JTSServices.RulesService.GetOverallRatingForStrategicAssessment(this.GetStrategicValues());
			if (r.Status == ResultStatus.EXCEPTION) throw r.ex;
			HandleResultDisplay(r, true);

			// If this is a prime target, make it so
			if (this.IsPrimeTarget && r.Result < StrategicAssessmentRating.VERYHIGH)
				r.Result = StrategicAssessmentRating.VERYHIGH;

			return r.Result;
		}

		public int VisibleStackCount() { return _componentStacks.Count(cs => cs.Value.HasVisibleComponents); }

		public IResult<IDemographic, IDemographic> AddDemographic(IDemographic demographic)
		{
			var r = new OperationResult<IDemographic, IDemographic>{Status = ResultStatus.SUCCESS};

			using (var txn = new TransactionScope())
			{
				try
				{
					this._demographics.Add(demographic);
					this._demographics = this._demographics.EnsureUnique().ToList();
					r.SuccessfulObjects.Add(demographic);
					txn.Complete();
				}
				catch (Exception ex)
				{
					r.FailedObjects.Add(demographic);
					r.ex = ex;
					r.Status = ResultStatus.EXCEPTION;
				}
			}			

			return r;
		}

		public IResult<IDemographic, IDemographic> RemoveDemographic(IDemographic demographic)
		{
			var r = new OperationResult<IDemographic, IDemographic> {Status = ResultStatus.SUCCESS};

			using (var txn = new TransactionScope())
			{
				try
				{
					this._demographics.Remove(demographic);
					this._demographics = this._demographics.EnsureUnique().ToList();
					r.SuccessfulObjects.Add(demographic);
					txn.Complete();
				}
				catch (Exception ex)
				{
					r.FailedObjects.Add(demographic);
					r.ex = ex;
					r.Status = ResultStatus.EXCEPTION;
				}
			}

			return r;
		}

		public IResult<IDemographic, IDemographic> RemoveDirectionFromDemographicOrientation(IDemographic demographic, Direction direction)
		{
			var r = new OperationResult<IDemographic, IDemographic> {Status = ResultStatus.SUCCESS};
			
			var removeFrom = _demographics.Where(d => d.Equals(demographic)).SingleOrDefault();

			if (removeFrom == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Failed to find {0} for tile.".F(demographic.Name));
				r.FailedObjects.Add(demographic);
				return r;
			}

			using (var txn = new TransactionScope())
			{
				try
				{
					removeFrom.Orientation.Remove(direction);
					ReCalculateTileInfo();
					r.SuccessfulObjects.Add(demographic);
					txn.Complete();
				}
				catch (Exception ex)
				{
					r.FailedObjects.Add(demographic);
					r.ex = ex;
					r.Status = ResultStatus.EXCEPTION;
				}
			}

			return r;
		}

		public List<string> GetAllDemographicNames()
		{
			var names = new List<string>();
			foreach (var d in GetAllDemographics())
			{
				if (!string.IsNullOrWhiteSpace(d.InstanceName) && d.InstanceName != Name) 
					names.Add(d.InstanceName);
			}
			return names;
		}

		public IResult<ITile, ITile> ReCalculateTileInfo()
		{
			var r = new OperationResult<ITile, ITile> {Status = ResultStatus.SUCCESS};

			try
			{
				CalculateTileModifiers();
				SetTileConsoleHelperValues();
				SetTileDestroyed();
				r.SuccessfulObjects.Add(this);
			}
			catch (Exception ex)
			{
				r.FailedObjects.Add(this);
				r.ex = ex;
				r.Status = ResultStatus.EXCEPTION;
			}

			return r;
		}

		private void CalculateTileModifiers()
		{
			NetAttackAdjustment = GetAllDemographics().Sum(d => d.DemographicClass.AttackModifier);
			NetDefenceAdjustment = GetAllDemographics().Sum(d => d.DemographicClass.DefenceModifier);
			NetStealthAdjustment = GetAllDemographics().Sum(d => d.DemographicClass.StealthModifier);
			NetMovementAdjustment = Convert.ToInt32(Math.Round(GetAllDemographics().Sum(d => d.DemographicClass.MovementModifier)));
			// No positive movement adjustment - only hinderance
			NetMovementAdjustment = (NetMovementAdjustment > 0) ? 0 : NetMovementAdjustment;
		}

		private void SetTileConsoleHelperValues()
		{
			ConsoleRenderHelper.HasMountains = AllGeography.Any(d => d.IsDemographicClass("Mountains"));
			ConsoleRenderHelper.HasMountain = AllGeography.Any(d => d.IsDemographicClass("Mountain"));
			ConsoleRenderHelper.HasHills = AllGeography.Any(d => d.IsDemographicClass("Hills"));
			ConsoleRenderHelper.HasLakes = AllGeography.Any(d => d.IsDemographicClass("Lake"));
			ConsoleRenderHelper.HasRivers = AllGeography.Any(d => d.IsDemographicClass("River"));
			ConsoleRenderHelper.HasCreeks = AllGeography.Any(d => d.IsDemographicClass("Creek"));
			ConsoleRenderHelper.HasShoreLineNorth = AllGeography.Any(d => d.IsDemographicClass("ShoreLineNorth"));
			ConsoleRenderHelper.HasShoreLineSouth = AllGeography.Any(d => d.IsDemographicClass("ShoreLineSouth"));
			ConsoleRenderHelper.HasShoreLineEast = AllGeography.Any(d => d.IsDemographicClass("ShoreLineEast"));
			ConsoleRenderHelper.HasShoreLineWest = AllGeography.Any(d => d.IsDemographicClass("ShoreLineWest"));
			ConsoleRenderHelper.HasShoreLineNorthWest = AllGeography.Any(d => d.IsDemographicClass("ShoreLineNorthWest"));
			ConsoleRenderHelper.HasShoreLineSouthWest = AllGeography.Any(d => d.IsDemographicClass("ShoreLineSouthWest"));
			ConsoleRenderHelper.HasShoreLineNorthEast = AllGeography.Any(d => d.IsDemographicClass("ShoreLineNorthEast"));
			ConsoleRenderHelper.HasShoreLineSouthEast = AllGeography.Any(d => d.IsDemographicClass("ShoreLineSouthEast"));

			ConsoleRenderHelper.IsSea = AllGeography.Any(d => d.IsDemographicClass("Sea"));
			ConsoleRenderHelper.IsNuclearWasteland = AllGeography.Any(d => d.IsDemographicClass("NuclearWasteland"));

			ConsoleRenderHelper.IsRiver = (ConsoleRenderHelper.HasRivers ||
												ConsoleRenderHelper.HasShoreLineNorth ||
												ConsoleRenderHelper.HasShoreLineEast ||
												ConsoleRenderHelper.HasShoreLineSouth ||
												ConsoleRenderHelper.HasShoreLineWest ||
												ConsoleRenderHelper.HasShoreLineNorthEast ||
												ConsoleRenderHelper.HasShoreLineNorthWest ||
												ConsoleRenderHelper.HasShoreLineSouthEast ||
												ConsoleRenderHelper.HasShoreLineSouthWest);

			ConsoleRenderHelper.HasForests = Flora.Any(d => d.IsDemographicClass("Forested"));
			ConsoleRenderHelper.HasWoodlands = Flora.Any(d => d.IsDemographicClass("Woodland"));
			ConsoleRenderHelper.HasMarsh = Flora.Any(d => d.IsDemographicClass("Marsh"));
			ConsoleRenderHelper.HasTrees = Flora.Any(d => d.IsDemographicClass("Trees"));

			ConsoleRenderHelper.HasMilitaryBase = Infrastructure.Any(d => d.IsDemographicClass("MilitaryBase"));
			ConsoleRenderHelper.HasCommandPost = Infrastructure.Any(d => d.IsDemographicClass("CommandPost") && d.Orientation.Any());
			ConsoleRenderHelper.HasAirports = Infrastructure.Any(d => d.IsDemographicClass("Airport") && d.Orientation.Any());
			ConsoleRenderHelper.HasCities = Infrastructure.Any(d => d.IsDemographicClass("Urban"));
			ConsoleRenderHelper.HasTown = Infrastructure.Any(d => d.IsDemographicClass("Town") && d.Orientation.Any());
			ConsoleRenderHelper.HasIndustrial = Infrastructure.Any(d => d.IsDemographicClass("Industrial"));
			ConsoleRenderHelper.HasRoad = Infrastructure.Any(d => d.IsDemographicClass("Road") && d.Orientation.Any());
			ConsoleRenderHelper.HasBridge = Infrastructure.Any(d => d.IsDemographicClass("Bridge") && d.Orientation.Any());
			ConsoleRenderHelper.HasDam = Infrastructure.Any(d => d.IsDemographicClass("Dam") && d.Orientation.Any());
			ConsoleRenderHelper.HasTracks = Infrastructure.Any(d => d.IsDemographicClass("TrainTrack") && d.Orientation.Any());

		}

		private void SetTileDestroyed()
		{
			IsDestroyed = ConsoleRenderHelper.IsNuclearWasteland;	
		}

		public void SetTileName()
		{
			if (string.IsNullOrEmpty(Name))
			{
				// check first for named towns/urban areas
				var urban = Infrastructure
								.Where(i => !string.IsNullOrEmpty(i.InstanceName))
								.FirstOrDefault(i =>	i.IsDemographicClass("Urban") || 
														i.IsDemographicClass("Town") || 
														i.IsDemographicClass("MilitaryBase") ||
														i.IsDemographicClass("Airport"));
				if (urban != null)
				{
					Name = urban.InstanceName;
					return;
				}
 					

				// next, check for named roads, bridges or dams
				var otherInfrastructure = Infrastructure
											.Where(i => !string.IsNullOrEmpty(i.InstanceName))
											.FirstOrDefault(i =>	i.IsDemographicClass("Road") || 
																	i.IsDemographicClass("Bridge") || 
																	i.IsDemographicClass("Dam"));
				if (otherInfrastructure != null)
				{
					Name = otherInfrastructure.InstanceName;
					return;
				}
 					

				// next, check for named geography
				var geography = AllGeography.FirstOrDefault(i => !string.IsNullOrEmpty(i.InstanceName));
				if (geography != null)
 					Name = geography.InstanceName;
			}
		}


		public List<IDemographic> GetAllDemographics() { return AllGeography.Concat(Infrastructure).Concat(Flora).ToList(); }

		public List<IDemographic> GetAllHybridDemographics() { return this.GetAllDemographics().Where(d => d.IsHybrid()).ToList(); }

		public IUnitStack GetCountryComponentStack(ICountry country)
		{
			if (!_componentStacks.ContainsKey(country.UID))
				_componentStacks.Add(country.UID, new UnitStack(country, this.Location));

			return _componentStacks[country.UID]; 
		}

		public List<IUnitStack> GetAllComponentStacks() { return _componentStacks.Select(cs => cs.Value).ToList(); }

		public IResult<IUnit, IUnit> AddComponentsToStacks(IEnumerable<IUnit> components)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			try
			{
				Action<IUnit> componentAction = c =>
				{
					lock (_componentStacks)
					{
						GetCountryComponentStack(c.Country).AddUnit(c);
						r.SuccessfulObjects.Add(c);
					}					
				};

				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(components, componentAction);
				}
				else
				{
					foreach(var c in components)
						componentAction(c);
				}
			}
			catch (Exception ex)
			{
				r.Status = API.ResultStatus.EXCEPTION;
				r.Messages.Add(ex.Message);
				r.ex = ex;
			}

			if (!r.SuccessfulObjects.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No units were added to unit stacks for the current tile.");
			}
				
			return r;
		}

		public IResult<IUnit, IUnit> RemoveComponentsFromStacks(IEnumerable<IUnit> components)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			Action<IUnit> componentAction = c =>
			{
				lock (_componentStacks)
				{
					if (_componentStacks[c.Country.UID] == null)
					{
						r.Status = API.ResultStatus.SOME_FAILURE;
						r.FailedObjects.Add(c);
						r.Messages.Add("Component stack for {0} does not exist at the current tile.".F(c.Country.Name));
					}

					GetCountryComponentStack(c.Country).RemoveUnit(c);
					//_componentStacks[c.Country.UID].RemoveUnit(c);
					r.SuccessfulObjects.Add(c);
				}
				
			};

			try
			{
				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(components, componentAction);
				}
				else
				{
					foreach(var c in components)
					{
						componentAction(c);
					}
				}
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.Messages.Add(ex.Message);
				r.ex = ex;
			}

			if (!r.SuccessfulObjects.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No units were removed from unit stacks for the current tile.");
			}
				
			return r;
		}

		public IResult<IUnit, IUnit> RefreshComponentStacks()
		{
		    var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

		    // Get all components to add to stacks
		    var units = TheGame().JTSServices.UnitService.GetAllUnitsAt(this.Location);
			
		    // Clear out the current stacks
		    _componentStacks.ToList().ForEach(kvp => kvp.Value.ClearUnits());

		    r = (OperationResult<IUnit, IUnit>)AddComponentsToStacks(units);
				
		    return r;
		}

		public void ResetComponentStackDisplayOrder()
		{
			int i = 1;

			GetAllComponentStacks().Where(cs => cs.HasVisibleComponents).ToList().ForEach(cs =>
			{
				cs.DisplayOrder = i; i++;
			});
		}


		// Rules

		public bool CanSupportInfrastructureBuilding(IDemographic demographic)
		{
			var r = TheGame().JTSServices.RulesService.TileCanSupportInfrastructureBuilding(this, demographic);

			if (r.Status == ResultStatus.EXCEPTION)
				throw r.ex;

			return r.Result;
		}
	}
}
