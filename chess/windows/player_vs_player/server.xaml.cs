using chess.classes;
using chess.user_control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace chess.windows.player_vs_player
{
    public partial class server : Window
    {
        public connection.server net;
        public game game;

        public server()
        {
            InitializeComponent();
            start_server();
        }

        public async void start_server()
        {
            try
            {
                this.net = new connection.server(1234);
                this.net.on_connect = connected;
                this.net.on_message += received;
                this.net.on_disconnect += disconnected;

                await this.net.start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Server Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Close();
            }
        }

        public async Task send_move_played(Move move)
        {
            await this.net.send(move.ToString());
        }

        public void connected()
        {
            this.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Client connected!");

                this.game = new game(board, bar, game.Gametype.player_vs_player, true, null, this.net);

                this.game.tie += () =>
                {
                    OnClosed(EventArgs.Empty);
                };
                this.game.game_over += () =>
                {
                    OnClosed(EventArgs.Empty);
                };
            });
        }

        public void received(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (msg.StartsWith("PROMOTION:"))
                {
                    msg = msg.Substring(10);
                    int x = int.Parse(msg[msg.Length - 2].ToString());
                    int y = int.Parse(msg[msg.Length - 1].ToString());
                    string pieceName = msg.Substring(0, msg.Length - 2);

                    foreach (cube c in this.game.board_values)
                    {
                        if (Grid.GetColumn(c) == x && Grid.GetRow(c) == y)
                        {
                            c.change_piece((piece.PieceType)Enum.Parse(typeof(piece.PieceType), pieceName));
                            break;
                        }
                    }
                }
                else if (msg.StartsWith("REMOVE:"))
                {
                    msg = msg.Substring(7);
                    int x = int.Parse(msg[msg.Length - 2].ToString());
                    int y = int.Parse(msg[msg.Length - 1].ToString());
                    this.game.board_values[x, y].change_piece(piece.PieceType.Empty);
                }
                else if (this.game.white_turn != this.game.am_i_white)
                {
                    Move move = Move.Parse(msg);
                    piece.PieceType movingPieceType = this.game.board_values[move.FromX, move.FromY].Type;

                    this.game.play_move(move);

                    if (movingPieceType.ToString().Contains("Pawn") && Math.Abs(move.FromY - move.ToY) == 2)
                    {
                        this.game.board_values[move.ToX, move.ToY].move_twice();
                    }

                    this.game.white_turn = this.game.am_i_white;

                    if (this.game.is_check(this.game.am_i_white))
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                if (this.game.board_values[i, j].Type.ToString().EndsWith("King") &&
                                    this.game.board_values[i, j].Type.ToString().Contains("White") == this.game.am_i_white)
                                {
                                    this.game.board_values[i, j].BorderBrush = new SolidColorBrush(Colors.Orange);
                                    this.game.board_values[i, j].BorderThickness = new Thickness(3);
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
                                if (this.game.board_values[i, j].Type.ToString().EndsWith("King") &&
                                    this.game.board_values[i, j].Type.ToString().Contains("White") == this.game.am_i_white)
                                {
                                    this.game.board_values[i, j].BorderBrush = null;
                                    this.game.board_values[i, j].BorderThickness = new Thickness(0);
                                    break;
                                }
                            }
                        }
                    }

                    // Check game-ending positions state
                    if (this.game.is_checkmate(this.game.white_turn))
                    {
                        MessageBox.Show("Checkmate! " + (this.game.am_i_white ? "Black" : "White") + " wins!");
                        this.game.game_over?.Invoke();
                    }
                    else if (this.game.is_tie(this.game.white_turn))
                    {
                        MessageBox.Show("Tie!");
                        this.game.tie?.Invoke();
                    }
                }
            });
        }

        public void disconnected()
        {
            this.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Client disconnected");
                OnClosed(EventArgs.Empty);
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            this.net?.stop();
            base.OnClosed(e);
        }
    }
}