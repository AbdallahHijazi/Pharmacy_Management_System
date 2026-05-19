using System.Drawing;
using System.Drawing.Imaging;

namespace Pharmacy.WinForms.Ui;

/// <summary>
/// Sets ListView row height via SmallImageList using a validated opaque spacer bitmap.
/// </summary>
internal static class ListViewRowHeight
{
    public const int DashboardSalesRowHeight = 46;
    private const int SpacerWidth = 16;

    public static void Apply(ListView listView, int height = DashboardSalesRowHeight)
    {
        if (listView is null || listView.IsDisposed)
        {
            return;
        }

        if (height <= 0)
        {
            listView.SmallImageList?.Dispose();
            listView.SmallImageList = null;
            return;
        }

        listView.SmallImageList?.Dispose();

        var imageList = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(SpacerWidth, height),
            TransparentColor = Color.FromArgb(1, 2, 3)
        };

        var source = CreateSpacerBitmap(SpacerWidth, height);
        if (source is null)
        {
            imageList.Dispose();
            return;
        }

        try
        {
            imageList.Images.Add("row-height", source);
        }
        finally
        {
            source.Dispose();
        }

        listView.SmallImageList = imageList;
    }

    private static Bitmap? CreateSpacerBitmap(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(255, 255, 255, 255));
        return bitmap;
    }
}
