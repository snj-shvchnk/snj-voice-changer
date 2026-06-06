namespace SnjVoiceChanger;

public sealed class AudioLevelMeterControl : Control
{
    private float _level;
    private float _peakHold;

    public AudioLevelMeterControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
    }

    public float Level
    {
        get => _level;
        set
        {
            var nextLevel = Math.Clamp(value, 0, 1);
            if (Math.Abs(nextLevel - _level) < 0.002f)
            {
                return;
            }

            _level = nextLevel;
            _peakHold = Math.Max(_peakHold * 0.94f, _level);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var graphics = e.Graphics;
        graphics.Clear(Color.FromArgb(28, 30, 32));

        var meterBounds = new Rectangle(16, 12, Math.Max(12, Width - 32), Math.Max(16, Height - 30));
        using var borderPen = new Pen(Color.FromArgb(64, 68, 72));
        graphics.DrawRectangle(borderPen, meterBounds);

        var litHeight = (int)(meterBounds.Height * _level);
        var litTop = meterBounds.Bottom - litHeight;

        DrawScale(graphics, meterBounds);

        if (litHeight > 0)
        {
            using var greenBrush = new SolidBrush(Color.FromArgb(0, 226, 112));
            graphics.FillRectangle(greenBrush, meterBounds.Left + 1, litTop, meterBounds.Width - 1, litHeight);

            if (_level > 0.72f)
            {
                var hotHeight = (int)(meterBounds.Height * (_level - 0.72f));
                using var yellowBrush = new SolidBrush(Color.FromArgb(238, 211, 64));
                graphics.FillRectangle(yellowBrush, meterBounds.Left + 1, meterBounds.Bottom - litHeight, meterBounds.Width - 1, hotHeight);
            }

            if (_level > 0.9f)
            {
                var clipHeight = (int)(meterBounds.Height * (_level - 0.9f));
                using var redBrush = new SolidBrush(Color.FromArgb(237, 72, 62));
                graphics.FillRectangle(redBrush, meterBounds.Left + 1, meterBounds.Bottom - litHeight, meterBounds.Width - 1, clipHeight);
            }
        }

        var peakY = meterBounds.Bottom - (int)(meterBounds.Height * _peakHold);
        using var peakPen = new Pen(Color.WhiteSmoke, 2);
        graphics.DrawLine(peakPen, meterBounds.Left + 1, peakY, meterBounds.Right - 1, peakY);

        using var captionBrush = new SolidBrush(Color.FromArgb(0, 226, 112));
        graphics.DrawString("RMS", Font, captionBrush, 6, Height - 18);
        graphics.DrawString(ToDecibels(_level), Font, captionBrush, Math.Max(34, Width - 52), Height - 18);
    }

    private static void DrawScale(Graphics graphics, Rectangle meterBounds)
    {
        using var linePen = new Pen(Color.FromArgb(70, 74, 78));

        foreach (var value in new[] { 0.25f, 0.5f, 0.75f })
        {
            var y = meterBounds.Bottom - (int)(meterBounds.Height * value);
            graphics.DrawLine(linePen, meterBounds.Left + 1, y, meterBounds.Right - 1, y);
        }
    }

    private static string ToDecibels(float level)
    {
        if (level <= 0.0001f)
        {
            return "-inf";
        }

        var decibels = 20 * Math.Log10(level);
        return $"{decibels:0.0}";
    }
}
