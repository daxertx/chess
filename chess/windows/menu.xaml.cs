using System.Windows;

namespace chess.windows
{
    public partial class menu : Window
    {
        public menu()
        {
            InitializeComponent();
        }

        private void PlayLocal_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new self_play.self();
            gameWindow.Show();
            this.Close();
        }

        private void PlayOnline_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Host game?\nYes = Host\nNo = Join","Online Mode",MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Yes)
            {
                var host = new player_vs_player.server();
                host.Show();
                this.Close();
            }
            else if (result == MessageBoxResult.No)
            {
                var join = new player_vs_player.client();
                join.Show();
                this.Close();
            }
        }

        private void PlayAI_Click(object sender, RoutedEventArgs e)
        {
        }
        private void PlayReplay_Click(object sender, RoutedEventArgs e)
        {
            database.talk_to_database db = new database.talk_to_database();
            if (db.amount_of_games() == 0)
            {
                MessageBox.Show("you have no recordings");
                return;
            }
            var gameWindow = new replay_mode.replay_game();
            gameWindow.Show();
            this.Close();
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open settings window
            // var settingsWindow = new Settings();
            // settingsWindow.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}