using System.Drawing;
using System.Windows.Forms;
using Pharmacy.WinForms.Services;

namespace Pharmacy.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly AuthService _authService;
    private bool _isLoggingIn;
    private const string DefaultLoginButtonText = "تسجيل الدخول";

    public LoginForm() : this(AppServices.AuthService)
    {
    }

    public LoginForm(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();

        loginButton.Click += async (_, _) => await OnLoginClickedAsync();

        AcceptButton = loginButton;
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
        loginErrorNotice.MaximumSize = new Size(loginStack.Width, 0);
        loginErrorNotice.Message = message;
        loginStack.PerformLayout();
    }

    private void ClearError()
    {
        loginErrorNotice.Message = string.Empty;
    }

    private void SetLoadingState(bool isLoading)
    {
        _isLoggingIn = isLoading;
        loadingOverlay.Visible = isLoading;
        loginButton.Enabled = !isLoading;
        emailInput.ReadOnly = isLoading;
        passwordInput.ReadOnly = isLoading;
        passwordInput.SetRevealInteractionEnabled(!isLoading);
        loginButton.Text = isLoading ? "جاري التحميل..." : DefaultLoginButtonText;
        UseWaitCursor = isLoading;
    }
}
