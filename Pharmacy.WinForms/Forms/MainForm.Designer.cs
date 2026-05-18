#nullable enable
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components;
    private TableLayoutPanel shellLayout = null!;
    private SidebarControl sidebar = null!;
    private Panel mainShell = null!;
    private TopBarControl topBar = null!;
    private Panel contentHost = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = PharmaTheme.SoftGreenBackground;
        ClientSize = new Size(1280, 800);
        DoubleBuffered = true;
        Font = PharmaTheme.BodyFont;
        MinimumSize = new Size(1024, 680);
        Name = "MainForm";
        RightToLeft = RightToLeft.Yes;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PharmaCare — لوحة التحكم";

        shellLayout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        sidebar = new SidebarControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };

        mainShell = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        topBar = new TopBarControl
        {
            Dock = DockStyle.Top
        };

        contentHost = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 4, 12, 12)
        };

        mainShell.Controls.Add(contentHost);
        mainShell.Controls.Add(topBar);

        shellLayout.Controls.Add(sidebar, 0, 0);
        shellLayout.Controls.Add(mainShell, 1, 0);

        Controls.Add(shellLayout);

        ResumeLayout(false);
    }
}
