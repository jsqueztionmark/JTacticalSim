using System;
using System.Collections;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.ServiceModel;
using System.Reflection;
using System.Web.Compilation;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.API.Game;
using JTacticalSim.AI;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.Data;
using JTacticalSim.Utility;
using JTacticalSim.DataContext;
using ctxUtil = JTacticalSim.DataContext.Utility;
using JTacticalSim.Component.GameBoard;


namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class AIService : BaseGameService, IAIService
	{
		static readonly object padlock = new object();

		private static volatile IAIService _instance;
		public static IAIService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new AIService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private AIService()
		{}

#region Service Methods

	// Unit Orders

		[OperationBehavior]
		public IResult<IUnit, IUnit> DeployUnitsFromTransportToNode(IUnit transport, IEnumerable<IUnit> units, INode destinationNode)
		{
			var r = new OperationResult<IUnit, IUnit>{Status = ResultStatus.SUCCESS};
			var currentNode = transport.GetNode();

			// Deployment node must be only 1 space from water transport : same space for other types
			// Units can be moved after if they have remaining movement points
			var deployDistance = RulesService.Instance.GetAllowableDeployDistanceForTransport(transport).Result;
			
			var distanceToNodeResult = CalculateNodeCountToNode(currentNode, destinationNode, deployDistance);
			if (distanceToNodeResult.Status == ResultStatus.FAILURE)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Deployment node is not within allowed deployment distance.");
				r.FailedObjects.AddRange(units);
				return r;
			}

			if (!transport.GetTransportedUnits().Any())
			{
			    r.Status = ResultStatus.FAILURE;
			    r.Messages.Add("No units currently transported.");
			    return r;
			}

			using (var txn = new TransactionScope())
			{
				Action<IUnit> componentAction = u =>
				{
					// Deploy the unit if the destination node is compatible and
					// the unit is actually being transported,
					if (!transport.GetTransportedUnits().Any(tu => tu.Equals(u)) || !TheGame().JTSServices.RulesService.UnitIsDeployableToNode(u, destinationNode).Result)
					{
						r.FailedObjects.Add(u);
						return;
					}
				
					u.LoadToLocation(destinationNode, currentNode);
					r.SuccessfulObjects.Add(u);
					// Remove from the transport table
					TheGame().JTSServices.DataService.RemoveUnitTransport(u, transport);
				};

				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(units, componentAction);
				}
				else
				{
					units.ToList().ForEach(componentAction);
				}

				TheGame().JTSServices.UnitService.UpdateUnits(new List<IUnit> { transport });

				txn.Complete();
			}

			if (!r.SuccessfulObjects.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No units were deployed due to incompatibility with the deployment node.");
				return r;
			}

			// If there are any failed objects and some successful objects return a SOME_FAILURE result
			if (r.FailedObjects.Any() && r.SuccessfulObjects.Any())
			{
				r.Status = ResultStatus.SOME_FAILURE;
				r.Messages.Add("Some units were not deployed due to incompatibility with the deployment node.");
				return r;
			}

			r.Messages.Add("All units deployed.");
			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> LoadUnitsToTransport(IUnit transport, List<IUnit> units)
		{
			var r = new OperationResult<IUnit, IUnit>{Status = ResultStatus.SUCCESS};
			INode transportNode = transport.GetNode();

			// units to load must be only 1 space from water transport : same space for other types
			// Units can be moved before they're loaded onto the transport
			var loadDistance = RulesService.Instance.GetAllowableLoadDistanceForTransport(transport).Result;
			var distanceToNodeResult = CalculateNodeCountToNode(units.FirstOrDefault().GetNode(), transportNode, loadDistance);
			
			if (distanceToNodeResult.Status == ResultStatus.FAILURE)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Units are not within maximum load distance of transport.");
				r.FailedObjects.AddRange(units);
				return r;
			}

			// Prune incompatible units for this transport
			units.RemoveAll(u => !u.IsCompatibleWithTransport(transport));

			if (!units.Any())
			{
			    r.Status = ResultStatus.FAILURE;
			    r.Messages.Add("Selected units are not compatible with the selected transport.");
				r.FailedObjects.AddRange(units);
			    return r;
			}

			// Prune any units already transporting other units
			units.RemoveAll(u => u.GetTransportedUnits().Any());

			if (!units.Any())
			{
			    r.Status = ResultStatus.FAILURE;
			    r.Messages.Add("Selected units are currently transporting other units and can not be loaded on this transport.");
				r.FailedObjects.AddRange(units);
			    return r;
			}

			// Validate weight limits
			var totalTransportWeightToLoad = units.Sum(u => u.TotalUnitWeight);
			var totalTransportWeight = totalTransportWeightToLoad + (transport.GetTransportedUnits().Sum(u => u.GetWeight()));
			
			if (transport.GetAllowableTransportWeight() < totalTransportWeight)
			{
			    r.Status = ResultStatus.FAILURE;
			    r.Messages.Add("Total combined unit weight exceeds allowable transport weight.");
				r.FailedObjects.AddRange(units);
			    return r;
			}

			using (var txn = new TransactionScope())
			{
				Action<IUnit> componentAction = u =>
				{
					if (!TheGame().JTSServices.RulesService.UnitCanTransportUnitTypeAndClass(transport, u).Result)
					{
						r.FailedObjects.Add(u);
						return;
					}

					u.LoadToLocation(transportNode, u.GetNode());
					TheGame().JTSServices.DataService.SaveUnitTransport(u, transport);
					r.SuccessfulObjects.Add(u);
				};

				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(units, componentAction);
				}
				else
				{
					units.ForEach(u =>
						{
							componentAction(u);
						});
				}

				TheGame().JTSServices.UnitService.UpdateUnits(new List<IUnit> { transport });

				txn.Complete();

			}
			

			if (!r.SuccessfulObjects.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No units were loaded due to incompatiblity with the transport type.");
				return r;
			}

			// If there are any failed objects and some successful objects return a SOME_FAILURE result
			if (r.FailedObjects.Any() && r.SuccessfulObjects.Any())
			{
				r.Status = ResultStatus.SOME_FAILURE;
				r.Messages.Add("Some units were not loaded due to incompatibility with the transport type.");
				return r;
			}

			r.Messages.Add("All units loaded.");
			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> AttachUnitToUnit (IUnit parent, IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit>{Status = ResultStatus.SUCCESS, Result = unit};

			// First, check that we can attach to the parent
			var canAttach = TheGame().JTSServices.RulesService.UnitCanAttachToUnit(unit, parent);
			
			if (!canAttach.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(canAttach.Message);
				r.FailedObjects.Add(unit);
				return r;
			}

			// First, make sure we're not already attached to a unit (can only be attached to one)
			if (unit.AttachedToUnit != null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit already attached. You must detach from the current unit first.");
				r.FailedObjects.Add(unit);
				return r;
			}

			var saveResult = TheGame().JTSServices.DataService.SaveUnitAssignment(unit, parent);
			r.ConvertResultData(saveResult);

			r.Messages.Add("Unit attached.");
			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> DetachUnitFromUnit(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit>{Status = ResultStatus.SUCCESS, Result = unit};

			// Can only detach if we are, indeed, attached. Makes sense...
			if (unit.AttachedToUnit == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit not currently attached.");
				r.FailedObjects.Add(unit);
				return r;
			}

			var saveResult = TheGame().JTSServices.DataService.RemoveUnitAssignment(unit, unit.AttachedToUnit);
			r.ConvertResultData(saveResult);

			r.Messages.Add("Unit detached.");
			return r;

		}

	// Pathfinding


		/// <summary>
		/// A* Implementation
		/// </summary>
		/// <param name="source"></param>
		/// <param name="faction"></param>
		/// <param name="supplyDistance"></param>
		/// <returns></returns>
		[OperationBehavior]
		public RouteInfo FindSupplyPath(INode source, IFaction faction, int supplyDistance)
		{
			// Use only friendly nodes
			var map = TheGame().JTSServices.NodeService.GetAllNodesWithinDistance(source, supplyDistance, false, false)
												.Where(n => n.Country.Faction.Equals(faction));

			// First, check to see if we have any targets
			var targetNodes = (from node in map 
							   let units = TheGame().JTSServices.UnitService.GetUnitsAt(node.Location, new[] {faction}) 
							   let demographics = node.DefaultTile.GetAllDemographics().Where(d => d.ProvidesMedical)
							   where units.Any(u => u.IsUnitClass("supply")) || demographics.Any(d => d.ProvidesSupply)
							   select node).ToList();

			// Skip if we don't have any supply units
			if (!targetNodes.Any())
				return null;

			var paths = new List<RouteInfo>();

			Action<INode> componentAction = n =>
			{
				paths.Add(FindPathToSupplyTarget(source, n, map, supplyDistance));
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(targetNodes, componentAction);	
			}
			else
			{
				targetNodes.ForEach(n =>
					{
						componentAction(n);
					});
			}

			return paths.FirstOrDefault(p => p != null);
		}


		private RouteInfo FindPathToSupplyTarget(INode source, 
												INode target, 
												IEnumerable<INode> map,
												int supplyDistance)
		{
			source.Target = target;

			var openList = new Queue<INode>(); 
			var closedList = new Queue<INode>();
	
			openList.Enqueue(source);

			while (openList.Any())
			{
				// Pushes the Lower F Value nodes to the top.
				openList.Sort();

				var currentNode = openList.Dequeue();

				currentNode.Source = source;

				// Check each node if it is faction for the path we're looking for
				// and see if it has any units
				if (currentNode.Equals(target))
				{
					return new RouteInfo(ReconstructPath(currentNode), source, target);
				}

				//openList.Remove(currentNode);
				closedList.Enqueue(currentNode);

				// Get all surrounding nodes that are also in the master move map
				var neighborNodes = currentNode.NeighborNodes.Select(nnInfo => nnInfo.Node)
										.Where(n => map.Any(mn => mn.Equals(n)))
										.Where(n => n.G <= supplyDistance)
										.ToList();

				var keepNodes = neighborNodes.Where(n => !Enumerable.Contains(closedList, n));

				foreach (var n in keepNodes)
				{
					n.Parent = currentNode;
					n.Source = source;
					n.Target = target;

					openList.Enqueue(n);
				}
			}

			// No available path
			return null;
		}


		/// <summary>
		/// A* Implementation
		/// The map param has already been pruned to include only traverseable nodes for the unit
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="map"></param>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationBehavior]
		public IResult<RouteInfo, IMoveableComponent> FindPath(INode source, 
																INode target, 
																IEnumerable<IPathableObject> map,
																IUnit unit)
		{

			var r = new OperationResult<RouteInfo, IMoveableComponent> {Status = ResultStatus.SUCCESS};

			//If the selected node is outside the movement bounds for the unit, save some processing
			if (!map.Any(n => n.Equals(target)))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("target node is not in searchable nodes.");
				r.Result = null;
				return r;
			}

			if (!TheGame().JTSServices.RulesService.TileIsAllowableForUnit(unit, target.DefaultTile).Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("unit can not occupy the target node.");
				r.Result = null;
				return r;
			}

			try
			{
				source.Target = target;
				var openList = new Queue<INode>(); 
				var closedList = new Queue<INode>();

				var mapList = map.ToList();
				mapList.PruneMapForPathFinding(target, source, unit.CurrentMoveStats.MovementPoints);

				openList.Enqueue(source);

				while (openList.Any())
				{
					openList.Sort();

					var currentNode = openList.Dequeue();
					currentNode.Source = source;
					
					// Magic sauce. We've reached the target, return the path
					if (currentNode.Equals(target))
					{
						r.Result = new RouteInfo(ReconstructPath(currentNode), source, target);
						return r;
					}

					closedList.Enqueue(currentNode);
					mapList.Remove(currentNode);

					var allowable = TheGame().JTSServices.NodeService.GetAllowableNeighborNodesForGrid(unit, currentNode, mapList).ToArray();

					Action<Tuple<INode, bool>> nodeAction = t =>
						{
							t.Item1.Parent = currentNode;
							t.Item1.Source = currentNode.Source;
							t.Item1.Target = currentNode.Target;
						};

					if (TheGame().IsMultiThreaded)
					{
						Parallel.ForEach(allowable, nodeAction);
					}
					else
					{
						foreach (var t in allowable)
						{
							nodeAction(t);
						}
					}

					foreach (var n in allowable)
					{
						n.Item1.Parent = currentNode;
						n.Item1.Source = currentNode.Source;
						n.Item1.Target = currentNode.Target;
					}

					var neighborNodes = allowable.Where(n =>
						{							
							var targetMod = (n.Item2 || unit.UnitInfo.UnitType.HasGlobalMovementOverride) ? 0 : n.Item1.DefaultTile.NetMovementAdjustment;
							var retVal = (n.Item1.G <= (unit.CurrentMoveStats.MovementPoints + targetMod));
							return retVal;
						});

					var keepNodes = neighborNodes.Where(n => !closedList.Contains(n.Item1));

					foreach (var n in keepNodes)
						openList.Enqueue(n.Item1);
				}
			}
			catch (Exception ex)
			{
				r.ex = ex;
				r.Status = ResultStatus.EXCEPTION;
				r.Result = null;
				return r;
			}

			// No available path
			r.Status = ResultStatus.FAILURE;
			r.Result = null;
			r.Messages.Add("No Available path from {0} to {1} for unit {2}".F(source.Location.ToStringForName(), 
																			target.Location.ToStringForName(), 
																			unit.Name));
			return r;
		}


		[OperationBehavior]
		public IResult<int?, INode> CalculateNodeCountToNode(INode source, INode target, int maxDistance)
		{
			var r = new OperationResult<int?, INode>{Status = ResultStatus.SUCCESS};

			for (var i = 0; i <= maxDistance; i++)
			{
				var nodesToSearch = TheGame().JTSServices.NodeService.GetNodesAtDistance(source.GetNode(), i);
				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(nodesToSearch, n =>
						{
							if (n.LocationEquals(target.Location)) r.Result = i;
						});
				}
				else
				{
					foreach (var n in nodesToSearch)
					{
						if (n.Equals(target)) r.Result = i;
					}
				}
			}

			// Unit was not within the max distance
			if (r.Result == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Target was not within the provided distance.");
			}
			return r;
		}

		[OperationBehavior]
		public IResult<int?, INode> CalculateNodeCountToUnit(IUnit source, IUnit target, int maxDistance)
		{
			var r = new OperationResult<int?, INode>{Status = ResultStatus.SUCCESS};

			for (var i = 1; i <= maxDistance; i++)
			{
				var nodesToSearch = TheGame().JTSServices.NodeService.GetNodesAtDistance(source.GetNode(), i);
				
				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(nodesToSearch, n =>
						{
							var units = TheGame().JTSServices.UnitService.GetUnitsAt(n.Location, new[] { target.Country.Faction });
							if (units.Any(u => u.Equals(target)))
							{
								r.Result = i;
							}
						});
				}
				else
				{
					foreach (var n in nodesToSearch)
					{
						var units = TheGame().JTSServices.UnitService.GetUnitsAt(n.Location, new[] { target.Country.Faction });
						if (units.Any(u => u.Equals(target)))
						{
							r.Result = i;
						}
					}
				}
			}

			// Unit was not within the max distance
			if (r.Result == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Target was not within the provided distance.");
			}
			return r;
		}


	// Battle 

		[OperationBehavior]
		public IResult<IUnit, IUnit> GetPrimeUnitTargetForUnit(List<IUnit> candidates, 
																IUnit attacker, 
																BattleType battleType)
		{
			// Based solely on whether the unit can do battle and the highest unit target desirability
			// which is highly based on unit strength.
			// This SHOULD work as the current attacker should be the strongest in the queue.....
			// but....
			// TODO: This may need to change.....

			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			try
			{
				// pull only defenders that this attacker can do battle with (attacking)
				var shortList = candidates.Where(u => TheGame().JTSServices.RulesService.UnitCanDoBattleWithUnit(attacker, u, battleType).Result).ToList();
			
				// No targets, return
				if (shortList.Count == 0)
				{
					r.Status = ResultStatus.FAILURE;
					r.Messages.Add("No suitable target found for {0}".F(attacker.Name));
					return r;
				}

				// Remote battle attackers will target other remote types with remaining fire points first
				// TODO: this should really be getting the unit with the max remaining fire points
				// If none are found, then it gets the best target as normal.
				if (battleType == BattleType.BARRAGE)
				{
					var remoteUnit = shortList.OrderByDescending(u => u.GetUnitTargetDesirabilityFactor())
										.FirstOrDefault(u => u.CurrentMoveStats.RemoteFirePoints > 0);
					
					r.Result = remoteUnit; 
				}

				if (r.Result == null)
					r.Result = shortList.OrderByDescending(u => u.GetUnitTargetDesirabilityFactor()).FirstOrDefault();

			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}

			return r;
		}
		
		[OperationBehavior]
		public IResult<List<ISkirmish>, ISkirmish> CreateFullCombatSkirmishes(IBattle battle)
		{
			var attackers = new List<IUnit>();
			var defenders = new List<IUnit>();

			battle.Attackers.ForEach(attackers.Add);
			battle.Defenders.ForEach(defenders.Add);

			var r = new OperationResult<List<ISkirmish>, ISkirmish>
				{
					Status = ResultStatus.SUCCESS, 
					Result = new List<ISkirmish>()
				};

			// ---------------------------------------------------------------
			// Handle remote attacks
			// ---------------------------------------------------------------
			if (battle.BattleType == BattleType.BARRAGE)
			{
				// Remove any attackers that can no longer fire
				foreach (var u in attackers.ToArray())
				{
					if (u.CurrentMoveStats.RemoteFirePoints == 0) attackers.Remove(u);
				}
			}

			// ------------------------------------------------------------------
			// Handle removing all attackers that have already engaged this turn
			// ------------------------------------------------------------------
			foreach (var u in attackers.ToArray())
			{
				if (!u.CanDoBattleThisTurn()) attackers.Remove(u);
			}

			attackers.OrderByDescending(u => u.GetUnitTargetDesirabilityFactor());

			// make sure the attackers are ordered by strength to get them in highest to lowest order
			Action<IUnit> componentAction = a =>
			{
				lock (defenders)
				{
					var defenderResult = GetPrimeUnitTargetForUnit(defenders, a, battle.BattleType);

					if (defenderResult.Status == ResultStatus.SUCCESS)
					{
						// a suitable defender was found
						var s = new Skirmish(a, defenderResult.Result, battle, SkirmishType.FULL);
						defenders.Remove(defenderResult.Result);
						r.Result.Add(s);
					}
				}
			};

			// Create as many skirmishes as possible
			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(attackers, componentAction);				
			}
			else
			{
				foreach (var a in attackers)
				{
					componentAction(a);
				}
			}

			// If none could be created, battle is done
			if (!r.Result.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No skirmishes could be created with the provided units.");
			}

			return r;
		}

		[OperationBehavior]
		public IResult<List<ISkirmish>, ISkirmish> CreateAirDefenceSkirmishes(IBattle battle)
		{
			var attackers = new List<IUnit>();
			var defenders = new List<IUnit>();

			battle.Defenders.ForEach(defenders.Add);
			battle.Attackers.ForEach(attackers.Add);

			// Keep only our attacking air units
			attackers.RemoveAll(u => !(u.IsUnitBaseType("plane") || u.IsUnitBaseType("helicopter")));
			defenders.RemoveAll(u => !(u.IsUnitBaseType("AirDefence")));

			return CreateSpecialDefenceSkirmishes(attackers, defenders, battle, SkirmishType.AIR_DEFENCE);
		}

		[OperationBehavior]
		public IResult<List<ISkirmish>, ISkirmish> CreateMissileDefenceSkirmishes(IBattle battle)
		{
			var attackers = new List<IUnit>();
			var defenders = new List<IUnit>();

			battle.Defenders.ForEach(defenders.Add);
			battle.Attackers.ForEach(attackers.Add);

			// Keep only our attacking air units
			attackers.RemoveAll(u => !(u.IsUnitBaseType("missile")));

			return CreateSpecialDefenceSkirmishes(attackers, defenders, battle, SkirmishType.MISSILE_DEFENCE);
			
		}

		[OperationBehavior]
		private IResult<List<ISkirmish>, ISkirmish> CreateSpecialDefenceSkirmishes(	List<IUnit> attackers, 
																					List<IUnit> defenders, 
																					IBattle battle,
																					SkirmishType skirmishType)
		{
			var r = new OperationResult<List<ISkirmish>, ISkirmish>
				{
					Status = ResultStatus.SUCCESS, 
					Result = new List<ISkirmish>()
				};


			// Remove all defenders that can not do battle with the attackers
			defenders.RemoveAll(d => attackers.Any(a => !TheGame().JTSServices.RulesService.UnitCanDoBattleWithUnit(d, a, battle.BattleType).Result));

			// Create as many skirmishes as possible
			Action<IUnit> componentAction = a =>
			{
				lock (defenders)
				lock (attackers)
				{
					foreach (var d in defenders.ToArray())
					{
						var s = new Skirmish(a, d, battle, skirmishType);
						r.Result.Add(s);
						defenders.Remove(d);
					}
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(attackers, componentAction);
			}
			else
			{
				foreach (var a in attackers)
				{
					componentAction(a);
				}
			}

			// If none could be created, battle is done
			if (!r.Result.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No skirmishes could be created with the provided units.");
			}

			return r;
		}



		[OperationBehavior]
		public IResult<IBattle, IBattle> ResolveNuclearBattle(IBattle battle, INode target)
		{
			var r = new OperationResult<IBattle, IBattle>();
			r.Result = battle;
			r.Status = ResultStatus.SUCCESS;
			var attacker = battle.Attackers.FirstOrDefault();
			var defenders = battle.Defenders;
			var player = TheGame().CurrentTurn.Player;

			if (attacker == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No attacking unit for battle.");
				r.FailedObjects.Add(battle);
				return r;
			}

			if (attacker.CurrentMoveStats.RemoteFirePoints < 1 || player.TrackedValues.NuclearCharges < 1)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} has no remaining fire points or nuclear charges.");
				r.FailedObjects.Add(battle);
				return r;
			}

			try
			{
				var distanceToTargetResult = CalculateNodeCountToNode(attacker.GetNode(), target, attacker.RemoteAttackDistance);
				if (distanceToTargetResult.Status == ResultStatus.FAILURE)
				{
					r.Status = ResultStatus.FAILURE;
					r.Messages.Add("target location is not within attack radius of {0}.".F(attacker.Name));
					return r;
				}
				if (distanceToTargetResult.Status == ResultStatus.EXCEPTION)
					throw distanceToTargetResult.ex;

				attacker.CurrentMoveStats.RemoteFirePoints --;
				player.TrackedValues.NuclearCharges --;

				// Save Player
				TheGame().JTSServices.GameService.UpdatePlayers(new List<IPlayer> { player });

				battle.DefeatedUnits.AddRange(defenders);
				battle.VictoryCondition = BattleVictoryCondition.ATTACKERS_VICTORIOUS;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;	
		}
		
		[OperationBehavior]
		public IResult<ISkirmish, ISkirmish> ResolveSkirmish(ISkirmish skirmish, BattleType battleType)
		{
			// -------------------------------------------------------
			// Simultaneous play a'la Axis and Allies ----------------
			// -------------------------------------------------------

			// -----------------------------------------------------------------------------------------------
			// This should work with both local and remote battle
			// should work with all skirmish types
			// Logic is that local battle will have a 0 distance to target for all units so the RemoteFirePoints
			// will remain at 0
			// Whereas remote battle will inherently take into account the remote distance
			// and adjust RemoteFirePoints in the cache accordingly
			// -----------------------------------------------------------------------------------------------

			var r = new OperationResult<ISkirmish, ISkirmish>();
			r.Result = skirmish;
			r.Status = ResultStatus.SUCCESS;

			try
			{
				var roundInfo = new SkirmishRoundInfo();
				roundInfo.Skirmish = skirmish;

				// Get these up front.
				// This is an expensive method and they don't change within the scope of a skirmish
				roundInfo.AttackerNetAttackValue = skirmish.Attacker.GetFullNetAttackValue();

				// Special Defence skirmish has the defenders on the offensive for this fir round
				roundInfo.DefenderNetDefenceValue = (skirmish.Type == SkirmishType.FULL) 
													? skirmish.Defender.GetFullNetDefenceValue()
													: skirmish.Defender.GetFullNetAttackValue();

				// while no victor
				while(skirmish.Destroyed.Count == 0)
				{
					// Check for battle compatibility -- this includes accounting for distance
					roundInfo.AttackerCanDoBattle = TheGame().JTSServices.RulesService.UnitCanDoBattleWithUnit(skirmish.Attacker, skirmish.Defender, battleType).Result;
					roundInfo.DefenderCanDoBattle = TheGame().JTSServices.RulesService.UnitCanDoBattleWithUnit(skirmish.Defender, skirmish.Attacker, battleType).Result;
						
					// Check for special Defence skirmish. Attackers can not fire.
					roundInfo.AttackerCanDoBattle = (skirmish.Type == SkirmishType.FULL);

					// Default for local battle
					roundInfo.DistanceToTarget = 0;

					// This isn't actually necessary as local battles will always have a distance of 0
					// I'm leaving this in here to
					//		a) as a gate to save some processing time
					//		b) assist in testing where the sim may be assembling units at different locations to simulate a local battle
					if (battleType == BattleType.BARRAGE)
					{
						// We'll need to know this during remote battle
						// Is the target within the attackers attack radius and if so, how far?
						// If not abandon the skirmish with a stalemate
						var distanceToTargetResult = CalculateNodeCountToUnit(skirmish.Attacker, skirmish.Defender, skirmish.Attacker.RemoteAttackDistance);
						if (distanceToTargetResult.Status == ResultStatus.FAILURE)
						{
							r.Status = ResultStatus.FAILURE;
							r.Messages.Add("{0} is not within attack radius of {1}.".F(skirmish.Defender.Name, skirmish.Attacker.Name));
							return r;
						}

						// Necessary to decrement cache RemoteFirePoints after remote firing
						roundInfo.DistanceToTarget = Convert.ToInt32(distanceToTargetResult.Result);
					}						

		// --------------------------  Attacker roll

					var attackResult = AttackRoll(roundInfo);
					roundInfo.Skirmish.DefenderEvaded = attackResult.StealthEffective;
					if (!attackResult.ContinueSkirmish) return r; // break out of the skirmish loop


		// --------------------------- Defender roll

					var defendResult = DefendRoll(roundInfo);	
					roundInfo.Skirmish.DefenderEvaded = defendResult.StealthEffective;
					if (!defendResult.ContinueSkirmish) return r; // break out of the skirmish loop

		// --------------------------- Finally

					// Special Defence skirmish - attacker does not fire
					if (skirmish.Type != SkirmishType.FULL) return r;
				}
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;			
		}

		[OperationBehavior]
		public IResult<INode, INode> ClaimNodeForVictorFaction(List<IUnit> units, INode node)
		{
			var r = new OperationResult<INode, INode>{Status = ResultStatus.SUCCESS};

			if (units == null || units.Count == 0)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Can not claim node for faction with no units present.");
				return r;
			}

			var country = units.First().Country;
			var enemiesAtNode = node.GetAllUnits().Any(u => !u.Country.Faction.Equals(country.Faction));

			if (enemiesAtNode)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Can not claim node for faction. There are still enemies present");
				return r;
			}

			var canClaimNode = units.Any(u => u.CanClaimLocationForFaction());

			if (!canClaimNode)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("No occupying units are able to claim this node.");
				return r;
			}

			if (!node.Country.Faction.Equals(country.Faction))
			{
				node.Country = country;
				TheGame().JTSServices.NodeService.UpdateNodes(new List<INode> { node });
			}

			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> HandleSpecialTurnEndUnitManagement()
		{
			var result = new OperationResult<IUnit, IUnit>();

			// ~~~~~~~~~~~~~~~~~~~~~ HANDLE PLANES
			
			// Any plane, not "landed" (occupying a location with an airport), and that has not moved this turn should expend fuel for 1 move
			// to simulate the effect of "circling". This eleviates the scenario where a plane can occupy one location and fight ad infinitum
			// over multiple turns without expending any fuel even though they are technically "flying"

			try
			{
				// Get all the current player's planes if they are configured to consume fuel
				var planes = TheGame().JTSServices.UnitService
									.GetAllUnits(TheGame().CurrentTurn.Player.Country)
									.Where(u => u.IsUnitBaseType("plane") && u.UnitInfo.UnitType.FuelConsumer);
			
				// Only affects planes which have NOT moved this turn
				foreach (var plane in planes.Where(plane => plane.CurrentMoveStats.MovementPoints == plane.MovementPoints))
				{
					plane.ConsumeFuel(1);

					if (!plane.UnitInfo.UnitType.FuelConsumer || plane.CurrentFuelRange > TheGame().GameBoard.DefaultAttributes.CellMeters) 
						continue;

					var fuelResult = TheGame().JTSServices.AIService.HandleZeroFuelForUnit(plane);
					result.Messages.Add(fuelResult.Message);
					
					if (fuelResult.Status != ResultStatus.SUCCESS)
						result.FailedObjects.Add(plane);
					else
						result.SuccessfulObjects.Add(plane);
				}
			}
			catch (Exception ex)
			{
				result.Status = ResultStatus.EXCEPTION;
				result.ex = ex;
			}
			
			// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	

			result.Status = ResultStatus.SUCCESS;
			return result;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> HandleSpecialTurnStartUnitManagement()
		{
			throw new NotImplementedException();
		}

	// AI player control

		[OperationBehavior]
		public IEnumerable<IUnitTaskType> GetUnitTaskTypes()
		{
			return ComponentRepository.GetUnitTaskTypes().Select(ut => ut.ToComponent());
		}

		[OperationBehavior]
		public IResult<IUnitTaskType, IUnitTaskType> SaveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes)
		{
			return ComponentRepository.SaveUnitTaskTypes(unitTaskTypes);
		}

		[OperationBehavior]
		public IResult<IUnitTaskType, IUnitTaskType> RemoveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes)
		{
			return ComponentRepository.RemoveUnitTaskTypes(unitTaskTypes);
		}

		[OperationBehavior]
		public IResult<IUnitTaskType, IUnitTaskType> UpdateUnitTaskTypes(List<IUnitTaskType> unitTaskTypes)
		{
			return ComponentRepository.UpdateUnitTaskTypes(unitTaskTypes);
		}


		[OperationBehavior]
		public IEnumerable<IMissionType> GetMissionTypes()
		{
			return ComponentRepository.GetMissionTypes().Select(ut => ut.ToComponent());
		}

		[OperationBehavior]
		public IResult<IMissionType, IMissionType> SaveMissionTypes(
			List<IMissionType> MissionTypes)
		{
			return ComponentRepository.SaveMissionTypes(MissionTypes);
		}

		[OperationBehavior]
		public IResult<IMissionType, IMissionType> RemoveMissionTypes(
			List<IMissionType> MissionTypes)
		{
			return ComponentRepository.RemoveMissionTypes(MissionTypes);
		}

		[OperationBehavior]
		public IResult<IMissionType, IMissionType> UpdateMissionTypes(
			List<IMissionType> MissionTypes)
		{
			return ComponentRepository.UpdateMissionTypes(MissionTypes);
		}



		//[OperationBehavior]
		//public IEnumerable<IStrategy> GetStrategies()
		//{
		//	return ComponentRepository.GetStrategies();
		//}

		//[OperationBehavior]
		//public IResult<IStrategy, IStrategy> SaveStrategy(IStrategy strategy)
		//{
		//	return ComponentRepository.SaveStrategy(strategy);
		//}

		//[OperationBehavior]
		//public IResult<IStrategy, IStrategy> RemoveStrategy(IStrategy strategy)
		//{
		//	return ComponentRepository.RemoveStrategy(strategy);
		//}

		//[OperationBehavior]
		//public IResult<IStrategy, IStrategy> UpdateStrategy(IStrategy strategy)
		//{
		//	return ComponentRepository.UpdateStrategy(strategy);
		//}


		[OperationBehavior]
		public IEnumerable<ITactic> GetTactics()
		{
			return ComponentRepository.GetTactics();
		}

		[OperationBehavior]
		public IResult<ITactic, ITactic> SaveTactic(ITactic tactic)
		{
			return ComponentRepository.SaveTactic(tactic);
		}

		[OperationBehavior]
		public IResult<ITactic, ITactic> RemoveTactic(ITactic tactic)
		{
			return ComponentRepository.RemoveTactic(tactic);
		}

		[OperationBehavior]
		public IResult<ITactic, ITactic> UpdateTactic(ITactic tactic)
		{
			return ComponentRepository.UpdateTactic(tactic);
		}

		public IResult<ITactic, ITactic> CreateNewTactic(IEnumerable<IMission> missions, IPlayer player, StrategicalStance stance)
		{
			var r = new OperationResult<ITactic, ITactic> { Status = ResultStatus.SUCCESS };
			var tactic = new Tactic(stance, player);

			// Add missions - this checks unit tasks rules by relagating the task assignment to the unit
			Action<IMission> missionAdd = m =>
			{
				lock (missions)
				{
					tactic.AddChildComponent(m);
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(missions, missionAdd);
			}
			else
			{
				foreach (var m in missions)
				{
					missionAdd(m);
				}					
			}

			r.Result = tactic;

			return r;
		}


		[OperationBehavior]
		public IResult<StrategicAssessmentInfo, ITile> DetermineTileStrategicValue(ITile tile, GameboardStrategicValueAttributesInfo attributes)
		{
			// 1:
			// AssessmentInfos calculate as follows:
			//		tile's comparison value is equal to (the net for that metric - the lowest value for that metric on the board)
			//		the comparison value range is the range between the lowest and highest values for that metric on the board (Max - Min)
			// This gives us an absolute comparative value for any given tile between the lowest and the highest metric values on the board
			// highest and lowest metric values are calculated on game start up.

			var r = new OperationResult<StrategicAssessmentInfo, ITile> {Status = ResultStatus.SUCCESS};
			var info = new StrategicAssessmentInfo();

			var defenseValue = (tile.NetDefenceAdjustment - attributes.Defense.Min);
			var offenseValue = (tile.NetAttackAdjustment - attributes.Offense.Min);
			var stealthValue = (tile.NetStealthAdjustment - attributes.Stealth.Min);
			var movementValue = (tile.NetMovementAdjustment - attributes.Movement.Min);
			var victoryPoints = (tile.VictoryPoints > 0) ? (tile.VictoryPoints - attributes.VictoryPoints.Min) : 0;

			// Determine Defensive Value
			info.DefensibleRating = ConvertToStrategicAssessmentRating(defenseValue, attributes.Defense.Range);

			// Determine Offensive Value
			info.OffensibleRating = ConvertToStrategicAssessmentRating(offenseValue, attributes.Offense.Range);

			// Determing Stealth Value
			info.StealthRating = ConvertToStrategicAssessmentRating(stealthValue, attributes.Stealth.Range);

			// Determine Movement Value
			info.MovementRating = ConvertToStrategicAssessmentRating(movementValue, attributes.Movement.Range);

			// Determine VictoryPoints value
			info.VictoryPointsRating = ConvertToStrategicAssessmentRating(victoryPoints, attributes.VictoryPoints.Range);

			// Determine Other Values
			// -- Strategic Chokepoint
			info.OtherAggragateRating = GetAggregateRating(tile);

			r.Result = info;
			return r;
		}



		[OperationBehavior]
		public IResult<IMission, IMission> GetCurrentMissionForUnitTask(IUnitTask task)
		{
			var retVal = new OperationResult<IMission, IMission> { Status = ResultStatus.SUCCESS };

			try
			{
				var tactics = GetTactics();
				var missions = tactics.SelectMany(t => t.GetChildComponents());
				var results = missions.Where(o => o.GetChildComponents().Contains(task)).ToArray();

				if (!results.Any() || results.SingleOrDefault() == null)
				{
					retVal.Status = ResultStatus.FAILURE;
					retVal.Messages.Add("No mission found for this task.");
					return retVal;
				}

				if (results.Count() > 1)
				{
					retVal.Status = ResultStatus.FAILURE;
					retVal.Messages.Add("Multiple found for this task.");
					return retVal;
				}

				retVal.Messages.Add("Mission found for task.");
				retVal.Result = results.Single();
				return retVal;
			}
			catch (Exception ex)
			{
				retVal.Status = ResultStatus.EXCEPTION;
				retVal.ex = ex;
				return retVal;
			}
		}

		[OperationBehavior]
		public IResult<ITactic, ITactic> GetCurrentTacticForMission(IMission mission)
		{
			var retVal = new OperationResult<ITactic, ITactic> { Status = ResultStatus.SUCCESS };

			try
			{
				var tactics = Cache().TurnStrategyCache.GetAll();
				var results = tactics.Where(t => t.GetChildComponents().Contains(mission)).ToArray();

				if (!results.Any() || results.SingleOrDefault() == null)
				{
					retVal.Status = ResultStatus.FAILURE;
					retVal.Messages.Add("No Tactic found for this mission.");
					return retVal;
				}

				if (results.Count() > 1)
				{
					retVal.Status = ResultStatus.FAILURE;
					retVal.Messages.Add("Multiple Tactics found for this mission.");
					return retVal;
				}

				retVal.Messages.Add("Tactic found for mission.");
				retVal.Result = results.Single();
				return retVal;
			}
			catch (Exception ex)
			{
				retVal.Status = ResultStatus.EXCEPTION;
				retVal.ex = ex;
				return retVal;
			}
		}

		//[OperationBehavior]
		//public IResult<IStrategy, IStrategy> GetCurrentStrategyForTactic(ITactic tactic)
		//{
		//	var retVal = new OperationResult<IStrategy, IStrategy> { Status = ResultStatus.SUCCESS };

		//	try
		//	{
		//		var strategies = Cache.TurnStrategyCache.GetAll();
		//		var results = strategies.Where(s => s.GetChildComponents().Contains(tactic)).ToArray();

		//		if (!results.Any() || results.SingleOrDefault() == null)
		//		{
		//			retVal.Status = ResultStatus.FAILURE;
		//			retVal.Messages.Add("No Strategy found for this tactic.");
		//			return retVal;
		//		}

		//		if (results.Count() > 1)
		//		{
		//			retVal.Status = ResultStatus.FAILURE;
		//			retVal.Messages.Add("Multiple Strategies found for this tactic.");
		//			return retVal;
		//		}

		//		retVal.Messages.Add("Strategy found for tactic.");
		//		retVal.Result = results.Single();
		//		return retVal;
		//	}
		//	catch (Exception ex)
		//	{
		//		retVal.Status = ResultStatus.EXCEPTION;
		//		retVal.ex = ex;
		//		return retVal;
		//	}
		//}

		[OperationBehavior]
		public IResult<ITactic, ITactic> ExecuteStrategies(List<ITactic> tactics)
		{
			var result = new OperationResult<ITactic, ITactic> { Status = ResultStatus.SUCCESS };

			tactics.ForEach(t => 
				{
					var exResult = t.Execute();
					if (exResult.Status == ResultStatus.SUCCESS)
					{
						result.SuccessfulObjects.Add(t);
					}						
					else
					{
						result.FailedObjects.Add(t);
						result.Status = (result.SuccessfulObjects.Any()) ? ResultStatus.SOME_FAILURE : ResultStatus.FAILURE;
					}						
				});
 
			return result;
		}

		

	#region Unit Tasks

		// Will we need this?
		[OperationBehavior]
		public IResult<IMission, IMission> CreateMission(IMissionType missionType)
		{
			throw new NotImplementedException();
		}
		
		// Will we need this?
		[OperationBehavior]
		public IResult<IMission, IMission> CreateMission(IMissionType missionType, IUnitTask unitTask)
		{
			throw new NotImplementedException();
		}

		[OperationBehavior]
		public IResult<IMission, IMission> CancelMission(IMission mission)
		{
			var result = new OperationResult<IMission, IMission>();

			try
			{
				var tacticResult = mission.GetCurrentTactic();

				if (tacticResult.Status == ResultStatus.EXCEPTION)
					throw tacticResult.ex;

				if (tacticResult.Status != ResultStatus.SUCCESS)
				{
					result.ConvertResultData(tacticResult);
					result.FailedObjects.Add(mission);
					return result;
				}

				// If this is the only mission for the tactic, remove the tactic
				if (tacticResult.Result.GetChildComponents().Count() == 1)
				{
					var removeResult = RemoveTactic(tacticResult.Result);

					if (removeResult.Status == ResultStatus.EXCEPTION)
						throw removeResult.ex;

					if (removeResult.Status != ResultStatus.SUCCESS)
					{
						result.ConvertResultData(removeResult);
						result.FailedObjects.Add(mission);
						return result;
					}
				}
				else // otherwise, remove the mission from the tactic
				{
					var removeResult = tacticResult.Result.RemoveChildComponent(mission);

					if (removeResult.Status == ResultStatus.EXCEPTION)
						throw removeResult.ex;

					if (removeResult.Status != ResultStatus.SUCCESS)
					{
						result.ConvertResultData(removeResult);
						result.FailedObjects.Add(mission);
						return result;
					}
				}

				result.Status = ResultStatus.SUCCESS;
				result.Result = mission;
				result.SuccessfulObjects.Add(mission);
			}
			catch (Exception ex)
			{
				result.Status = ResultStatus.EXCEPTION;
				result.ex = ex;
				result.FailedObjects.Add(mission);
			}

			return result;
		}

		[OperationBehavior]
		public IResult<IMission, IUnit> GetCurrentAssignedMissionForUnit(IUnit unit)
		{
			var result = new OperationResult<IMission, IUnit>();

			try
			{
				// Crawls the strategy cache to find a mission with
				var missions = Cache().TurnStrategyCache
										.GetAll()
										.SelectMany(t => t.GetChildComponents())
										.Where(m => m.GetAssignedUnit().Equals(unit)).ToArray();
				
				
				if (!missions.Any())
				{
					result.Status = ResultStatus.FAILURE;
					result.Messages.Add("{0} does not have a current mission assigned.".F(unit.Name));
				}
				else if (missions.Count() > 1)
				{
					// We are only supposed to have one active mission per unit at any one time	
					result.Status = ResultStatus.EXCEPTION;
					result.ex = new Exception("{0} currently has multiple missions assigned which is not allowed.".F(unit.Name));
					result.FailedObjects.Add(unit);
				}
				else
				{
					result.Status = ResultStatus.SUCCESS;
					result.Messages.Add("Mission found for {0}.".F(unit.Name));
					result.SuccessfulObjects.Add(unit);
					result.Result = missions.Single();
				}
			}
			catch (Exception ex)
			{
				result.Status = ResultStatus.EXCEPTION;
				result.ex = ex;
				result.FailedObjects.Add(unit);
			}

			return result;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> AttemptRefuelUnit(IUnit unit)
		{
			var result = new OperationResult<IUnit, IUnit>();
			result.Status = ResultStatus.SUCCESS;

			var canRefuelResult = TheGame().JTSServices.RulesService.UnitCanRefuelAtLocation(unit, unit.GetNode());
			if (!canRefuelResult.Result)
			{				
				result.Messages.Add(canRefuelResult.Message);
				return result;
			}

			unit.CurrentFuelRange = unit.UnitInfo.UnitType.FuelRange;
			TheGame().JTSServices.UnitService.UpdateUnits(new List<IUnit> {unit});
			return result;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> HandleZeroFuelForUnit(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit>();

			// if we've stopped and checked for fuel
			// are we still out?
			if (unit.CurrentFuelRange == unit.UnitInfo.UnitType.FuelRange)
			{
				r.Messages.Add("{0} has refueled.".F(unit.Name));
			}
			else
			{
				try
				{
					var outOfFuelMessage = unit.UnitInfo.UnitType.BaseType.OutOfFuelMoveResultMessage;

					// Crash the planes
					// TODO: handle this better
					if (unit.IsUnitBaseType("plane"))
						unit.RemoveFromGame();

					// Crash helicopters if they're over water
					// We'll use the criteria that if a land-based unit can not occupy the tile
					// (a landed chopper is essentially a land-based unit, analagous to lightarmor)
					// this will consider any overrides as well
					var tileSupportsLandBasedUnits = 
						TheGame().JTSServices.RulesService.TileIsAllowableForUnitType(TheGame().JTSServices.UnitService.GetUnitTypeByName("lightarmor"), 
																						unit.GetNode().DefaultTile);

					if (unit.IsUnitBaseType("helicopter") && !tileSupportsLandBasedUnits.Result)
					{
						unit.RemoveFromGame();
						outOfFuelMessage = "crashed into the water!!";
					}

					r.Messages.Add("{0} is out of fuel.".F(unit.Name));
					r.Messages.Add("{0} {1}".F(unit.Name, outOfFuelMessage));
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.FailedObjects.Add(unit);
					return r;
				}
				
			}

			r.Status = ResultStatus.SUCCESS;
			r.SuccessfulObjects.Add(unit);
			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> BuildInfrastructure(IUnit unit, ITile tile, IDemographic demographic)
		{
			var result = new OperationResult<IUnit, IUnit>();

			if (tile == null || demographic == null)
			{
				result.Status = ResultStatus.FAILURE;
				result.Messages.Add("Arguments not of the correct type.");
				result.FailedObjects.Add(unit);
				return result;
			}

			using (var txn = new TransactionScope())
			{
				var tileResult = tile.AddDemographic(demographic);
				tile.ReCalculateTileInfo();
				TheGame().Renderer.ResetTileDemographics(TheGame().JTSServices.NodeService.GetAllNodes());
				var tileUpdateResult = TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
				var nodeUpdateResult = TheGame().JTSServices.NodeService.UpdateNodes(new List<INode> { tile.GetNode() });

				if (tileResult.Status == ResultStatus.EXCEPTION || tileUpdateResult.Status == ResultStatus.EXCEPTION)
				{
					result.Status = ResultStatus.EXCEPTION;
					result.ex = tileResult.ex ?? tileUpdateResult.ex;
					return result;
				}
				if (tileResult.Status == ResultStatus.FAILURE || tileUpdateResult.Status == ResultStatus.FAILURE)
				{
					result.Status = ResultStatus.FAILURE;
					result.Messages.Add((tileResult.Status == ResultStatus.FAILURE) ? tileResult.Message : tileUpdateResult.Message);
					return result;
				}

				txn.Complete();
				result.ConvertResultData(tileResult);
				return result;
			}
			
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> DestroyInfrastructure(IUnit unit, ITile tile, IDemographic demographic, Direction direction)
		{
			var result = new OperationResult<IUnit, IUnit>();

			if (tile == null || demographic == null)
			{
				result.Status = ResultStatus.FAILURE;
				result.Messages.Add("Arguments not of the correct type.");
				result.FailedObjects.Add(unit);
				return result;
			}

			using (var txn = new TransactionScope())
			{
				var tileResult = new OperationResult<IDemographic, IDemographic>();

				// Allow for removing only the direction from a demographic when destroying infrastructure
				if (direction != Direction.NONE)
					tileResult = tile.RemoveDirectionFromDemographicOrientation(demographic, direction) as OperationResult<IDemographic, IDemographic>;
				else
					tileResult = tile.RemoveDemographic(demographic) as OperationResult<IDemographic, IDemographic>;
					
				tile.ReCalculateTileInfo();
				TheGame().Renderer.ResetTileDemographics(TheGame().JTSServices.NodeService.GetAllNodes());
				var tileUpdateResult = TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
				var nodeUpdateResult = TheGame().JTSServices.NodeService.UpdateNodes(new List<INode> { tile.GetNode() });

				if (tileResult.Status == ResultStatus.EXCEPTION || tileUpdateResult.Status == ResultStatus.EXCEPTION)
				{
					result.Status = ResultStatus.EXCEPTION;
					result.ex = (tileResult.ex != null) ? tileResult.ex : tileUpdateResult.ex;
					return result;
				}
				if (tileResult.Status == ResultStatus.FAILURE || tileUpdateResult.Status == ResultStatus.FAILURE)
				{
					result.Status = ResultStatus.FAILURE;
					result.Messages.Add((tileResult.Status == ResultStatus.FAILURE) ? tileResult.Message : tileUpdateResult.Message);
					return result;
				}

				txn.Complete();
				result.ConvertResultData(tileResult);
				return result;
			}
		}

		[OperationBehavior]
		public IResult<IEnumerable<object>, TaskExecutionArgument> GetExecutionArgumentObjects(TaskExecutionArgument argument)
		{
			var result = new OperationResult<IEnumerable<object>, TaskExecutionArgument>();
			var objs = new List<object>();

			if (!argument.HasValues)
			{
				result.Status = ResultStatus.FAILURE;
				result.Messages.Add("Argument has no values.");
				result.FailedObjects.Add(argument);
				return result;
			}

			switch (argument.Type)
			{
				case "int":
					{
						objs.AddRange(argument.Values.Select(value => Convert.ToInt32(value)).Cast<object>());
						break;
					}
				case "string":
					{
						objs.AddRange(argument.Values);
						break;
					}
				default:	// Component - get instances from context
					{
						var typeArgument = Type.GetType("{0}, {1}".F(argument.Type, argument.Assembly)); 

						if (typeArgument == null)
						{
							result.Status = ResultStatus.FAILURE;
							result.Messages.Add("Argument type could not be determined.");
							result.FailedObjects.Add(argument);
							return result;
						}

						// Component type
						if (typeof(IBaseComponent).IsAssignableFrom(typeArgument))
						{
							foreach (var value in argument.Values)
							{
								var method = TheGame().JTSServices.GenericComponentService.GetType().GetMethod("GetByID").MakeGenericMethod(typeArgument);
								var obj = method.Invoke(TheGame().JTSServices.GenericComponentService, new object[] {Convert.ToInt32(value)});
								objs.Add(obj); 	
							}
						}
						
						break;
					}
			}

			result.Result = objs;
			result.Status = ResultStatus.SUCCESS;
			return result;
		}

	#endregion


	#region Private Methods

		/// <summary>
		/// Perform game operations for the attacker's skirmish round
		/// </summary>
		/// <param name="roundInfo"></param>
		/// <returns></returns>
		private SkirmishRollResult AttackRoll(SkirmishRoundInfo roundInfo)
		{
			Thread.Sleep(50);
			var bpv = DataRepository.GetGameBasePointValues();
			var r = new SkirmishRollResult();
			var attackerRoll = Die.Roll(bpv.CombatRoll);

			// Decrement RemoteFirePoints hit or miss - if necessary
			// DistanceToTarget is just 0 for local battle
			if (roundInfo.Skirmish.Attacker.CurrentMoveStats.RemoteFirePoints > 0)
				roundInfo.Skirmish.Attacker.CurrentMoveStats.RemoteFirePoints --;


			if (attackerRoll <= roundInfo.AttackerNetAttackValue && roundInfo.AttackerCanDoBattle) // Attack Hit
			{
				r.MedicalEffective = MedicalEffective(roundInfo.Skirmish.Defender);
				r.StealthEffective = StealthEffective(roundInfo.Skirmish.Defender);
				// Check for medical effective and stealth evasion for defender
				// This is the only logical variance for stealth. It's assumed that once the attacker fires, they have lost any stealth
				// advantage. Conversely, when the defender fires back (DefendRoll), they have also lost any stealth advantage for
				// this round... regained on the next round.
				if (!r.MedicalEffective && !r.StealthEffective)
				{
					roundInfo.Skirmish.Destroyed.Add(roundInfo.Skirmish.Defender);

					// Defending roll only if not medical effective
					var defenderRoll = Die.Roll(bpv.CombatRoll);

					//Decrement RemoteFirePoints hit or miss - if necessary
					if (roundInfo.Skirmish.Defender.CurrentMoveStats.RemoteFirePoints > 0)
						roundInfo.Skirmish.Defender.CurrentMoveStats.RemoteFirePoints --;

					if (defenderRoll <= roundInfo.DefenderNetDefenceValue && roundInfo.DefenderCanDoBattle) // Defence hit
					{
						// Check for medical effective for attacker. Attackers are assumed to have revealed their position, so no stealth evasion.
						if (!MedicalEffective(roundInfo.Skirmish.Attacker))
							roundInfo.Skirmish.Destroyed.Add(roundInfo.Skirmish.Attacker);
						else
							roundInfo.Skirmish.Victor = roundInfo.Skirmish.Attacker;
					}
					else
					{
						roundInfo.Skirmish.Victor = roundInfo.Skirmish.Attacker;
					}

				}

				// If the defender or both are destroyed,
				// or this is a special Defence skirmish (one fire round), skirmish ends
				r.DestroyedUnits = roundInfo.Skirmish.Destroyed.Any();

				return r;
			}

			// Default is to continue the skirmish
			return r;
		}

		/// <summary>
		/// Perform game operations for the defender's skirmish round
		/// </summary>
		/// <param name="roundInfo"></param>
		private SkirmishRollResult DefendRoll(SkirmishRoundInfo roundInfo)
		{
			Thread.Sleep(50);
			var bpv = DataRepository.GetGameBasePointValues();
			var r = new SkirmishRollResult();
			var defenderRoll = Die.Roll(bpv.CombatRoll);

			// Decrement RemoteFirePoints hit or miss - if necessary
			// DistanceToTarget is just 0 for local battle
			if (roundInfo.Skirmish.Defender.CurrentMoveStats.RemoteFirePoints > 0)
				roundInfo.Skirmish.Defender.CurrentMoveStats.RemoteFirePoints --;

			if (defenderRoll <= roundInfo.DefenderNetDefenceValue && roundInfo.DefenderCanDoBattle) // Defence Hit
			{
				r.MedicalEffective = MedicalEffective(roundInfo.Skirmish.Attacker);
				// No medical support for helicopters during air defence firing
				// planes should not be getting medical support anyway
				// Attacker has medical support... defender has fired, so no stealth advantage this round
				if (!r.MedicalEffective)
				{
					// Attacker is removed immediately
					roundInfo.Skirmish.Destroyed.Add(roundInfo.Skirmish.Attacker);
					roundInfo.Skirmish.Victor = roundInfo.Skirmish.Defender;
					r.DestroyedUnits = true;
				}
			}

			return r;
		}

		/// <summary>
		/// Checks to see if medical is available and attempts to recover the unit if so
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		private bool MedicalEffective(IUnit unit)
		{
			var bpv = DataRepository.GetGameBasePointValues();
			if (!unit.HasMedicalSupport()) return false;

			var medicalRoll = Die.Roll(bpv.CombatRoll);
			return (medicalRoll <= bpv.MedicalSupportBase);
		}

		private bool StealthEffective(IUnit unit)
		{
			var bpv = DataRepository.GetGameBasePointValues();
			var stealthRoll = Die.Roll(bpv.StealthRoll);
			return (stealthRoll <= unit.GetFullNetStealthValue());
		}

		/// <summary>
		/// Reconstructs a path from the target object back to the source object as a collection of PathNodes
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		private IEnumerable<IPathNode> ReconstructPath(IPathableObject o)
		{
			var path = new List<PathNode>();

			while (o.G != 0)
			{
				var n = TheGame().JTSServices.NodeService.GetNodeAt(o.Location).Clone().ToPathableObject<PathNode>();
				path.Add(n);
				o = o.Parent;
			}

			// Add current node
			path.Add(TheGame().JTSServices.NodeService.GetNodeAt(o.Location).Clone().ToPathableObject<PathNode>());

			return path;
		}

		private StrategicAssessmentRating ConvertToStrategicAssessmentRating(double value, double range)
		{
			if (!(range > 0)) return StrategicAssessmentRating.NONE;

			var items = Enum.GetValues(typeof(StrategicAssessmentRating));
			var enumUpper = Convert.ToInt32(items.GetValue(items.GetUpperBound(0)));
			var r = (int)Math.Round(Convert.ToDouble(((value * enumUpper)) / range));

			return (StrategicAssessmentRating)r;
		}

		private StrategicAssessmentRating GetAggregateRating(ITile tile)
		{		
			var items = Enum.GetValues(typeof(StrategicAssessmentRating));
			var enumUpper = Convert.ToInt32(items.GetValue(items.GetUpperBound(0)));

			// Minimum if any : MEDIUM
			// Maximum is enum max

			var baseVal = 0;
			var intVal = 0;

			intVal += (tile.IsGeographicChokePoint) ? 1 : 0;
			intVal += (tile.Infrastructure.Any(dem => dem.IsDemographicClass("bridge"))) ? 1 : 0;
			intVal += (tile.Infrastructure.Any(dem => dem.IsDemographicClass("airport"))) ? 1 : 0;
			intVal += (tile.Infrastructure.Any(dem => dem.IsDemographicClass("militarybase") || dem.IsDemographicClass("commandpost"))) ? 1 : 0;

			// If any criteria met - start at medium. add from there
			if (intVal > 0)
				baseVal = (int)StrategicAssessmentRating.MEDIUM + intVal;

			// Max is the enum max - obviously
			return (baseVal > enumUpper) ? (StrategicAssessmentRating)enumUpper : (StrategicAssessmentRating)baseVal;
		}


	#endregion

#endregion
		
	}

	sealed class SkirmishRoundInfo
	{
		public ISkirmish Skirmish { get; set; }
		public double AttackerNetAttackValue { get; set; }
		public double DefenderNetDefenceValue { get; set; }
		public bool AttackerCanDoBattle { get; set; }
		public bool DefenderCanDoBattle { get; set; }
		public int DistanceToTarget { get; set; }
	}


	/// <summary>
	/// Used to return messages through to the skirmish
	/// </summary>
	sealed class SkirmishRollResult
	{
		public bool DestroyedUnits { get; set; }
		public bool MedicalEffective { get; set; }
		public bool StealthEffective { get; set; }
		public bool ContinueSkirmish { get {return !DestroyedUnits && !StealthEffective;}}
		public SkirmishRollResult()
		{
			DestroyedUnits = false;					// Default to continue skirmish;
			MedicalEffective = false;
			StealthEffective = false;
		}
	}

}
