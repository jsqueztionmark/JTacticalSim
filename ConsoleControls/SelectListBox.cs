using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public class SelectListBox<T> : PagedControlBase<T>
	{
		public string Bullet { get; set; }
		public ConsoleColor SelectHighlightColor { get; set; }

		public SelectListBox()
			: base(BoxDisplayType.DISPLAY, PromptType.SELECT_ITEM)
		{
			Bullet = "∙";
		}

		public override int Draw()
		{
			var retVal = 0;

			if (Visible)
			{
				On_PreRender(new EventArgs());

				retVal = FillText();

				var l = Console.CursorLeft;
				var t = Console.CursorTop;

				if (Border)
					DrawWithBorder();
				else
					DrawWithoutBorder();

				DrawCaption();
				DrawCloseWindowX();

				if (_promptType != PromptType.NONE)	CreatePrompt();
			
				On_PostRender(new EventArgs());

				Console.SetCursorPosition(l, t);
			}

			if (HasFocus) HandlePrompt();
			return retVal;
			
		}

		public override int FillText()
		{
			On_TextPreRender(new EventArgs());

			if (!_pages.Any() || _currentPage.ItemCount == 0) return 0;

			var currentLine = TopOrigin + 2;

			// Determine longest line
			var lines = _currentPage.SelectionItems.Select(kvp =>
									{
										var displayString = string.Format("{0} - {1}", kvp.Key.ToString(), kvp.Value.Text);
										return displayString;
									});

			var longestLineLength =  lines.Max(line => line.Length); 

			ExpandControlForContent(PageSize, longestLineLength);
			Fill();

			var l = Console.CursorLeft;
            var t = Console.CursorTop;

			foreach (var kvp in _currentPage.SelectionItems)
			{
				if (kvp.Equals(_highlightedItem))
				{
					Console.BackgroundColor = SelectHighlightColor;
					Console.ForegroundColor = BorderBackColor;
				}
				else
				{
					Console.BackgroundColor = BackColor;
					Console.ForegroundColor = ForeColor;
				}

				if (kvp.Value.Value != null)
				{
					Console.SetCursorPosition(LeftOrigin + 2, currentLine);
					// Allow for filler spaces
					var bullet = (string.IsNullOrWhiteSpace(kvp.Value.Text)) ? " " : Bullet;
					Console.WriteLine("{0} ", bullet);
				}				

				if (kvp.Value.ItemColorSet)
					Console.ForegroundColor = kvp.Value.ItemColor;

				Console.SetCursorPosition(LeftOrigin + 4, currentLine);
				Console.Write("{0}", kvp.Value.Text);
				currentLine++;
			}

			Console.ResetColor();
			Console.SetCursorPosition(l, t);

			On_TextPostRender(new EventArgs());

			return PageSize;
		}

		protected override void HandlePrompt()
		{
			Console.BackgroundColor = BackColor;

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
				if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.NumPad5 || CancelKeys.Contains(keyInfo.Key))
				{
					SelectedItem = null;
					On_EscapePressed();
					Erase();
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
						// Set the selected list item and get rid of the box
						On_ItemSelected();
						Erase();
						return;
					}
				}
				if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.NumPad8)
				{
					// Very top of the list
					if (_highlightedItem.Key == 1 && _currentPage.PageNum == 1)
					{
						ClearAndRedraw();
						return;
					}

					if (_highlightedItem.Key == 1)
					{
						CyclePages(ConsoleControls.CycleDirection.DOWN);
					}
					else
					{
						_highlightedItem =
						_currentPage.SelectionItems.Where(kvp => kvp.Key == _highlightedItem.Key - 1)
												.Select(kvp => kvp)
												.SingleOrDefault();
						

						On_SelectionChanged();
					}

					ClearAndRedraw();
					return;
				}
				if (keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.NumPad2)
				{
					// Very bottom of the list
					if (_currentPage.PageNum == PageCount && _highlightedItem.Key == _currentPage.ItemCount)
					{
						ClearAndRedraw();
						return;
					}

					if (_highlightedItem.Key == PageSize)
					{
						CyclePages(ConsoleControls.CycleDirection.UP);
					}
					else
					{

						_highlightedItem =
						_currentPage.SelectionItems.Where(kvp => kvp.Key == _highlightedItem.Key + 1)
													.Select(kvp => kvp)
													.SingleOrDefault();

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

	public class ListBoxItem<T>
	{
		private ConsoleColor _itemColor;
		public ConsoleColor ItemColor
		{
			set 
			{ 
				_itemColor = value;
				ItemColorSet = true;
			}
			get { return _itemColor; }
		}
		public bool ItemColorSet { get; private set; }
		public string Text { get; set; }
		public T Value { get; set; }

		public ListBoxItem(T value, string text)
		{
			ItemColorSet = false;
			Value = value;
			Text = text;
		}
	}

	public class ItemPage<T>
	{
		public int PageNum { get; private set; }
		public readonly Dictionary<int, ListBoxItem<T>> SelectionItems;
		public int ItemCount { get { return SelectionItems.Count; }}

		public KeyValuePair<int, ListBoxItem<T>> FirstItem
		{
			get 
			{
				var minKey = SelectionItems
								.Where(kvp => (kvp.Value != null && kvp.Value.Value != null))
								.Min(i => i.Key);
				return SelectionItems.SingleOrDefault(i => i.Key == minKey); 
			}
		}

		public KeyValuePair<int, ListBoxItem<T>> LastItem
		{
			get 
			{
				var maxKey = SelectionItems
								.Where(kvp => (kvp.Value != null && kvp.Value.Value != null))
								.Max(i => i.Key);
				return SelectionItems.SingleOrDefault(i => i.Key == maxKey); 
			}
		}

		public bool IsFirstItem(int itemKey)
		{
			return (!SelectionItems.Any(kvp => kvp.Key < itemKey && (kvp.Value != null && kvp.Value.Value != null)));
		}

		public bool IsLastItem(int itemKey)
		{
			return (!SelectionItems.Any(kvp => kvp.Key > itemKey && (kvp.Value != null && kvp.Value.Value != null)));
		}


		public ItemPage(int pageNum)
		{
			PageNum = pageNum;
			SelectionItems = new Dictionary<int, ListBoxItem<T>>();
		}

		public void AddItem(ListBoxItem<T> item)
		{
			var nextItemNum = SelectionItems.Count + 1;
			SelectionItems.Add(nextItemNum, item);
		}

	}
}
