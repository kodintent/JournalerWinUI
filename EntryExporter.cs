using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using Windows.Foundation.Collections;

namespace JournalerWinUI
{
    internal class EntryExporter
    {
        internal const string file_ext_text = ".txt";
        internal const string file_ext_jso = ".jso";
        internal const string file_ext_appfiles = ".jso";


        internal static async Task<bool> ExportEntries(StorageFolder journaler_directory, StorageFolder journaler_dir_subfolder)
        {
            IReadOnlyList<StorageFile> list_entries_files = await FileOps.GetStorageFileListShallowTextFiles(journaler_dir_subfolder);
            if (list_entries_files is null) { return false; }
            if (list_entries_files.Count < 1) { return false; }

            string filename_export = string.Concat(Values.FilenameExportPrefix, Values.GetNewDatetimezoneISO(), file_ext_appfiles);
            bool exported = await CombineAndSaveFilesAsync(list_entries_files, MainWindow.JournalerDirectory, filename_export); // Use MainWindow’s global

            if (exported)
            {
                foreach (var file in list_entries_files)
                {
                    await file.DeleteAsync();
                }
            }
            return exported;
        }


        // phind - generated - slightly modified
        internal static async Task<bool> CombineAndSaveFilesAsync(IReadOnlyList<StorageFile> sourceFiles, StorageFolder destinationFolder, string filename)
        {
            bool exported = false;
            try
            {
                // Step 1: Collect all lines from the source files into a single IList<string>
                List<string> allLines = new List<string>();
                foreach (var file in sourceFiles)
                {
                    var lines = await FileIO.ReadLinesAsync(file);
                    allLines.AddRange(lines);
                }

                // Step 2: Save this combined content to a new file in the specified StorageFolder
                StorageFile newFile = await destinationFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(newFile, allLines);

                // Step 3: Verify that the new file contains exactly all the lines from the source files
                var readLines = await FileIO.ReadLinesAsync(newFile);
                bool verificationResult = allLines.SequenceEqual(readLines); // instead of my long old version!

                if (verificationResult is false)
                {
                    Debug.WriteLine("Error: export verification failed.");
                    await newFile.DeleteAsync();
                }
                exported = verificationResult;

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file access errors)
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
            return exported;

        }
    }
}
