using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

public sealed class PlaceholderPageControl : UserControl
{
    public PlaceholderPageControl(string pageTitle)
    {
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.SoftGreenBackground;
        RightToLeft = RightToLeft.Yes;

        var panel = new Panel
        {
            Anchor = AnchorStyles.None,
            AutoSize = true,
            BackColor = PharmaTheme.CardBackground,
            Padding = new Padding(32)
        };

        var title = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.TitleFont,
            ForeColor = PharmaTheme.TextDark,
            Text = pageTitle,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var message = new Label
        {
            AutoSize = true,
            Font = PharmaTheme.BodyFont,
            ForeColor = PharmaTheme.MutedText,
            Margin = new Padding(0, 12, 0, 0),
            Text = $"صفحة {pageTitle} غير مفعلة بعد.",
            TextAlign = ContentAlignment.MiddleCenter
        };

        var stack = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        stack.Controls.Add(title);
        stack.Controls.Add(message);
        panel.Controls.Add(stack);
        Controls.Add(panel);

        Resize += (_, _) =>
        {
            panel.Left = (ClientSize.Width - panel.Width) / 2;
            panel.Top = (ClientSize.Height - panel.Height) / 2;
        };
    }

    internal void ApplyThemeVisuals()
    {
        BackColor = PharmaTheme.SoftGreenBackground;
        foreach (Control hosted in Controls)
        {
            if (hosted is not Panel panel)
            {
                continue;
            }

            panel.BackColor = PharmaTheme.CardBackground;
            RefreshPlaceholderLabels(panel);
        }

        Invalidate(true);
    }

    private static void RefreshPlaceholderLabels(Control root)
    {
        foreach (Control c in root.Controls)
        {
            if (c is Label lab)
            {
                if ((lab.Font?.Style & FontStyle.Bold) == FontStyle.Bold)
                {
                    lab.Font = PharmaTheme.TitleFont;
                    lab.ForeColor = PharmaTheme.TextDark;
                }
                else
                {
                    lab.Font = PharmaTheme.BodyFont;
                    lab.ForeColor = PharmaTheme.MutedText;
                }
            }

            RefreshPlaceholderLabels(c);
        }
    }
}
