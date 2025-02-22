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
        private readonly WinForms.NotifyIcon notifyIcon;

        public App()
        {
            this.notifyIcon = new NotifyIcon();
        }

        protected override void OnStartup(Win.StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            MainWindow.Show();

            this.notifyIcon.Icon = new System.Drawing.Icon("Resources/instagram_icon.ico");
            this.notifyIcon.Text = "Classes Reminder";
            this.notifyIcon.Click += NotifyIcon_Click;

            this.notifyIcon.ContextMenuStrip = new WinForms.ContextMenuStrip();
            this.notifyIcon.ContextMenuStrip.Items.Add("Test", Drawing.Image.FromFile("Resources/instagram_icon.ico"), OnTestClicked);

            this.notifyIcon.Visible = true;

            this.notifyIcon.ShowBalloonTip(3000, "Application started", "The School classes reminder is now in your apps tray!", ToolTipIcon.Info);

            base.OnStartup(e);
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
            this.notifyIcon.Dispose();

            base.OnExit(e);
        }
    }

}
