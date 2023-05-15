using System;
using System.Windows;

namespace Minesweeper
{
    public partial class CustomLevelDialog : Window
    {
        public CustomLevelDialog()
        {
            InitializeComponent();
        }

        public int NumberOfRows { get; private set; }
        public int NumberOfCols { get; private set; }
        public int NumberOfMines { get; private set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRows.Text, out int rows) &&
                int.TryParse(txtCols.Text, out int cols) &&
                int.TryParse(txtMines.Text, out int mines))
            {
                if (rows > 0 && cols > 0 && mines > 0 && mines < rows * cols)
                {
                    NumberOfRows = rows;
                    NumberOfCols = cols;
                    NumberOfMines = mines;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid input. Please enter positive numbers and make sure the number of mines is less than the total cells.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter valid numbers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

