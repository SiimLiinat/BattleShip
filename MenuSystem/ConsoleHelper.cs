using System;

namespace MenuSystem
{
    public static class ConsoleHelper
    {
        public static int MultipleChoice(bool canCancel, MenuItem[] options, int lines = 1)
        {
        const int startX = 5;
        const int startY = 2;
        var optionsPerLine = lines;
        const int spacingPerLine = 5;

        int currentSelection = 0;
        
        ConsoleKey key;
        Console.Clear();
        Console.CursorVisible = false;
        
        do
        {

            for (int i = 0; i < options.Length; i++)
            {
                Console.SetCursorPosition(startX + (i % optionsPerLine) * spacingPerLine, startY + i / optionsPerLine);

                if(i == currentSelection)
                    Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(options[i]);

                Console.ResetColor();
            }

            key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.LeftArrow:
                {
                    if (currentSelection % optionsPerLine > 0)
                        currentSelection--;
                    break;
                }
                case ConsoleKey.RightArrow:
                {
                    if (currentSelection % optionsPerLine < optionsPerLine - 1)
                        currentSelection++;
                    break;
                }
                case ConsoleKey.UpArrow:
                {
                    if (currentSelection >= optionsPerLine)
                        currentSelection -= optionsPerLine;
                    break;
                }
                case ConsoleKey.DownArrow:
                {
                    if (currentSelection + optionsPerLine < options.Length)
                        currentSelection += optionsPerLine;
                    break;
                }
                case ConsoleKey.Escape:
                {
                    if (canCancel)
                        return -1;
                    break;
                }
                }
        } while (key != ConsoleKey.Enter);

        Console.CursorVisible = true;
        return currentSelection;
        }
    }
}