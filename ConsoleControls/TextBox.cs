using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public class TextBox : ConsoleBox
	{
		public event EventHandler TextEntered;

		public ConsoleColor LabelColor { get; set; }
		public string LabelText { get; set; }
		public ConsoleColor MessageColor { get; set; }
		public string Message { get; set; }
		public int MaxCharacters { get; set; }

		public TextBox()
			: base(BoxDisplayType.TEXT, PromptType.OK_CANCEL)
		{
			Height = 6;
			Width = 40;
			LabelText = string.Empty;
			Message = string.Empty;
			LabelColor = ForeColor;
			MaxCharacters = 255;
		}

		protected override void HandlePrompt()
		{
			Console.ForegroundColor = ForeColor;
			Console.BackgroundColor = BackColor;

			if (!String.IsNullOrWhiteSpace(Message))
			{
				Console.SetCursorPosition(this.LeftOrigin + 2, this.TopOrigin + 2);
				Console.ForegroundColor = MessageColor;
				Console.Write(Message);
				Console.ForegroundColor = ForeColor;
			}

			Console.SetCursorPosition(this.LeftOrigin + 2, this.TopOrigin + 4);

			if (!String.IsNullOrWhiteSpace(LabelText))
			{
				Console.ForegroundColor = LabelColor;
				Console.Write(LabelText);
				Console.ForegroundColor = ForeColor;
			}

			Text = ReadLineOrEsc();
			On_TextEntered();	
			return;
		}

		private void On_TextEntered()
		{
			Console.ResetColor();
			Erase();
			if (TextEntered != null) TextEntered(this, new EventArgs());
		}

		private string ReadLineOrEsc()
		{
			string retString = "";
			int curIndex = 0;

			do
			{
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);

				if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
				{
					if (keyInfo.Key == ConsoleKey.X)
					{
						this.Text = null;
						this.Message = null;
						On_WindowClosePressed();
						return null;
					}
				}

				// handle Esc
				if (keyInfo.Key == ConsoleKey.Escape || CancelKeys.Contains(keyInfo.Key))
				{
					On_EscapePressed();
					this.Text = null;
					this.Message = null;
					return null;
				}

				// disallow tabs
				if (keyInfo.Key == ConsoleKey.Tab)
				{
					continue;
				}

				// handle Enter
				if (keyInfo.Key == ConsoleKey.Enter)
				{
					if (string.IsNullOrWhiteSpace(retString))
						continue;
						
					return retString;				
				}

				// handle backspace
				if (keyInfo.Key == ConsoleKey.Backspace)
				{
					if (curIndex > 0)
					{
						retString = retString.Remove(retString.Length - 1);
						Console.Write(keyInfo.KeyChar);
						Console.Write(' ');
						Console.Write(keyInfo.KeyChar);
						curIndex--;
					}
				}
				else
				// handle all other keypresses
				{
					if (curIndex >= MaxCharacters)
						continue;

					retString += keyInfo.KeyChar;
					Console.Write(keyInfo.KeyChar);
					curIndex++;
				}
			}
			while (true);
		}
	}
}
