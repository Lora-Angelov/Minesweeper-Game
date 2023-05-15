using System.Windows;

namespace Minesweeper
{
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            if (sender == btnOption1)
            {
                mainWindow.OptionText = "Option 1";
            }
            else if (sender == btnOption2)
            {
                mainWindow.OptionText = "Option 2";
            }

            mainWindow.Show();
            this.Close();
        }
    }
}
