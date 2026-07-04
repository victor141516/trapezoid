using System.Drawing.Drawing2D;

namespace Trapezoid;

internal sealed class ActionGlyph : Control
{
    private readonly WindowActionDefinition _definition;

    public ActionGlyph(WindowActionDefinition definition)
    {
        _definition = definition;
        Size = new Size(34, 24);
        MinimumSize = Size;
        MaximumSize = Size;
        DoubleBuffered = true;
        Margin = new Padding(0, 3, 8, 3);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var frame = new Rectangle(2, 3, Width - 5, Height - 7);
        using var framePen = new Pen(Color.FromArgb(140, 140, 140), 1.2f);
        using var fillBrush = new SolidBrush(Color.FromArgb(33, 150, 243));

        graphics.DrawRectangle(framePen, frame);
        if (_definition.Preview is { } preview)
        {
            var rect = new Rectangle(
                frame.Left + (int)Math.Round(frame.Width * preview.X),
                frame.Top + (int)Math.Round(frame.Height * preview.Y),
                Math.Max(2, (int)Math.Round(frame.Width * preview.Width)),
                Math.Max(2, (int)Math.Round(frame.Height * preview.Height)));
            graphics.FillRectangle(fillBrush, rect);
            return;
        }

        DrawDynamicGlyph(graphics, frame, fillBrush, framePen);
    }

    private void DrawDynamicGlyph(Graphics graphics, Rectangle frame, Brush brush, Pen pen)
    {
        var center = new Point(frame.Left + frame.Width / 2, frame.Top + frame.Height / 2);
        switch (_definition.Id)
        {
            case WindowActionId.PreviousDisplay:
            case WindowActionId.NextDisplay:
                var direction = _definition.Id == WindowActionId.NextDisplay ? 1 : -1;
                graphics.FillRectangle(brush, frame.Left + 9, frame.Top + 5, 7, 7);
                graphics.DrawLine(pen, center.X - direction * 6, center.Y, center.X + direction * 6, center.Y);
                graphics.DrawLine(pen, center.X + direction * 6, center.Y, center.X + direction * 2, center.Y - 4);
                graphics.DrawLine(pen, center.X + direction * 6, center.Y, center.X + direction * 2, center.Y + 4);
                break;
            case WindowActionId.Restore:
                graphics.DrawRectangle(pen, frame.Left + 8, frame.Top + 4, 11, 8);
                graphics.DrawRectangle(pen, frame.Left + 11, frame.Top + 7, 11, 8);
                break;
            default:
                graphics.FillEllipse(brush, center.X - 4, center.Y - 4, 8, 8);
                break;
        }
    }
}
