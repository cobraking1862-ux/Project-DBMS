using System.Windows;
using UniversityEventManagement.BLL;

namespace UniversityEventManagement.UI.Views;

/// <summary>
/// Login window for administrator authentication.
/// Validates credentials against the SystemAdmins table via BLL.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly AuthService _authService;

    public LoginWindow()
    {
        InitializeComponent();
        _authService = new AuthService();
        
        // Handle Enter key for login
        txtPassword.KeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnLogin_Click(this, new RoutedEventArgs());
            }
        };
        
        txtUsername.KeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnLogin_Click(this, new RoutedEventArgs());
            }
        };
    }

    /// <summary>
    /// Handles the login button click event.
    /// Validates credentials and opens the main dashboard on success.
    /// </summary>
    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            // Clear previous error
            HideError();

            // Validate input
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password.");
                txtPassword.Focus();
                return;
            }

            // Disable button during authentication
            btnLogin.IsEnabled = false;
            btnLogin.Content = "Authenticating...";

            // Attempt login via BLL
            var admin = _authService.Login(username, password);

            if (admin != null)
            {
                // Login successful - open main dashboard
                var dashboard = new MainWindow(admin);
                dashboard.Show();
                this.Close();
            }
            else
            {
                // Login failed
                ShowError("Invalid username or password. Please try again.");
                txtPassword.Password = string.Empty;
                txtPassword.Focus();
            }
        }
        catch (BusinessException ex)
        {
            ShowError($"Authentication failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            ShowError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            // Re-enable button
            btnLogin.IsEnabled = true;
            btnLogin.Content = "Sign In";
        }
    }

    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    private void ShowError(string message)
    {
        txtErrorMessage.Text = message;
        txtErrorMessage.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the error message.
    /// </summary>
    private void HideError()
    {
        txtErrorMessage.Text = string.Empty;
        txtErrorMessage.Visibility = Visibility.Collapsed;
    }
}
