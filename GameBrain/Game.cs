using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DAL;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Console = Colorful.Console;

namespace GameBrain
{
    public class Game
    {
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
        public Board Board1 = new();
        public List<Ship> Ships1 = new();
        public EPlayerType EPlayerType1;
        
        public Board Board2 = new();
        public List<Ship> Ships2 = new();
        public EPlayerType EPlayerType2;
        public EShipsCanTouch EShipsCanTouch { get; set; } = EShipsCanTouch.No;
        public ENextMoveAfterHit ENextMoveAfterHit { get; set; } = ENextMoveAfterHit.OtherPlayer;
        
        // count, length
        private Dictionary<int, int> ShipStandards { get; set; } = new() {{1, 1}, {2, 1}, {3, 1}, {4, 1}, {5, 1}};
        
        private bool _nextMoveByPlayer1 = true;

        public void SetBoardSize(int width, int height)
        {
            Board1 = new Board(width, height);
            Board2 = new Board(width, height);
        }
        
        public void SetShipStandards(Dictionary<int, int> newStandards)
        {
            ShipStandards = newStandards;
        }
        
        public Dictionary<int, int> GetShipStandards()
        {
            return ShipStandards;
        }
        
        public void PrintShipStandards()
        {
            foreach (var (key, value) in GetShipStandards())
            {
                Console.WriteLine($"Ship length: {key}, ship count: {value}");
            }
        }

        public int GetShipCount()
        {
            var count = 0;
            foreach (var (_, value) in ShipStandards)
            {
                count += value;
            }

            return count;
        }
        
        private static void HitShip(List<Ship> ships, int x, int y)
        {
            foreach (var ship in ships)
            {
                if (ship.CoordinateInCoordinates(x, y))
                {
                    ship.MakeAHit(x, y);
                }
                break;
            }
        }

        public int IsWinner()
        {
            if (AllShipsSunk(Ships1)) return 1;
            if (AllShipsSunk(Ships2)) return 2;
            return -1;
        }

        public static bool AllShipsSunk(List<Ship> ships)
        {
            return ships.All(ship => ship.IsSunk());
        }
        
        public string GetSerializedGameState()
        {
            var state = new GameState()
            {
                NextMoveByPlayer1 = _nextMoveByPlayer1,
                Width = Board1.GetWidth(),
                Height = Board1.GetHeight()
            };
            state.Board1 = new CellState[state.Width][];
            state.Board2 = new CellState[state.Width][];
            for (int i = 0; i < state.Board1.Length; i++)
            {
                state.Board1[i] = new CellState[state.Height];
                state.Board2[i] = new CellState[state.Height];
            }
            
            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    state.Board1[x][y] = Board1.board[x, y];
                    state.Board2[x][y] = Board2.board[x, y];
                }
            }

            state.Ships = GetShipStandards();

            return JsonSerializer.Serialize(state);
        }

        public void SetGameStateFromJsonString(string jsonString)
        {
            var state = JsonSerializer.Deserialize<GameState>(jsonString);
            
            // restore actual state from deserialized state
            _nextMoveByPlayer1 = state!.NextMoveByPlayer1;
            Board1 =  new Board(state.Width, state.Height);
            Board2 =  new Board(state.Width, state.Height);
            
            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    Board1.board[x, y] = state.Board1[x][y];
                    Board2.board[x, y] = state.Board2[x][y];
                }
            }
            SetShipStandards(state.Ships!);
        }

        public bool IsNextMoveByPlayer1()
        {
            return _nextMoveByPlayer1;
        }
        
        public void SetNextMoveToOtherPlayer()
        {
            _nextMoveByPlayer1 = !_nextMoveByPlayer1;
        }
        
        public Board GetBoard1()
        {
            var res = new Board(Board1.GetWidth(), Board1.GetHeight());
            Array.Copy(Board1.board, res.board, Board1.GetHeight());
            return res;
        }
        
        public Board GetBoard2()
        {
            var res = new Board(Board2.GetWidth(), Board2.GetHeight());
            Array.Copy(Board2.board, res.board, Board2.GetHeight());
            return res;
        }
        
        public static (int x, int y) GetShipCoordinates(Game game, Board board)
        {
            Console.WriteLine("Choose ship starting position. Example: A1");
            while (true)
            {
                var userValue = Console.ReadLine()?.ToUpper();
                if (userValue == null || userValue.Length < 2) continue;
                var x = Alphabet.IndexOf(userValue[0]);
                var yValidation = int.TryParse(userValue.Substring(1), out _);
                var y = -1;
                if (yValidation)
                {
                    y = int.Parse(userValue.Substring(1)) - 1;
                }
                if (x != -1 && x < game.Board1.GetWidth() && y < game.Board1.GetHeight() && y >= 0 && board.CanMakeMove(x, y))
                {
                    return (x, y);
                }
                Console.WriteLine("Please enter a valid position similar to format A2 or J10 that has not already been hit.");
            }
        }

        public async Task<int> SaveGameToDataBase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(@"Server=barrel.itcollege.ee,1533; User Id=student;Password=Student.Bad.password.0;Database=siliin_battleship;MultipleActiveResultSets=true");
            var db = new ApplicationDbContext(optionsBuilder.Options);
            
            var _context = db;
            
            var game = new Domain.Game
            {
                TurnA = IsNextMoveByPlayer1(),
                GameOptionId = 1,
                GameJson = GetSerializedGameState(),
                CreatedAt = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss"),
                Boards = new List<Domain.Board>(),
                Players = new List<Player>()
            };
            db.Games.Add(game);
            
            var gameOption = await _context.GameOptions
                .Where(x => x.BoardLength == this.Board1.GetHeight() 
                            && x.BoardWidth == this.Board1.GetWidth()
                            && x.EShipsCanTouch == EShipsCanTouch
                            && x.ENextMoveAfterHit == ENextMoveAfterHit)
                .FirstOrDefaultAsync();
            if (gameOption == null)
            {
                gameOption = new GameOption
                {
                    BoardLength = this.Board1.GetHeight(),
                    BoardWidth = this.Board1.GetWidth(),
                    EShipsCanTouch = EShipsCanTouch,
                    ENextMoveAfterHit = ENextMoveAfterHit,
                    Games = new List<Domain.Game>()
                };   
            }
            game.GameOption = gameOption;
            gameOption.Games?.Add(game);

            var Player1 = new Player
            {
                EPlayerType = EPlayerType1,
                Game = game,
                Name = EPlayerType1.ToString()
            };
            var Player2 = new Player
            {
                EPlayerType = EPlayerType2,
                Game = game,
                Name = EPlayerType2.ToString()
            };
            
            db.Players.Add(Player1);
            db.Players.Add(Player2);
            
            var Board1 = new Domain.Board
            {
                Game = game
            };
            var Board2 = new Domain.Board
            {
                Game = game
            };
            game.Boards.Add(Board1);
            game.Boards.Add(Board2);
            
            game.Players.Add(Player1);
            game.Players.Add(Player2);
            db.Boards.Add(Board1);
            db.Boards.Add(Board2);

            foreach (var ship in Ships1)
            {
                var newShip = new Domain.Ship
                {
                    Name = ship.Length + "x1",
                    Size = ship.Length,
                    IsSunken = ship.IsSunk(),
                    Board = Board1
                };
                db.Ships.Add(newShip);
            }
            
            foreach (var ship in Ships2)
            {
                var newShip = new Domain.Ship
                {
                    Name = ship.Length + "x1",
                    Size = ship.Length,
                    IsSunken = ship.IsSunk(),
                    Board = Board2
                };
                db.Ships.Add(newShip);
            }
            
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
            
            db.SaveChanges();
            return game.GameId;
        }
    }
}