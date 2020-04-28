using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection; 


namespace Support
{
    //code double checked 2/24/2020 
    public class Copyer
    {
        #region Constructor
        public Copyer(ApplicationType AppType)
        {
            logger = new Logger(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), AppType);
            fileCount = 0;
            totalCopySize = 0;
            netCopySize = 0;
            stopWatchTime = 0; 
        }
        #endregion

        #region Properties
        private int fileCount;
        private double totalCopySize;
        private double netCopySize;
        private double stopWatchTime; 
        private readonly Logger logger;
        #endregion

        #region Private Methods
        private string[] GetAllFiles(string Path)
        {
            return Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories);
        }
        #endregion

        #region Public Methods
        public string BuildDestinationSubFolderPath(string SourcePath, string DestinationPath)
        {
            DirectoryInfo SourceFolder = new DirectoryInfo(SourcePath);
            return DestinationPath + @"\" + SourceFolder.Name;
        }
        public int FindNumberOfFiles(string SourcePath)
        {
            if (!Support.CheckPath(SourcePath, PathType.Directory)) throw new ArgumentException("Source path does not exist.");
            try
            {
                string[] SourceFiles = GetAllFiles(SourcePath);
                return SourceFiles.Length; 
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public Dictionary<string, double> CalculateSourceSize(string SourcePath)
        {
            if (!Support.CheckPath(SourcePath, PathType.Directory)) throw new ArgumentException("File path does not exist.");
            try
            {
                string[] SourceFiles = GetAllFiles(SourcePath); 
                long Size = 0;
                foreach (string SourceFile in SourceFiles)
                {
                    FileInfo SourceFileInfo = new FileInfo(SourceFile);
                    Size += SourceFileInfo.Length; 
                }
                return Support.ByteConverter(Size);
            }
            catch (Exception Ex)
            {
                throw Ex; 
            }
        }
        public Dictionary<string,double> CalculateSizeDifference(string SourcePath, string DestinationPath)
        {
            long SizeDifference=0; 

            if (!Support.CheckPath(SourcePath, PathType.Directory))
            {
                throw new ArgumentException("The source path does not exist.");
            }

            string DestinationSubFolderPath = BuildDestinationSubFolderPath(SourcePath, DestinationPath);

            if (!Support.CheckPath(DestinationSubFolderPath, PathType.Directory)) return CalculateSourceSize(SourcePath);
            else
            {
                try
                {
                    string[] SourceFiles = GetAllFiles(SourcePath);
                    string[] DestinationFiles = GetAllFiles(DestinationSubFolderPath);

                    foreach (string SourceFile in SourceFiles)
                    {
                        FileInfo SourceFileInfo = new FileInfo(SourceFile);
                        string RootSourceFilePath = SourceFile.Substring(SourcePath.Length);
                        if (File.Exists(DestinationSubFolderPath + RootSourceFilePath))
                        {
                            FileInfo DestinationFileInfo = new FileInfo(DestinationSubFolderPath + RootSourceFilePath);
                            if (SourceFileInfo.LastWriteTime > DestinationFileInfo.LastWriteTime)
                            {
                                SizeDifference += SourceFileInfo.Length - DestinationFileInfo.Length;
                            }
                        }
                        else
                        {
                            SizeDifference += SourceFileInfo.Length;
                        }
                    }

                    return Support.ByteConverter(SizeDifference);
                }
                catch (Exception EX)
                {
                    throw EX; 
                }
               
            }
        }
        public Dictionary<string, double> FindRemainingDiskSpaceOnDestination(string DestinationPath)
        {
            DirectoryInfo DestinationFolder = new DirectoryInfo(DestinationPath);
            DriveInfo DestinationDrive = new DriveInfo(Path.GetPathRoot(DestinationFolder.FullName));
            return Support.ByteConverter(DestinationDrive.AvailableFreeSpace); 
        }
        public Dictionary<string, double> Backup(string SourcePath, string DestinationPath, bool Recurse = false) //Need to add error handling 
        {
            if (SourcePath == DestinationPath)
            {
                throw new ArgumentException("Source and Destination can't be the same.");
            }
            Stopwatch StopWatch = new Stopwatch();
            StopWatch.Reset();
            StopWatch.Start();
            try
            {
                if (!Support.CheckPath(SourcePath, PathType.Directory))
                {
                    logger.WriteToLogFile("Source Folder does not exist", Severity.Error);
                    throw new ArgumentException("Source Folder does not exist");
                }
            }
            catch
            {
                throw; 
            }
            try
            {
                if (!Support.CheckPath(DestinationPath, PathType.Directory))
                {
                    try
                    {
                        Support.CreateFolder(DestinationPath);
                        logger.WriteToLogFile("Created Destination Folder: " + DestinationPath, Severity.Info);
                    }
                    catch
                    {
                        logger.WriteToLogFile("Unable to create destination folder. Issue may be a typo in path, or improper permissions", Severity.Error);
                        throw;
                    }
                }
            }
            catch
            {
                throw; 
            }
           
            string DestinationSubFolderPath = BuildDestinationSubFolderPath(SourcePath, DestinationPath);

            try
            {
                if (!Support.CheckPath(DestinationSubFolderPath, PathType.Directory))
                {
                    try
                    {
                        Support.CreateFolder(DestinationSubFolderPath);
                        logger.WriteToLogFile("Created Destination Sub-Folder: " + DestinationSubFolderPath, Severity.Info);
                    }
                    catch
                    {
                        logger.WriteToLogFile("Unable to create destination source subfolder: " + DestinationSubFolderPath + " . Issue may be a typo in path, or improper permissions", Severity.Error);
                        throw;
                    }
                }
            }
            catch
            {
                throw; 
            }

            DirectoryInfo SourceDirectory = new DirectoryInfo(SourcePath);
            FileInfo[] SourceFiles = SourceDirectory.GetFiles();

            try
            {
                foreach (FileInfo SourceFile in SourceFiles)
                {
                    string DestinationFilePath = Path.Combine(DestinationSubFolderPath, SourceFile.Name);

                    if (File.Exists(DestinationFilePath))
                    {
                        FileInfo ExistingFileInDestination = new FileInfo(DestinationFilePath);

                        if (ExistingFileInDestination.LastWriteTime < SourceFile.LastWriteTime)
                        {
                            SourceFile.CopyTo(DestinationFilePath, true);
                            fileCount++;
                            totalCopySize += SourceFile.Length;
                            netCopySize += SourceFile.Length - ExistingFileInDestination.Length;
                        }
                    }
                    else
                    {
                        SourceFile.CopyTo(DestinationFilePath, true);
                        fileCount++;
                        totalCopySize += SourceFile.Length;
                        netCopySize += SourceFile.Length;
                    }
                }
            }
            catch (Exception EX)
            {
                logger.WriteToLogFile("There was an issue with the backup between: " + SourcePath + " and " + DestinationPath + ". The Exception thrown is: " + EX.Message, Severity.Critical);
            }
            stopWatchTime += StopWatch.ElapsedMilliseconds;
            try
            {
                DirectoryInfo[] ChildSourceDirectories = SourceDirectory.GetDirectories();

                foreach (DirectoryInfo ChildSourceDirectory in ChildSourceDirectories)
                {
                    Backup(ChildSourceDirectory.FullName, DestinationSubFolderPath, true);
                }
            }
            catch (Exception EX)
            {
                logger.WriteToLogFile("There was an issue getting the sub directories of: " + SourceDirectory.FullName + ". The Exception thrown is: "  + EX.Message, Severity.Critical);
                throw EX;
            }

            if(Recurse == false)
            {
                if (fileCount == 0)
                {
                    logger.WriteToLogFile("Successful check, all files are in sync between: " + SourcePath + " and " + DestinationPath, Severity.Info);
                    return Support.TimeConverter(stopWatchTime);
                }
                else
                {
                    string LogMsg = "Succesfully copied:" + fileCount.ToString() + " files from: " + SourcePath + " to: " + DestinationSubFolderPath + " for a Total Size: " + totalCopySize.ToString() + " and Net Size: " + netCopySize.ToString() + " In: " + stopWatchTime.ToString() + "ms";
                    logger.WriteToLogFile(LogMsg, Severity.Info);
                    fileCount = 0;
                    totalCopySize = 0;
                    netCopySize = 0;
                    return Support.TimeConverter(stopWatchTime); 
                }

            }

            return Support.TimeConverter(stopWatchTime);
        }
        #endregion
    }
}
