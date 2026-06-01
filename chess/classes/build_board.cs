using chess.user_control;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static chess.classes.piece;

namespace chess.classes
{
    public class build_board
    {
        public static cube[,] spawn_pieces(Grid board, bool player_is_white = true)
        {
            PieceType[,] pieces = new PieceType[8, 8]
            {
                {
                    PieceType.BlackRook,
                    PieceType.BlackKnight,
                    PieceType.BlackBishop,
                    PieceType.BlackQueen,
                    PieceType.BlackKing,
                    PieceType.BlackBishop,
                    PieceType.BlackKnight,
                    PieceType.BlackRook
                },
                {
                    PieceType.BlackPawn,
                    PieceType.BlackPawn,
                    PieceType.BlackPawn,
                    PieceType.BlackPawn,
                    PieceType.BlackPawn,
                    PieceType.BlackPawn,
                    PieceType.BlackPawn,
                    PieceType.BlackPawn
                },
                //4 middle rows are empty
                { PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty },
                { PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty },
                { PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty },
                { PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty, PieceType.Empty },
                {
                    PieceType.WhitePawn,
                    PieceType.WhitePawn,
                    PieceType.WhitePawn,
                    PieceType.WhitePawn,
                    PieceType.WhitePawn,
                    PieceType.WhitePawn,
                    PieceType.WhitePawn,
                    PieceType.WhitePawn
                },
                {
                    PieceType.WhiteRook,
                    PieceType.WhiteKnight,
                    PieceType.WhiteBishop,
                    PieceType.WhiteQueen,
                    PieceType.WhiteKing,
                    PieceType.WhiteBishop,
                    PieceType.WhiteKnight,
                    PieceType.WhiteRook
                }
            };
            cube[,] cubes = new cube[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    cube cube = new cube((x+y)%2==0, pieces[y, x], player_is_white);
                    cubes[y, x] = cube;
                }
            }
            return cubes;
        }
        public static void color_board(Grid board)
        {
            for (int x = 0; x < board.ColumnDefinitions.Count; x++)
            {
                for (int y = 0; y < board.RowDefinitions.Count; y++)
                {
                    if ((x + y) % 2 == 0)
                    { 
                        //white
                        Button white = new Button();
                        white.Background = new SolidColorBrush(Colors.Beige);
                        board.Children.Add(white); Grid.SetColumn(white, x);
                        Grid.SetRow(white, y);
                    }
                    else
                    {
                        //black
                        Button black = new Button();
                        black.Background = new SolidColorBrush(Colors.BurlyWood);
                        board.Children.Add(black); Grid.SetColumn(black, x);
                        Grid.SetRow(black, y);
                    }
                }
            }
        }
        public static string type_to_img(PieceType piece)
        {
            switch (piece)
            {
                case PieceType.WhitePawn:
                    return "chess-pawn-white.png";

                case PieceType.WhiteRook:
                    return "chess-rook-white.png";

                case PieceType.WhiteKnight:
                    return "chess-knight-white.png";

                case PieceType.WhiteBishop:
                    return "chess-bishop-white.png";

                case PieceType.WhiteQueen:
                    return "chess-queen-white.png";

                case PieceType.WhiteKing:
                    return "chess-king-white.png";

                case PieceType.BlackPawn:
                    return "chess-pawn-black.png";

                case PieceType.BlackRook:
                    return "chess-rook-black.png";

                case PieceType.BlackKnight:
                    return "chess-knight-black.png";

                case PieceType.BlackBishop:
                    return "chess-bishop-black.png";

                case PieceType.BlackQueen:
                    return "chess-queen-black.png";

                case PieceType.BlackKing:
                    return "chess-king-black.png";

                default:
                    return null;
            }
        }
    }
}