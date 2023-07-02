using System;
using System.Linq;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	/// <summary>
	/// Used to handle prompt input differently from the regular select box
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SelectListContainer<T> : SelectListBox<T>
	{
		private IScreenRenderer ParentScreen { get; set; }

		public SelectListContainer(IScreenRenderer parentScreen)
		{
			ParentScreen = parentScreen;
		}

		protected override void HandlePrompt()
		{
			ConsoleKeyInfo keyInfo = Console.ReadKey(true);

			if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
			{
				if (keyInfo.Key == ConsoleKey.X)
				{
					On_WindowClosePressed();
					return;
				}
			}

			// No key modifiers
			if (((keyInfo.Modifiers & ConsoleModifiers.Control) == 0) &&
				((keyInfo.Modifiers & ConsoleModifiers.Alt) == 0) &&
				((keyInfo.Modifiers & ConsoleModifiers.Shift) == 0))
			{
				if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.NumPad5)
				{
					SelectedItem = null;
					On_EscapePressed();
					return;
				}
				if (keyInfo.Key == ConsoleKey.Enter)
				{
					SelectedItem = _currentPage.SelectionItems
								.Where(kvp => kvp.Key == _highlightedItem.Key)
								.Select(kvp => kvp.Value)
								.SingleOrDefault();

					if (SelectedItem != null)
					{
						On_ItemSelected();
						return;
					}
				}
				if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.NumPad8)
				{
					// Very top of the list
					if (_currentPage.IsFirstItem(_highlightedItem.Key) && _currentPage.PageNum == 1)
					{
						ClearAndRedraw();
						return;
					}

					if (_currentPage.IsFirstItem(_highlightedItem.Key))
					{
						CyclePages(CycleDirection.DOWN);
					}
					else
					{
						do
						{
							_highlightedItem =
							_currentPage.SelectionItems.Where(kvp => kvp.Key == _highlightedItem.Key - 1)
														.Select(kvp => kvp)
														.SingleOrDefault();
						} while (_highlightedItem.Value == null || _highlightedItem.Value.Value == null);

						On_SelectionChanged();
					}

					ClearAndRedraw();
					return;
				}
				if (keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.NumPad2)
				{
					// Very bottom of the list
					if (_currentPage.PageNum == PageCount && _currentPage.IsLastItem(_highlightedItem.Key))
					{
						ClearAndRedraw();
						return;
					}

					if (_currentPage.IsLastItem(_highlightedItem.Key))
					{
						CyclePages(CycleDirection.UP);
					}
					else
					{
						do
						{
							_highlightedItem =
							_currentPage.SelectionItems.Where(kvp => kvp.Key == _highlightedItem.Key + 1)
														.Select(kvp => kvp)
														.SingleOrDefault();

						} while (_highlightedItem.Value == null || _highlightedItem.Value.Value == null);

						On_SelectionChanged();

					}

					ClearAndRedraw();
					return;
				
				}
			}

			// If we didn't find an item that matched the input, clear and try again
			ClearAndRedraw();
		}
	}
}
