using System.Drawing.Drawing2D;

namespace Trapezoid;

internal static class IconFactory
{
    public static Icon CreateIcon()
    {
        var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (appIcon is not null)
        {
            return appIcon;
        }

        using var bitmap = new Bitmap(32, 32);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var framePen = new Pen(Color.FromArgb(45, 45, 48), 2f);
            using var fillBrush = new SolidBrush(Color.FromArgb(33, 150, 243));
            using var whitePen = new Pen(Color.White, 2f);

            graphics.DrawRoundedRectangle(framePen, new Rectangle(4, 5, 24, 21), 3);
            var points = new[]
            {
                new Point(9, 9),
                new Point(25, 9),
                new Point(21, 22),
                new Point(6, 22)
            };
            graphics.FillPolygon(fillBrush, points);
            graphics.DrawLine(whitePen, 15, 9, 12, 22);
        }

        var handle = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(handle).Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(handle);
        }
    }
}

internal static class GraphicsExtensions
{
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
    {
        using var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.DrawPath(pen, path);
    }
}
