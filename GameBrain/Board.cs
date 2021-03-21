using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Domain.Enums;

namespace GameBrain
{
    public class Board
    {
        public CellState[,] board;
        private static int Width { get; set; }
        private static int Height { get; set; }
        
        public Board()
        {
            Width = 10;
            Height = 10;
            board = new CellState[Width,Height];
        }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            board = new CellState[Width,Height];
        }

        public CellState[,] GetCellStates()
        {
            var res = new CellState[Width, Height];
            Array.Copy(board, res, Height);
            return res;
        }

        public CellState? GetSquare(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                return board[x, y];
            }
            return null;
        }

        public (bool, bool) MakeMove(Game game, int x, int y)
        {
            (bool, bool) moveBool = new(false, false);
            if (CanMakeMove(x, y))
            {
                moveBool.Item1 = true;
                if (IsAHit(x, y))
                {
                    moveBool.Item2 = true;
                    using (StreamWriter writeText =
                        new(@"C:\Users\Siim\RiderProjects\icd0008-2020f\BattleShip\GameBrain\moveHistory.txt", true))
                    {
                        writeText.WriteLine(x + ";" + y);
                    }
                    foreach (var ship in game.Ships2.Where(ship => ship.CoordinateInCoordinates(x, y)))
                    {
                        ship.MakeAHit(x, y);
                        if (ship.Coordinates.Count == 0 && game.IsWinner() == -1) Console.WriteLine("Battleship sunk.", Color.Aqua);
                    }
                    if (game.ENextMoveAfterHit == ENextMoveAfterHit.SamePlayer) game.SetNextMoveToOtherPlayer();
                }
                else
                {
                    game.SetNextMoveToOtherPlayer();
                }
                board[x, y] = IsAHit(x, y) ? CellState.X : CellState.O;   
            }
            return moveBool;
        }
        
        public bool CanMakeMove(int x, int y)
        {   
            return x >= 0 && y >= 0 
                          && x <= board.GetLength(0) 
                          && y <= board.GetLength(1) 
                          && (board[x, y] == CellState.Empty || board[x, y] == CellState.S);
        }

        public List<(int, int)> GetEmptySquaresWithNoCornerNeighbours()
        {
            var notEmpty = new List<(int, int)>();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (board[i, j] == CellState.Empty && 
                        !GetShipSquares().Contains((i, j + 1)) &&
                        !GetShipSquares().Contains((i, j - 1)) &&
                        !GetShipSquares().Contains((i - 1, j)) &&
                        !GetShipSquares().Contains((i + 1, j)))
                    { 
                        notEmpty.Add((i, j));
                    }
                }
            }
            return notEmpty;
        }
        
        public List<(int, int)> GetEmptySquaresWithNoNeighbours()
        {
            var notEmpty = new List<(int, int)>();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (board[i, j] == CellState.Empty && 
                        !GetShipSquares().Contains((i, j + 1)) &&
                        !GetShipSquares().Contains((i, j - 1)) &&
                        !GetShipSquares().Contains((i - 1, j)) &&
                        !GetShipSquares().Contains((i + 1, j)) &&
                        !GetShipSquares().Contains((i + 1, j + 1)) &&
                        !GetShipSquares().Contains((i - 1, j - 1)) &&
                        !GetShipSquares().Contains((i - 1, j + 1)) &&
                        !GetShipSquares().Contains((i + 1, j - 1)))
                    { 
                        notEmpty.Add((i, j));
                    }
                }
            }
            return notEmpty;
        }
        
        public List<(int, int)> GetEmptySquares()
        {
            var empty = new List<(int, int)>();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (board[i, j] == CellState.Empty) empty.Add((i, j));
                }
            }
            return empty;
        }
        
        public List<(int, int)> GetShipSquares()
        {
            var ships = new List<(int, int)>();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (board[i, j] == CellState.S) ships.Add((i, j));
                }
            }
            return ships;
        }
        
        public List<(int, int)> GetPotentialSquares()
        {
            var potential = new List<(int, int)>();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (board[i, j] == CellState.Empty || board[i, j] == CellState.S) potential.Add((i, j));
                }
            }
            return potential;
        }

        private bool IsAHit(int x, int y)
        {
            return board[x, y] == CellState.S;
        }
        

        public int GetHeight()
        {
            return Height;
        }
        
        public int GetWidth()
        {
            return Width;
        }
        
        public static string SquareString(CellState cellState)
        {
            switch (cellState)
            {
                case CellState.Empty: return " ";
                case CellState.O: return "0";
                case CellState.S: return "S";
                case CellState.X: return "X";
            }

            return "-";
        }
        public static string EnemySquareString(CellState cellState)
        {
            switch (cellState)
            {
                case CellState.Empty: return " ";
                case CellState.O: return "0";
                case CellState.S: return " ";
                case CellState.X: return "X";
            }

            return "-";
        }
    }
}