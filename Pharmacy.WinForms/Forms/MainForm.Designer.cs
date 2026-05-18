#nullable enable
using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components;
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
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PharmaCare — لوحة التحكم";

        sidebar = new SidebarControl();

        mainShell = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Padding = new Padding(0)
        };

        topBar = new TopBarControl();

        contentHost = new Panel
        {
            BackColor = PharmaTheme.Background,
            Dock = DockStyle.Fill,
            Padding = new Padding(4, 4, 8, 12)
        };

        mainShell.Controls.Add(contentHost);
        mainShell.Controls.Add(topBar);

        Controls.Add(mainShell);
        Controls.Add(sidebar);

        ResumeLayout(false);
    }
}
