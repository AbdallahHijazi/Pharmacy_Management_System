using System.Drawing;

namespace Pharmacy.WinForms.Ui;

internal static class TextLayoutHelper
{
    private const string ArabicMeasureSample = "أبجد ٠١٢٣ ل.س";

    public static int LineHeight(Font font, int extraPadding = 8)
    {
        var size = TextRenderer.MeasureText(
            ArabicMeasureSample,
            font,
            new Size(int.MaxValue / 4, int.MaxValue / 4),
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        return size.Height + extraPadding;
    }

    public static Rectangle DeflateVertical(Rectangle bounds, int padding)
    {
        if (padding <= 0)
        {
            return bounds;
        }

        return new Rectangle(
            bounds.X,
            bounds.Y + padding,
            bounds.Width,
            Math.Max(1, bounds.Height - padding * 2));
    }
}
