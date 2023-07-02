using System;
using System.Linq;
using ConsoleControls.Utility;

namespace ConsoleControls
{
	public abstract class ConsoleControl : IConsoleControl, IDisposable
	{

#region Events

		public delegate void PreRenderEvent(object sender, EventArgs e);
		public delegate void PostRenderEvent(object sender, EventArgs e);
		public delegate void TextPreRenderEvent(object sender, EventArgs e);
		public delegate void TextPostRenderEvent(object sender, EventArgs e);
		public delegate void ErasedEvent(object sender, EventArgs e);
		public delegate void ClearedEvent(object sender, EventArgs e);

		public event PreRenderEvent PreRender;
		public event PostRenderEvent PostRender;
		public event TextPreRenderEvent TextPreRender;
		public event TextPostRenderEvent TextPostRender;
		public event ErasedEvent Erased;
		public event ClearedEvent Cleared;

#endregion

		public int LeftOrigin { get; set; }
		public int TopOrigin { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public ConsoleColor BackColor { get; set; }
		public ConsoleColor ForeColor { get; set; }
		public ConsoleColor DropShadowColor { get; set; }
		public ConsoleColor EraseColor { get; set; }
		public bool DropShadow { get; set; }
		public bool HasFocus { get; set; }
		public bool Visible { get; set; }
		public char FillElement { get; set; }

		private string _text;
		public virtual String Text
		{
			get
			{
				return _text;
			}
			set
			{
				var totalLines = 0;
				var longestLineLength = 0;
				var displayMe = WordWrap.WrapString(value, Width - 2, 0, out totalLines);

				var lines = displayMe.Split(Environment.NewLine.ToCharArray());

				longestLineLength = lines.Max(line => line.Length);

				ExpandControlForContent(lines.Count(), longestLineLength);
				_text = value;
			}
		}

		protected string[] ContentLines
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_text))
					return new string[] { };

				var totalLines = 0;
				var displayMe = WordWrap.WrapString(_text, Width - 2, 0, out totalLines);
				var lines = displayMe.Split(Environment.NewLine.ToCharArray());
				return lines;
			}
		}

		protected int LongestContentLineLength { get { return (ContentLines.Any()) ? ContentLines.Max(line => line.Length) : 0; } } 

		protected ConsoleControl()
		{
			HasFocus = false;
			Visible = true;
			FillElement = ' ';
			DropShadowColor = ConsoleColor.Gray;
			EraseColor = ConsoleColor.Black;
		}

		/// <summary>
		/// Draws the control to the screen
		/// Returns the number of lines of content drawn
		/// </summary>
		public virtual int Draw()
		{
			if (!Visible) return 0;

			On_PreRender(new EventArgs());

			var l = Console.CursorLeft;
            var t = Console.CursorTop;

			Console.BackgroundColor = BackColor;

			// Do first - FillText expands the box height to fit
			var retVal = FillText();

			var i = 0;

			do
			{
				Console.SetCursorPosition((LeftOrigin), (TopOrigin + i));
				Console.Write(String.Concat(Enumerable.Repeat(FillElement, Width)));
				i++;
			} while (i <= Height);

			if (DropShadow) DrawDropShadow();

			Console.ResetColor();
			Console.SetCursorPosition(l, t);

			On_PostRender(new EventArgs());

			return retVal;
		}

		/// <summary>
		/// Fills the content area of the control with the given color
		/// </summary>
		/// <param name="bgColor"></param>
		public virtual void Fill(ConsoleColor bgColor)
		{
			if (!Visible) return;

			var l = Console.CursorLeft;
            var t = Console.CursorTop;

			Console.BackgroundColor = bgColor;
			Console.ForegroundColor = ForeColor;
			var i = 0;

			do
			{
				Console.SetCursorPosition((LeftOrigin), (TopOrigin + i));
				Console.Write(String.Concat(Enumerable.Repeat(FillElement, Width + 1)));
				i++;
			} while (i < Height + 1);

			Console.ResetColor();
			Console.SetCursorPosition(l, t);
		}

		/// <summary>
		/// Fills the content area of the control with the background color
		/// </summary>
		public virtual void Fill()
		{
			Fill(BackColor);
		}

		/// <summary>
		/// Writes the control's text to the control area
		/// Returns the number of lines drawn to the control
		/// </summary>
		public virtual int FillText()
		{
			On_TextPreRender(new EventArgs());

			if (String.IsNullOrWhiteSpace(Text)) return 0;

			var currentLine = TopOrigin;
			ExpandControlForContent(ContentLines.Count(), LongestContentLineLength);

			var l = Console.CursorLeft;
            var t = Console.CursorTop;
			Console.BackgroundColor = BackColor;
			Console.ForegroundColor = ForeColor;

			foreach (var line in ContentLines)
			{
				Console.SetCursorPosition(LeftOrigin, currentLine);
				Console.Write(line);
				currentLine++;
			}

			Console.ResetColor();
			Console.SetCursorPosition(l, t);

			On_TextPostRender(new EventArgs());

			return ContentLines.Count();
		}

		/// <summary>
		/// Clears the content area of the control
		/// </summary>
		public virtual void Clear() 
		{ 
			var l = Console.CursorLeft;
            var t = Console.CursorTop;
			ConsoleColor colorBefore = Console.BackgroundColor;
			
			try
			{
				Console.BackgroundColor = this.BackColor;
				string spaces = new string(' ', Width);
				for (int i = 1; i < Height; i++)
				{
					Console.SetCursorPosition(LeftOrigin + 1, TopOrigin + i);
					Console.Write(spaces);
				}
			}
			finally
			{
				Console.BackgroundColor = colorBefore;
				Console.SetCursorPosition(l, t);

				On_Cleared(new EventArgs());
			}
		
		}

		public virtual int ClearAndRedraw() 
		{ 
			this.Clear();
			this.Fill(); // Assure that the control is filled if the draw override does not
			return this.Draw();
		}

		/// <summary>
		/// Fills the total control space with a background color
		/// </summary>
		/// <param name="bgColor"></param>
		public virtual void Erase()
		{
			Console.BackgroundColor = EraseColor;
			var i = 0;
			var dropShadow = (DropShadow) ? 1 : 0;

			do
			{
				Console.SetCursorPosition((LeftOrigin), (TopOrigin + i));
				Console.Write(String.Concat(Enumerable.Repeat(" ", Width + 2 + dropShadow)));
				i++;
			} while (i <= Height + 2 + dropShadow);

			Console.ResetColor();

			On_Erased(new EventArgs());
		}

		protected virtual void DrawDropShadow()
		{
			// DropShadow
			if (DropShadow)
			{
				Console.BackgroundColor = DropShadowColor;
				Console.ForegroundColor = ConsoleColor.Black;

				var i = 0;

				do
				{
					Console.SetCursorPosition((LeftOrigin + 2 + Width), (TopOrigin + 1 + i));
					Console.Write("▒");
					i++;
				} while (i < Height + 1);


				Console.SetCursorPosition(LeftOrigin + 1, (TopOrigin + Height + 2));
				Console.Write(String.Concat(Enumerable.Repeat("▒", Width + 2)));
			}
		}

		protected virtual void ExpandControlForContent(int contentHeight, int contentWidth)
		{
			if (contentHeight > Height)
			{
				Height = contentHeight;
			}

			if (contentWidth > Width)
			{
				Width = contentWidth;
			}
		}		

		public void CenterPositionHorizontal(int offset = 0)
		{
			LeftOrigin = ((Console.WindowWidth / 2) - ((Width / 2) + offset));
		}

		public void CenterPositionVertical(int offset = 0)
		{
			TopOrigin = ((Console.WindowHeight / 2) - ((Height / 2) + offset));
		}

#region Event Handlers

		public virtual void On_PreRender(EventArgs e)
		{
			if (PreRender != null) PreRender(this, e);
		}

		public virtual void On_PostRender(EventArgs e)
		{
			if (PostRender != null) PostRender(this, e);
		}

		public virtual void On_TextPreRender(EventArgs e)
		{
			if (TextPreRender != null) TextPreRender(this, e);
		}

		public virtual void On_TextPostRender(EventArgs e)
		{
			if (TextPostRender != null) TextPostRender(this, e);
		}

		public virtual void On_Cleared(EventArgs e)
		{
			if (Cleared != null) Cleared(this, e);
		}

		public virtual void On_Erased(EventArgs e)
		{
			if (Erased != null) Erased(this, e);
		}

#endregion

		public void Dispose()
		{
			Console.CursorVisible = false;
		}
	}
}
