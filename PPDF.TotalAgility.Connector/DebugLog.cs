using System;
using System.IO;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// Writes diagnostics to C:\Temp\TAConnectorDebug.txt.
    ///
    /// Never write secrets here: no passwords, session ids or user names.
    /// Creates the folder when needed and never throws, so a missing folder
    /// or a locked file cannot break the connector.
    /// </summary>
    internal static class DebugLog
    {
        private const string Folder = @"C:\Temp";
        private const string FilePath = @"C:\Temp\TAConnectorDebug.txt";

        public static void Write(string source, string message)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                File.AppendAllText(FilePath,
                    DateTime.Now + " | " + source + " | " + message + "\r\n");
            }
            catch { }
        }
    }
}
