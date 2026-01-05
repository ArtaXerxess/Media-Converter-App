#if ANDROID
using Android.Content;
using Android.App;
using Android.Net;
using Uri = Android.Net.Uri;
using Android.OS;
#endif
using Microsoft.Extensions.Logging;

namespace Nana_Format_Factory;

public partial class MainPage : ContentPage
{
    private ILogger<MainPage> _logger;
    private readonly IMediaConverter _converter;
    private TaskCompletionSource<Uri> _saveFileTcs;

    public MainPage(IMediaConverter converter, ILogger<MainPage> logger)
    {
        InitializeComponent();
        _converter = converter;
        _logger = logger;
    }

    public string selectedFilePath { get; set; }

    private async Task<string> CopyToAppStorageAsync(FileResult file)
    {
        var fileName = file.FileName;
        var destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using var sourceStream = await file.OpenReadAsync();
        using var destinationStream = File.Create(destinationPath);

        await sourceStream.CopyToAsync(destinationStream);

        return destinationPath;
    }

    private async void Convert_Button_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedFilePath))
        {
            await DisplayAlert("No File", "Please select a file first.", "OK");
            return;
        }

        Convert_Button.IsEnabled = false;
        ConvertingLabel.Text = "Converting… please wait";

        string output = null;

        try
        {

            output = await _converter.ConvertAsync(selectedFilePath);

#if ANDROID
            if (!string.IsNullOrEmpty(output))
            {
                var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;

                var outputFileName = "Converted_" + Path.GetFileName(output);
                var finalPath = Path.Combine(downloadsPath, outputFileName);

                File.Copy(output, finalPath, overwrite: true);

                ConvertingLabel.Text = $"✅ Saved to Downloads/{outputFileName}";
            }
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error ❌", ex.Message, "OK");
        }
        finally
        {

            TryDeleteFile(selectedFilePath);
            TryDeleteFile(output);

            selectedFilePath = null;
            Convert_Button.IsEnabled = true;
        }
    }



//#if ANDROID
//    private Task<Uri> SaveFileAndroidAsync(string filePath)
//    {
//        _saveFileTcs = new TaskCompletionSource<Uri>();

//        var fileName = Path.GetFileName(filePath);
//        var intent = new Intent(Intent.ActionCreateDocument);
//        intent.AddCategory(Intent.CategoryOpenable);
//        intent.SetType("*/*");
//        intent.PutExtra(Intent.ExtraTitle, fileName);

//        var activity = Platform.CurrentActivity;
//        activity.StartActivityForResult(intent, 1000);

//        return _saveFileTcs.Task;
//    }
//#endif

    private void TryDeleteFile(string path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            _logger.LogError(path, "Failed to delete file at {Path}", path);
        }
    }

    private async void SelectFile_Button_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select a file to convert",
        });

        result!.FileName = result.FileName.Replace(" ", "_");

        var file_name = result.FileName;

        ConvertingLabel.Text = $"Selected File 👉 \"{file_name}\"";

        if (result is null)
            return;

        selectedFilePath = await CopyToAppStorageAsync(result);

        await DisplayAlert(
            "File Ready",
            Path.GetFileName(selectedFilePath),
            "OK"
        );
    }
}
