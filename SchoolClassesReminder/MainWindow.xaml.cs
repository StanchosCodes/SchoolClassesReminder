﻿using System.Diagnostics.Metrics;
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

            PopulateComboBoxHours();
            PopulateComboBoxMinutes();
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
                && ((checkBoxBigRecess.IsChecked == true && !string.IsNullOrEmpty(txtDurationOfBigRecess.Text)) || checkBoxBigRecess.IsChecked == false)
                && comboBoxHours.SelectedIndex > -1
                && comboBoxMinutes.SelectedIndex > -1)
            {
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

                DateTime now = DateTime.Now;
                DateTime classesStart = new DateTime(now.Year, now.Month, now.Day, this.classesStartTime.Hours, this.classesStartTime.Minutes, this.classesStartTime.Seconds);

                TimeSpan timeSpanCompareResult = now - classesStart;
                int compareResult = DateTime.Compare(now, classesStart);

                if (compareResult == 0) // now is the starting time of the classes
                {
                    InitiateTimer(this.durationOfClassesInTicks);
                    SetDetailsFields();

                    this.classesCounter++;
                }
                else if (compareResult > 0) // the starting time of the classes has passed
                {
                    long passedTicks = timeSpanCompareResult.Ticks;

                    InitiateTimer(this.durationOfClassesInTicks - passedTicks);
                    SetDetailsFields();

                    this.classesCounter++;
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

                        this.classesCounter++;

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

            lblTimer.Content = this.timeLeft.ToString().Substring(0, 8);

            if (this.durationOfClasses > 15 && TimeSpan.FromMinutes(this.durationOfClasses).Ticks - this.timeLeft.Ticks == this.classAbsenceInTicks)
            {
                App.ShowBallonTip("Absence time!", "15 minutes have passed. You can consider class as absent from now on.");
            }

            if (this.durationOfClasses > 2 && this.timeLeft.Minutes == 2 && this.timeLeft.Seconds == 0)
            {
                App.ShowBallonTip("Get ready!", "There are 2 minutes left. Prepare for class to finish.");
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
        }

        private void PopulateComboBoxHours()
        {
            for (int i = 0; i <= 23; i++)
            {
                comboBoxHours.Items.Add(i.ToString("D2"));
            }
        }

        private void PopulateComboBoxMinutes()
        {
            for (int i = 0; i <= 59; i++)
            {
                comboBoxMinutes.Items.Add(i.ToString("D2"));
            }
        }
    }
}