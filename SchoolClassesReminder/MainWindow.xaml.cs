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
        private TimeSpan classesStartTime;
        private TimeSpan timeLeft;
        private DispatcherTimer timer;
        private long durationOfClassesInTicks = 0;
        private long durationOfRecessesInTicks = 0;
        private long durationOfBigRecessInTicks = 0;
        private int bigRecessAfterClass = 0;
        private long classAbsenceInTicks = TimeSpan.FromMinutes(15).Ticks; // the ticks of 15 minutes - the time when the class is considered as absent
        private Dictionary<String, long> timeline = new Dictionary<string, long>();

        // 7 classes - 45 minutes each - 10 minutes recess - 20 minutes big recess after the 3rd class
        // timeline class - recess - class - recess - class - bigRecess - class - recess - class - recess - class - recess - class
        // 45 - 10 - 45 - 10 - 45 - 20 - 45 - 10 - 45 - 10 - 45 - 10 - 45
        // Dictionary<String, long> timeline - dictionary with key class/recess/bigRecess and value long duration of the class/recess in ticks

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

            PopulateComboBoxHours();
            PopulateComboBoxMinutes();
            AddHintToField("6", txtNumberOfClasses);
            AddHintToField("45", txtDurationOfClasses);
            AddHintToField("10", txtDurationOfRecesses);
            AddHintToField("20", txtDurationOfBigRecess);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
                App.ShowBallonTip("Application minimized", "The School classes reminder is now in your apps tray!");
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
                && ((checkBoxBigRecess.IsChecked == true && !string.IsNullOrEmpty(txtDurationOfBigRecess.Text)) || checkBoxBigRecess.IsChecked == false)
                && comboBoxHours.SelectedIndex > -1
                && comboBoxMinutes.SelectedIndex > -1
                && ValidateAllFields())
            {
                if (checkBoxBigRecess.IsChecked == true && int.Parse(txtNumberOfClasses.Text) > 1 && listBoxClasses.SelectedIndex == 0)
                {
                    listBoxClassesError.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    listBoxClassesError.Visibility = Visibility.Collapsed;
                }

                this.classesStartTime = new TimeSpan(int.Parse(comboBoxHours.Text), int.Parse(comboBoxMinutes.Text), 0);
                this.numberOfClasses = int.Parse(txtNumberOfClasses.Text);
                this.durationOfClasses = int.Parse(txtDurationOfClasses.Text);
                this.durationOfRecesses = int.Parse(txtDurationOfRecesses.Text);

                lblClassesStartTime.Content = this.classesStartTime;
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
                    lblDurationOfBigRecess.Visibility = Visibility.Collapsed;
                    lblValueDurationOfBigRecess.Visibility = Visibility.Collapsed;
                }

                txtNumberOfClasses.Clear();
                txtDurationOfClasses.Clear();
                txtDurationOfRecesses.Clear();

                Win.MessageBox.Show("Details added!");

                this.durationOfClassesInTicks = this.durationOfClasses * TimeSpan.TicksPerMinute;
                this.durationOfRecessesInTicks = this.durationOfRecesses * TimeSpan.TicksPerMinute;

                this.timeline.Clear();

                for (int i = 1; i <= this.numberOfClasses; i++)
                {
                    this.timeline.Add($"Class {i}", this.durationOfClassesInTicks);
                    
                    if (i != this.numberOfClasses)
                    {
                        if (i == this.bigRecessAfterClass)
                        {
                            this.timeline.Add("BigRecess", this.durationOfBigRecessInTicks);
                        }
                        else
                        {
                            this.timeline.Add($"Recess {i}", this.durationOfRecessesInTicks);
                        }
                    }
                }

                DateTime now = DateTime.Now;
                DateTime classesStart = new DateTime(now.Year, now.Month, now.Day, this.classesStartTime.Hours, this.classesStartTime.Minutes, this.classesStartTime.Seconds);

                TimeSpan timeSpanCompareResult = now - classesStart;
                int compareResult = DateTime.Compare(now, classesStart);

                if (compareResult == 0) // now is the starting time of the classes
                {
                    InitiateTimer(this.durationOfClassesInTicks);
                    SetDetailsFields();

                    this.timeline.Remove("Class 1");
                }
                else if (compareResult > 0) // the starting time of the classes has passed
                {
                    long passedTicks = timeSpanCompareResult.Ticks;

                    if (passedTicks > this.durationOfClassesInTicks)
                    {
                        for (int i = 0; i < this.numberOfClasses; i++)
                        {
                            passedTicks -= this.durationOfClassesInTicks;
                            this.timeline.Remove(this.timeline.Keys.First());

                            if (passedTicks < this.durationOfClassesInTicks)
                            {
                                break;
                            }
                        }

                        if (passedTicks > this.durationOfRecessesInTicks)
                        {
                            passedTicks -= this.durationOfRecessesInTicks;
                            this.timeline.Remove(this.timeline.Keys.First());
                        }
                    }
                        
                    if (this.timeline.Keys.First().Contains("Class"))
                    {
                        InitiateTimer(this.durationOfClassesInTicks - passedTicks);
                    }
                    else if (this.timeline.Keys.First().Contains("BigRecess"))
                    {
                        InitiateTimer(this.durationOfBigRecessInTicks - passedTicks);
                    }
                    else
                    {
                        InitiateTimer(this.durationOfRecessesInTicks - passedTicks);
                    }

                    SetDetailsFields();

                    this.timeline.Remove(this.timeline.Keys.First());
                }
                else // the starting time of the classes is yet to come
                {
                    if (this.timer.IsEnabled)
                    {
                        this.timer.Stop();
                    }

                    lblTimer.Content = new TimeSpan();
                    lblCurrentClass.Visibility = Visibility.Hidden;

                    DispatcherTimer waitTimer = new DispatcherTimer
                    {
                        Interval = timeSpanCompareResult.Negate()
                    };

                    waitTimer.Tick += (sender, args) =>
                    {
                        waitTimer.Stop();

                        InitiateTimer(this.durationOfClassesInTicks);
                        SetDetailsFields();

                        this.timeline.Remove("Class 1");

                        App.ShowBallonTip("Class started", "Your class has started!");
                    };

                    waitTimer.Start();

                    string timeToStartClass = new TimeSpan(timeSpanCompareResult.Negate().Ticks).ToString().Substring(0, 8);
                    string timeToStartClassPostfix = timeSpanCompareResult.Negate().Hours > 0 ? "hours" : timeSpanCompareResult.Negate().Minutes > 0 ? "minutes" : "seconds";
                    App.ShowBallonTip("Class will start soon", $"Your class will start in {timeToStartClass} {timeToStartClassPostfix}!");
                }
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

                if (this.timeline.Count > 0)
                {
                    string[] nextKeySplited = this.timeline.Keys.First().Split(" ");

                    if (nextKeySplited[0].Contains("Class"))
                    {
                        InitiateTimer(this.durationOfClassesInTicks);
                        App.ShowBallonTip("The recess has ended", "Your next class starts now!");


                        lblCurrentClass.Content = $"Your current class is: {nextKeySplited[1]}";
                    }
                    else if (nextKeySplited[0].Contains("BigRecess"))
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

                    this.timeline.Remove(this.timeline.Keys.First());

                    if (this.timeline.Count > 0)
                    {
                        if (this.timeline.First().Key.Contains("Class"))
                        {
                            if (this.timeline.Count == 1)
                            {
                                lblTitleNext.Content = "Last class in:";
                            }
                            else
                            {
                                lblTitleNext.Content = "Next class in:";
                            }
                        }
                        else if (this.timeline.Keys.First().Contains("BigRecess"))
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

            lblTimer.Content = this.timeLeft.ToString().Substring(0, 8);

            if (this.lblCurrentClass.Content.ToString()!.Contains("class"))
            {
                if (this.durationOfClasses > 15 && TimeSpan.FromMinutes(this.durationOfClasses).Ticks - this.timeLeft.Ticks == this.classAbsenceInTicks)
                {
                    App.ShowBallonTip("Absence time!", "15 minutes have passed. You can consider class as absent from now on.");
                }

                if (this.durationOfClasses > 2 && this.timeLeft.Minutes == 2 && this.timeLeft.Seconds == 0)
                {
                    App.ShowBallonTip("Get ready!", "There are 2 minutes left. Prepare for class to finish.");
                }
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

        private void SetDetailsFields()
        {
            lblCurrentClass.Visibility = Visibility.Visible;
            string[] nextClassKeySplited = this.timeline.Keys.First().Split(" ");

            if (nextClassKeySplited[0].Contains("Class"))
            {
                lblCurrentClass.Content = $"Your current class is: {nextClassKeySplited[1]}";
            }
            else if (nextClassKeySplited[0].Contains("BigRecess"))
            {
                lblCurrentClass.Content = $"You are in big recess!";
            }
            else
            {
                lblCurrentClass.Content = $"You are in recess!";
            }

            if (this.numberOfClasses == 1)
            {
                lblTitleNext.Content = "This is your last class!";
            }
            else
            {
                if (this.timeline.Keys.ElementAt(1).Contains("Class"))
                {
                    lblTitleNext.Content = "Next class in:";
                }
                else if (this.timeline.Keys.ElementAt(1).Contains("BigRecess"))
                {
                    lblTitleNext.Content = "Big recess in:";
                }
                else
                {
                    lblTitleNext.Content = "Recess in:";
                }
            }
        }

        private void PopulateComboBoxHours()
        {
            for (int i = 1; i <= 23; i++)
            {
                comboBoxHours.Items.Add(i.ToString("D2"));
            }
        }

        private void PopulateComboBoxMinutes()
        {
            for (int i = 1; i <= 59; i++)
            {
                comboBoxMinutes.Items.Add(i.ToString("D2"));
            }
        }

        private void AddHintToField(string hint, System.Windows.Controls.TextBox field)
        {
            field.Text = hint;
            field.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void TxtNumberOfClasses_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtNumberOfClasses.Foreground == System.Windows.Media.Brushes.Gray)
            {
                txtNumberOfClasses.Text = "";
                txtNumberOfClasses.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TxtNumberOfClasses_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNumberOfClasses.Text))
            {
                if (ValidateIntField(txtNumberOfClasses.Text))
                {
                    txtNumberOfClassesError.Visibility = Visibility.Collapsed;
                    int classesCount = int.Parse(txtNumberOfClasses.Text);

                    listBoxClasses.Items.Clear();
                    listBoxClasses.Items.Add("Choose a class");
                    listBoxClasses.SelectedIndex = 0;

                    for (int i = 0; i < classesCount - 1; i++)
                    {
                        string numPostfix = i == 0 ? "st" : i == 1 ? "nd" : i == 2 ? "rd" : "th";

                        listBoxClasses.Items.Add($"After the {i + 1}{numPostfix} class");
                    }
                }
                else
                {
                    txtNumberOfClassesError.Visibility = Visibility.Visible;
                }
            }
            else
            {
                AddHintToField("6", txtNumberOfClasses);
                txtNumberOfClassesError.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtDurationsOfClasses_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDurationOfClasses.Foreground == System.Windows.Media.Brushes.Gray)
            {
                txtDurationOfClasses.Text = "";
                txtDurationOfClasses.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TxtDurationsOfClasses_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDurationOfClasses.Text))
            {
                AddHintToField("45", txtDurationOfClasses);
                txtDurationOfClassesError.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!ValidateIntField(txtDurationOfClasses.Text))
                {
                    txtDurationOfClassesError.Visibility = Visibility.Visible;
                }
                else
                {
                    txtDurationOfClassesError.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void TxtDurationsOfRecesses_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDurationOfRecesses.Foreground == System.Windows.Media.Brushes.Gray)
            {
                txtDurationOfRecesses.Text = "";
                txtDurationOfRecesses.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TxtDurationsOfRecesses_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDurationOfRecesses.Text))
            {
                AddHintToField("10", txtDurationOfRecesses);
                txtDurationOfRecessesError.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!ValidateIntField(txtDurationOfRecesses.Text))
                {
                    txtDurationOfRecessesError.Visibility = Visibility.Visible;
                }
                else
                {
                    txtDurationOfRecessesError.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void TxtDurationsOfBigRecess_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDurationOfBigRecess.Foreground == System.Windows.Media.Brushes.Gray)
            {
                txtDurationOfBigRecess.Text = "";
                txtDurationOfBigRecess.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TxtDurationsOfBigRecess_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDurationOfBigRecess.Text))
            {
                AddHintToField("20", txtDurationOfBigRecess);
                txtDurationOfBigRecessError.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!ValidateIntField(txtDurationOfBigRecess.Text))
                {
                    txtDurationOfBigRecessError.Visibility = Visibility.Visible;
                }
                else
                {
                    txtDurationOfBigRecessError.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool ValidateIntField(string text)
        {
            return int.TryParse(text, out int result);
        }

        private bool ValidateAllFields()
        {
            return ValidateIntField(txtNumberOfClasses.Text)
                && ValidateIntField(txtDurationOfClasses.Text)
                && ValidateIntField(txtDurationOfRecesses.Text)
                && ((checkBoxBigRecess.IsChecked == true 
                    && ValidateIntField(txtDurationOfBigRecess.Text) 
                    && txtDurationOfBigRecess.Foreground != System.Windows.Media.Brushes.Gray) || checkBoxBigRecess.IsChecked == false)
                && txtNumberOfClasses.Foreground != System.Windows.Media.Brushes.Gray
                && txtDurationOfClasses.Foreground != System.Windows.Media.Brushes.Gray
                && txtDurationOfRecesses.Foreground != System.Windows.Media.Brushes.Gray;
        }
    }
}