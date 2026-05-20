namespace Pharmacy.WinForms.Ui;

internal static class FontScaleManager
{
    public static event EventHandler? Changed;

    private static int _level = 2;

    public static int Level => _level;

    public static float Multiplier => _level switch
    {
        1 => 0.92f,
        3 => 1.14f,
        _ => 1f
    };

    public static void SetLevel(int level)
    {
        var next = Math.Clamp(level, 1, 3);
        if (next == _level)
        {
            return;
        }

        _level = next;
        Changed?.Invoke(null, EventArgs.Empty);
    }
}
