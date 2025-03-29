using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Windowing; // For AppWindow
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace JournalerWinUI
{
    public sealed partial class MainWindow : Window
    {
        private bool datetimezone_protected;
        private bool x_close_pressed;

        public static StorageFolder JournalerDirectory { get; private set; }

        public MainWindow()
        {
            Debug.WriteLine("MainWindow constructor started");
            this.InitializeComponent();

            this.Closed += MainWindow_Closed;

            AppWindow appWindow = this.AppWindow;
            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            //var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            //var appWindow = AppWindow.GetFromWindowId(windowId);

            // Set dark title bar and custom title in dark mode only
            if (appWindow != null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32(1000, 600)); // Set width and height
                //Debug.WriteLine($"Appwindow Title default is: {appWindow.Title}");
                appWindow.Title = "Journaler";

                var titleBar = appWindow.TitleBar;
                titleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;

                // Apply custom title bar colors only in dark mode
                if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    titleBar.BackgroundColor = Windows.UI.Color.FromArgb(255, 32, 32, 32); // Dark gray
                    titleBar.ForegroundColor = Windows.UI.Color.FromArgb(255, 200, 200, 200); // Light text
                    titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(255, 32, 32, 32); // Match buttons
                    titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(255, 200, 200, 200);
                    titleBar.InactiveBackgroundColor = Windows.UI.Color.FromArgb(255, 64, 64, 64); // Slightly lighter when inactive
                    titleBar.InactiveForegroundColor = Windows.UI.Color.FromArgb(255, 200, 200, 200);
                }
            }

            datetimezone_protected = false;
            x_close_pressed = false;
            datetimezone.KeyUp += Datetimezone_KeyUp;
            textentry.GotFocus += Textentry_GotFocus;

            JournalerDirectory = Task.Run(() => LocalSettings.GetJournalerDir()).Result;
            Debug.WriteLine($"JournalerDirectory set to: {JournalerDirectory?.Path ?? "null"}");

            if (JournalerDirectory == null)
            {
                Debug.WriteLine("JournalerDirectory is null");
                SetUiActive(false);
                setfolder.IsEnabled = true;
                export.Content = "?";
            }
            else
            {
                Debug.WriteLine($"JournalerDirectory: {JournalerDirectory.Path}");
                textentry.Loaded += (s, e) =>
                {
                    LoadComboBoxItems(combobox1, Values.KeyLocationItems, Values.KeyLocationSelected);
                    LoadComboBoxItems(combobox2, Values.KeyTagItems, Values.KeyTagSelected);
                    LoadEntryBackupIfExists();
                    CreateNewEntryIfEmpty();
                    _ = UpdateSavedFileCountAsync();
                };
            }

            Debug.WriteLine("MainWindow constructor completed");
        }


        // ##################################################
        // ### OTHER FUNCTIONS
        // ##################################################

        public async Task UpdateSavedFileCountAsync()
        {
            Debug.WriteLine("UpdateSavedFileCountAsync started");
            StorageFolder journalerDirSubfolder = await GetJournalerDirSubfolder();
            if (journalerDirSubfolder == null)
            {
                Debug.WriteLine("journalerDirSubfolder is null");
                export.Content = "?";
            }
            else
            {
                Debug.WriteLine($"journalerDirSubfolder: {journalerDirSubfolder.Path}");
                int count = await FileOps.CountTextFileEntries(journalerDirSubfolder);
                Debug.WriteLine($"File count: {count}");
                export.Content = count;
            }
            Debug.WriteLine("UpdateSavedFileCountAsync completed");
        }

        internal static void SetJournalerDirectory(StorageFolder folder)
        {
            JournalerDirectory = folder;
            Debug.WriteLine($"JournalerDirectory updated to: {folder?.Path ?? "null"}");
        }

        private void BackupCurrentEntry()
        {
            Debug.WriteLine("BackupCurrentEntry started");
            if (!string.IsNullOrEmpty(textentry.Text.Trim()))
            {
                LocalSettings.PutStringInLocalsettings(Values.KeyDatetimezone, datetimezone.Text);
                LocalSettings.PutStringInLocalsettings(Values.KeyTextentry, textentry.Text);
                Debug.WriteLine("BackupCurrentEntry completed - textentry had text so backed up");
            }
            else
            {
                Debug.WriteLine("BackupCurrentEntry completed - textentry was empty so no backup");
            }
        }

        private void CreateNewEntryIfEmpty()
        {
            if (!datetimezone_protected && string.IsNullOrWhiteSpace(textentry.Text))
            {
                Debug.WriteLine("CreateNewEntryIfEmpty started");
                datetimezone.Text = Values.GetNewDatetimezoneJournal();
                Debug.WriteLine("Set datetimezone.Text");
                textentry.Text = "";
                Debug.WriteLine("Set textentry.Text");
                textentry.Focus(FocusState.Programmatic);
                Debug.WriteLine("CreateNewEntryIfEmpty set new entry");
            }
        }

        private async Task<bool> SaveCurrentEntryIfHasText()
        {
            string entryTextTrim = textentry.Text.Trim();
            if (!string.IsNullOrEmpty(entryTextTrim))
            {
                string datetimezoneText = datetimezone.Text;
                string entryText = entryTextTrim;
                string entryFilename = EntryManager.CreateFilename(datetimezoneText);
                string spSavedDtz = $" (saved {Values.GetNewDatetimezoneISO()})";
                string locationText = combobox1.Text ?? "";
                string tagsText = combobox2.Text ?? "";
                string entryJournal = EntryManager.CreateJournalEntry(datetimezoneText, locationText, tagsText, entryText, spSavedDtz);

                StorageFolder journalerDirSubfolder = await GetJournalerDirSubfolder();
                if (journalerDirSubfolder != null)
                {
                    StorageFile fileResult = await FileOps.WriteStringAndReturnNewStorageFileTextUtf8(journalerDirSubfolder, entryFilename, entryJournal, true);
                    if (fileResult == null)
                    {
                        BackupCurrentEntry();
                        Debug.WriteLine("SaveCurrentEntryIfHasText failed - backed up");
                    }
                    else
                    {
                        textentry.Text = "";
                        datetimezone_protected = false;
                        Debug.WriteLine("SaveCurrentEntryIfHasText succeeded - cleared textentry");
                        return true;
                    }
                }
                else
                {
                    Debug.WriteLine("SaveCurrentEntryIfHasText - journalerDirSubfolder is null");
                }
            }
            return false;
        }

        private async Task<StorageFolder> GetJournalerDirSubfolder()
        {
            if (JournalerDirectory == null)
            {
                Debug.WriteLine("GetJournalerDirSubfolder - JournalerDirectory is null");
                return null;
            }
            Debug.WriteLine($"GetJournalerDirSubfolder - JournalerDirectory: {JournalerDirectory.Path}");
            StorageFolder journalerDirSubfolder = await JournalerDirectory.CreateFolderAsync(Values.FolderSavedEntries, CreationCollisionOption.OpenIfExists);
            Debug.WriteLine($"GetJournalerDirSubfolder - journalerDirSubfolder: {journalerDirSubfolder.Path}");
            return journalerDirSubfolder;
        }

        private void SetUiActive(bool isActive)
        {
            newentry.IsEnabled = isActive;
            export.IsEnabled = isActive;
            setfolder.IsEnabled = isActive;
        }

        private void LoadEntryBackupIfExists()
        {
            Debug.WriteLine("LoadEntryBackupIfExists started");
            string savedDatetimezone = LocalSettings.GetStringFromLocalsettings(Values.KeyDatetimezone);
            string savedTextentry = LocalSettings.GetStringFromLocalsettings(Values.KeyTextentry);

            if (!string.IsNullOrWhiteSpace(savedTextentry))
            {
                Debug.WriteLine("Restoring backup data");
                datetimezone.Text = savedDatetimezone;
                textentry.Text = savedTextentry;
                datetimezone_protected = true;
            }
            EntryManager.ClearEntryBackup();
            Debug.WriteLine("LoadEntryBackupIfExists completed");
        }

        private void LoadComboBoxItems(ComboBox comboBox, string itemsKey, string selectedKey)
        {
            Debug.WriteLine($"LoadComboBoxItems for {itemsKey}");
            var items = new List<string>(LocalSettings.GetStringListFromLocalsettings(itemsKey));
            comboBox.ItemsSource = items;

            string selectedItem = LocalSettings.GetStringFromLocalsettings(selectedKey) ?? "";
            if (!string.IsNullOrEmpty(selectedItem))
            {
                comboBox.Text = selectedItem;
            }

            comboBox.TextSubmitted += (s, args) =>
            {
                if (!string.IsNullOrEmpty(args.Text) && !items.Contains(args.Text))
                {
                    items.Add(args.Text);
                    LocalSettings.PutStringListInLocalsettings(itemsKey, items);
                    LocalSettings.PutStringInLocalsettings(selectedKey, args.Text);
                    comboBox.ItemsSource = new List<string>(items);
                    comboBox.Text = args.Text;
                    Debug.WriteLine($"TextSubmitted saved '{args.Text}' to {selectedKey}");
                }
            };

            comboBox.SelectionChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(comboBox.Text))
                {
                    LocalSettings.PutStringInLocalsettings(selectedKey, comboBox.Text);
                    Debug.WriteLine($"SelectionChanged saved '{comboBox.Text}' to {selectedKey}");
                }
            };
        }

        private void DeleteComboBox1Item(object sender, RoutedEventArgs e)
        {
            var items = combobox1.ItemsSource as List<string>;
            if (items != null && !string.IsNullOrEmpty(combobox1.Text) && items.Contains(combobox1.Text))
            {
                items.Remove(combobox1.Text);
                LocalSettings.PutStringListInLocalsettings(Values.KeyLocationItems, items);
                combobox1.ItemsSource = new List<string>(items);
                combobox1.Text = items.Count > 0 ? items[0] : "";
                LocalSettings.PutStringInLocalsettings(Values.KeyLocationSelected, combobox1.Text);
            }
        }

        private void DeleteComboBox2Item(object sender, RoutedEventArgs e)
        {
            var items = combobox2.ItemsSource as List<string>;
            if (items != null && !string.IsNullOrEmpty(combobox2.Text) && items.Contains(combobox2.Text))
            {
                items.Remove(combobox2.Text);
                LocalSettings.PutStringListInLocalsettings(Values.KeyTagItems, items);
                combobox2.ItemsSource = new List<string>(items);
                combobox2.Text = items.Count > 0 ? items[0] : "";
                LocalSettings.PutStringInLocalsettings(Values.KeyTagSelected, combobox2.Text);
            }
        }

        // ##################################################
        // ### USER UI EVENTS
        // ##################################################

        private async void Click_newentry(object sender, RoutedEventArgs e)
        {
            bool saved = await SaveCurrentEntryIfHasText();
            if (saved)
            {
                datetimezone_protected = false;
                await UpdateSavedFileCountAsync();
            }
            CreateNewEntryIfEmpty();
        }

        private async void Click_export(object sender, RoutedEventArgs e)
        {
            SetUiActive(false);
            StorageFolder journalerDirSubfolder = await GetJournalerDirSubfolder();
            if (journalerDirSubfolder != null)
            {
                bool exported = await EntryExporter.ExportEntries(JournalerDirectory, journalerDirSubfolder);
                if (exported)
                {
                    export.Content = await FileOps.CountTextFileEntries(journalerDirSubfolder);
                }
            }
            SetUiActive(true);
        }

        private async void Click_setfolder(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await FileOps.GetUserPickedStorageFolder(this, Values.DialogSelectJournalerDir);
            if (folder != null)
            {
                var mru = StorageApplicationPermissions.MostRecentlyUsedList;
                string token = mru.Add(folder);
                LocalSettings.PutStringInLocalsettings(LocalSettings.key_journaler_dir_mrutoken, token);
                SetJournalerDirectory(folder);
                SetUiActive(true);
            }
        }

        private async void Saveclose_Click(object sender, RoutedEventArgs e)
        {
            bool saved = await SaveCurrentEntryIfHasText();
            if (saved)
            {
                datetimezone_protected = false;
                await UpdateSavedFileCountAsync();
            }
            else
            {
                BackupCurrentEntry();
            }
            this.Close();
        }

        // ##################################################
        // ### XAML EVENTS
        // ##################################################

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            BackupCurrentEntry();
        }

        private void Datetimezone_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            datetimezone_protected = true;
        }

        private void Textentry_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Textentry_GotFocus started");
            CreateNewEntryIfEmpty();
            Debug.WriteLine("Textentry_GotFocus completed");
        }
    }
}