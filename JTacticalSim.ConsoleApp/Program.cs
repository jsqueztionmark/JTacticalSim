using System;
using System.Runtime.InteropServices;
using JTacticalSim.API;

namespace JTacticalSim.ConsoleApp
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var ctx = GameContext.Instance;
			ctx.InitializeGame(true);			
			ctx.GameLoop(0.0);
		}    
		

#region Set font

	[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	static extern bool GetCurrentConsoleFontEx(
			IntPtr consoleOutput, 
			bool maximumWindow,
			ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

	[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	static extern bool SetCurrentConsoleFontEx(
			IntPtr consoleOutput, 
			bool maximumWindow,
			CONSOLE_FONT_INFO_EX consoleCurrentFontEx);

	private const int STD_OUTPUT_HANDLE = -11;
	private const int TMPF_TRUETYPE = 4;
	private const int LF_FACESIZE = 32;
	private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

	[StructLayout(LayoutKind.Sequential)]
	internal struct COORD
	{
		internal short X;
		internal short Y;

		internal COORD(short x, short y)
		{
			X = x;
			Y = y;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CONSOLE_FONT_INFO_EX 
	{
		internal uint cbSize;
		internal uint nFont;
		internal COORD dwFontSize;
		internal int FontFamily;
		internal int FontWeight;
		internal fixed char FaceName[LF_FACESIZE];
	}

#endregion

	}
}
