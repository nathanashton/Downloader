using System;
using System.IO;

namespace Download
{

    public static class Log
    {
        
        public static StreamWriter LogFile;
        public static StreamWriter ExceptionLogFile;
        public static StreamWriter EmailLog;
        public static DateTime EmailLogDate;
        public enum Level
        {
            Errors,
            Info,
            All
        }
        
        public static Level level;
        
        
        static string Now()
        {
            return DateTime.Now.ToString();
        }
        
        
        static void Open()
        {
            LogFile = new StreamWriter("Downloader.log", true);
        }
        
        static void OpenExceptionLog()
        {
            ExceptionLogFile = new StreamWriter("Downloader_Exception.log", true);
        }
        
        
        static void Close()
        {
            if (LogFile != null)
            {
                LogFile.Close();
            }
        }
        
        
        
        static void CloseExceptionLog()
        {
            if (ExceptionLogFile != null)
            {
                ExceptionLogFile.Close();
            }
        }
        
        
        public static void Exception(Exception in_exception)
        {
                OpenExceptionLog();
                ExceptionLogFile.WriteLine(Now() + ": EXCEPTION: " + in_exception.ToString());
                CloseExceptionLog();            
        }
        
        
        public static void Error(string in_message, Exception in_exception)
        {
            if (level == Level.Errors || level == Level.All)
            {            
                Open();
                LogFile.WriteLine(Now() + ": ERROR: " + in_message + ": " + in_exception.Message);
                Close();
            }
        }
        
        
        public static void Error(string in_message)
        {
            if (level == Level.Errors || level == Level.All)
            {
                Open();
                LogFile.WriteLine(Now() + ": ERROR: " + in_message);
                Close();
            }
        }
        
        
        public static void Info(string in_message)
        {
            if (level == Level.Info || level == Level.All)
            {
                Open();
                LogFile.WriteLine(Now() + ": INFO: " + in_message);
                Close();
            }
        }
        
        
        public static void Info(string in_message, Exception in_exception)
        {
            if (level == Level.Info || level == Level.All)
            {
                Open();
                LogFile.WriteLine(Now() + ": INFO: " + in_message + ": " + in_exception.Message);
                Close();
            }
        }
    }
}
