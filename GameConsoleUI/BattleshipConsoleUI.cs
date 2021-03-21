using System;
using System.Drawing;
using GameBrain;
using Console = Colorful.Console;

namespace GameConsoleUI
{
    public static class BattleshipConsoleUI
    {
        public static void DrawBoards(Board board1, Board board2)
        {
            const int startX = 0;
            const int startY = 7;
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int row = 1;
            // const int optionsPerLine = 1;
            // const int spacingPerLine = 14;
            
            Console.SetCursorPosition(startX, startY);
            var width = board1.GetWidth(); //x
            var height = board1.GetHeight(); //y
            
            Console.SetCursorPosition(4 * width / 2 - 4, 7);
            Console.WriteLine("Opponent's board");
            Console.SetCursorPosition((8 + 4 * width) * 3 / 2, 7);
            Console.WriteLine("Your board");
            Console.WriteLine("");
            
            Console.Write("     +");
            for (int colIndex = 0; colIndex < width; colIndex++)
            {
                Console.Write("---+");
            }
            Console.Write("     ");
            Console.Write("     +");
            for (int colIndex = 0; colIndex < width; colIndex++)
            {
                Console.Write("---+");
            }
            Console.WriteLine();
            
            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                Console.Write(String.Format("{0}   |", row.ToString("D2")));
                for (int colIndex = 0; colIndex < width; colIndex++)
                {
                    var cell = board1.board[colIndex, rowIndex];
                    switch (cell)
                    {
                        case CellState.X:
                            Console.Write($" { Board.EnemySquareString(cell)}");
                            break;
                        case CellState.O:
                            Console.Write($" { Board.EnemySquareString(cell)}", Color.Aqua);
                            break;
                        default:
                            Console.Write($" { Board.EnemySquareString(cell)}");
                            break;
                    }
                    Console.Write(" |");
                }
                Console.Write("     ");
                Console.Write(String.Format("{0}   |", row++.ToString("D2")));
                for (int colIndex = 0; colIndex < width; colIndex++)
                {
                    var cell = board2.board[colIndex, rowIndex];
                    switch (cell)
                    {
                        case CellState.X:
                            Console.Write($" { Board.SquareString(cell)}", Color.Black);
                            break;
                        case CellState.O:
                            Console.Write($" { Board.SquareString(cell)}", Color.Aqua);
                            break;
                        case CellState.S:
                            Console.Write($" { Board.SquareString(cell)}", Color.Magenta);
                            break;
                        default:
                            Console.Write($" { Board.SquareString(cell)}");
                            break;
                    }
                    Console.Write(" |");
                }
                
                Console.WriteLine();
                Console.Write("     +");
                for (int colIndex = 0; colIndex < width; colIndex++)
                {
                    Console.Write("---+");
                }
                Console.Write("     ");
                Console.Write("     +");
                for (int colIndex = 0; colIndex < width; colIndex++)
                {
                    Console.Write("---+");
                }
                Console.WriteLine();
            }
            // letters
            for (int i = 0; i < 2; i++)
            {
                Console.Write("       ");
                for (int letter = 0; letter < board1.GetWidth(); letter++)
                {
                    Console.Write(alphabet[letter]);
                    Console.Write("   ");
                }
                Console.Write("    ");
            }
            Console.WriteLine("");
            Console.WriteLine("");
        }

        
    }
}