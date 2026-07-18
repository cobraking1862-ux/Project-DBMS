using System.Windows;
using System.Windows.Controls;
using UniversityEventManagement.BLL;
using UniversityEventManagement.DAL;

namespace UniversityEventManagement.UI.Views;

/// <summary>
/// Main dashboard window displaying event summary data from vw_AdminDashboard_Summary view.
/// Implements 3-tier architecture: UI -> BLL -> DAL.
/// </summary>
public partial class MainWindow : Window
{
    private readonly DashboardService _dashboardService;
    private readonly SystemAdmin _currentAdmin;

    public MainWindow(SystemAdmin admin)
    {
        InitializeComponent();
        _currentAdmin = admin;
        _dashboardService = new DashboardService();
        
        // Initialize UI with admin info
        InitializeUI();
        
        // Load dashboard data
        LoadDashboardData();
    }

    /// <summary>
    /// Initializes the UI with current admin information.
    /// </summary>
    private void InitializeUI()
    {
        txtUserName.Text = _currentAdmin.FullName;
        txtWelcomeMessage.Text = $"Welcome back, {_currentAdmin.Username}";
    }

    /// <summary>
    /// Loads dashboard data from the vw_AdminDashboard_Summary view via BLL.
    /// </summary>
    private async void LoadDashboardData()
    {
        try
        {
            ShowLoading();
            
            // Simulate async operation for better UX
            await Task.Delay(100);
            
            var summaries = _dashboardService.GetDashboardSummary();
            
            if (summaries.Count == 0)
            {
                ShowEmptyState();
            }
            else
            {
                dgDashboard.ItemsSource = summaries;
                txtRecordCount.Text = $"({summaries.Count} records)";
                ShowDataGrid();
            }
        }
        catch (BusinessException ex)
        {
            ShowError($"Failed to load dashboard: {ex.Message}");
        }
        catch (Exception ex)
        {
            ShowError($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads filtered dashboard data based on status selection.
    /// </summary>
    private void LoadFilteredData(string? status)
    {
        try
        {
            ShowLoading();

            List<DashboardSummary> summaries;
            
            if (string.IsNullOrEmpty(status) || status == "All Events")
            {
                summaries = _dashboardService.GetDashboardSummary();
            }
            else
            {
                summaries = _dashboardService.GetDashboardSummaryByStatus(status);
            }

            if (summaries.Count == 0)
            {
                ShowEmptyState();
            }
            else
            {
                dgDashboard.ItemsSource = summaries;
                txtRecordCount.Text = $"({summaries.Count} records)";
                ShowDataGrid();
            }
        }
        catch (BusinessException ex)
        {
            ShowError($"Failed to filter data: {ex.Message}");
        }
        catch (Exception ex)
        {
            ShowError($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows the loading panel.
    /// </summary>
    private void ShowLoading()
    {
        pnlLoading.Visibility = Visibility.Visible;
        pnlError.Visibility = Visibility.Collapsed;
        pnlEmpty.Visibility = Visibility.Collapsed;
        dgDashboard.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Shows the DataGrid with data.
    /// </summary>
    private void ShowDataGrid()
    {
        pnlLoading.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Collapsed;
        pnlEmpty.Visibility = Visibility.Collapsed;
        dgDashboard.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Shows the empty state panel.
    /// </summary>
    private void ShowEmptyState()
    {
        pnlLoading.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Collapsed;
        pnlEmpty.Visibility = Visibility.Visible;
        dgDashboard.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Shows the error panel with a message.
    /// </summary>
    private void ShowError(string message)
    {
        pnlLoading.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Visible;
        pnlEmpty.Visibility = Visibility.Collapsed;
        dgDashboard.Visibility = Visibility.Collapsed;
        txtErrorMessage.Text = message;
    }

    /// <summary>
    /// Handles the logout button click.
    /// </summary>
    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Confirm Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }

    /// <summary>
    /// Handles the refresh button click.
    /// </summary>
    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadDashboardData();
    }

    /// <summary>
    /// Handles the status filter selection change.
    /// </summary>
    private void CboStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cboStatusFilter.SelectedItem is ComboBoxItem selectedItem)
        {
            string? status = selectedItem.Content?.ToString();
            LoadFilteredData(status);
        }
    }
}
