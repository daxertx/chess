using chess.connection;
using chess.user_control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace chess.classes
{
    public class game
    {
        public enum Gametype
        {
            self_play,
            player_vs_player,
            ai_vs_player,
            replay
        }
        public castle_codition castle_conditions = new castle_codition();



        public Action<cube> promotion;
        public Action game_over;
        public Action tie;
        public Action<Move> new_move;
        public Action space;
        //only player vs player
        public Gametype gametype;
        public bool am_i_white;
        public client net_client;
        public server net_server;
        //
        public bool white_turn = true;
        private cube selectedCube = null;
        public List<Move> possible_moves = new List<Move>();
        public cube[,] board_values = new cube[8, 8];
        ChessMoves movesEngine;

        public Grid board;
        public Grid promotion_board;

        int x = 0;
        int y = 0;

        public game(Grid board,Grid promo = null, Gametype gametype = Gametype.self_play, bool am_white = true, client net_client = null, server net_server = null)
        {
            this.board = board;
            this.promotion_board = promo;
            this.gametype = gametype;
            //
            this.net_client = net_client;
            this.net_server = net_server;
            //
            if (this.gametype == Gametype.player_vs_player)
            {
                this.am_i_white = am_white;
            }
            board_ready(board,promo);
        }

        public void board_ready(Grid board,Grid promotion)
        {
            build_board.color_board(board);
            if(this.gametype ==Gametype.player_vs_player)
                this.board_values = build_board.spawn_pieces(board,this.am_i_white);
            else
                this.board_values = build_board.spawn_pieces(board);
            this.movesEngine = new ChessMoves(this.board_values,this.castle_conditions);
            board.Focusable = true;
            board.Focus();
            board.KeyDown += (s, e) => key_down(s, e);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var cell = this.board_values[i, j];

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);

                    cell.MouseUp += piece_click;
                    cell.BorderBrush = new SolidColorBrush(Colors.Black);
                    cell.MouseEnter += (s, e) => enter(s);

                    cell.MouseLeave += (s, e) => leave(s);


                    board.Children.Add(cell);
                }
            }
        }
        public void key_down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                this.space?.Invoke();
            }
        }
        public void enter(object sender)
        {
            cube cube = sender as cube;

            this.x = Grid.GetColumn(cube); // column
            this.y = Grid.GetRow(cube);    // row
            if ((cube.BorderBrush is SolidColorBrush brush && (brush.Color != Colors.Blue && brush.Color != Colors.Green && brush.Color != Colors.Orange) || cube.BorderThickness == new Thickness(0)))
            {

                cube.BorderBrush = new SolidColorBrush(Colors.Red);
                cube.BorderThickness = new Thickness(3);
            }
        }
        public void leave(object sender)
        {
            cube cube = sender as cube;
            if (cube.BorderBrush is SolidColorBrush brush && (brush.Color != Colors.Blue && brush.Color != Colors.Green && brush.Color != Colors.Orange))
            {
                cube.BorderBrush = null;
                cube.BorderThickness = new Thickness(0);
            }

        }
        public void update_king_or_rook_if_moved(cube cell,int x,int y)
        {
            if (cell.Type.ToString().Contains("King"))
            {
                //king moved
                if (cell.Type.ToString().StartsWith("White"))
                {
                    this.castle_conditions.white_king_moved = true;
                }
                else
                {
                    this.castle_conditions.black_king_moved = true;
                }
            }
            if (cell.Type.ToString().Contains("Rook"))
            {
                if (cell.Type.ToString().StartsWith("White"))
                {
                    if (x == 7 && y == 7)
                    {
                        this.castle_conditions.white_left_rook_moved = true;
                    }
                    else if (x == 7 && y == 0)
                    {
                        this.castle_conditions.white_right_rook_moved = true;
                    }
                }
                else
                {
                    if (x == 0 && y == 7)
                    {
                        this.castle_conditions.black_left_rook_moved = true;
                    }
                    else if (x == 0 && y == 0)
                    {
                        this.castle_conditions.black_right_rook_moved = true;
                    }
                }
            }
        }
        public void piece_click(object sender, MouseButtonEventArgs e)
        {
            cube cell = sender as cube;

            if (cell == null)
                return;

            if (this.gametype == Gametype.self_play)
            {
                if ((cell.Type != piece.PieceType.Empty && (cell.BorderBrush is SolidColorBrush brush && (brush.Color != Colors.Blue))) || cell.can_move_here == null)
                {
                    //mark pieces
                    if (this.selectedCube != null)
                    {
                        //reset selected
                        this.selectedCube.BorderBrush = null;
                        this.selectedCube.BorderThickness = new Thickness(0);
                    }


                    piece.PieceType[,] types = new piece.PieceType[8, 8];
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (this.x == j && this.y == i)
                            {
                                this.board_values[i, j].can_move_here = null;

                                this.board_values[i, j].BorderBrush = new SolidColorBrush(Colors.Red);
                                this.board_values[i, j].BorderThickness = new Thickness(3);
                                types[i, j] = board_values[i, j].Type;
                                continue;
                            }

                            types[i, j] = board_values[i, j].Type;
                            this.board_values[i, j].can_move_here = null;
                            if (this.board_values[i, j].BorderBrush is SolidColorBrush brush2 && brush2.Color == Colors.Orange)
                            {
                                continue;
                            }
                            this.board_values[i, j].BorderBrush = null;
                            this.board_values[i, j].BorderThickness = new Thickness(0);
                        }
                    }
                    List<Move> moves = movesEngine.PossibleMoves(cell.Type, y, x, types);
                    foreach (var move in moves)
                    {
                        this.board_values[move.ToX, move.ToY].BorderBrush = new SolidColorBrush(Colors.Blue);
                        this.board_values[move.ToX, move.ToY].BorderThickness = new Thickness(3);
                        this.board_values[move.ToX, move.ToY].can_move_here = move;
                        this.possible_moves.Add(move);
                    }
                    this.selectedCube = cell;
                    cell.BorderBrush = new SolidColorBrush(Colors.Green);
                    cell.BorderThickness = new Thickness(3);
                }
                else
                {
                    //move pieces
                    if (this.selectedCube == null)
                    {
                        return;
                    }
                    if ((this.selectedCube.Type.ToString().StartsWith("White") && !this.white_turn) || (this.selectedCube.Type.ToString().StartsWith("Black") && this.white_turn))
                    {
                        return;
                    }
                    piece.PieceType[,] types = new piece.PieceType[8, 8];
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            types[i, j] = this.board_values[i, j].Type;
                        }
                    }
                    int piecey = Grid.GetColumn(this.selectedCube);
                    int piecex = Grid.GetRow(this.selectedCube);

                    int movetoy = Grid.GetColumn(cell); // column
                    int movetox = Grid.GetRow(cell);    // row
                    List<Move> moves = this.movesEngine.PossibleMoves(this.selectedCube.Type, piecex, piecey, types);

                    bool piece_clicked_color_is_white = this.selectedCube.Type.ToString().StartsWith("White");
                    piece.PieceType piece_type_move_from = this.selectedCube.Type;
                    piece.PieceType piece_type_moved = cell.Type;

                    foreach (Move move in moves)
                    {
                        if (move.ToX == movetox && move.ToY == movetoy)//compare move to where clicked
                        {
                            //move piece
                            if (piecex == move.FromX && piecey == move.FromY && movetox == move.ToX && movetoy == move.ToY)
                            {
                                //move piece
                                cell.change_piece(piece_type_move_from);
                                this.selectedCube.change_piece(piece.PieceType.Empty);
                                foreach (Move movee in moves)
                                {
                                    int x = movee.ToX;
                                    int y = movee.ToY;
                                    this.board_values[x, y].BorderBrush = null;
                                    this.board_values[x, y].BorderThickness = new Thickness(0);
                                    this.board_values[x, y].can_move_here = null;
                                }
                                break;
                            }
                        }
                    }


                    
                    bool is_checked = is_check(piece_clicked_color_is_white);
                    if (is_checked)
                    {
                        //reverse board changes
                        this.selectedCube.change_piece(piece_type_move_from);
                        cell.change_piece(piece_type_moved);
                        return;
                    }
                    if(Math.Abs(movetoy - piecey) == 2 && cell.Type.ToString().Contains("Pawn"))
                    {
                        cell.move_twice();
                    }
                    if (Math.Abs(movetoy - piecey) > 1 && cell.Type.ToString().Contains("King"))
                    {
                        //הצרחה
                        //if negative then queenside
                        if (movetoy - piecey < 0)
                        {
                            //queenside
                            this.board_values[movetox, movetoy + 1].change_piece(this.board_values[movetox, 0].Type);
                            this.board_values[movetox, 0].change_piece(piece.PieceType.Empty);
                        }
                        else
                        {
                            //kingside
                            this.board_values[movetox, movetoy - 1].change_piece(this.board_values[movetox, 7].Type);
                            this.board_values[movetox, 7].change_piece(piece.PieceType.Empty);
                        }
                        
                    }
                    if(cell.Type.ToString().Contains("King"))
                    {
                        if (this.am_i_white)
                        {
                            this.castle_conditions.white_king_moved = true;
                        }
                        else
                        {
                            this.castle_conditions.black_king_moved = true;
                        }
                    }
                    if (cell.Type.ToString().Contains("Rook"))
                    {
                        if (this.am_i_white)
                        {
                            if(Grid.GetRow(cell) == 7 && Grid.GetColumn(cell) == 7)
                            {
                                this.castle_conditions.white_left_rook_moved = true;
                            }
                            else if (Grid.GetRow(cell) == 7 && Grid.GetColumn(cell) == 0)
                            {
                                this.castle_conditions.white_right_rook_moved = true;
                            }
                        }
                        else
                        {
                            if (Grid.GetRow(cell) == 0 && Grid.GetColumn(cell) == 7)
                            {
                                this.castle_conditions.black_left_rook_moved = true;
                            }
                            else if (Grid.GetRow(cell) == 0 && Grid.GetColumn(cell) == 0)
                            {
                                this.castle_conditions.black_right_rook_moved = true;
                            }
                        }
                    }
                    if (is_check(!piece_clicked_color_is_white))
                    {
                        //is checking
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                types[i, j] = this.board_values[i, j].Type;
                                if (this.board_values[i, j].Type.ToString().EndsWith("King") && this.board_values[i, j].Type.ToString().Contains("White") == !piece_clicked_color_is_white)
                                {
                                    //enemy king in check
                                    this.board_values[i, j].BorderBrush = new SolidColorBrush(Colors.Orange);
                                    this.board_values[i, j].BorderThickness = new Thickness(3);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                types[i, j] = this.board_values[i, j].Type;
                                if (this.board_values[i, j].Type.ToString().EndsWith("King") && this.board_values[i, j].Type.ToString().Contains("White") == !piece_clicked_color_is_white)
                                {
                                    //enemy king not in check
                                    this.board_values[i, j].BorderBrush = null;
                                    this.board_values[i, j].BorderThickness = new Thickness(0);
                                    break;
                                }
                            }
                        }
                    }

                    this.new_move.Invoke(new Move(piecex, piecey, movetox, movetoy));//saves move

                    this.white_turn = !this.white_turn;
                    if (is_checkmate(this.white_turn))
                    {
                        MessageBox.Show("Checkmate! " + (piece_clicked_color_is_white ? "Black" : "White") + " wins!");
                        this.game_over?.Invoke();
                    }
                    else if (is_tie(this.white_turn))
                    {
                        MessageBox.Show("Tie!");
                        this.tie?.Invoke();
                    }
                    try_promote(cell);
                }
            }
            else if (this.gametype == Gametype.player_vs_player)
            {
                if ((cell.Type != piece.PieceType.Empty && (cell.BorderBrush is SolidColorBrush brush && (brush.Color != Colors.Blue))) || cell.can_move_here == null)
                {
                    //mark pieces
                    if (this.selectedCube != null)
                    {
                        //reset selected
                        this.selectedCube.BorderBrush = null;
                        this.selectedCube.BorderThickness = new Thickness(0);
                    }

                    piece.PieceType[,] types = new piece.PieceType[8, 8];
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (this.x == j && this.y == i)
                            {
                                this.board_values[i, j].can_move_here = null;

                                this.board_values[i, j].BorderBrush = new SolidColorBrush(Colors.Red);
                                this.board_values[i, j].BorderThickness = new Thickness(3);
                                types[i, j] = board_values[i, j].Type;
                                continue;
                            }
                            this.board_values[i, j].can_move_here = null;
                            this.board_values[i, j].BorderBrush = null;
                            this.board_values[i, j].BorderThickness = new Thickness(0);
                            types[i, j] = board_values[i, j].Type;
                        }
                    }
                    List<Move> moves = movesEngine.PossibleMoves(cell.Type, y, x, types);
                    foreach (var move in moves)
                    {
                        this.board_values[move.ToX, move.ToY].BorderBrush = new SolidColorBrush(Colors.Blue);
                        this.board_values[move.ToX, move.ToY].BorderThickness = new Thickness(3);
                        this.board_values[move.ToX, move.ToY].can_move_here = move;
                        this.possible_moves.Add(move);
                    }
                    this.selectedCube = cell;
                    cell.BorderBrush = new SolidColorBrush(Colors.Green);
                    cell.BorderThickness = new Thickness(3);
                }
                else
                {
                    //move pieces

                    bool piece_clicked_color_is_white = this.selectedCube.Type.ToString().StartsWith("White");

                    if (this.am_i_white == !this.white_turn)
                    {
                        //not your turn
                        return;
                    }
                    if (this.am_i_white != piece_clicked_color_is_white)
                    {
                        //clicked piece isnt your color
                        return;
                    }
                    if (this.selectedCube == null)
                    {
                        return;
                    }

                    piece.PieceType[,] types = new piece.PieceType[8, 8];
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            types[i, j] = this.board_values[i, j].Type;
                        }
                    }
                    int piecey = Grid.GetColumn(this.selectedCube);
                    int piecex = Grid.GetRow(this.selectedCube);

                    int movetoy = Grid.GetColumn(cell); // column
                    int movetox = Grid.GetRow(cell);    // row
                    List<Move> moves = this.movesEngine.PossibleMoves(this.selectedCube.Type, piecex, piecey, types);

                    Move move_picked = new Move();
                    piece.PieceType piece_type_move_from = this.selectedCube.Type;
                    piece.PieceType piece_type_moved = cell.Type;
                    foreach (Move move in moves)
                    {
                        if (move.ToX == movetox && move.ToY == movetoy)//compare move to where clicked
                        {
                            //move piece
                            if (piecex == move.FromX && piecey == move.FromY && movetox == move.ToX && movetoy == move.ToY)
                            {
                                move_picked = move;
                                //move piece
                                cell.change_piece(this.selectedCube.Type);//moved to new cell
                                this.selectedCube.change_piece(piece.PieceType.Empty);//moved from is now empty
                                
                                
                                foreach (Move movee in moves)
                                {
                                    int x = movee.ToX;
                                    int y = movee.ToY;
                                    this.board_values[x, y].BorderBrush = null;
                                    this.board_values[x, y].BorderThickness = new Thickness(0);
                                    this.board_values[x, y].can_move_here = null;
                                }
                                break;
                            }
                        }
                    }
                    //finished moving piece
                    bool is_checked = is_check(piece_clicked_color_is_white);

                    //was in check before move
                    if (is_checked)
                    {
                        //still in check after move
                        //reverse board changes
                        this.selectedCube.change_piece(piece_type_move_from);
                        cell.change_piece(piece_type_moved);
                        //cell is move piece moves to
                        //selected cube is piece that moved
                        return;
                    }
                    int X = Grid.GetColumn(cell);
                    int Y = Grid.GetRow(cell);

                    int xfrom = Grid.GetColumn(this.selectedCube);
                    int yfrom = Grid.GetRow(this.selectedCube);
                    //move turn
                    this.white_turn = !this.white_turn;
                    
                    try_promote(cell);
                    //
                    Move move2 = new Move(-1,-1,-1,-1);
                    if (Math.Abs(movetoy - piecey) == 2 && cell.Type.ToString().Contains("Pawn"))
                    {
                        cell.move_twice();
                    }

                    if (Math.Abs(movetoy - piecey) > 1 && cell.Type.ToString().Contains("King"))
                    {
                        //הצרחה
                        if (movetoy - piecey < 0)
                        {
                            // queenside
                            this.board_values[movetox, movetoy + 1].change_piece(this.board_values[movetox, 0].Type);
                            this.board_values[movetox, 0].change_piece(piece.PieceType.Empty);

                        }
                        else
                        {
                            // kingside
                            this.board_values[movetox, movetoy - 1].change_piece(this.board_values[movetox, 7].Type);
                            this.board_values[movetox, 7].change_piece(piece.PieceType.Empty);

                        }
                        update_king_or_rook_if_moved(cell,x,y);
                    }
                    if (is_check(!piece_clicked_color_is_white))
                    {
                        //is checking
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                types[i, j] = this.board_values[i, j].Type;
                                if (this.board_values[i, j].Type.ToString().EndsWith("King") && this.board_values[i, j].Type.ToString().Contains("White") == !piece_clicked_color_is_white)
                                {
                                    //enemy king in check
                                    this.board_values[i, j].BorderBrush = new SolidColorBrush(Colors.Orange);
                                    this.board_values[i, j].BorderThickness = new Thickness(3);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                types[i, j] = this.board_values[i, j].Type;
                                if (this.board_values[i, j].Type.ToString().EndsWith("King") && this.board_values[i, j].Type.ToString().Contains("White") == !piece_clicked_color_is_white)
                                {
                                    //enemy king not in check
                                    this.board_values[i, j].BorderBrush = null;
                                    this.board_values[i, j].BorderThickness = new Thickness(0);
                                    break;
                                }
                            }
                        }
                    }
                    //
                    if (this.am_i_white)
                    {
                        //send move to client
                        this.net_server.send_move(move_picked);
                        if (move2.FromX != -1)
                        {
                            this.net_server.send_move(move2);
                        }
                    }
                    else
                    {
                        //send move to server
                        this.net_client.send_move(move_picked);
                        if(move2.FromX != -1)
                        {
                            this.net_client.send_move(move2);
                        }
                    }
                    
                    if (is_checkmate(this.white_turn))
                    {
                        MessageBox.Show("Checkmate! " + (piece_clicked_color_is_white ? "White" : "Black") + " wins!");
                        this.game_over?.Invoke();
                    }
                    else if (is_tie(this.white_turn))
                    {
                        MessageBox.Show("Tie!");
                        this.tie?.Invoke();
                    }
                }
            }
        }
        public void try_promote(cube cell)
        {
            //check if pawn is in promotion row
            bool res = cell.Type.ToString().Contains("Pawn") && (Grid.GetRow(cell) == 0 || Grid.GetRow(cell) == 7);
            if(res)
                promote_pawn(cell);
        }
        public void promote_pawn(cube cell)
        {
            this.promotion_board.Children.Clear();
            piece.PieceType b;
            piece.PieceType q;
            piece.PieceType k;
            piece.PieceType r;
            if (this.am_i_white)
            {
                b = piece.PieceType.WhiteBishop;
                q = piece.PieceType.WhiteQueen;
                k = piece.PieceType.WhiteKnight;
                r = piece.PieceType.WhiteRook;
            }
            else
            {
                b = piece.PieceType.BlackBishop;
                q = piece.PieceType.BlackQueen;
                k = piece.PieceType.BlackKnight;
                r = piece.PieceType.BlackRook;
            }

            cube cubeb = new cube(false, b);
            cube cubeq = new cube(false, q);
            cube cubek = new cube(false, k);
            cube cuber = new cube(false, r);
            cubeb.MouseUp += (s, e) => promote_pawn_to(cell, b);
            cubeq.MouseUp += (s, e) => promote_pawn_to(cell, q);
            cubek.MouseUp += (s, e) => promote_pawn_to(cell, k);
            cuber.MouseUp += (s, e) => promote_pawn_to(cell, r);
            Grid.SetColumn(cubeb, 0);
            Grid.SetColumn(cubeq, 1);
            Grid.SetColumn(cubek, 2);
            Grid.SetColumn(cuber, 3);
            this.promotion_board.Children.Add(cubeb);
            this.promotion_board.Children.Add(cubeq);
            this.promotion_board.Children.Add(cubek);
            this.promotion_board.Children.Add(cuber);
        }
        public void promote_pawn_to(cube cell, piece.PieceType new_type)
        {
            cell.change_piece(new_type);
            this.promotion_board.Children.Clear();
            if(this.gametype == Gametype.player_vs_player)
            {
                //send msg
                if(this.net_server != null)
                    this.net_server.send_promotion(new_type, Grid.GetColumn(cell), Grid.GetRow(cell));
                else if(this.net_client != null)
                    this.net_client.send_promotion(new_type, Grid.GetColumn(cell), Grid.GetRow(cell));
            }
        }
        public bool is_check(bool is_white)
        {

            int white_king_x = 0;
            int white_king_y = 0;
            int black_king_x = 0;
            int black_king_y = 0;
            piece.PieceType[,] types = new piece.PieceType[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    types[i, j] = this.board_values[i, j].Type;
                    if (this.board_values[i, j].Type == piece.PieceType.WhiteKing)
                    {
                        white_king_x = i;
                        white_king_y = j;
                    }
                    if (this.board_values[i, j].Type == piece.PieceType.BlackKing)
                    {
                        black_king_x = i;
                        black_king_y = j;
                    }
                }
            }
            foreach (cube cell in this.board_values)
            {
                if (cell.Type == piece.PieceType.Empty)
                {
                    continue;
                }
                List<Move> moves = movesEngine.PossibleMoves(cell.Type, Grid.GetRow(cell), Grid.GetColumn(cell), types);
                foreach (Move move in moves)
                {
                    if (is_white && move.ToX == white_king_x && move.ToY == white_king_y)
                    {
                        return true;
                    }
                    if (!is_white && move.ToX == black_king_x && move.ToY == black_king_y)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool is_checkmate(bool is_white)
        {
            if (!is_check(is_white))
            {
                return false;
            }
            piece.PieceType[,] types = new piece.PieceType[8, 8];
            int counter = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if(this.board_values[i, j].Type != piece.PieceType.Empty)
                    {
                        counter++;
                    }
                    types[i, j] = this.board_values[i, j].Type;
                }
            }
            if(counter <= 2)
            {
                //only kings left
                return true;
            }
            foreach (cube cell in this.board_values)
            {
                if (cell.Type == piece.PieceType.Empty)
                    continue;//empty
                if (!piece_my_color(is_white, cell.Type))
                    continue;//same color
                //testing moving pieces
                List<Move> moves = this.movesEngine.PossibleMoves(cell.Type, Grid.GetRow(cell), Grid.GetColumn(cell), types);

                foreach (Move move in moves)
                {
                    piece.PieceType moving_piece = cell.Type;
                    piece.PieceType captured = this.board_values[move.ToX, move.ToY].Type;

                    //tries move
                    this.board_values[move.FromX, move.FromY].change_piece(piece.PieceType.Empty);
                    this.board_values[move.ToX, move.ToY].change_piece(moving_piece);

                    bool still_in_check = is_check(is_white);
                    //reverse move
                    this.board_values[move.FromX, move.FromY].change_piece(moving_piece);
                    this.board_values[move.ToX, move.ToY].change_piece(captured);

                    if (!still_in_check)
                    {
                        //there is a move that can save the king
                        return false;
                    }
                }
            }
            //checkmate nothing can save king
            return true;
        }
        public bool is_tie(bool is_white)
        {
            foreach(cube cell in this.board_values)
            {
                if (cell.Type == piece.PieceType.Empty)
                    continue;//empty
                if (!piece_my_color(is_white, cell.Type))
                    continue;//not my color

                piece.PieceType[,] types = new piece.PieceType[8, 8];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        types[i, j] = this.board_values[i, j].Type;
                    }
                }
                List<Move> moves = this.movesEngine.PossibleMoves(cell.Type, Grid.GetRow(cell), Grid.GetColumn(cell), types);
                if (moves.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }
        public bool piece_my_color(bool is_white, piece.PieceType type)
        {
            if (is_white)
            {
                return type.ToString().StartsWith("White");
            }
            else
            {
                return type.ToString().StartsWith("Black");
            }
        }
        public void play_move(Move move)
        {
            piece.PieceType type = this.board_values[move.FromX, move.FromY].Type;
            this.board_values[move.ToX, move.ToY].change_piece(type);
            this.board_values[move.FromX, move.FromY].change_piece(piece.PieceType.Empty);
        }
    }
}
