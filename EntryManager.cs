using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace JournalerWinUI
{
    internal class EntryManager
    {
        public static void BackupCurrentEntryIfHasText(TextBox datetimezone, TextBox textentry)
        {
            if (!string.IsNullOrWhiteSpace(textentry.Text))
            {
                LocalSettings.PutStringInLocalsettings(Values.KeyDatetimezone, datetimezone.Text);
                LocalSettings.PutStringInLocalsettings(Values.KeyTextentry, textentry.Text);
            }
        }

        public static void ClearEntryBackup()
        {
            LocalSettings.PutStringInLocalsettings(Values.KeyDatetimezone, "");
            LocalSettings.PutStringInLocalsettings(Values.KeyTextentry, "");
        }

        public static string CreateFilename(string datetimezonetext) => datetimezonetext.Replace(":", "-") + Values.FileExtAppFiles;

        public static string CreateJournalEntry(string datetimezonetext, string tag1text, string tag2text, string entrytext, string savedDtz)
        {
            entrytext = entrytext.Replace("\"", "\\\"").Replace("\r\n", " <br> ").Replace("\r", " <br> ").Replace("\n", " <br> ");
            tag1text = tag1text == Values.TagDefaultItem ? "" : tag1text;
            tag2text = tag2text == Values.TagDefaultItem ? "" : tag2text;

            string[] parts = datetimezonetext.Split(' ');
            string date = parts[0];
            string time = parts[1];
            string tz6c = parts[2];
            return string.Concat(
            Values.JsonStart, Values.JsonDate, date, Values.JsonTime, time, Values.JsonTz6c, tz6c,
            Values.JsonLocation, tag1text, Values.JsonInput, Values.TagEntryInput,
            Values.JsonContent, entrytext, savedDtz, //Values.JsonDevice, Values.DeviceCode,
            Values.JsonTags, tag2text, Values.JsonEnd);
        }


    }
}
