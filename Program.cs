using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;
using System.Windows.Forms;

namespace IceClicker;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private readonly Label status = new() { AutoSize = true, Text = "Oprit", Location = new Point(18, 20) };
    private readonly NumericUpDown threshold = new() { Minimum = 40, Maximum = 99, Value = 78, Location = new Point(165, 55), Width = 70 };
    private readonly NumericUpDown delay = new() { Minimum = 200, Maximum = 10000, Increment = 100, Value = 900, Location = new Point(165, 88), Width = 70 };
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 250 };
    private Mat? template;
    private bool running;
    private bool busy;
    private DateTime lastClick = DateTime.MinValue;

    public MainForm()
    {
        Text = "IceClicker";
        ClientSize = new System.Drawing.Size(320, 185);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Controls.Add(status);
        Controls.Add(new Label { Text = "Potrivire minimă (%)", AutoSize = true, Location = new Point(18, 58) });
        Controls.Add(threshold);
        Controls.Add(new Label { Text = "Pauză click (ms)", AutoSize = true, Location = new Point(18, 91) });
        Controls.Add(delay);
        var start = new Button { Text = "Pornește / Oprește (F8)", Location = new Point(18, 125), Width = 210 };
        start.Click += (_, _) => Toggle();
        Controls.Add(start);
        var path = Path.Combine(AppContext.BaseDirectory, "assets", "target.png");
        if (File.Exists(path)) template = Cv2.ImRead(path, ImreadModes.Color);
        if (template is null || template.Empty())
        {
            status.Text = "Lipsește assets/target.png";
            start.Enabled = false;
        }
        timer.Tick += (_, _) => Scan();
        timer.Start();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F8) { Toggle(); return true; }
        if (keyData == Keys.F9) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Toggle()
    {
        running = !running;
        status.Text = running ? "Activ – caut obiectul" : "Oprit";
    }

    private void Scan()
    {
        if (!running || busy || template is null) return;
        busy = true;
        try
        {
            var bounds = SystemInformation.VirtualScreen;
            using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap)) g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bitmap.Size);
            using var screenshot = BitmapConverter.ToMat(bitmap);
            using var screenshotGray = new Mat();
            using var templateGray = new Mat();
            Cv2.CvtColor(screenshot, screenshotGray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(template, templateGray, ColorConversionCodes.BGR2GRAY);
            double best = -1;
            System.Drawing.Point bestPoint = default;
            // Multi-scale matching: 55% to 145% of the reference image.
            foreach (var scale in new[] { 0.55, 0.7, 0.85, 1.0, 1.15, 1.3, 1.45 })
            {
                using var resized = new Mat();
                Cv2.Resize(templateGray, resized, OpenCvSharp.Size.Zero, scale, scale);
                if (resized.Width > screenshotGray.Width || resized.Height > screenshotGray.Height || resized.Width < 8 || resized.Height < 8) continue;
                using var result = new Mat();
                Cv2.MatchTemplate(screenshotGray, resized, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out _, out double max, out _, out OpenCvSharp.Point where);
                if (max > best)
                {
                    best = max;
                    bestPoint = new System.Drawing.Point(bounds.Left + where.X + resized.Width / 2, bounds.Top + where.Y + resized.Height / 2);
                }
            }
            status.Text = $"Activ – potrivire: {best:P0}";
            if (best >= (double)threshold.Value / 100 && (DateTime.UtcNow - lastClick).TotalMilliseconds >= (double)delay.Value)
            {
                Native.SetCursorPos(bestPoint.X, bestPoint.Y);
                Native.mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
                Native.mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
                lastClick = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            running = false;
            status.Text = "Eroare: " + ex.Message;
        }
        finally { busy = false; }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Dispose();
        template?.Dispose();
        base.OnFormClosed(e);
    }
}

internal static class Native
{
    [DllImport("user32.dll")] internal static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] internal static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);
}
