using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess.classes
{
    public class piece
    {
        public enum PieceType
        {
            Empty,
            WhitePawn, WhiteRook, WhiteKnight, WhiteBishop, WhiteQueen, WhiteKing,
            BlackPawn, BlackRook, BlackKnight, BlackBishop, BlackQueen, BlackKing
        }
    }
}
