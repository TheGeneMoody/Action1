using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

class ScreenBorder : Form
{
    private static string targetProcess = "action1_remote";
    private Label statusLabel;

    public ScreenBorder(Rectangle bounds)
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.Bounds = bounds;
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.BackColor = Color.Lime;
        this.TransparencyKey = Color.Lime;
        this.Opacity = 1;

        this.Load += (s, e) =>
        {
            SetWindowLong(this.Handle, GWL_EXSTYLE,
                GetWindowLong(this.Handle, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            AddStatusLabel();
        };
    }
    private void AddStatusLabel()
    {
        statusLabel = new Label();
        statusLabel.Text = "Remote session active";
        statusLabel.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        statusLabel.ForeColor = Color.Black;
        statusLabel.BackColor = Color.White;
        statusLabel.AutoSize = true;
        statusLabel.Padding = new Padding(10, 5, 10, 5);

        using (Graphics g = CreateGraphics())
        {
            Size textSize = TextRenderer.MeasureText(statusLabel.Text, statusLabel.Font);
            statusLabel.Size = new Size(textSize.Width + statusLabel.Padding.Horizontal, textSize.Height + statusLabel.Padding.Vertical);
        }

        int x = (this.Width - statusLabel.Width) / 2;
        int y = this.Height - statusLabel.Height - 50;
        statusLabel.Location = new Point(x, y);

        // Rounded corners
        GraphicsPath path = new GraphicsPath();
        int radius = 12;
        Rectangle rect = new Rectangle(0, 0, statusLabel.Width, statusLabel.Height);
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseAllFigures();
        statusLabel.Region = new Region(path);

        this.Controls.Add(statusLabel);
        statusLabel.BringToFront();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Pen pen = new Pen(ColorTranslator.FromHtml("#2BABE4"), 10))
        {
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
        }
    }

    public void SetVisible(bool visible)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => this.Visible = visible));
        }
        else
        {
            this.Visible = visible;
        }
    }

    // Win32 API
    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    const int GWL_EXSTYLE = -20;
    const int WS_EX_LAYERED = 0x80000;
    const int WS_EX_TRANSPARENT = 0x20;

    [STAThread]
    static void Main()
    {
        bool createdNew;
        using (Mutex mutex = new Mutex(true, "Global\\BorderOverlaySingleInstance", out createdNew)) //prevent multiple instances.
        {
            if (!createdNew)
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Only use the primary screen
            var bounds = Screen.PrimaryScreen.Bounds;
            var overlay = new ScreenBorder(bounds);

            Thread t = new Thread(() => Application.Run(overlay));
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            // Timer loop to check for the target process every 3 seconds
            System.Windows.Forms.Timer processCheckTimer = new System.Windows.Forms.Timer();
            processCheckTimer.Interval = 3000;
            // determine default show state.
            overlay.SetVisible(Process.GetProcessesByName(targetProcess).Length > 0);
            processCheckTimer.Tick += (s, e) =>
            {
                overlay.SetVisible(Process.GetProcessesByName(targetProcess).Length > 0);
            };
            processCheckTimer.Start();
            Application.Run();
        }
    }
}
