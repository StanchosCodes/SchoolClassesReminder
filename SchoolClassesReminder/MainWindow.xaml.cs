using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Win = System.Windows;

namespace SchoolClassesReminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Win.Window
    {
        private int numberOfClasses;
        private int durationOfClasses;
        private int durationOfRecesses;
        private int durationOfBigRecess;
        private TimeSpan timeLeft;
        private DispatcherTimer timer;
        private int classesCounter = 0;
        private int recessesCounter = 0;
        private long durationOfClassesInTicks = 0;
        private long durationOfRecessesInTicks = 0;
        private long durationOfBigRecessInTicks = 0;
        private int bigRecessAfterClass = 0;
        private long classAbsenceInTicks = TimeSpan.FromMinutes(15).Ticks; // the ticks of 15 minutes - the time when the class is considered as absent

        public MainWindow()
        {
            InitializeComponent();

            this.numberOfClasses = 0;
            this.durationOfClasses = 0;
            this.durationOfRecesses = 0;
            this.durationOfBigRecess = 0;
            this.timer = new DispatcherTimer();
            checkBoxBigRecess.Click += CheckBoxBigRecess_Clicked;
            txtNumberOfClasses.LostFocus += TxtNumberOfClasses_LostFocus;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void TxtNumberOfClasses_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNumberOfClasses.Text))
            {
                int classesCount = int.Parse(txtNumberOfClasses.Text);

                listBoxClasses.Items.Clear();
                listBoxClasses.Items.Add("Choose a class");
                listBoxClasses.SelectedIndex = 0;

                for (int i = 0; i < classesCount; i++)
                {
                    string numPostfix = i == 0 ? "st" : i == 1 ? "nd" : i == 2 ? "rd" : "th";

                    listBoxClasses.Items.Add($"After the {i + 1}{numPostfix} class");
                }
            }
        }

        private void CheckBoxBigRecess_Clicked(object sender, RoutedEventArgs e)
        {
            if (checkBoxBigRecess.IsChecked == true)
            {
                lblClasses.Visibility = Visibility.Visible;
                listBoxClasses.Visibility = Visibility.Visible;
                lblTypeDurationOfBigRecess.Visibility = Visibility.Visible;
                txtDurationOfBigRecess.Visibility = Visibility.Visible;
            } else
            {
                lblClasses.Visibility = Visibility.Collapsed;
                listBoxClasses.Visibility = Visibility.Collapsed;
                lblTypeDurationOfBigRecess.Visibility = Visibility.Collapsed;
                txtDurationOfBigRecess.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonAddDetails_Click(object sender, Win.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNumberOfClasses.Text) 
                && !string.IsNullOrEmpty(txtDurationOfClasses.Text) 
                && !string.IsNullOrEmpty(txtDurationOfRecesses.Text) 
                && ((checkBoxBigRecess.IsChecked == true && !string.IsNullOrEmpty(txtDurationOfBigRecess.Text)) || checkBoxBigRecess.IsChecked == false))
            {
                this.numberOfClasses = int.Parse(txtNumberOfClasses.Text);
                this.durationOfClasses = int.Parse(txtDurationOfClasses.Text);
                this.durationOfRecesses = int.Parse(txtDurationOfRecesses.Text);

                lblNumberOfClasses.Content = this.numberOfClasses;
                lblDurationOfClasses.Content = this.durationOfClasses + " minutes";
                lblDurationOfRecesses.Content = this.durationOfRecesses + " minutes";

                if (checkBoxBigRecess.IsChecked == true)
                {
                    lblDurationOfBigRecess.Visibility = Visibility.Visible;
                    lblValueDurationOfBigRecess.Visibility = Visibility.Visible;

                    this.durationOfBigRecess = int.Parse(txtDurationOfBigRecess.Text);
                    lblValueDurationOfBigRecess.Content = this.durationOfBigRecess + " minutes";
                    txtDurationOfBigRecess.Clear();

                    this.bigRecessAfterClass = listBoxClasses.SelectedIndex;
                    this.durationOfBigRecessInTicks = this.durationOfBigRecess * TimeSpan.TicksPerMinute;
                }
                else
                {
                    lblDurationOfBigRecess.Visibility = Visibility.Hidden;
                    lblValueDurationOfBigRecess.Visibility = Visibility.Hidden;
                }

                txtNumberOfClasses.Clear();
                txtDurationOfClasses.Clear();
                txtDurationOfRecesses.Clear();

                Win.MessageBox.Show("Details added!");

                this.durationOfClassesInTicks = this.durationOfClasses * TimeSpan.TicksPerMinute;
                this.durationOfRecessesInTicks = this.durationOfRecesses * TimeSpan.TicksPerMinute;

                InitiateTimer(this.durationOfClassesInTicks);
                lblCurrentClass.Visibility = Visibility.Visible;
                this.classesCounter = 0;
                lblCurrentClass.Content = $"Your current class is: {this.classesCounter + 1}";

                if (this.numberOfClasses == 1)
                {
                    lblTitleNext.Content = "This is your last class!";
                }
                else
                {
                    if (this.bigRecessAfterClass == 1)
                    {
                        lblTitleNext.Content = "Big recess in:";
                    }
                    else
                    {
                        lblTitleNext.Content = "Recess in:";
                    }
                }

                this.classesCounter++;
            }
            else
            {
                Win.MessageBox.Show("Not all fields are filled correctly!");
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (this.timeLeft.Hours == 0 && this.timeLeft.Minutes == 0 && this.timeLeft.Seconds == 0)
            {
                this.timer.Stop();
                
                if (classesCounter < this.numberOfClasses)
                {
                    if (recessesCounter < classesCounter)
                    {
                        if (classesCounter == this.bigRecessAfterClass)
                        {
                            InitiateTimer(this.durationOfBigRecessInTicks);
                            App.ShowBallonTip("The class has ended", "Your big recess starts now!");
                            lblCurrentClass.Content = "You are in big recess!";
                        }
                        else
                        {
                            InitiateTimer(this.durationOfRecessesInTicks);
                            App.ShowBallonTip("The class has ended", "Your recess starts now!");
                            lblCurrentClass.Content = "You are in recess!";
                        }

                        if (this.numberOfClasses - this.classesCounter == 1)
                        {
                            lblTitleNext.Content = "Last class in:";
                        }
                        else
                        {
                            lblTitleNext.Content = "Next class in:";
                        }
                        
                        this.recessesCounter++;
                    }
                    else
                    {
                        InitiateTimer(this.durationOfClassesInTicks);
                        App.ShowBallonTip("The recess has ended", "Your next class starts now!");
                        this.classesCounter++;

                        if (this.classesCounter != this.numberOfClasses)
                        {
                            if (this.classesCounter == this.bigRecessAfterClass)
                            {
                                lblTitleNext.Content = "Big recess in:";
                            }
                            else
                            {
                                lblTitleNext.Content = "Recess in:";
                            }
                        }
                        else
                        {
                            lblTitleNext.Content = "This is your last class!";
                        }

                        lblCurrentClass.Content = $"Your current class is: {this.classesCounter}";
                    }
                }
                else
                {
                    App.ShowBallonTip("All classes finished!", "You are ready to go!");
                    lblTitleNext.Content = "All classes are finished!";
                    lblCurrentClass.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                this.timeLeft = this.timeLeft.Subtract(TimeSpan.FromSeconds(1));
            }

            lblTimer.Content = this.timeLeft;

            if (TimeSpan.FromMinutes(this.durationOfClasses).Ticks - this.timeLeft.Ticks == this.classAbsenceInTicks)
            {
                App.ShowBallonTip("Absence time!", "15 minutes have passed. You can consider class as absent from now on.");
            }
        }

        // Initiates a timer with duration the given ticks
        private void InitiateTimer(long ticks)
        {
            this.timeLeft = new TimeSpan(ticks);

            if (this.timer.IsEnabled)
            {
                this.timer.Stop();
            }

            this.timer = new DispatcherTimer();

            this.timer.Interval = TimeSpan.FromSeconds(1);
            this.timer.Tick += Timer_Tick;
            this.timer.Start();
        }
    }
}