using System.Drawing;
using System.Windows.Forms;
using Pharmacy.WinForms.Services;

namespace Pharmacy.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly AuthService _authService;
    private bool _isLoggingIn;
    private const string DefaultLoginButtonText = "Login";

    public LoginForm() : this(AppServices.AuthService)
    {
    }

    public LoginForm(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();

        loginButton.Click += async (_, _) => await OnLoginClickedAsync();
        showPasswordCheckBox.CheckedChanged += (_, _) =>
            passwordInput.IsPassword = !showPasswordCheckBox.Checked;

        AcceptButton = loginButton;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState != FormWindowState.Minimized && ClientSize.Width > 0 && ClientSize.Height > 0)
        {
            Region = new Region(RoundedGeometry.Create(ClientRectangle, 22));
        }
    }

    private async Task OnLoginClickedAsync()
    {
        if (_isLoggingIn)
        {
            return;
        }

        ClearError();
        SetLoadingState(true);

        try
        {
            var result = await _authService.LoginAsync(emailInput.InputText, passwordInput.InputText);

            if (!result.Success)
            {
                ShowError(result.ErrorMessage ?? "تعذر تسجيل الدخول.");
                return;
            }

            passwordInput.InputText = string.Empty;

            Hide();
            using var mainForm = new MainForm(_authService);
            mainForm.ShowDialog();
            Show();
            emailInput.Focus();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.Visible = true;
    }

    private void ClearError()
    {
        errorLabel.Text = string.Empty;
        errorLabel.Visible = false;
    }

    private void SetLoadingState(bool isLoading)
    {
        _isLoggingIn = isLoading;
        loadingOverlay.Visible = isLoading;
        loginButton.Enabled = !isLoading;
        emailInput.ReadOnly = isLoading;
        passwordInput.ReadOnly = isLoading;
        showPasswordCheckBox.Enabled = !isLoading;
        loginButton.Text = isLoading ? "Signing in..." : DefaultLoginButtonText;
        UseWaitCursor = isLoading;
    }
}
