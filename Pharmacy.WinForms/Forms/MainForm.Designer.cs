#nullable enable
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components;

    /// <summary>Root grid: column 0 = Sidebar (RTL right), column 1 = main content stack.</summary>
    private TableLayoutPanel shellLayout = null!;

    /// <summary>Single shared sidebar instance for the whole session.</summary>
    private SidebarControl sidebar = null!;

    /// <summary>Hosts TopBar + contentHost; never removed on navigation.</summary>
    private Panel mainShell = null!;

    /// <summary>Single shared top bar inside main shell (not over sidebar).</summary>
    private TopBarControl topBar = null!;

    /// <summary>Only this panel's child is swapped when navigating.</summary>
    private Panel contentHost = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = PharmaTheme.Background;
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
            Name = "shellLayout",
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes,
            RowCount = 1
        };
        shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AppShellLayout.SidebarColumnWidth));
        shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        sidebar = new SidebarControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Name = "sidebar"
        };

        mainShell = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Name = "mainShell",
            Padding = new Padding(0)
        };

        topBar = new TopBarControl
        {
            Dock = DockStyle.Top,
            Name = "topBar"
        };

        contentHost = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Name = "contentHost",
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
