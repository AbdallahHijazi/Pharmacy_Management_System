using Pharmacy.WinForms.Controls;

namespace Pharmacy.WinForms.Ui;

/// <summary>Applies the active theme to standard WinForms controls recursively.</summary>
internal static class ThemeApplier
{
    public static void ApplyThemeRecursive(Control? root)
    {
        if (root is null || root.IsDisposed)
        {
            return;
        }

        ApplyControl(root);

        foreach (Control child in root.Controls)
        {
            ApplyThemeRecursive(child);
        }
    }

    private static void ApplyControl(Control control)
    {
        switch (control)
        {
            case RoundedFieldBox field:
                field.ApplyThemeVisuals();
                return;

            case SegmentChipButton or FontSizeSegmentButton:
                control.Invalidate();
                return;

            case RoundedNeutralButton neutral:
                neutral.RefreshThemeVisuals();
                return;

            case RoundedPrimaryOutlineButton outline:
                outline.RefreshThemeVisuals();
                return;

            case RoundedIconButton icon:
                icon.RefreshThemeVisuals();
                return;

            case GradientRoundedButton gradient:
                gradient.ForeColor = PharmaTheme.OnPrimary;
                gradient.Invalidate();
                return;

            case Label label:
                ApplyLabel(label);
                return;

            case TextBox textBox:
                textBox.BackColor = PharmaTheme.InputSurface;
                textBox.ForeColor = PharmaTheme.TextDark;
                textBox.Font = PharmaTheme.BodyFont;
                return;

            case Panel panel:
                if (IsSettingsCardPanel(panel))
                {
                    panel.Invalidate(true);
                    return;
                }

                if (ShouldApplyPanelBackColor(panel))
                {
                    panel.BackColor = ResolvePanelBackColor(panel);
                }

                return;
        }
    }

    private static void ApplyLabel(Label label)
    {
        if (IsSettingsCardPanel(label.Parent as Panel))
        {
            return;
        }

        if (label.Font.Name.Contains("MDL2", StringComparison.OrdinalIgnoreCase))
        {
            label.ForeColor = PharmaTheme.PrimaryGreen;
            return;
        }

        var bold = label.Font.Style.HasFlag(FontStyle.Bold);
        label.Font = bold
            ? PharmaTheme.ArabicFont(label.Font.Size, FontStyle.Bold)
            : PharmaTheme.BodyFont;

        label.ForeColor = label.Tag is string tag && tag.StartsWith("cap:", StringComparison.Ordinal)
            ? PharmaTheme.OnSurfaceVariant
            : PharmaTheme.TextDark;
    }

    private static bool IsSettingsCardPanel(Panel? panel) =>
        panel is not null && string.Equals(panel.GetType().Name, "SettingsCardPanel", StringComparison.Ordinal);

    private static bool ShouldApplyPanelBackColor(Panel panel) =>
        panel.Name is not "mainShell" and not "shellLayout" and not "contentHost"
        && !IsSettingsCardPanel(panel);

    private static Color ResolvePanelBackColor(Control panel)
    {
        if (panel.Parent?.BackColor == PharmaTheme.SidebarLightBackground)
        {
            return PharmaTheme.SidebarLightBackground;
        }

        if (panel.Parent?.BackColor == PharmaTheme.SidebarBackground)
        {
            return PharmaTheme.SidebarBackground;
        }

        return PharmaTheme.Background;
    }
}
