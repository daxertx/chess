using chess.classes;
using chess.database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace chess.windows.replay_mode
{
    public partial class replay_game : Window
    {
        public int game_id;
        public int move_index = 0;
        public game game;
        public int max_moves = 0;
        public talk_to_database db;

        private DataRowCollection activeGameMoves;

        public replay_game()
        {
            InitializeComponent();
            this.db = new talk_to_database();

            var dataTable = this.db.RetrieveTable("SELECT game_id, white_player_name, black_player_name, [time] FROM games", "games").Tables[0];

            foreach (DataRow row in dataTable.Rows)
            {
                string white_player = row.Field<string>("white_player_name");
                string black_player = row.Field<string>("black_player_name");
                string time = row.Field<DateTime>("time").ToString("yyyy-MM-dd HH:mm:ss");

                string display = $"{white_player} vs {black_player} at {time}";
                ComboBoxItem item = new ComboBoxItem();
                item.Content = display;

                item.Tag = row.Field<int>("game_id");

                gameSelector.Items.Add(item);
            }
        }

        private void load_replay(object sender, RoutedEventArgs e)
        {
            //start replay logic
            if (gameSelector.SelectedItem == null)
            {
                MessageBox.Show("Please select a game first.");
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)gameSelector.SelectedItem;
            this.game_id = (int)selectedItem.Tag;

            var movesTable = this.db.RetrieveTable($"SELECT * FROM moves WHERE game_id={this.game_id} ORDER BY move_num ASC", "moves").Tables[0];
            this.activeGameMoves = movesTable.Rows;
            this.max_moves = this.activeGameMoves.Count;

            this.move_index = 0;

            ui.Children.RemoveAt(0);
            ui.Children.RemoveAt(0);

            this.game = new game(board,null, game.Gametype.replay);
            this.game.space += next_move;
        }

        private void next_move()
        {
            //change move from board
            if (this.game == null || this.activeGameMoves == null)
            {
                return;
            }

            if (this.move_index >= this.max_moves)
            {
                MessageBox.Show("No more moves to replay.");
                return;
            }

            DataRow movedr = this.activeGameMoves[this.move_index];

            Move move = data_row_to_move(movedr);
            this.game.play_move(move);
            this.move_index++;
        }

        public Move data_row_to_move(DataRow row)
        {
            //db -> Move class
            int from_x = row.Field<int>("from_x");
            int from_y = row.Field<int>("from_y");
            int to_x = row.Field<int>("to_x");
            int to_y = row.Field<int>("to_y");
            return new Move(from_x, from_y, to_x, to_y);
        }

        private void next_move(object sender, RoutedEventArgs e)
        {
            next_move();
        }
    }
}
