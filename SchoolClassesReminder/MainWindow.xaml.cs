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

        public MainWindow()
        {
            InitializeComponent();

            this.numberOfClasses = 0;
            this.durationOfClasses = 0;
            this.durationOfRecesses = 0;
            this.durationOfBigRecess = 0;
            this.timer = new DispatcherTimer();
        }

        private void ButtonAddDetails_Click(object sender, Win.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNumberOfClasses.Text) && !string.IsNullOrEmpty(txtDurationOfClasses.Text) && !string.IsNullOrEmpty(txtDurationOfRecesses.Text))
            {
                this.numberOfClasses = int.Parse(txtNumberOfClasses.Text);
                this.durationOfClasses = int.Parse(txtDurationOfClasses.Text);
                this.durationOfRecesses = int.Parse(txtDurationOfRecesses.Text);
                this.durationOfBigRecess = int.Parse(txtDurationOfBigRecess.Text);

                lblNumberOfClasses.Content = this.numberOfClasses;
                lblDurationOfClasses.Content = this.durationOfClasses + " minutes";
                lblDurationOfRecesses.Content = this.durationOfRecesses + " minutes";
                lblDurationOfBigRecess.Content = this.durationOfBigRecess + " minutes";

                txtNumberOfClasses.Clear();
                txtDurationOfClasses.Clear();
                txtDurationOfRecesses.Clear();
                txtDurationOfBigRecess.Clear();

                long durationOfClassesInTicks = this.durationOfClasses * TimeSpan.TicksPerMinute;

                this.timeLeft = new TimeSpan(durationOfClassesInTicks);

                if (this.timer.IsEnabled)
                {
                    this.timer.Stop();
                    this.timer = new DispatcherTimer();
                }

                this.timer.Interval = TimeSpan.FromSeconds(1);
                this.timer.Tick += Timer_Tick;
                this.timer.Start();

                Win.MessageBox.Show("Details added!");
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
                Win.MessageBox.Show("The class has ended. Your recess starts now!");
            }
            else
            {
                this.timeLeft = this.timeLeft.Subtract(TimeSpan.FromSeconds(1));
            }

            lblTimer.Content = this.timeLeft;
        }
    }
}