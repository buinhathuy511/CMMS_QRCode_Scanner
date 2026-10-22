namespace CMMS_QRCode_Scanner.Features.ExportOutbound;

public partial class ExportSetupPage : ContentPage
{
    private readonly ExportSetupViewModel _viewModel;

    public ExportSetupPage(ExportSetupViewModel viewModel)
    {
        _viewModel = viewModel;
        try
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportSetupPage] Constructor error: {ex}");
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 20,
                    Spacing = 12,
                    Children =
                    {
                        new Label { Text = "⚠️ Lỗi khởi tạo màn hình Xuất Kho", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.Red },
                        new Label { Text = ex.Message, FontSize = 14, FontAttributes = FontAttributes.Bold },
                        new Editor { Text = ex.ToString(), IsReadOnly = true, HeightRequest = 450, BackgroundColor = Colors.LightGray }
                    }
                }
            };
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Load movement types on first appearance
            if (_viewModel != null && _viewModel.MovementTypes.Count == 0)
            {
                await _viewModel.InitializeAsync();
            }

            // Update empty state visibility
            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportSetupPage] Error in OnAppearing: {ex}");
            await DisplayAlert("Lỗi OnAppearing", ex.Message, "OK");
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        try
        {
            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportSetupPage] Error in OnNavigatedTo: {ex}");
        }
    }

    private void UpdateEmptyState()
    {
        if (_viewModel == null) return;
        if (emptyState != null)
            emptyState.IsVisible = _viewModel.ExportItems.Count == 0;
        if (itemsList != null)
            itemsList.IsVisible = _viewModel.ExportItems.Count > 0;
    }
}
