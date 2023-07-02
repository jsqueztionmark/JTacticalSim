using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleControls.Utility;

namespace ConsoleControls
{
	public class PagedConsoleBox : PagedControlBase<string[]>
	{
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				// Create pages from text
				CreatePagesFromText();
			}
		}

		public PagedConsoleBox()
			: base(BoxDisplayType.DISPLAY, PromptType.NONE)
		{
		}

		public override int FillText()
		{
			if (!Visible) return 0;

			On_TextPreRender(new EventArgs());

			if (PageCount < 1) return 0;

			var offset = (Border) ? 2 : 1;

			var currentLine = TopOrigin + offset;

			var l = Console.CursorLeft;
            var t = Console.CursorTop;
			var currentText = _currentPage.SelectionItems.SingleOrDefault().Value.Value;

			Console.BackgroundColor = BackColor;
			Console.ForegroundColor = ForeColor;

			foreach (var line in currentText)
			{
				Console.SetCursorPosition(LeftOrigin + offset, currentLine);
				Console.Write(line);
				currentLine++;
			}

			Console.ResetColor();
			Console.SetCursorPosition(l, t);

			On_TextPostRender(new EventArgs());

			return ContentLines.Count();
		}

		public override int ClearAndRedraw()
		{
			return base.ClearAndRedraw();
		}

		public override void AddItem(ListBoxItem<string[]> item)
		{
			var nextPageNum = PageCount + 1;
			var fillPage = _pages.SingleOrDefault(p => p.PageNum == _pages.Count);

			if (fillPage == null || fillPage.ItemCount >= 1) // restrict select lists to 1 - 9
			{
				var newPage = new ItemPage<string[]>(nextPageNum);
				newPage.AddItem(item);
				_pages.Add(newPage);
			}
			else
			{
				fillPage.AddItem(item);
			}
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
				if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.NumPad8)
				{
					// Very top of the list
					if (_currentPage.PageNum == 1)
					{
						ClearAndRedraw();
						return;
					}

					if (_currentPage.PageNum > 1)
					{
						CyclePages(ConsoleControls.CycleDirection.DOWN);
					}

					ClearAndRedraw();
					return;
				}
				if (keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.NumPad2)
				{
					// Very bottom of the list
					if (_currentPage.PageNum == PageCount)
					{
						ClearAndRedraw();
						return;
					}

					if (_currentPage.PageNum < PageCount)
					{
						CyclePages(ConsoleControls.CycleDirection.UP);
					}

					ClearAndRedraw();
					return;
				
				}
			}

			// If we didn't find an item that matched the input, clear and try again
			ClearAndRedraw();				

		}

		private void CreatePagesFromText()
		{
			if (String.IsNullOrWhiteSpace(Text)) return;

			var pageHeight = Height - 2;;
			var pageLines = new List<string>();
			var lineNum = 0;

			// Batch and create pages
			foreach (var line in ContentLines)
			{
				lineNum++;
				var newPage = lineNum > 0 && (lineNum % pageHeight == 0 || lineNum == ContentLines.Count());
				if (newPage)
				{
					var item = new ListBoxItem<string[]>(pageLines.ToArray(), "");
					AddItem(item);
					pageLines.Clear();
					continue;
				}
				pageLines.Add(line);
			}
		}
	}
}
