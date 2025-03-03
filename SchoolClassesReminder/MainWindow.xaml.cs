﻿using System.Windows;
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
        private long dirationOfClassesInTicks = 0;
        private long dirationOfRecessesInTicks = 0;
        private long dirationOfBigRecessInTicks = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.numberOfClasses = 0;
            this.durationOfClasses = 0;
            this.durationOfRecesses = 0;
            this.durationOfBigRecess = 0;
            this.timer = new DispatcherTimer();
            checkBoxBigRecess.Click += CheckBoxBigRecess_Clicked;
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
                lblClasses.Visibility = Visibility.Hidden;
                listBoxClasses.Visibility = Visibility.Hidden;
                lblTypeDurationOfBigRecess.Visibility = Visibility.Hidden;
                txtDurationOfBigRecess.Visibility = Visibility.Hidden;
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

                    this.dirationOfBigRecessInTicks = this.durationOfBigRecess * TimeSpan.TicksPerMinute;
                } else
                {
                    lblDurationOfBigRecess.Visibility = Visibility.Hidden;
                    lblValueDurationOfBigRecess.Visibility = Visibility.Hidden;
                }

                txtNumberOfClasses.Clear();
                txtDurationOfClasses.Clear();
                txtDurationOfRecesses.Clear();

                Win.MessageBox.Show("Details added!");

                this.dirationOfClassesInTicks = this.durationOfClasses * TimeSpan.TicksPerMinute;
                this.dirationOfRecessesInTicks = this.durationOfRecesses * TimeSpan.TicksPerMinute;

                InitiateTimer(this.dirationOfClassesInTicks);
                lblTitleNext.Content = "Recess in:";
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
                        InitiateTimer(this.dirationOfRecessesInTicks);
                        lblTitleNext.Content = "Next class in:";
                        App.ShowBallonTip("The class has ended", "Your recess starts now!");
                        this.recessesCounter++;
                    } else
                    {
                        InitiateTimer(this.dirationOfClassesInTicks);
                        App.ShowBallonTip("The recess has ended", "Your next class starts now!");
                        this.classesCounter++;

                        if (this.classesCounter != this.numberOfClasses)
                        {
                            lblTitleNext.Content = "Recess in:";
                        }
                        else
                        {
                            lblTitleNext.Content = "This is your last class!";
                        }
                    }
                }
                else
                {
                    App.ShowBallonTip("All classes finished!", "You are ready to go!");
                    lblTitleNext.Content = "All classes are finished!";
                }
            }
            else
            {
                this.timeLeft = this.timeLeft.Subtract(TimeSpan.FromSeconds(1));
            }

            lblTimer.Content = this.timeLeft;
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