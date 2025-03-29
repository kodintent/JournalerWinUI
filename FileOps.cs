using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

namespace JournalerWinUI
{
    internal class FileOps
    {
        internal const string file_ext_text = ".txt";
        internal const string file_ext_jso = ".jso";
        internal const string file_ext_appfiles = ".jso";

        // FILE LIST

        internal static async Task<IReadOnlyList<StorageFile>> GetStorageFileListShallowTextFiles(StorageFolder storageFolder)
        {
            if (storageFolder != null)
            {
                //Debug.WriteLine("folderSelectedIsOK");
                QueryOptions queryOptions = new QueryOptions();
                queryOptions.FolderDepth = FolderDepth.Shallow;
                queryOptions.FileTypeFilter.Add(file_ext_appfiles);
                StorageFileQueryResult queryResult = storageFolder.CreateFileQueryWithOptions(queryOptions);
                IReadOnlyList<StorageFile> storagefileList = await queryResult.GetFilesAsync();
                if (storagefileList.Count > 0)
                {
                    return storagefileList;
                }
            }
            return null;
        }


        // READ FILES

        internal static async Task<IList<string>> RetrieveLinesListFromStorageFileTextUtf8(StorageFile storageFile)
        {
            try
            {
                return await FileIO.ReadLinesAsync(storageFile, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // WRITE FILES

        internal static async Task<StorageFile> WriteStringAndReturnNewStorageFileTextUtf8(
            StorageFolder storageFolder, string filename, string text, bool replace)
        {
            CreationCollisionOption creationCollisionOption = (replace) ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName;
            try
            {
                // null ref exception after click + button - storgefolder null
                StorageFile newFile = await storageFolder.CreateFileAsync(filename, creationCollisionOption);
                await FileIO.WriteTextAsync(newFile, text, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                return newFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static async Task<StorageFile> WriteLinesAndReturnNewStorageFileTextUtf8(
            StorageFolder storageFolder, string filename, List<string> text_lines, bool replace)
        {
            CreationCollisionOption creationCollisionOption = (replace) ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName;
            try
            {
                StorageFile newFile = await storageFolder.CreateFileAsync(filename, creationCollisionOption);
                await FileIO.WriteLinesAsync(newFile, text_lines, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                return newFile;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // PICKERS

        internal static async Task<StorageFolder> GetUserPickedStorageFolder(Window window, string button_commit_text)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.CommitButtonText = button_commit_text;
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            // Initialize the picker with the window handle
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();
            return storageFolder;
        }


        internal static async Task<int> CountTextFileEntries(StorageFolder storageFolder)
        {
            try
            {
                Debug.WriteLine($"CountTextFileEntries - folder: {storageFolder.Path}");
                QueryOptions queryOptions = new QueryOptions();
                queryOptions.FolderDepth = FolderDepth.Shallow;
                queryOptions.FileTypeFilter.Add(file_ext_appfiles);
                StorageFileQueryResult queryResult = storageFolder.CreateFileQueryWithOptions(queryOptions);
                Debug.WriteLine("CountTextFileEntries - query created");
                IReadOnlyList<StorageFile> storagefileList = await queryResult.GetFilesAsync();
                Debug.WriteLine($"CountTextFileEntries - files found: {storagefileList.Count}");
                return storagefileList.Count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CountTextFileEntries failed: {ex.Message} - Stack: {ex.StackTrace}");
                return 0;
            }
        }

    }
}
