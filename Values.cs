using System;

namespace JournalerWinUI
{
    internal class Values
    {
        // General
        public const string DialogSelectJournalerDir = "SELECT JOURNALER FOLDER";
        public const string FolderSavedEntries = "JournalerSavedEntries";
        public const string FilenameExportPrefix = "Journaler_Exported_Entries_";
        public const string TagEntryMethod = " (typ/fin)";
        public const string TagEntryInput = "typ/fin";
        public const string DeviceCode = "";
        public const string TagDefaultItem = "no tag";

        // File Extensions
        public const string FileExtText = ".txt";
        public const string FileExtJso = ".jso";
        public const string FileExtAppFiles = ".jso";

        // Local Settings Keys
        public const string KeyDatetimezone = "datetimezone";
        public const string KeyTextentry = "textentry";

        public const string KeyLocationItems = "locationItems"; // Previously KeyTag1Items
        public const string KeyLocationSelected = "locationSelected"; // Previously KeyTag1Selected
        public const string KeyTagItems = "tagItems"; // Previously KeyTag2Items
        public const string KeyTagSelected = "tagSelected"; // Previously KeyTag2Selected
        public const string KeyJournalerDirMruToken = "journaler_dir_mrutoken";

        // JSON Formatting
        public const string JsonStart = "{";
        public const string JsonDate = "\"date\": \"";
        public const string JsonTime = "\", \"time\": \"";
        public const string JsonTz6c = "\", \"tz6c\": \"";
        public const string JsonLocation = "\", \"location\": \"";
        public const string JsonInput = "\", \"input\": \"";
        public const string JsonContent = "\", \"content\": \"";
        public const string JsonDevice = "\", \"device\": \"";
        public const string JsonTags = "\", \"tags\": \"";
        public const string JsonEnd = "\"}";

        public static string GetNewDatetimezoneJournal() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
        public static string GetNewDatetimezoneFilename() => DateTime.Now.ToString("yyyyMMdd_HHmmsszzz").Replace(":", "");
        public static string GetNewDatetimezoneISO() => DateTime.Now.ToString("yyyyMMddTHHmmsszzz").Replace(":", "");

    }
}
