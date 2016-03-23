/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * EventLogger.cs
 * Static methods helping on logging operations
 */

using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class EventLogger : MonoBehaviour
{
    private static StreamWriter _fileWriter;
    private static string _fileName;

    public static void PrintToLog(object message)
    {
        if (_fileWriter == null)
        {
            string fileTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
            _fileName = Application.persistentDataPath + "/" + fileTimestamp + ".txt";
        }

        _fileWriter = new StreamWriter(_fileName, true);
            
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        _fileWriter.WriteLine(timestamp + " " + message);
        _fileWriter.Close();
    }
}