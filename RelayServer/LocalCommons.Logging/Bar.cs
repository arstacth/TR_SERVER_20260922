using System;

namespace LocalCommons.Logging
{
	public class Bar
	{
		public static void OverwriteConsoleMessage(string message)
		{
			Console.CursorLeft = 0;
			int num = Console.WindowWidth - 1;
			if (message.Length > num)
			{
				message = message.Substring(0, num - 3) + "...";
			}
			message += new string(' ', num - message.Length);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(message);
			Console.ResetColor();
		}

		public static void RenderConsoleProgress(int percentage)
		{
			RenderConsoleProgress(percentage, '▐', Console.ForegroundColor, "");
		}

		public static void RenderConsoleProgress(int percentage, char progressBarCharacter, ConsoleColor color, string message)
		{
			Console.CursorVisible = false;
			ConsoleColor foregroundColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.CursorLeft = 0;
			int num = Console.WindowWidth - 1;
			int num2 = (int)((double)(num * percentage) / 100.0);
			Console.Write(new string(progressBarCharacter, num2) + new string(' ', num - num2));
			if (string.IsNullOrEmpty(message))
			{
				message = "";
			}
			Console.CursorTop++;
			OverwriteConsoleMessage(message);
			Console.CursorTop--;
			Console.ForegroundColor = foregroundColor;
			Console.CursorVisible = true;
		}
	}
}
