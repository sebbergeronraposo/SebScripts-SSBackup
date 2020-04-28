using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Support
{
    //Code double checked 2/24/2020
    public class Logger
    {
        #region Constructor
        public Logger(string exeFolderPath, ApplicationType callingApp)
        {
            if(!Support.CheckPath(exeFolderPath, PathType.Directory))
            {
                throw new System.ArgumentException("File Path specified does not exist or is not for SSBackup."); 
            }

            switch (callingApp)
            {
                case ApplicationType.Client:
                    logFolderPath = exeFolderPath + @"\Client\Log\";
                    break;
                case ApplicationType.Service:
                    logFolderPath = exeFolderPath + @"\Service\Log\";
                    break;
                case ApplicationType.AnyApp:
                    logFolderPath = exeFolderPath + @"\Log\";
                    break;
                default:
                    throw new System.ArgumentException("Could not create Logger.");

            }
            if (!Support.CheckPath(logFolderPath, PathType.Directory))
            {
                try
                {
                    Support.CreateFolder(logFolderPath);
                }
                catch
                {
                    throw; 
                }
                
            }
            try
            {
                CreateLogFile();
            }
            catch
            {
                throw; 
            }
            if (callingApp == ApplicationType.Service) //Only start background thread for services. For other apps, simply log to current day file and don't worry about creating new files or compression 
            {
                try
                {
                    this.StartLogFileCheckThread();
                }
                catch (Exception EX)
                {
                    throw EX;
                }
            }
        }
        #endregion

        #region Properties
        private readonly string logFolderPath;
        private string logFilePath;
        private string day;
        #endregion

        #region Private Methods
        private void CheckLogFileTimeSpan()
        {
            while (true)
            {
                string NewDay = DateTime.Now.ToShortDateString();
                if (NewDay != day)
                {
                    try
                    {
                        CreateLogFile();
                    }
                    catch
                    {
                        throw;
                    }
                }
                if (CheckIfFirstDayOfMonth())
                {
                    try
                    {
                        Compressor();
                    }
                    catch
                    {
                        throw;
                    }
                }
                Thread.Sleep(1000);
            }

        }
        private bool CheckIfFirstDayOfMonth()
        {
            DateTime Now = DateTime.Now;
            DateTime FirstOfMonth = new DateTime(Now.Year, Now.Month, 1);

            if (FirstOfMonth == Now.Date)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void StartLogFileCheckThread()
        {
            Thread LogThread = new Thread(new ThreadStart(this.CheckLogFileTimeSpan))
            {
                Name = "LogThread"
            };
            LogThread.Start();
        }
        private void Compressor()
        {
            DirectoryInfo LogFolder = new DirectoryInfo(logFolderPath);
            try
            {
                FileInfo[] LogFilesToCompress = LogFolder.GetFiles("*.txt");
                int PreviousMonth = (DateTime.Now).AddMonths(-1).Month;
                DateTime PrevMonth = new DateTime(DateTime.Now.Year, PreviousMonth, 01);
                string FolderNameToCompress = logFolderPath + "Log_" + PrevMonth.Month.ToString() + "-" + PrevMonth.Year.ToString();
                DirectoryInfo CompressedFolder = Directory.CreateDirectory(FolderNameToCompress);
                string CompressedFolderName = CompressedFolder.FullName.Remove(CompressedFolder.FullName.Length - 1) + ".Zip";
                foreach (FileInfo LogFileToCompress in LogFilesToCompress)
                {
                    LogFileToCompress.MoveTo(CompressedFolder.FullName + LogFileToCompress.Name);
                }

                ZipFile.CreateFromDirectory(CompressedFolder.FullName, CompressedFolderName);
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }
        private void CreateLogFile()
        {
            day = DateTime.Now.ToShortDateString().Replace("/","-");
            logFilePath = logFolderPath + "Log_" + day + "_.txt";
            if (Support.CheckPath(logFilePath, PathType.File)) return;
            try
            {
                WriteToLogFile("Log File Created", Severity.Info); 
            }
            catch
            {
                throw; 
            }
            
        }
        #endregion

        #region Internal Methods
        public void WriteToLogFile(string msg, Severity severity)
        {
            string TimeStamp = DateTime.Now.ToString();
            string LogMsg;
            switch (severity)
            {
                case Severity.Info:
                    LogMsg = TimeStamp + " |INFO| " + msg;
                    break;
                case Severity.Warning:
                    LogMsg = TimeStamp + " |WARNING| " + msg;
                    break;
                case Severity.Error:
                    LogMsg = TimeStamp + " |ERROR| " + msg;
                    break;
                case Severity.Critical:
                    LogMsg = TimeStamp + " |CRITICAL| " + msg;
                    break;
                default:
                    LogMsg = TimeStamp + " No Severity Specified " + msg;
                    break;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath,true))
                {
                    writer.WriteLine(LogMsg); 
                }
            }
            catch 
            {
                throw;
            }
        }
        #endregion
    }
}
