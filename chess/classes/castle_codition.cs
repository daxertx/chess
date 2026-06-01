using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess.classes
{
    public class castle_codition
    {

        public bool white_king_moved = false;
        public bool black_king_moved = false;

        public bool white_left_rook_moved = false;
        public bool white_right_rook_moved = false;
        public bool black_left_rook_moved = false;
        public bool black_right_rook_moved = false;
        public castle_codition()
        {
        }
    }
}
