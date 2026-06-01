using chess.classes;
using chess.database;
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

namespace chess.windows.self_play
{
    public partial class self : Window
    {

        public game game;
        public talk_to_database db = new talk_to_database();
        public int game_id;
        public self()
        {
            InitializeComponent();
            this.game = new game(board,bar);
            int game_id = this.db.add_game("self", "self");
            this.game_id = game_id;
            this.game.new_move += load_move_to_db;
            
            this.game.tie += () =>
            {
                MessageBox.Show("Tie!");
                OnClosed(EventArgs.Empty);
            };

            this.game.game_over += () =>
            {
                MessageBox.Show("Game over!");
                OnClosed(EventArgs.Empty);
            };
        }
        public void load_move_to_db(Move move)
        {
            this.db.add_move(this.game_id, move);
        }
    }
}
