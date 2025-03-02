using System.Configuration;
using System.Data;
using System.Windows.Media;
using Win = System.Windows;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace SchoolClassesReminder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Win.Application
    {
        private static WinForms.NotifyIcon notifyIcon = null!;
        //private readonly MainWindow mainWindow;

        public App()
        {
            notifyIcon = new NotifyIcon();
        }

        protected override void OnStartup(Win.StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            MainWindow.Show();

            notifyIcon.Icon = new System.Drawing.Icon("Resources/instagram_icon.ico");
            notifyIcon.Text = "Classes Reminder";
            notifyIcon.Click += NotifyIcon_Click;

            notifyIcon.ContextMenuStrip = new WinForms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Test", Drawing.Image.FromFile("Resources/instagram_icon.ico"), OnTestClicked);

            notifyIcon.Visible = true;

            notifyIcon.ShowBalloonTip(3000, "Application started", "The School classes reminder is now in your apps tray!", ToolTipIcon.Info);

            base.OnStartup(e);
        }

        public static void ShowBallonTip(string title, string message)
        {
            notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        private void OnTestClicked(object? sender, EventArgs e)
        {
            Win.MessageBox.Show("It works!");
        }

        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            MainWindow.WindowState = Win.WindowState.Normal;
            MainWindow.Activate();
        }
        protected override void OnExit(Win.ExitEventArgs e)
        {
            notifyIcon.Dispose();

            base.OnExit(e);
        }
    }

}
