namespace Pharmacy.WinForms.Ui;

internal static class UiFeedback
{
    public static void ShowFeatureNotAvailable(IWin32Window? owner, string featureName)
    {
        MessageBox.Show(
            owner,
            $"هذه الميزة غير مفعلة بعد.\n\n{featureName}",
            "غير متاح",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void ShowPageNotAvailable(IWin32Window? owner, string pageName)
    {
        MessageBox.Show(
            owner,
            $"صفحة {pageName} غير مفعلة بعد.",
            "غير متاح",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void ShowSuccess(IWin32Window? owner, string message, string title = "تم")
    {
        MessageBox.Show(
            owner,
            message,
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void ShowError(IWin32Window? owner, string message, string title = "خطأ")
    {
        MessageBox.Show(
            owner,
            message,
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
