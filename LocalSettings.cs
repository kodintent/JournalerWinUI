using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.Storage;

namespace JournalerWinUI
{
    internal class LocalSettings
    {
        internal static string key_journaler_dir_mrutoken = "journaler_dir_mrutoken";


        // using MRU token to store a storageFolder, not a string. From old version.
        internal static async Task<bool> SetJournalerDir(Window window)
        {
            StorageFolder journaler_directory = await FileOps.GetUserPickedStorageFolder(window, Values.DialogSelectJournalerDir);
            if (journaler_directory == null) { return false; }

            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            string journaler_dir_mrutoken = mru.Add(journaler_directory);
            Debug.WriteLine(" - new journaler MRUtoken = " + journaler_dir_mrutoken);

            LocalSettings.PutStringInLocalsettings(key_journaler_dir_mrutoken, journaler_dir_mrutoken);
            MainWindow.SetJournalerDirectory(journaler_directory); // Call MainWindow’s setter
            return true;
        }
        internal static async Task<StorageFolder> GetJournalerDir()
        {
            string journaler_dir_mrutoken = LocalSettings.GetStringFromLocalsettings(key_journaler_dir_mrutoken);
            Debug.WriteLine(" - retrieved journaler MRUtoken = " + journaler_dir_mrutoken);
            if (string.IsNullOrEmpty(journaler_dir_mrutoken)) { return null; }

            try
            {
                var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
                StorageFolder folder = await mru.GetFolderAsync(journaler_dir_mrutoken);
                Debug.WriteLine($" - resolved folder: {folder.Path}");
                return folder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetJournalerDir failed: {ex.Message}");
                return null; // Graceful fallback
            }
        }

        internal static string GetStringFromLocalsettings(string key)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey(key))
            {
                string value = localSettings.Values[key] as string;
                Debug.WriteLine("  local settings retrieved ... ", value);
                if (value == null)
                {
                    return "";
                }
                return value;
            }
            return "";
        }
        internal static void PutStringInLocalsettings(string key, string value)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }

        // List<string> must be serialized to store in the settings.
        internal static List<string> GetStringListFromLocalsettings(string key)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey(key))
            {
                string joinedList = localSettings.Values[key] as string;
                Debug.WriteLine("  local settings retrieved ... ", joinedList);
                if (joinedList == null)
                {
                    return new List<string>();
                }
                // deserialize it
                string[] string_array = joinedList.Split("_*##*_");
                return string_array.ToList();

            }
            return new List<string>();
        }
        internal static void PutStringListInLocalsettings(string key, List<string> stringList)
        {
            //string serializedList = JsonConvert.SerializeObject(stringList);
            string joinedList = string.Join("_*##*_", stringList);

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = joinedList;
        }

    }
}
