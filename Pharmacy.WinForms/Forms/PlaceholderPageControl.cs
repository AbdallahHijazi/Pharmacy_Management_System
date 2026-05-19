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
}
