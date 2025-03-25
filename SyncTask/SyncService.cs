using System;
using System.IO;

namespace SyncTask
{

    /// <summary>
    /// Main class of program logic & data flow.
    /// SyncService class is responsible for the synchronization of files and 
    /// directories. 
    /// </summary>
    public class SyncService : IFileHandler
    {
        /// <summary>
        /// Reference to the view.
        /// Use ref to display messages to the user. 
        /// </summary>
        private readonly IView _view;

        /// <summary>
        /// Reference to logger. 
        /// Use class to add info to the log and write log file
        /// </summary>
        private readonly Logger _logger;


        /// <summary>
        /// Public constructor of sync service.
        /// Initializes the view and logger
        /// </summary>
        /// <param name="view">reference of view to be received</param>
        public SyncService(IView view)
        {
            // initialize view with given reference
            _view = view;

            // initialize logger with given view reference
            _logger = new Logger(_view);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destFolder"></param>
        /// <param name="interval"></param>
        /// <param name="logTextPath"></param>
        public void CloneFolder(string sourcePath, string destFolder,
            string logTextPath)
        {

            // Create destination folder directory
            Directory.CreateDirectory(destFolder);

            // Foreach file in the directory
            // Obtain and copy all files!
            foreach (string filePath in Directory.GetFiles(sourcePath))
            {
                // Try to copy file to destination
                try
                {
                    // Get file name
                    string fileName = Path.GetFileName(filePath);

                    // path of destination file
                    string destFilePath = Path.Combine(destFolder, fileName);

                    // Copy file, with file destination & overwrite it
                    // Any file will be overwritten!
                    File.Copy(filePath, destFilePath, true);

                    // Display message to user
                    LogInfo($"{fileName} copied to {destFilePath}",
                        ConsoleColor.Green);

                }

                // catch exception if file cannot be copied
                catch (IOException e)
                {
                    // Display error message
                    LogInfo($"Error copying file: {e.Message}",
                        ConsoleColor.Red);
                }
            }

            // check source
            //CheckSource(sourcePath, destFolder);

            // Copy all subdirectories recursively
            SubFolderClone(sourcePath, destFolder);

            // Remove files and directories not in the source
            RemoveClonedFile(destFolder, sourcePath);


            // Remove directories not in the source
            RemoveClonedDirectories(destFolder, sourcePath);

            // finalize all operations & write log file to given path
            _logger.WriteLogFile(logTextPath);
        }



        private void CheckSource(string sourcePath, string destFolder)
        {
            // Remove directories not in the source
            foreach (string subDirPath in Directory.GetDirectories(destFolder))
            {

                try
                {
                    // Get directory name
                    string dirName = Path.GetFileName(subDirPath);

                    // If directory does not exist in source, delete it!
                    if (!Directory.Exists(Path.Combine(sourcePath, dirName)))
                    {
                        // delete directory with given path
                        Directory.Delete(subDirPath, true);

                        // log info
                        LogInfo($"{dirName} deleted from {subDirPath}",
                            ConsoleColor.Red);
                    }
                }

                catch (IOException e)
                {
                    // log error message
                    LogInfo(
                        $"Error deleting directory {subDirPath}: {e.Message}",
                        ConsoleColor.Red);
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destFolder"></param>
        private void SubFolderClone(string sourcePath, string destFolder)
        {
            // Copy all subdirectories recursively
            foreach (string subFolderPath in Directory.GetDirectories(sourcePath))
            {
                // path of sub folder
                string folderName = Path.GetFileName(subFolderPath);

                string destSubFolderPath = Path.Combine(destFolder, folderName);

                // Create the destination subfolder
                Directory.CreateDirectory(destSubFolderPath);

                LogInfo($"{folderName} copied to {destSubFolderPath}",
                    ConsoleColor.Green);

                // Recursively call itselft until no sub folder is left
                SubFolderClone(subFolderPath, destSubFolderPath);
            }

            // Copy all files in the current directory
            foreach (string filePath in Directory.GetFiles(sourcePath))
            {
                try
                {
                    // Get the file name
                    string fileName = Path.GetFileName(filePath);

                    // Path of the destination file
                    string destFilePath = Path.Combine(destFolder, fileName);

                    // Copy the file, overwriting if it exists
                    File.Copy(filePath, destFilePath, true);

                    LogInfo($"{fileName} copied to {destFilePath}",
                        ConsoleColor.Green);

                }
                catch (IOException e)
                {
                    LogInfo($"Error copying file: {e.Message}",
                        ConsoleColor.Red);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destFolder"></param>
        /// <param name="sourcePath"></param>
        private void RemoveClonedFile(string destFolder, string sourcePath)
        {
            // Remove files and directories not in the source
            foreach (string filePath in Directory.GetFiles(destFolder))
            {

                try
                {
                    // Get file name
                    string fileName = Path.GetFileName(filePath);

                    // If file does not exist in source, delete it!
                    if (!File.Exists(Path.Combine(sourcePath, fileName)))
                    {
                        // delete file with given path
                        File.Delete(filePath);

                        LogInfo($"{fileName} deleted from {filePath}",
                            ConsoleColor.Red);
                    }
                }

                catch (IOException e)
                {
                    LogInfo(
                        $"Error deleting file {filePath}: {e.Message}",
                        ConsoleColor.Red);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destFolder"></param>
        /// <param name="sourcePath"></param>
        private void RemoveClonedDirectories(string destFolder, string sourcePath)
        {
            foreach (string subDirPath in Directory.GetDirectories(destFolder))
            {
                try
                {
                    // Get directory name
                    string dirName = Path.GetFileName(subDirPath);

                    // Path of the corresponding directory in the source
                    string sourceSubDirPath = Path.Combine(sourcePath, dirName);

                    // If the directory does not exist in the source, delete it
                    if (!Directory.Exists(sourceSubDirPath))
                    {
                        // Delete the directory and its contents
                        Directory.Delete(subDirPath, true);


                        LogInfo(
                            $"Directory: {dirName} deleted from: {subDirPath}",
                            ConsoleColor.Red);


                    }
                    else
                    {
                        // Recursively check subdirectories
                        RemoveClonedDirectories(subDirPath, sourceSubDirPath);
                    }
                }
                catch (IOException e)
                {

                    LogInfo(
                        $"Error deleting directory {subDirPath}: {e.Message}",
                        ConsoleColor.Red);
                }
            }
        }

        private void LogInfo(string logText, ConsoleColor color)
        {
            _logger.AddLog(logText, color);
        }

        private void LogInfo(string logText)
        {
            _logger.AddLog(logText);
        }

    }

}