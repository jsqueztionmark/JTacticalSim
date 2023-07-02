using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using ConsoleControls.Utility;

namespace ConsoleControls
{
	public class ConsoleBox : ConsoleControl
	{
		public event EventHandler EscapePressed;
		public event EventHandler WindowClosePressed;

		public ConsoleColor BorderForeColor { get; set; }
		public ConsoleColor BorderBackColor { get; set; }
		public ConsoleColor PromptColor { get; set; }
		public ConsoleColor? CaptionColor { get; set; }
		public string PromptText { get; set; }
		public IConsoleBoxElements DrawElements { get; set; }
		public bool Border { get; set; }
		public bool CloseWindowX { get; set; }
		public List<ConsoleKey> CancelKeys { get; set; }
		public String Caption { get; set; }
		public bool? Prompt { get; private set; }

		protected BoxDisplayType _displayType { get; set; }
		protected PromptType _promptType { get; set; }

		public ConsoleBox(BoxDisplayType displayType, PromptType promptType)
		{
			_displayType = displayType;
			_promptType = promptType;

			Console.CursorVisible = (displayType == BoxDisplayType.CMD || displayType == BoxDisplayType.TEXT);

			LeftOrigin = 0;
			TopOrigin = 0;
			Height = 2;
			Width = (Width < GetPromptText().Length) ? Width = GetPromptText().Length + 3 : 2;
			BorderForeColor = ConsoleColor.White;
			BorderBackColor = ConsoleColor.Black;
			BackColor = ConsoleColor.Black;
			DrawElements = new ConsoleBoxElements('*');
			CancelKeys = new List<ConsoleKey>();
			DropShadow = false;
			Border = true;
			HasFocus = true;
			CloseWindowX = false;
		}

		/// <summary>
		/// Draws the box outline
		/// </summary>
		public override int Draw()
		{
			var retVal = 0;

			if (Visible)
			{
				On_PreRender(new EventArgs());

				ExpandControlForContent(ContentLines.Count(), LongestContentLineLength);

				var l = Console.CursorLeft;
				var t = Console.CursorTop;

				if (Border)
					DrawWithBorder();
				else
					DrawWithoutBorder();

				FillText();

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
			if (!Visible) return 0;

			On_TextPreRender(new EventArgs());

			var offset = (Border) ? 2 : 1;

			if (String.IsNullOrWhiteSpace(Text)) return 0;

			var currentLine = TopOrigin + offset;
			ExpandControlForContent(ContentLines.Count(), LongestContentLineLength);

			var l = Console.CursorLeft;
            var t = Console.CursorTop;
			Console.BackgroundColor = BackColor;
			Console.ForegroundColor = ForeColor;

			foreach (var line in ContentLines)
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

		public void DrawHorizontalLine(int vertLocation)
		{
			Console.ForegroundColor = BorderForeColor;
			Console.BackgroundColor = BorderBackColor;
			Console.SetCursorPosition(LeftOrigin, vertLocation);
			Console.Write(DrawElements.BookendRight);
			Console.Write(String.Concat(Enumerable.Repeat(DrawElements.TopHorizontal, Width)));
			Console.Write(DrawElements.LeftVertical);
		}

		protected virtual void DrawCaption()
		{
			// Only render if the box width can handle the caption size
			if (String.IsNullOrWhiteSpace(Caption) || Width < Caption.Length + 6)
				return;

			Console.SetCursorPosition(LeftOrigin + 2, TopOrigin);

			Console.ForegroundColor = BorderForeColor;
			Console.BackgroundColor = BorderBackColor;
			Console.Write(string.Format("{0}", DrawElements.BookendLeft));

			if (CaptionColor != null) Console.ForegroundColor = (ConsoleColor)CaptionColor;
			Console.Write(string.Format(" {0} ", Caption));

			Console.ForegroundColor = BorderForeColor;
			Console.Write(string.Format("{0}", DrawElements.BookendRight));

			Console.ResetColor();
		}

		protected virtual void DrawCloseWindowX()
		{
			// Only render if the box width can handle the caption size
			if (!CloseWindowX) return;

			Console.SetCursorPosition(LeftOrigin + (Width - 8), TopOrigin);
			Console.ForegroundColor = BorderForeColor;
			Console.BackgroundColor = BorderBackColor;
			Console.Write(DrawElements.BookendLeft);
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(string.Format(" {0} ","Ctl+X"));
			Console.ForegroundColor = BorderForeColor;
			Console.BackgroundColor = BorderBackColor;
			Console.Write(DrawElements.BookendRight);

			Console.ResetColor();
		}

		protected void DrawWithBorder()
		{
			Console.ForegroundColor = BorderForeColor;
			Console.BackgroundColor = BorderBackColor;

			// Top
			Console.SetCursorPosition(LeftOrigin, TopOrigin);
			Console.Write(DrawElements.TopLeft);
			Console.Write(String.Concat(Enumerable.Repeat(DrawElements.TopHorizontal, Width)));
			Console.Write(DrawElements.TopRight);

			// Bottom
			Console.SetCursorPosition(LeftOrigin, (TopOrigin + Height + 1));
			Console.Write(DrawElements.BottomLeft);
			Console.Write(String.Concat(Enumerable.Repeat(DrawElements.BottomHorizontal, Width)));
			Console.Write(DrawElements.BottomRight);

			if (Height > 0)
			{
				var i = 0;
				// Verticals
				do
				{
					Console.SetCursorPosition(LeftOrigin, (TopOrigin + 1 + i));
					Console.Write(DrawElements.LeftVertical);
					Console.SetCursorPosition((LeftOrigin + 1 + Width), (TopOrigin + 1 + i));
					Console.Write(DrawElements.RightVertical);
					i++;
				} while (i < Height);
			}			

			if (DropShadow) DrawDropShadow();

			Console.ResetColor();
		}

		protected void DrawWithoutBorder()
		{
			Console.BackgroundColor = BackColor;
			Console.ForegroundColor = ForeColor;

			var i = 0;

			do
			{
				Console.SetCursorPosition((LeftOrigin), (TopOrigin + i));
				Console.Write(String.Concat(Enumerable.Repeat(FillElement, Width)));
				i++;
			} while (i <= Height);

			if (DropShadow) DrawDropShadow();

			Console.ResetColor();
		}

		protected override void ExpandControlForContent(int contentHeight, int contentWidth)
		{
			var captionOffset = (!String.IsNullOrWhiteSpace(Caption)) ? 1 : 0;
			var promptHeight = (_promptType != PromptType.NONE) ? 2 : 0;
			base.ExpandControlForContent(contentHeight + promptHeight + captionOffset, contentWidth);
		}

		protected virtual void CreatePrompt()
		{
			Console.SetCursorPosition(LeftOrigin + 2, (TopOrigin + Height));
			Console.BackgroundColor = BackColor;
			Console.ForegroundColor = PromptColor;
			Console.Write(GetPromptText());
			Console.ResetColor();
		}

		protected virtual void HandlePrompt()
		{
			switch (_promptType)
			{
				case PromptType.CTLX_ONLY:
					{
						ConsoleKeyInfo keyInfo = Console.ReadKey(true);

						if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
						{
							if (keyInfo.Key == ConsoleKey.X || CancelKeys.Contains(keyInfo.Key))
							{
								On_WindowClosePressed();
								return;
							}
						}

						ClearAndRedraw();
						return;
					}
				case PromptType.PRESS_ANY_KEY:
					{
						ConsoleKeyInfo keyInfo = Console.ReadKey(true);
						Erase();
						return;
					}
				case PromptType.YES_NO:
					{	
						ConsoleKeyInfo keyInfo = Console.ReadKey(true);

						if (CancelKeys.Contains(keyInfo.Key))
						{
							Prompt = null;
							Erase();
							return;
						}

						if (keyInfo.Key != ConsoleKey.Y && keyInfo.Key != ConsoleKey.N)
						{
							ClearAndRedraw();
						}
						
						Prompt = (keyInfo.Key == ConsoleKey.Y);
						Erase();
						return;
					}
				case PromptType.YES_NO_CANCEL:
					{	
						ConsoleKeyInfo keyInfo = Console.ReadKey(true);

						if (keyInfo.Key == ConsoleKey.Escape || CancelKeys.Contains(keyInfo.Key))
						{
							Prompt = null;
							Erase();
							return;
						}

						if (keyInfo.Key != ConsoleKey.Y && keyInfo.Key != ConsoleKey.N)
						{
							ClearAndRedraw();
						}
						
						Prompt = (keyInfo.Key == ConsoleKey.Y);
						Erase();
						return;
					}
				case PromptType.OK_CANCEL:
					{	
						ConsoleKeyInfo keyInfo = Console.ReadKey(true);

						if (keyInfo.Key == ConsoleKey.Escape || CancelKeys.Contains(keyInfo.Key))
						{
							Prompt = null;
							Erase();
							return;
						}

						if (keyInfo.Key != ConsoleKey.Enter)
						{
							ClearAndRedraw();
						}
						
						Prompt = true;
						Erase();
						return;
					}
				case PromptType.SELECT_ITEM:
					{
						// Handled within the select box
						break;
					}
			}
		}

		protected virtual string GetPromptText()
		{
			if (!String.IsNullOrWhiteSpace(PromptText))
				return PromptText;

			switch (_promptType)
			{
				case PromptType.PRESS_ANY_KEY:
					{
						return ".... press any key to continue";
					}
				case PromptType.YES_NO:
					{
						return "Y:Yes / N:No";
					}
				case PromptType.YES_NO_CANCEL:
					{
						return "Y:Yes / N:No / Esc:Cancel";
					}
				case PromptType.SELECT_ITEM:
					{
						return "Scroll ▲/▼  Select:Enter";
					}
				case PromptType.OK_CANCEL:
					{
						return "Enter:OK / Esc:Cancel";
					}
				default:
					return string.Empty;
			}
		}

		protected virtual void On_EscapePressed()
		{
			if (this.EscapePressed != null) this.EscapePressed(this, new EventArgs());
		}

		protected virtual void On_WindowClosePressed()
		{
			if (this.WindowClosePressed != null) WindowClosePressed(this, new EventArgs());
		}
	}

#region DrawElements
	
	public interface IConsoleBoxElements
	{
		char TopLeft { get; set; }
		char TopRight { get; set; }
		char BottomLeft { get; set; }
		char BottomRight { get; set; }
		char TopHorizontal { get; set; }
		char BottomHorizontal { get; set; }
		char LeftVertical { get; set; }
		char RightVertical { get; set; }
		char BookendRight { get; set; }
		char BookendLeft { get; set; }
	}

	public class ConsoleBoxElements : IConsoleBoxElements
	{
		public char TopLeft { get; set; }
		public char TopRight { get; set; }
		public char BottomLeft { get; set; }
		public char BottomRight { get; set; }
		public char TopHorizontal { get; set; }
		public char BottomHorizontal { get; set; }
		public char LeftVertical { get; set; }
		public char RightVertical { get; set; }
		public char BookendRight { get; set; }
		public char BookendLeft { get; set; }

		public ConsoleBoxElements(char elementCharacter)
		{
			TopLeft = 
			TopRight = 
			BottomLeft = 
			BottomRight = 
			TopHorizontal = 
			BottomHorizontal = 
			LeftVertical = 
			BookendLeft = 
			BookendRight =
			RightVertical = elementCharacter;
		}

		public ConsoleBoxElements()
		{
			TopLeft =
			TopRight =
			BottomLeft =
			BottomRight =
			TopHorizontal =
			BottomHorizontal =
			LeftVertical =
			BookendLeft = 
			BookendRight = 
			RightVertical = ' ';
		}
	}

	public class DoubleLineBoxElements : ConsoleBoxElements
	{
		public DoubleLineBoxElements()
		{
			TopLeft = '╔';
			TopRight = '╗';
			BottomLeft = '╚';
			BottomRight = '╝';
			TopHorizontal = '═';
			BottomHorizontal = '═';
			LeftVertical = '║';
			RightVertical = '║';
			BookendLeft = '╡';
			BookendRight = '╞';
		}
	}

	public class SingleTopDoubleBottomLineBoxElements : ConsoleBoxElements
	{
		public SingleTopDoubleBottomLineBoxElements()
		{
			TopLeft = '┌';
			TopRight = '╖';
			BottomLeft = '╘';
			BottomRight = '╝';
			TopHorizontal = '─';
			BottomHorizontal = '═';
			LeftVertical = '│';
			RightVertical = '║';
			BookendLeft = '┤';
			BookendRight = '├';
		}
	}

	public class SingleLineBoxElements : ConsoleBoxElements
	{
		public SingleLineBoxElements()
		{
			TopLeft = '┌';
			TopRight = '┐';
			BottomLeft = '└';
			BottomRight = '┘';
			TopHorizontal = '─';
			BottomHorizontal = '─';
			LeftVertical = '│';
			RightVertical = '│';
			BookendLeft = '┤';
			BookendRight = '├';
		}
	}

#endregion


}
