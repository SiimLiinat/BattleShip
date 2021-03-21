using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Console = Colorful.Console;
using DAL;
using Domain;
using Domain.Enums;
using GameBrain;
using GameConsoleUI;
using MenuSystem;
using Microsoft.EntityFrameworkCore;
using Board = Domain.Board;
using Game = GameBrain.Game;
using Ship = GameBrain.Ship;

namespace ConsoleApp
{
    class Program
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

        private static void Main()
        {
            PrintBattleShipAscii();

            var menu = new Menu(MenuLevel.Level0);
            menu.AddMenuItem(new MenuItem("Human vs Human", "1", HumanVsHuman));
            menu.AddMenuItem(new MenuItem("Human vs AI", "2", HumanVsAI));
            menu.AddMenuItem(new MenuItem("AI vs AI", "3", AIVsAI));
            menu.AddMenuItem(new MenuItem("Exit program", "X", ExitMenuAction));

            menu.RunMenu();
        }

        static string ExitMenuAction ()
        {
            Console.Clear();
            Console.WriteLine("Exiting program...");
            return "X";
        }

        private static string HumanVsHuman()
        {
            var game = new Game();
            game.EPlayerType1 = EPlayerType.Human;
            game.EPlayerType2 = EPlayerType.Human;
            var menu = new Menu(MenuLevel.Level1);
            
            menu.AddMenuItem(new MenuItem("Add ships by hand for player 1", "S1", () =>
            {
                menu.RemoveMenuItem("CS");
                foreach (var (key, value) in game.GetShipStandards())
                {
                    for (int i = 0; i < value; i++)
                    {
                        while (true)
                        {
                            var ship = new Ship(key);
                            Console.Clear();
                            var side = -1;
                            BattleshipConsoleUI.DrawBoards(game.Board2, game.Board1);
                            Console.WriteLine("Next ship is " + key + " squares long. Is it horizontal or vertical? Press 0 for horizontal, 1 for vertical.");
                            while (!(side == 0 || side == 1))
                            {
                                var result = int.TryParse(Console.ReadKey().KeyChar.ToString(), out var number);
                                if (result) side = number;
                            }
                            var (x, y) = Game.GetShipCoordinates(game, game.Board1);
                            if (game.Board1.GetEmptySquares().Contains((x, y)))
                            {
                                ship = Ship.AddShip(game.Board1, x, y, side, key, game.EShipsCanTouch);
                                if (ship != null && ship.Coordinates.Count != 0)
                                {
                                    game.Ships1.Add(ship!);
                                    Console.WriteLine("Added ship with coordinates:");
                                    foreach (var (item1, item2) in ship!.Coordinates)
                                    {
                                        game.Board1.board[item1, item2] = CellState.S;
                                
                                        Console.Write(Alphabet[item1]+(1+item2));
                                    }
                                    Console.Clear();
                                    BattleshipConsoleUI.DrawBoards(game.Board1, game.Board1);
                                    Console.WriteLine("          Press any key to Continue!", Color.Blue);
                                    break;
                                }
                                else
                                {
                                    Console.Clear();
                                    Console.WriteLine("Ship could not be placed in that position. Please try again.");
                                }
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Ship could not be placed in that position. Please try again.");
                            }
                            Console.ReadKey();
                        }
                    }
                }
                menu.RemoveMenuItem("S1");
                menu.RemoveMenuItem("GR1");
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Generate for player 1.", "GR1", () =>
            {
                menu.RemoveMenuItem("CS");
                game.Ships1 = AI.AddRandomShips(game, game.Board1, game.EShipsCanTouch);
                Console.Clear();
                Console.WriteLine("Ships have been generated for player 1. Press a button to continue.");
                Console.ReadKey();
                menu.RemoveMenuItem("S1");
                menu.RemoveMenuItem("GR1");
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Add ships by hand for player 2", "S2", () =>
            {
                menu.RemoveMenuItem("CS");
                foreach (var (key, value) in game.GetShipStandards())
                {
                    for (int i = 0; i < value; i++)
                    {
                        while (true)
                        {
                            var ship = new Ship(key);
                            Console.Clear();
                            var side = -1;
                            BattleshipConsoleUI.DrawBoards(game.Board1, game.Board2);
                            Console.WriteLine("Next ship is " + key + " squares long. Is it horizontal or vertical? Press 0 for horizontal, 1 for vertical.");
                            while (!(side == 0 || side == 1))
                            {
                                var result = int.TryParse(Console.ReadKey().KeyChar.ToString(), out var number);
                                if (result) side = number;
                            }
                            var (x, y) = Game.GetShipCoordinates(game, game.Board1);
                            if (game.Board1.GetEmptySquares().Contains((x, y)))
                            {
                                ship = Ship.AddShip(game.Board2, x, y, side, key, game.EShipsCanTouch);
                                if (ship != null && ship.Coordinates.Count != 0)
                                {
                                    game.Ships2.Add(ship!);
                                    Console.WriteLine("Added ship with coordinates:");
                                    foreach (var (item1, item2) in ship!.Coordinates)
                                    {
                                        game.Board1.board[item1, item2] = CellState.S;
                                
                                        Console.Write(Alphabet[item1]+(1+item2));
                                    }
                                    Console.Clear();
                                    BattleshipConsoleUI.DrawBoards(game.Board1, game.Board1);
                                    Console.WriteLine("          Press any key to Continue!", Color.Blue);
                                    break;
                                }
                                else
                                {
                                    Console.Clear();
                                    Console.WriteLine("Ship could not be placed in that position. Please try again.");
                                }
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Ship could not be placed in that position. Please try again.");
                            }
                            Console.ReadKey();
                        }
                    }
                }
                menu.RemoveMenuItem("S2");
                menu.RemoveMenuItem("GR2");
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Generate ships for player 2.", "GR2", () =>
            {
                menu.RemoveMenuItem("CS");
                game.Ships2 = AI.AddRandomShips(game, game.Board2, game.EShipsCanTouch);
                menu.RemoveMenuItem("S2");
                menu.RemoveMenuItem("GR2");
                Console.Clear();
                Console.WriteLine("Ships have been generated for player 2. Press a button to continue.");
                Console.ReadKey();
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem($"Make a move", "1",() =>
            {
                GameBrain.Board board = game.IsNextMoveByPlayer1() ? game.Board2 : game.Board1;
                Menu menu2 = new(MenuLevel.Level2Plus, game.Board1.GetWidth());
                for (var j = 0; j < game.Board1.GetWidth(); j++)
                {
                    for (var i = 0; i < game.Board1.GetHeight(); i++)
                    {
                        var iCopy = i;
                        var jCopy = j;
                        var cellState = board.board[iCopy, jCopy];
                        menu2.AddMenuItem(new MenuItem(CellString(cellState), Alphabet[iCopy] + jCopy.ToString(), () =>
                        {
                            var move = board.MakeMove(game, iCopy, jCopy);
                            if (!move.Item1) return "";
                            Console.Clear();
                            if (game.IsNextMoveByPlayer1()) BattleshipConsoleUI.DrawBoards(game.Board2, game.Board1);
                            else BattleshipConsoleUI.DrawBoards(game.Board1, game.Board2);
                            if (game.IsWinner() != -1)
                            {
                                Console.WriteAscii($@"Player {game.IsWinner()} has won.");
                                Console.WriteLine("          Press any key to exit!", Color.Blue);
                                Console.ReadKey();
                                menu.BreakMenu = true;
                                return "X";
                            }
                            Console.WriteLine("          Press any key to Continue or Enter to skip directly to next move!", Color.Blue);
                            while (!Console.KeyAvailable) {}
                            return "P";
                        }));
                    }
                }
                menu2.RunMenu();
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Save game", "2",() =>
            {
                SaveGameAction(game);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Load game", "L", () =>
            {
                LoadGameAction(game);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Save game to database", "DB", () =>
            {
                game.SaveGameToDataBase();
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Change settings", "CS", () =>
            {
                ChangeSettings(game);
                return "";
            }));

            menu.AddMenuItem(new MenuItem($"Return to Main Menu", "M", MainMenuAction));
            
            menu.AddMenuItem(new MenuItem($"Exit game", "X", () =>
            {
                menu.BreakMenu = true;
                return ExitMenuAction();
            }));

            var userChoice = menu.RunMenu();
            return userChoice;
        }
        
        private static string HumanVsAI()
        {
            var game = new Game();
            game.EPlayerType1 = EPlayerType.Human;
            game.EPlayerType2 = EPlayerType.AI;
            game.Ships2 = AI.AddRandomShips(game, game.Board2, game.EShipsCanTouch);
            var menu = new Menu(MenuLevel.Level1);
            (int, int)? moveAI = null;
            
            menu.AddMenuItem(new MenuItem("Add ships by hand", "S1", () =>
            {
                menu.RemoveMenuItem("CS");
                foreach (var (key, value) in game.GetShipStandards())
                {
                    for (int i = 0; i < value; i++)
                    {
                        while (true)
                        {
                            var ship = new Ship(key);
                            Console.Clear();
                            var side = -1;
                            BattleshipConsoleUI.DrawBoards(game.Board2, game.Board1);
                            Console.WriteLine("Next ship is " + key + " squares long. Is it horizontal or vertical? Press 0 for horizontal, 1 for vertical.");
                            while (!(side == 0 || side == 1))
                            {
                                var result = int.TryParse(Console.ReadKey().KeyChar.ToString(), out var number);
                                if (result) side = number;
                            }
                            var (x, y) = Game.GetShipCoordinates(game, game.Board1);
                            if (game.Board1.GetEmptySquares().Contains((x, y)))
                            {
                                ship = Ship.AddShip(game.Board1, x, y, side, key, game.EShipsCanTouch);
                                if (ship != null && ship.Coordinates.Count != 0)
                                {
                                    game.Ships1.Add(ship!);
                                    Console.WriteLine("Added ship with coordinates:");
                                    foreach (var (item1, item2) in ship!.Coordinates)
                                    {
                                        game.Board1.board[item1, item2] = CellState.S;
                                
                                        Console.Write(Alphabet[item1]+(1+item2));
                                    }
                                    Console.Clear();
                                    BattleshipConsoleUI.DrawBoards(game.Board1, game.Board1);
                                    Console.WriteLine("          Press any key to Continue!", Color.Blue);
                                    break;
                                }
                                else
                                {
                                    Console.Clear();
                                    Console.WriteLine("Ship could not be placed in that position. Please try again.");
                                }
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Ship could not be placed in that position. Please try again.");
                            }
                            Console.ReadKey();
                        }
                    }
                }
                menu.RemoveMenuItem("S1");
                menu.RemoveMenuItem("GR1");
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Generate ships by random", "GR1", () =>
            {
                menu.RemoveMenuItem("CS");
                game.Ships1 = AI.AddRandomShips(game, game.Board1, game.EShipsCanTouch);
                menu.RemoveMenuItem("S1");
                menu.RemoveMenuItem("GR1");
                Console.Clear();
                Console.WriteLine("Ships have been generated for player. Press a button to continue.");
                Console.ReadKey();
                return "";
            }));

            menu.AddMenuItem(new MenuItem($"Make a move", "1", () =>
            {
                Menu menu2 = new(MenuLevel.Level2Plus, game.Board1.GetWidth());
                for (var j = 0; j < game.Board1.GetWidth(); j++)
                {
                    for (var i = 0; i < game.Board1.GetHeight(); i++)
                    {
                        var iCopy = i;
                        var jCopy = j;
                        var cellState = game.Board2.board[iCopy, jCopy];
                        var (canMakeMove, isAHit) = (false, false);
                        menu2.AddMenuItem(new MenuItem(CellString(cellState), Alphabet[iCopy] + jCopy.ToString(), () => {
                            (canMakeMove, isAHit) = game.Board2.MakeMove(game, iCopy, jCopy);
                            if (!canMakeMove) return "";
                            Console.Clear();
                            BattleshipConsoleUI.DrawBoards(game.Board2, game.Board1);
                            if (Game.AllShipsSunk(game.Ships2))
                            {
                                Console.WriteAscii("Player has won.");
                                Console.WriteLine("          Press any key to exit!", Color.Blue);
                                while (!Console.KeyAvailable) {}
                                menu.BreakMenu = true;
                                return "X";
                            }
                            Console.WriteLine("          Press any key to Continue!", Color.Blue);
                            Console.ReadKey();
                            if (isAHit && game.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer) return "P";
                            do
                            {
                                Console.Clear();
                                Console.WriteLine("AI is thinking...");
                                Thread.Sleep(1000);
                                var (a, b) = AI.MakeStatisticalMove(game.Board1, moveAI);
                                (canMakeMove, isAHit) = game.Board1.MakeMove(game, a, b);
                                if (!canMakeMove) return "";
                                if (isAHit) moveAI = (a, b);
                                BattleshipConsoleUI.DrawBoards(game.Board1, game.Board2);
                                if (Game.AllShipsSunk(game.Ships1))
                                {
                                    Console.WriteAscii("AI has won.");
                                    Console.WriteLine("          Press any key to exit!", Color.Aqua);
                                    Console.ReadKey();
                                    menu.BreakMenu = true;
                                    return "X";
                                }
                                Console.WriteLine("Press any key to Continue!", Color.Blue);
                                Console.ReadKey();
                            } while (isAHit && game.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer);
                            return "P";
                        }));
                    }
                }
                menu2.RunMenu();
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem($"Save game", "2",() =>
            {
                SaveGameAction(game);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Load game", "L", () =>
            {
                LoadGameAction(game);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Save game to database", "DB", () =>
            {
                game.SaveGameToDataBase();
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Change settings", "CS", () =>
            {
                ChangeSettings(game);
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem($"Return to Main Menu", "M", MainMenuAction));
            menu.AddMenuItem(new MenuItem($"Exit game", "X", () =>
            {
                menu.BreakMenu = true;
                return ExitMenuAction();
            }));

            var userChoice = menu.RunMenu();


            return userChoice;
        }
        
        private static string AIVsAI()
        {
            var game = new Game();
            game.Ships1 = AI.AddRandomShips(game, game.Board1, game.EShipsCanTouch);
            game.Ships2 = AI.AddRandomShips(game, game.Board2, game.EShipsCanTouch);
            game.EPlayerType1 = EPlayerType.AI;
            game.EPlayerType2 = EPlayerType.AI;
            var menu = new Menu(MenuLevel.Level1);
            (int, int)? moveAI = null;

            menu.AddMenuItem(new MenuItem($"Start game", "1",() =>
            {
                menu.RemoveMenuItem("CS");
                do
                {
                    GameBrain.Board board = game.IsNextMoveByPlayer1() ? game.Board2 : game.Board1;
                    List<Ship> ships = game.IsNextMoveByPlayer1() ? game.Ships2 : game.Ships1;
                    Console.Clear();
                    Console.WriteLine("AI is thinking...");
                    var (a, b) = AI.MakeStatisticalMove(board, moveAI);
                    var (canMakeMove, isAHit) = board.MakeMove(game, a, b);
                    if (!canMakeMove) return "";
                    Console.Clear();
                    BattleshipConsoleUI.DrawBoards(board, game.IsNextMoveByPlayer1() ? game.Board1 : game.Board2);
                    if (isAHit) moveAI = (a, b);
                    else moveAI = null;
                    if (game.IsWinner() != -1)
                    {
                        Console.WriteAscii($@"AI {game.IsWinner()} has won.");
                        Console.WriteLine("          Press any key to exit!", Color.Blue);
                        Console.ReadKey();
                        menu.BreakMenu = true;
                        return "M";
                    }
                    Thread.Sleep(1000);
                    if (!isAHit && game.ENextMoveAfterHit != ENextMoveAfterHit.SamePlayer) game.SetNextMoveToOtherPlayer();
                } while (game.IsWinner() == -1);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Save game", "2",() =>
            {
                SaveGameAction(game);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Load game", "L", () =>
            {
                LoadGameAction(game);
                return "";
            }));
            menu.AddMenuItem(new MenuItem($"Save game to database", "DB", () =>
            {
                game.SaveGameToDataBase();
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem("Change settings", "CS", () =>
            {
                ChangeSettings(game);
                return "";
            }));
            
            menu.AddMenuItem(new MenuItem($"Return to Main Menu", "M", MainMenuAction));
            menu.AddMenuItem(new MenuItem($"Exit game", "X", () =>
            {
                menu.BreakMenu = true;
                return ExitMenuAction();
            }));

            var userChoice = menu.RunMenu();


            return userChoice;
        }

        private static string MainMenuAction()
        {
            Console.Clear();
            return "";
        }

        private static string ChangeSettings (Game game)
        {
            Console.Clear();
            Console.WriteLine("How big should the board be? Enter an integer for example 5.");
            var boardSize = 5;
            var result = int.TryParse(Console.ReadLine(), out var number);
            if (result && boardSize > 0) boardSize = number;
            else
            {
                Console.WriteLine("Could not parse answer. Defaulted to 10x10.");
            }
            game.SetBoardSize(boardSize, boardSize);
            
            Console.WriteLine("Can ships touch? 0 - No. 1 - Corners only. 2 - Yes.");
            var canShipsTouch = 0;
            var shipsTouch = int.TryParse(Console.ReadLine(), out var touch);
            if (shipsTouch && touch >= 0 && touch < 3) canShipsTouch = touch;
            else
            {
                Console.WriteLine("Could not parse answer. Defaulted to No.");
            }
            game.EShipsCanTouch = (EShipsCanTouch) canShipsTouch;
            
            Console.WriteLine("Whose move is it after a ship has been hit? 0 - Same player who hit the ship. 1 - Other player");
            var nextMove = 0;
            var moveAfterHit = int.TryParse(Console.ReadLine(), out var move);
            if (moveAfterHit && move >= 0 && move < 2) nextMove = move;
            else
            {
                Console.WriteLine("Could not parse answer. Defaulted to other player..");
            }
            game.ENextMoveAfterHit = (ENextMoveAfterHit) nextMove;
            
            Console.WriteLine("Do you want to change ship sizes or counts? Current ships are:");
            game.PrintShipStandards();
            Console.WriteLine("Do you want to change these? Y/N?");
            var answer = Console.ReadKey();
            Console.WriteLine("");
            if (answer.KeyChar == 'Y')
            {
                var standards = new Dictionary<int, int>();
                while (true)
                {
                    Console.WriteLine("How long should the ship be?");
                    var length = int.TryParse(Console.ReadLine(), out var integer);
                    Console.WriteLine("How many of this size ships should there be?");
                    var count = int.TryParse(Console.ReadLine(), out var secondInteger);
                    if (length && count)
                    {
                        standards.Add(integer, secondInteger);
                        Console.WriteLine("Would you like to add another ship? Y/N");
                    }
                    else
                    {
                        Console.WriteLine("Sorry, could not parse your answers. Would you like to try again? Y/N");
                    }
                    if (Console.ReadKey().KeyChar != 'Y') break;
                }
                game.SetShipStandards(standards);
            }
            Console.WriteLine("");
            Console.WriteLine("Ship sizes and counts set to:");
            game.PrintShipStandards();
            Console.WriteLine("Press a key to return back to menu.");
            Console.ReadKey();
            return "";
        }

        private static void LoadGameAction(Game game)
        {
            var files = Directory.EnumerateFiles(".", "*.json").ToList();
            Console.Clear();
            for (var i = 0; i < files.Count; i++)
            {
                Console.WriteLine($"{i} - {files[i]}");
            }

            var fileNo = Console.ReadLine();
            var fileName = files[int.Parse(fileNo.Trim())];

            var jsonString = File.ReadAllText(fileName);

            game.SetGameStateFromJsonString(jsonString);
            
            Console.Clear();
        }
        
        private static void SaveGameAction(Game game)
        {
            Console.Clear();
            var defaultName = "save_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".json";
            Console.WriteLine($"Save game as {defaultName} ? Y/N");
            var confirmation = Console.ReadLine().Trim().ToUpper();
            string filename;
            while (confirmation.Equals(null) || confirmation.Length == 0)
            {
                Console.WriteLine($"Save game as {defaultName} ? Please answer Y/N");
                confirmation = Console.ReadLine().Trim();
            }
            if (confirmation == "Yes" || confirmation == "Y")
            {
                filename = defaultName;
            }
            else
            {
                Console.WriteLine("Write the name of the game save file.");
                filename = Console.ReadLine().Trim();
                while (filename.Equals(null) || filename.Length == 0)
                {
                    Console.WriteLine($"Write the name of the game save file. The filename cannot be empty.");
                    filename = Console.ReadLine().Trim();
                }
            }
            Console.Clear();
            Console.WriteLine($"Saving game to {filename}.json");

            var serializedGame = game.GetSerializedGameState();
            File.WriteAllText(filename, serializedGame);
        }

        private static void PrintBattleShipAscii()
        {
            // Battleship ASCII art by Matthew Bace. https://ascii.co.uk/art/battleship
            // Wave ASCII art by cjr. https://ascii.co.uk/art/wave
            // Text to ASCII: http://www.network-science.de/ascii/
            
            SlowPrint(130, @"                                                  |__end
                                                  |\/end
                                                  ---end
                                                  / | [end
                                           !      | |||end
                                         _/|     _/|-++'end
                                     +  +--|    |--|--|_ |-end
                                  { /|__|  |/\__|  |--- |||__/end
                                 +---------------___[}-_===_.'____                 /\end
                             ____`-' ||___-{]_| _[}-  |     |_[___\==--            \/   _end
              __..._____--==/___]_|__|_____________________________[___\==--____,------' .7end
             |                                                                     BB-61/end
              \_________________________________________________________________________|");
            Console.WriteLine("``'-.,_,.-'``'-.,_,.='``'-.,_,.-'``'-.,_,.='````'-.,_,.-'``'-.,_,.='``'-.,_,.-'``'-.,_,.='````'-.,_,.-'`", Color.Aqua);
            
            while (!Console.KeyAvailable)
            {
                Thread.Sleep(1200);
                SlowPrint(130, @"end
                              ____        __  __  __          __    _     end
                             / __ )____ _/ /_/ /_/ /__  _____/ /_  (_)___ end
                            / __  / __ `/ __/ __/ / _ \/ ___/ __ \/ / __ \end
                           / /_/ / /_/ / /_/ /_/ /  __(__  ) / / / / /_/ /end
                          /_____/\__,_/\__/\__/_/\___/____/_/ /_/_/ .___/ end
                                                                 /_/      ");
                Thread.Sleep(1500);
                Console.WriteLine("                                     Press any key to Continue!");
                Console.ReadKey();
                break;
            }
        }

        private static void SlowPrint(int time, string text)
        {
            while (!Console.KeyAvailable){
                foreach (var letter in text.Split("end"))
                {
                    Console.Write(letter);
                    Thread.Sleep(time);
                }
                Console.WriteLine("");

                break;
            }
        }

        private static string CellString(CellState cellState)
        {
            switch (cellState)
            {
                case CellState.Empty: return "?";
                case CellState.O: return "O";
                case CellState.S: return "?";
                case CellState.X: return "❌";
            }

            return "-";
        }
    }
}