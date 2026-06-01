using chess.classes;
using chess.user_control;
using System;
using System.Collections.Generic;
using static chess.classes.piece;

public struct Move
{
    public int FromX, FromY;
    public int ToX, ToY;

    public Move(int fx, int fy, int tx, int ty)
    {
        FromX = fx;
        FromY = fy;
        ToX = tx;
        ToY = ty;
    }
    public static Move Parse(string msg)
    {
        // Expected format: "(1,2) -> (2,3)"
        if (msg.Length != 14)
            throw new FormatException("Invalid move format");
        int fromX = int.Parse(msg[1].ToString());
        int fromY = int.Parse(msg[3].ToString());
        int toX = int.Parse(msg[10].ToString());
        int toY = int.Parse(msg[12].ToString());
        return new Move(fromX, fromY, toX, toY);
    }
    public override string ToString()
    {
        return $"({this.FromX},{this.FromY}) -> ({this.ToX},{this.ToY})";
    }
}


public class ChessMoves
{
    public cube[,] board;
    public castle_codition castle_conditions;

    // Parameterless constructor — used in game.cs
    // En passant and castling flags won't be checked when board is null
    public ChessMoves()
    {
        this.board = null;
    }

    public ChessMoves(cube[,] board,castle_codition cd)
    {
        this.board = board;
        this.castle_conditions = cd;
    }

    public List<Move> PossibleMoves(PieceType p, int x, int y, PieceType[,] board)
    {
        List<Move> moves = new List<Move>();

        switch (p)
        {
            case PieceType.WhitePawn:
                PawnMoves(x, y, board, moves, -1, true);
                break;

            case PieceType.BlackPawn:
                PawnMoves(x, y, board, moves, 1, false);
                break;

            case PieceType.WhiteRook:
            case PieceType.BlackRook:
                SlidingMoves(x, y, board, moves, new (int, int)[]
                {
                    (1,0), (-1,0), (0,1), (0,-1)
                });
                break;

            case PieceType.WhiteBishop:
            case PieceType.BlackBishop:
                SlidingMoves(x, y, board, moves, new (int, int)[]
                {
                    (1,1), (1,-1), (-1,1), (-1,-1)
                });
                break;

            case PieceType.WhiteQueen:
            case PieceType.BlackQueen:
                SlidingMoves(x, y, board, moves, new (int, int)[]
                {
                    (1,0), (-1,0), (0,1), (0,-1),
                    (1,1), (1,-1), (-1,1), (-1,-1)
                });
                break;

            case PieceType.WhiteKnight:
            case PieceType.BlackKnight:
                KnightMoves(x, y, board, moves);
                break;

            case PieceType.WhiteKing:
            case PieceType.BlackKing:
                KingMoves(x, y, board, moves);
                break;
        }

        return moves;
    }

    public bool InBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    bool IsEmpty(PieceType[,] board, int x, int y)
    {
        return InBoard(x, y) && board[x, y] == PieceType.Empty;
    }

    bool IsWhite(PieceType p)
    {
        return p.ToString().StartsWith("White");
    }

    bool IsEnemy(PieceType[,] board, int x, int y, bool isWhite)
    {
        if (!InBoard(x, y))
            return false;
        if (board[x, y] == PieceType.Empty)
            return false;
        return IsWhite(board[x, y]) != isWhite;
    }

    void PawnMoves(int x, int y, PieceType[,] board, List<Move> moves, int dir, bool isWhite)
    {
        // Forward one square
        if (IsEmpty(board, x + dir, y))
            moves.Add(new Move(x, y, x + dir, y));

        // Forward two squares from starting row
        if ((isWhite && x == 6 || !isWhite && x == 1)
            && IsEmpty(board, x + dir, y)
            && IsEmpty(board, x + 2 * dir, y))
        {
            moves.Add(new Move(x, y, x + 2 * dir, y));
        }

        // En passant — only when cube board is available
        if (this.board != null)
        {
            if (InBoard(x, y - 1) && IsEnemy(board, x, y - 1, isWhite) && this.board[x, y - 1].is_pawn_moved_twice)
                moves.Add(new Move(x, y, x + dir, y - 1));

            if (InBoard(x, y + 1) && IsEnemy(board, x, y + 1, isWhite) && this.board[x, y + 1].is_pawn_moved_twice)
                moves.Add(new Move(x, y, x + dir, y + 1));
        }

        // Normal diagonal captures
        if (IsEnemy(board, x + dir, y - 1, isWhite))
            moves.Add(new Move(x, y, x + dir, y - 1));

        if (IsEnemy(board, x + dir, y + 1, isWhite))
            moves.Add(new Move(x, y, x + dir, y + 1));
    }

    void KnightMoves(int x, int y, PieceType[,] board, List<Move> moves)
    {
        int[,] d =
        {
            {2,1},{2,-1},{-2,1},{-2,-1},
            {1,2},{1,-2},{-1,2},{-1,-2}
        };

        bool isWhite = IsWhite(board[x, y]);

        for (int i = 0; i < 8; i++)
        {
            int nx = x + d[i, 0];
            int ny = y + d[i, 1];

            if (!InBoard(nx, ny))
                continue;

            if (IsEmpty(board, nx, ny) || IsEnemy(board, nx, ny, isWhite))
                moves.Add(new Move(x, y, nx, ny));
        }
    }

    void KingMoves(int x, int y, PieceType[,] board, List<Move> moves)
    {
        bool isWhite = IsWhite(board[x, y]);

        // Normal king moves (one square in any direction)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (!InBoard(nx, ny))
                    continue;
                if (IsEmpty(board, nx, ny) || IsEnemy(board, nx, ny, isWhite))
                    moves.Add(new Move(x, y, nx, ny));
            }
        }

        if (this.board != null)
        {
            if (this.board[x,y].Type.ToString().Contains("White"))
            {
                if(!this.castle_conditions.white_king_moved)
                {
                    //king hasnt moved
                    if (InBoard(x, y + 3) 
                        && board[x, y + 3].ToString().EndsWith("Rook")//look rook
                        && !this.castle_conditions.black_right_rook_moved
                        && IsEmpty(board, x, y + 1)
                        && IsEmpty(board, x, y + 2))//nothing is blocking
                    {
                        moves.Add(new Move(x, y, x, y + 2));
                    }
                    //queen castle
                    if (InBoard(x, y - 4)
                        && board[x, y - 4].ToString().EndsWith("Rook")
                        && !this.castle_conditions.white_left_rook_moved
                        && IsEmpty(board, x, y - 1)
                        && IsEmpty(board, x, y - 2)
                        && IsEmpty(board, x, y - 3))
                    {
                        moves.Add(new Move(x, y, x, y - 2));
                    }
                }
            }
            else
            {
                if (!this.castle_conditions.black_king_moved)
                {
                    //king hasnt moved
                    if (InBoard(x, y + 3)
                        && board[x, y + 3].ToString().EndsWith("Rook")//look rook
                        && !this.castle_conditions.black_right_rook_moved//rook hasnt moved
                        && IsEmpty(board, x, y + 1)
                        && IsEmpty(board, x, y + 2))//nothing is blocking
                    {
                        moves.Add(new Move(x, y, x, y + 2));
                    }
                    //queen castle
                    if (InBoard(x, y - 4)
                        && board[x, y - 4].ToString().EndsWith("Rook")
                        && !this.castle_conditions.black_left_rook_moved
                        && IsEmpty(board, x, y - 1)
                        && IsEmpty(board, x, y - 2)
                        && IsEmpty(board, x, y - 3))
                    {
                        moves.Add(new Move(x, y, x, y - 2));
                    }
                }
            }
            
        }
    }

    void SlidingMoves(int x, int y, PieceType[,] board, List<Move> moves, (int dx, int dy)[] dirs)
    {
        bool isWhite = IsWhite(board[x, y]);

        foreach (var (dx, dy) in dirs)
        {
            int nx = x + dx;
            int ny = y + dy;
            while (InBoard(nx, ny))
            {
                if (IsEmpty(board, nx, ny))
                {
                    moves.Add(new Move(x, y, nx, ny));
                }
                else
                {
                    if (IsEnemy(board, nx, ny, isWhite))
                        moves.Add(new Move(x, y, nx, ny));
                    break;
                }

                nx += dx;
                ny += dy;
            }
        }
    }
}