using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using JTacticalSim.API;
using JTacticalSim.AI;
using JTacticalSim.API.AI;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Utility;
using JTacticalSim;

namespace JTacticalSim.Component.AI
{
	internal class UnitTaskHandler : BaseGameObject
	{
		public UnitTaskHandler()
			: base(GameObjectType.HANDLER)
		{ }

		public IResult<IUnitTask, IUnitTask> ExecuteTask(IUnitTask task, IEnumerable<TaskExecutionArgument> args)
		{
			var result = new OperationResult<IUnitTask, IUnitTask> {Result = task};
			
			// TODO: Maybe move this to a passed delegate method to call when task is complete
			task.DecrementTurnsToComplete();

			switch (task.TaskType.Name)
			{
				case "Attack" :
				case "Defend" :
				case "ProduceUnits" :
				case "AssignUnits" :
				case "PlaceUnits" :
					{
						// Nothing yet - for player AI
						break;
					}
				case "MoveToLocation":
					{
						var sourceNodeArg = args.SingleOrDefault(a => a.Name == "sourceNode");
						var targetNodeArg = args.SingleOrDefault(a => a.Name == "targetNode");
						var sourceNodeResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(sourceNodeArg);
						var targetNodeResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(targetNodeArg);
						
						if (sourceNodeResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(sourceNodeResult);
							return result;
						}

						if (targetNodeResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(targetNodeResult);
							return result;
						}
						
						var sourceNode =  sourceNodeResult.Result.SingleOrDefault() as INode;
						var targetNode = targetNodeResult.Result.SingleOrDefault() as INode;

						var r = task.GetUnitAssigned().MoveToLocation(targetNode, sourceNode);
						result.ConvertResultData(r);

						break;
					}
				case "LoadUnits":
					{
						var unitsResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(args.SingleOrDefault());

						if (unitsResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(unitsResult);
							return result;
						}

						var units = unitsResult.Result as List<IUnit>;

						var r = task.GetUnitAssigned().LoadUnits(units);
						result.ConvertResultData(r);

						break;
					}
				case "DeployUnits":
					{
						var unitsArg = args.SingleOrDefault(a => a.Name == "units");
						var nodeArg = args.SingleOrDefault(a => a.Name == "node"); 
						var unitsResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(unitsArg);
						var nodeResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(nodeArg);

						if (unitsResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(unitsResult);
							return result;
						}

						if (nodeResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(nodeResult);
							return result;
						}

						var units = unitsResult.Result as List<IUnit>;
						var node = nodeResult.Result.SingleOrDefault() as INode;

						var r = task.GetUnitAssigned().DeployUnits(units, node);
						result.ConvertResultData(r);

						break;
					}
				case "BuildInfrastructure" :
					{
						var tileArg = args.SingleOrDefault(a => a.Name == "tile");
						var demographicArg = args.SingleOrDefault(a => a.Name == "demographic"); 
						var directionArg = args.SingleOrDefault(a => a.Name == "direction");
						var tileArgResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(tileArg);
						var demographicArgResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(demographicArg);
						var directionArgResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(directionArg);
						
						if (tileArgResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(tileArgResult);
							return result;
						}

						if (demographicArgResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(demographicArgResult);
							return result;
						}

						if (directionArgResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(directionArgResult);
							return result;
						}

						var tile = tileArgResult.Result.SingleOrDefault() as ITile;
						var demographic = demographicArgResult.Result.SingleOrDefault() as IDemographic;
						var direction = (Direction)directionArgResult.Result.SingleOrDefault();

						var r = task.GetUnitAssigned().BuildInfrastructure(task, tile, demographic, direction);
						result.ConvertResultData(r);

						break;
					}
				case "DestroyInfrastructure" :
					{
						var tileArg = args.SingleOrDefault(a => a.Name == "tile");
						var demographicArg = args.SingleOrDefault(a => a.Name == "demographic"); 
						var directionArg = args.SingleOrDefault(a => a.Name == "direction");
						var tileArgResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(tileArg);
						var demographicArgResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(demographicArg);
						var directionArgResult = TheGame().JTSServices.AIService.GetExecutionArgumentObjects(directionArg);
						
						if (tileArgResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(tileArgResult);
							return result;
						}

						if (demographicArgResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(demographicArgResult);
							return result;
						}

						if (directionArgResult.Status != ResultStatus.SUCCESS)
						{
							result.FailedObjects.Add(task);
							result.ConvertResultData(directionArgResult);
							return result;
						}

						var tile = tileArgResult.Result.SingleOrDefault() as ITile;
						var demographic = demographicArgResult.Result.SingleOrDefault() as IDemographic;
						var direction = (Direction)directionArgResult.Result.SingleOrDefault();

						var r = task.GetUnitAssigned().DestroyInfrastructure(task, tile, demographic, direction);
						result.ConvertResultData(r);

						break;
					}
				default:
					{
						result.Status = ResultStatus.EXCEPTION;
						result.ex = new Exception("Task type {0} not found.".F(task.TaskType.Name));
						break;
					}
			}

			return result;
		}

	}
}
