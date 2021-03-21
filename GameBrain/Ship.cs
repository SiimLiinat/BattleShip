using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Enums;

namespace GameBrain
{
    public class Ship
    {
        public List<(int, int)> Coordinates = new();
        public int Length;

        public Ship()
        {
            
        }
        public Ship(int length)
        {
            Length = length;
        }

        public bool CanAddCoordinate(int x, int y)
        {
            return true;
        }

        public void AddCoordinate(int x, int y)
        {
            Coordinates.Add((x, y));
        }
        
        public bool CoordinateInCoordinates(int x, int y)
        {
            return Coordinates.Contains((x, y));
        }

        public void MakeAHit(int x, int y)
        {
            if (CoordinateInCoordinates(x, y))
            {
                Coordinates.Remove((x, y));
            }
        }
        
        public bool IsSunk()
        {
            return Coordinates.Count == 0;
        }

        public static bool CanAddShip(Board board, Ship ship)
        {
            var emptySquares = board.GetEmptySquares();
            return ship.Coordinates.All(coordinate => !emptySquares.Contains(coordinate));
        }

        public static Ship? AddRandomShip(Board board, int length, EShipsCanTouch eShipsCanTouch)
        {
            if (board.GetHeight() < length || board.GetWidth() < length)
                throw new ArithmeticException("Ship cannot be larger than board's sides.");
            
            var random = new Random();
            var side = random.Next(2);
            // 0 == Horizontal, 1 == Vertical

            var ship = CreateRandomShip(board, length, side, eShipsCanTouch);
            if (ship == null) return null;
            foreach (var (item1, item2) in ship.Coordinates)
            {
                board.board[item1, item2] = CellState.S;
            }
            return ship;
        }

        private static Ship? CreateRandomShip(Board board, int length, int side, EShipsCanTouch eShipsCanTouch)
        {
            var random = new Random();
            Ship ship = new(length);
            var emptySquares = new List<(int, int)>();
            if (eShipsCanTouch == EShipsCanTouch.Corner)
            {
                emptySquares = board.GetEmptySquaresWithNoCornerNeighbours();
            }
            else if (eShipsCanTouch == EShipsCanTouch.No)
            {
                emptySquares = board.GetEmptySquaresWithNoNeighbours();
            }
            else if (eShipsCanTouch == EShipsCanTouch.Yes)
            {
                emptySquares = board.GetEmptySquares();
            }
            var potentialSquares = new List<(int, int)>();
            if (side == 0) potentialSquares = emptySquares.FindAll(x => x.Item1 <= board.GetWidth() - length);
            if (side == 1) potentialSquares = emptySquares.FindAll(x => x.Item2 <= board.GetHeight() - length);
            while (ship.Coordinates.Count != length)
            {
                if (potentialSquares.Count == 0) break;
                if (ship.Coordinates.Count != 0) ship.Coordinates.Clear();
                var currentSquare = potentialSquares[random.Next(potentialSquares.Count)];
                potentialSquares.Remove(currentSquare);
                ship.Coordinates.Add(currentSquare);
                for (var i = 0; i < length - 1; i++)
                {
                    (int, int) sideSquare = side switch
                    {
                        0 => (currentSquare.Item1 + 1, currentSquare.Item2),
                        1 => (currentSquare.Item1, currentSquare.Item2 + 1),
                        _ => new()
                    };
                    if (emptySquares.Contains(sideSquare))
                    {
                        ship.Coordinates.Add(sideSquare);
                        currentSquare = sideSquare;
                    }
                    else break;
                }
            }
            return ship.Coordinates.Count == length ? ship : null;
        }

        public static Ship? AddShip(Board board, int x, int y, int side, int length, EShipsCanTouch eShipsCanTouch)
        {
            Ship ship = new(length);
            ship.Coordinates.Add((x, y));
            var emptySquares = new List<(int, int)>();
            if (eShipsCanTouch == EShipsCanTouch.Corner)
            {
                emptySquares = board.GetEmptySquaresWithNoCornerNeighbours();
            }
            else if (eShipsCanTouch == EShipsCanTouch.No)
            {
                emptySquares = board.GetEmptySquaresWithNoNeighbours();
            }
            else if (eShipsCanTouch == EShipsCanTouch.Yes)
            {
                emptySquares = board.GetEmptySquares();
            }

            var currentX = x;
            var currentY = y;
            for (int i = 0; i < length - 1; i++)
            {
                if (side == 0)
                {
                    if (emptySquares.Contains((++currentX, y)))
                    {
                        ship.Coordinates.Add((currentX, y));
                    }
                    else break;
                }
                else if (side == 1)
                {
                    if (emptySquares.Contains((x, ++currentY)))
                    {
                        ship.Coordinates.Add((x, currentY));
                    }
                    else break;
                }
            }
            
            if (ship.Coordinates.Count != length)
            {
                currentX = x;
                currentY = y;
                var shipCount = ship.Coordinates.Count;
                for (int i = 0; i < length - shipCount; i++)
                {
                    if (side == 0)
                    {
                        if (emptySquares.Contains((--currentX, y)))
                        {
                            ship.Coordinates.Add((currentX, y));
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (side == 1)
                    {
                        if (emptySquares.Contains((x, --currentY)))
                        {
                            ship.Coordinates.Add((x, currentY));
                        }
                        else break;
                    }
                }
            }
            return ship.Coordinates.Count == length ? ship : null;
        }
    }
}