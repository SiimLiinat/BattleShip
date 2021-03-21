using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain.Enums;

namespace GameBrain
{
    public static class AI
    {
        private static (int x, int y) MakeRandomMove(Board board, (int, int)? move)
        {
            var rnd = new Random();
            if (move != null)
            {
                var neighbour = GetRandomNeighbour(move.Value, board);
                if (neighbour != move.Value) return neighbour;
            }
            List<(int, int)> squares = board.GetPotentialSquares();
            return squares[rnd.Next(squares.Count)];
        }

        public static (int x, int y) MakeStatisticalMove(Board board, (int, int)? move)
        {
            if (move != null) return MakeRandomMove(board, move);
            List<(int, int)> squares = new();
            using(StreamReader readtext = new StreamReader(@"C:\Users\Siim\RiderProjects\icd0008-2020f\BattleShip\GameBrain\moveHistory.txt"))
            {
                string? line;
                while ((line = readtext.ReadLine() ?? null) != null)
                {
                    string[] lines = line.Split(";");
                    var a = int.Parse(lines[0]);
                    var b = int.Parse(lines[1]);
                    squares.Add((a, b));
                }
            }

            Dictionary<(int, int), int> squareCounts = new();
            foreach (var square in squares)
            {
                if (squareCounts.ContainsKey(square))
                {
                    squareCounts[square] = ++squareCounts[square];
                }
                else
                {
                    squareCounts[square] = 1;
                }
            }
            List<KeyValuePair<(int, int), int>> squareCountAsList = squareCounts.ToList();
            squareCountAsList.Sort((x, y) => x.Value.CompareTo(y.Value));
            List<(int, int)> potentialMoves = new ();
            foreach (var (key, _) in squareCountAsList)
            {
                var a = key.Item1; 
                var b = key.Item2;
                if (board.CanMakeMove(a, b))
                {
                    potentialMoves.Add((a, b));
                    if (potentialMoves.Count > 4) break;
                }
            }
            var random = new Random();
            return potentialMoves.Count != 0 ? potentialMoves[random.Next(potentialMoves.Count)] : MakeRandomMove(board, move);
        }

        public static List<Ship> AddRandomShips(Game game, Board board, EShipsCanTouch eShipsCanTouch)
        {
            List<Ship> ships = new();
            foreach (var (key, value) in game.GetShipStandards())
            {
                for (var i = 0; i < value; i++)
                {
                    var ship = Ship.AddRandomShip(board, key, eShipsCanTouch);
                    if (ship == null) throw new NullReferenceException("Created ship had value null.");
                    ships.Add(ship);
                }
            }

            return ships;
        }

        private static (int, int) GetRandomNeighbour((int, int) square, Board board)
        {
            var rnd = new Random();
            var a = square.Item1;
            var b = square.Item2;
            List<(int, int)> neighbours = new();
            neighbours.Append((a - 1, b - 1))
                .Append((a - 1, b))
                .Append((a - 1, b + 1))
                .Append((a, b + 1))
                .Append((a, b))
                .Append((a, b - 1))
                .Append((a + 1, b + 1))
                .Append((a + 1, b))
                .Append((a + 1, b - 1));
            neighbours = neighbours.FindAll(e => board.CanMakeMove(e.Item1, e.Item2));
            if (neighbours.Count > 0)
            {
                return neighbours[rnd.Next(neighbours.Count)];
            }

            return square;
        }
    }
}