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
using System.Windows.Threading;

namespace Minesweeper
{

    /// Interaction logic for MainWindow.xaml

    public partial class MainWindow : Window
    {
        public string OptionText { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
        }
        public class Cell : Button
        {
            public bool IsOpened { get; set; } = false;
        }


        //Levels of difficulty
        public enum GameLevel { Easy = 10, Normal = 20, Hard = 40 }

        //Game area depending on difficulty level that's selected
        private void cmbGameLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isload)
            {
                ComboBoxItem item = ((ComboBoxItem)cmbGameLevel.SelectedItem);
                string str = item.Content.ToString();
                int number = Convert.ToInt32(item.Tag);

                CreateGameArea(number, null);
            }
        }

        //Variables used to store the game state
        bool isload = false;
        private bool firstClick = true;
        private bool gameOver = false;
        private bool isInitializing = false;
        private DispatcherTimer gameTimer;
        private int elapsedTime;

        private void SetupTimer()
        {
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;

            elapsedTime = 0;
            timerText.Text = "Time: 0";
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            elapsedTime++;
            timerText.Text = $"Time: {elapsedTime}";
        }

        private void FirstClick(int totalMines, Button clickedButton)
        {
            if (firstClick)
            {
                int totalPlace = wrpPanel.Children.Count;
                placeMines(totalMines, totalPlace, clickedButton);
                firstClick = false;
            }
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }


        //Create grid and place mines
        private void CreateGameArea(int totalMine, Button clickedButton = null)
        {
            gameTimer.Stop();
            elapsedTime = 0;
            timerText.Text = "Time: 0";
            isInitializing = true;
            gameOver = false;

            int count = 10;
            if (totalMine == 10) { count = 10; }
            else if (totalMine == 20) { count = 17; }
            else if (totalMine == 45) { count = 20; }

            wrpPanel.Children.Clear();

            wrpPanel.Width = count * 25;

            for (int i = 0; i < count * count; i++)
            {
                Cell cell = new Cell();
                cell.Width = 25;
                cell.Height = 25;
                cell.Tag = false;

                cell.Click += Btn_Click;
                cell.MouseRightButtonDown += Btn_MouseRightButtonDown;

                // Reset openedNonMineCells to 0
                openedNonMineCells = 0;
                totalNonMineCells = count * count - totalMine;

                if (i % 2 == 0)
                {
                    cell.Background = new SolidColorBrush(Color.FromRgb(255, 182, 193));
                }
                else
                {
                    cell.Background = new SolidColorBrush(Color.FromRgb(255, 192, 203));
                }
                originalButtonColors[cell] = (SolidColorBrush)cell.Background;
                wrpPanel.Children.Add(cell);
            }
            placeMines(totalMine, count * count, clickedButton);
            isInitializing = false;
        }


        //Original colours of the cells
        private Dictionary<Button, SolidColorBrush> originalButtonColors = new Dictionary<Button, SolidColorBrush>();

        //Get surrounding buttons of a given button
        private List<Button> GetSurroundingButtons(Button btn)
        {
            List<Button> surroundingButtons = new List<Button>();
            int index = wrpPanel.Children.IndexOf(btn);
            int count = (int)Math.Sqrt(wrpPanel.Children.Count);

            int row = index / count;
            int col = index % count;

            for (int i = Math.Max(row - 1, 0); i <= Math.Min(row + 1, count - 1); i++)
            {
                for (int j = Math.Max(col - 1, 0); j <= Math.Min(col + 1, count - 1); j++)
                {
                    int newIndex = i * count + j;
                    if (newIndex != index)
                    {
                        Button newBtn = (Button)wrpPanel.Children[newIndex];
                        surroundingButtons.Add(newBtn);
                    }
                }
            }

            return surroundingButtons;
        }

        //Variables to store the number of opened non-mine cells and the total non-mine cells
        private int openedNonMineCells = 0;
        private int totalNonMineCells = 0;


        // Method to count the mines surrounding a button
        private int CountSurroundingMines(Button btn)
        {
            return GetSurroundingButtons(btn).Count(b => (bool)b.Tag);
        }

        //Right-clickig a cell
        private void Btn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gameOver) return;
            Button btn = (Button)sender;

            SolidColorBrush openedCellColor = new SolidColorBrush(Color.FromRgb(211, 211, 211));
            // Check if the cell is opened
            if (btn.Background.ToString() == openedCellColor.ToString()) 
            {
                return;
            }

            if (btn.Content == null || btn.Content.ToString() != "🚩")
            {
                btn.Content = "🚩";
                btn.Background = new SolidColorBrush(Color.FromRgb(255, 69, 0));
                e.Handled = true;

            }
            else
            {
                btn.Content = "";
                btn.Background = originalButtonColors[btn];
            }
        }

        //Left-clicking a cell; Check if pressed button is a mine or not ; Reveal mines ; Checks for win/lose
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!gameTimer.IsEnabled)
            {
                gameTimer.Start();
            }

            if (gameOver) return;

            Button btn = (Button)sender;
            Cell cell = new Cell();

            // Check if the cell is flagged
            if (btn.Content != null && btn.Content.ToString() == "🚩")
            {
                return;
            }

            // Call FirstClick() method to place mines after the first click
            int totalMines = (int)GameLevel.Easy; // Default to Easy lvl
            if (cmbGameLevel.SelectedItem != null)
            {
                ComboBoxItem item = (ComboBoxItem)cmbGameLevel.SelectedItem;
                totalMines = Convert.ToInt32(item.Tag);
            }
            FirstClick(totalMines, btn);

            //Check if the cell contains a bomb
            if ((bool)btn.Tag == true)
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(112, 128, 144));
                btn.Content = "💣";
                MessageBox.Show("Oooopsie daisy");
                ShowAllMines();
                gameOver = true;
                gameTimer.Stop();
            }
            else
            {
                int surroundingMines = CountSurroundingMines(btn);
                if (surroundingMines > 0)
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(211, 211, 211));
                    btn.Content = surroundingMines.ToString();
                }
                else
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(211, 211, 211));
                    btn.IsEnabled = false;
                    cell.IsOpened = true;

                    foreach (Button b in GetSurroundingButtons(btn))
                    {
                        if (b.IsEnabled)
                        {
                            Btn_Click(b, null);
                        }
                    }

                    /*// Increment the counter only if the game is not over
                    if ((bool)btn.Tag != true)
                    {
                        openedNonMineCells++;
                    }

                    // Check if the user has won
                    if (!isInitializing && openedNonMineCells == totalNonMineCells && !gameOver)
                    {
                        MessageBox.Show("Congratulations, you won!");
                        ShowAllMines(); // Show all mines with the correct flags
                        RestartGame();
                    }*/
                    // Call CheckWinCondition method
                    CheckWinCondition();
                }
            }
        }

        //Place random mines on the grid
        private void placeMines(int totalMines, int totalPlace, Button clickedButton)
        {
            Random rnd = new Random();
            int counter = 0;

            do
            {
                Button btn = (Button)wrpPanel.Children[rnd.Next(0, totalPlace)];

                if ((bool)btn.Tag == false && btn != clickedButton)
                {
                    btn.Tag = true;

                    counter++;
                }
            } while (counter < totalMines);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            isload = true;
            CreateGameArea(10);
        }

        private void ShowAllMines()
        {
            gameOver = true;
            foreach (Button btn in wrpPanel.Children)
            {
                if ((bool)btn.Tag == true)
                {
                    if (btn.Content != null && btn.Content.ToString() == "🚩")
                    {
                        // Correctly flagged cells
                        btn.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34));
                        btn.Content = "💣";
                    }
                    else
                    {
                        // Unflagged cells with bombs
                        btn.Background = new SolidColorBrush(Color.FromRgb(112, 128, 144));
                        btn.Content = "💣";
                    }
                }
                else
                {
                    // Incorrectly flagged cells (no bomb)
                    if (btn.Content != null && btn.Content.ToString() == "🚩")
                    {
                        btn.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        btn.Content = "❌";
                    }
                }
            }
        }

        private int GetTotalMines()
        {
            ComboBoxItem item = ((ComboBoxItem)cmbGameLevel.SelectedItem);
            if (item == null)
            {
                return 10; // Default to easy (10 mines) if no item is selected
            }

            GameLevel level = (GameLevel)Enum.Parse(typeof(GameLevel), item.Tag.ToString());
            return (int)level;
        }

        private int CountNonFlaggedNonOpenedCells()
        {
            int nonFlaggedNonOpenedCells = 0;
            foreach (Cell cell in wrpPanel.Children)
            {
                if (cell.Content == null || cell.Content.ToString() != "🚩")
                {
                    if (!cell.IsOpened)
                    {
                        nonFlaggedNonOpenedCells++;
                    }
                }
            }

            return nonFlaggedNonOpenedCells;
        }

        private void CheckWinCondition()
        {
            int unopenedCells = 0;
            foreach (Button btn in wrpPanel.Children)
            {
                if (btn.Background.ToString() != new SolidColorBrush(Color.FromRgb(211, 211, 211)).ToString())
                {
                    unopenedCells++;
                }
            }

            if (cmbGameLevel.SelectedItem != null)
            {
                int totalMines = Convert.ToInt32(((ComboBoxItem)cmbGameLevel.SelectedItem).Tag);
                int nonFlaggedNonOpenedCells = CountNonFlaggedNonOpenedCells();

                if (unopenedCells == totalMines)
                {
                    MessageBox.Show("Yaaaay! Go do something useful now");
                    gameTimer.Stop();

                }
            }
        }

        private void RestartGame()
        {
            GameLevel level;

            if (totalNonMineCells == 90)
            {
                level = GameLevel.Easy;
            }
            else if (totalNonMineCells == 289)
            {
                level = GameLevel.Normal;
            }
            else
            {
                level = GameLevel.Hard;
            }

            CreateGameArea((int)level, null);
            gameOver = false;
        }

    }
}


