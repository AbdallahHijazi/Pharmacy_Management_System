using System.Drawing;

namespace Pharmacy.WinForms.Ui;

/// <summary>
/// Sets ListView row height via SmallImageList using a validated opaque spacer bitmap.
/// </summary>
internal static class ListViewRowHeight
{
    public const int DashboardSalesRowHeight = 44;
    private const int SpacerWidth = 8;

    public static void Apply(ListView listView, int height = DashboardSalesRowHeight)
    {
        if (listView is null || listView.IsDisposed)
        {
            return;
        }

        if (height <= 0)
        {
            listView.SmallImageList = null;
            return;
        }

        listView.SmallImageList?.Dispose();

        var imageList = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(SpacerWidth, height)
        };

        using var source = CreateSpacerBitmap(SpacerWidth, height);
        if (source is null)
        {
            imageList.Dispose();
            return;
        }

        using var ownedCopy = new Bitmap(source);
        imageList.Images.Add("row-height", ownedCopy);
        listView.SmallImageList = imageList;
    }

    private static Bitmap? CreateSpacerBitmap(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        return bitmap;
    }
}
