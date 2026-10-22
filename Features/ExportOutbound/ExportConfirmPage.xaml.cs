namespace CMMS_QRCode_Scanner.Features.ExportOutbound;

public partial class ExportConfirmPage : ContentPage
{
    private readonly ExportConfirmViewModel _viewModel;

    public ExportConfirmPage(ExportConfirmViewModel viewModel)
    {
        _viewModel = viewModel;
        try
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportConfirmPage] Constructor error: {ex}");
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 20,
                    Spacing = 12,
                    Children =
                    {
                        new Label { Text = "⚠️ Lỗi khởi tạo màn hình Xác nhận", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.Red },
                        new Editor { Text = ex.ToString(), IsReadOnly = true, HeightRequest = 450, BackgroundColor = Colors.LightGray }
                    }
                }
            };
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            _viewModel.Initialize();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportConfirmPage] Error in OnAppearing: {ex}");
        }
    }
}
