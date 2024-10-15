using Microsoft.Win32;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Controls;
namespace CRB.Models;

public class Settings
{
    public string DcsSavedGamesPath { get; set; }
    private string SettingsFileName = "settings.json";
    private string ExpectedSubPath = @"Config\RouteToolPresets";

    public string LoadSettings()
    {

        string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);

        if (File.Exists(settingsFilePath))
        {
            // Load the settings.json file
            try
            {

                string json = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                Debug.WriteLine(settings);

                if (Directory.Exists(settings.DcsSavedGamesPath) &&
                    Directory.Exists(Path.Combine(settings.DcsSavedGamesPath, ExpectedSubPath)))
                {
                    DcsSavedGamesPath = settings.DcsSavedGamesPath;
                    // Valid path found in settings.json
                    //MessageBox.Show("Settings loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "Settings loaded successfully!";
                }
                else
                {
                    // Invalid path in settings.json
                    System.Windows.MessageBox.Show("Invalid path in settings. Please select your 'Saved Games' 'DCS' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return PromptForDcsSavedGamesFolder();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error reading settings file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return PromptForDcsSavedGamesFolder();
            }
        }
        else
        {
            System.Windows.MessageBox.Show("Combat Route Builder needs access to your 'Saved Games' 'DCS' folder.  Please select it now.  For more information, see the documentation.", "Select your 'Saved Games' 'DCS' Folder.", MessageBoxButton.OK, MessageBoxImage.Information);
            // settings.json does not exist, prompt for folder
            return PromptForDcsSavedGamesFolder();
        }
    }

    private string PromptForDcsSavedGamesFolder()
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select DCS Saved Games Folder";
            dialog.UseDescriptionForTitle = true; // Requires .NET Core 3.0+ or .NET 5.0+
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                // User canceled the dialog, close the application
                System.Windows.Application.Current.Shutdown();
                return "";
            }

            string selectedPath = dialog.SelectedPath;

            if (Directory.Exists(Path.Combine(selectedPath, ExpectedSubPath)))
            {
                SaveSettings(selectedPath);
                return "Settings saved successfully!";
                //System.Windows.MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Selected folder does not appear to be the valid 'Saved Games' 'DCS' folder. Please try again. If this error persists, check the documentation or contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return PromptForDcsSavedGamesFolder();                
            }
        }
        }
        private void SaveSettings(string dcsSavedGamesPath)
        {
            //convert settings object to json and save
        DcsSavedGamesPath = dcsSavedGamesPath;
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName), json);
    }
}