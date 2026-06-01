using chess.classes;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static chess.classes.piece;

namespace chess.user_control
{
    public partial class cube : UserControl
    {
        public PieceType Type;
        public Move? can_move_here = null;
        public bool is_pawn_moved_twice = false;
        public cube(bool isWhite, PieceType type,bool player_is_white = true)
        {
            InitializeComponent();
            if (isWhite)
            {
                cube_map.Background = new SolidColorBrush(Colors.Beige);
            }
            else
            {
                cube_map.Background = new SolidColorBrush(Colors.BurlyWood);
            }
            if (type == PieceType.Empty)
                return;

          
            change_piece(type);
        }

        public PieceType? change_piece(PieceType type)
        {
            this.is_pawn_moved_twice = false;
            PieceType? before = this.Type;
            this.can_move_here = null;
            if (type == PieceType.Empty)
            {
                cube_map.Content = null;
                this.Type = type;
                return before;
            }

            string imgName = build_board.type_to_img(type);//get piece type and then make it to string
            var uri = new Uri(
                $"pack://application:,,,/assets/chess_pieces/{imgName}",
                UriKind.Absolute
            );
            BitmapImage bitmap = new BitmapImage(uri);
            Image img = new Image();
            img.Source = bitmap;
            cube_map.Content = img;
            this.Type = type;
            return before;
        }
        public void move_twice()
        {
            this.is_pawn_moved_twice = true;
        }
    }
}
