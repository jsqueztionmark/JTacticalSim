using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace JTacticalSim.ConsoleApp.CommandLineUtil
{
	public static class ConsoleUtils
	{
		public static void CenterConsole() 
		{
			IntPtr hWin = GetConsoleWindow();
			RECT rc;
			GetWindowRect(hWin, out rc);
			Screen scr = Screen.FromPoint(new Point(rc.left, rc.top));
			int x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
			int y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;
			MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
		}

		// P/Invoke declarations
		private struct RECT { public int left, top, right, bottom; }
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

		public static bool GetYesNoInputAsBool()
		{
			string input;
			System.Console.Write("Yes[Y]/No[N] : ");
			input = GetInputWithCancel().ToLowerInvariant();


			while (string.IsNullOrEmpty(input) || (input != "yes" && input != "y" && input != "no" && input != "n"))
			{
				System.Console.ForegroundColor = ConsoleColor.Red;
				System.Console.Write("Yes[Y]/No[N] : ");
				System.Console.ResetColor();
				input = GetInputWithCancel();
			}

			switch (input)
			{
				case "yes" :
				case "y" :
					return true;
				case "no" :
				case "n" :
					return false;
				default :
					return false;
			}

		}

		public static string GetInputWithCancel()
		{
			var input = System.Console.ReadLine();
			//if (input.ToLowerInvariant() == "CANCEL")
			//    CancelCommand();
			return input;	
		}

		public static bool IsValidateNumericInput(string input)
		{
			int result;
			return Int32.TryParse(input, out result);
		}
	}
	

	public class Arguments{
        // Variables
        private StringDictionary Parameters;

        // Constructor
        public Arguments(string[] Args)
        {
            Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase|RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase|RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach(string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                Parts = Spliter.Split(Txt,3);

                switch(Parts.Length){
                // Found a value (for the last parameter 
                // found (space separator))
                case 1:
                    if(Parameter != null)
                    {
                        if(!Parameters.ContainsKey(Parameter)) 
                        {
                            Parts[0] = 
                                Remover.Replace(Parts[0], "$1");

                            Parameters.Add(Parameter, Parts[0]);
                        }
                        Parameter=null;
                    }
                    // else Error: no parameter waiting for a value (skipped)
                    break;

                // Found just a parameter
                case 2:
                    // The last parameter is still waiting. 
                    // With no value, set it to true.
                    if(Parameter!=null)
                    {
                        if(!Parameters.ContainsKey(Parameter)) 
                            Parameters.Add(Parameter, "true");
                    }
                    Parameter=Parts[1];
                    break;

                // Parameter with enclosed value
                case 3:
                    // The last parameter is still waiting. 
                    // With no value, set it to true.
                    if(Parameter != null)
                    {
                        if(!Parameters.ContainsKey(Parameter)) 
                            Parameters.Add(Parameter, "true");
                    }

                    Parameter = Parts[1];

                    // Remove possible enclosing characters (",')
                    if(!Parameters.ContainsKey(Parameter))
                    {
                        Parts[2] = Remover.Replace(Parts[2], "$1");
                        Parameters.Add(Parameter, Parts[2]);
                    }

                    Parameter=null;
                    break;
                }
            }
            // In case a parameter is still waiting
            if(Parameter != null)
            {
                if(!Parameters.ContainsKey(Parameter)) 
                    Parameters.Add(Parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists 
        // (overriding C# indexer property)
        public string this [string Param]
        {
            get
            {
                return(Parameters[Param]);
            }
        }
    }

}
