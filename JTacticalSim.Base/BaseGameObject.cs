using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API;
using JTacticalSim.API.Service;
using JTacticalSim.Cache;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim
{
	/// <summary>
	/// The bottom level game object container
	/// </summary>
	public abstract class BaseGameObject : IBaseGameObject
	{
		public GameObjectType GOType { get; set;}

		public BaseGameObject(GameObjectType goType)
		{
			GOType = goType;
		}

		public IGame TheGame() { return Game.Instance; }
		public IGameCacheDependencies Cache() { return GameCache.Instance; }

		/// <summary>
		/// Displays all results from a result object to the user
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TObject"></typeparam>
		/// <param name="result"></param>
		/// <param name="failureOnly"></param>
		public virtual void HandleResultDisplay<TResult, TObject>(IResult<TResult, TObject> result, bool failureOnly)
		{
			switch (result.Status)
			{
				case ResultStatus.EXCEPTION:
					{
						TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, result.Message, result.ex);
						return;
					}
				case ResultStatus.FAILURE:
					{
						TheGame().Renderer.DisplayUserMessage(MessageDisplayType.WARNING, result.Message, null);
						return;
					}
				case ResultStatus.SUCCESS:
					{
						if (failureOnly) return;
						TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, result.Message, null);
						return;
					}
				case ResultStatus.SOME_FAILURE:
					{
						TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, result.Message, null);
						return;
					}
				default:
					{
						if (failureOnly) return;
						TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, result.Message, null);
						return;
					}

			}
		}
	}
}
