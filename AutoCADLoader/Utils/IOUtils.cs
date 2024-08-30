using AutoCADLoader.Properties;
using System.IO;

namespace AutoCADLoader.Utils
{
    public static class IOUtils
    {
        public static int DirectoryTimeoutSeconds => LoaderSettings.DirectoryAccessTimeout;


        public static bool IsDirectoryAccessible(string path, int? directoryAccessTimeout = null)
        {
            bool isAccessible = false;

            try
            {
                Task<bool> task = Task.Run(() => // Task is used for a custom timeout value (Directory methods are often blocking, with a long timeout)
                {
                    DirectoryInfo di = new(path);
                    if (di.Exists)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

                Task.WhenAny(
                    Task.Delay(TimeSpan.FromSeconds(directoryAccessTimeout ?? DirectoryTimeoutSeconds)),
                    task).Wait();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    isAccessible = task.Result;
                }
            }
            catch
            {
                isAccessible = false;
            }

            return isAccessible;
        }

        /// <summary>
        /// Check if any files within the source folder are different from the target folder as quickly as possible.
        /// </summary>
        /// <returns></returns>
        public static bool AnyFolderDifferenceQuick(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool checkSubfolders = true, bool checkForDateDifference = true)
        {
            FileInfo[] sourceFiles;
            try
            {
                sourceFiles = sourceDirectory.GetFiles();

            }
            catch
            {
                return false;
            }

            // Check all the files in this directory
            foreach (FileInfo fileSrc in sourceFiles)
            {
                string targetFile = Path.Combine(targetDirectory.FullName, fileSrc.Name);
                bool different = AnyFileDifference(fileSrc, targetFile, checkForDateDifference);

                if (different)
                {
                    return true;
                }
            }

            if (checkSubfolders)
            {
                // Check all the files within subdirectories
                foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = new DirectoryInfo(targetDirectory.FullName + "\\" + diSourceSubDir.Name);
                    bool subfolderDifferent = AnyFolderDifferenceQuick(diSourceSubDir, nextTargetSubDir);

                    if (subfolderDifferent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// A wrapper for AnyFolderDifferenceQuick with string parameters.
        /// </summary>
        public static bool AnyFolderDifferenceQuick(string sourceDirectoryString, string targetDirectoryString, bool checkSubfolders = true, bool checkForDateDifference = true)
        {
            DirectoryInfo sourceDirectory = new(sourceDirectoryString);
            DirectoryInfo targetDirectory = new(targetDirectoryString);
            return AnyFolderDifferenceQuick(sourceDirectory, targetDirectory, checkSubfolders, checkForDateDifference);
        }

        /// <returns>True if the files specified in the parameters are different from each other, otherwise false.</returns>
        public static bool AnyFileDifference(FileInfo sourceFile, string targetFileName, bool checkForDateDifference = true)
        {
            if (sourceFile is null || !sourceFile.Exists)
            {
                // Skip file because it does not exist/cannot be accessed
                return false;
            }

            if (File.Exists(targetFileName) && checkForDateDifference)
            {
                FileInfo targetFile = new(targetFileName);
                DateTime targetLastModified = targetFile.LastWriteTime;
                DateTime sourceLastModified = sourceFile.LastWriteTime;
                try
                {
                    if (sourceLastModified != targetLastModified)
                    {
                        // The source file is different to the target file
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // TODO: What are we expecting to catch here?
                }
            }
            else
            {
                // The source file is new and does not exist in the target directory
                return true;
            }

            return false;
        }

        public static bool AnyFileDifferenceExperimental(string path1, string path2)
        {
            return File.ReadLines(path1).SequenceEqual(File.ReadLines(path2));
        }

        /// <summary>
        /// Copies from the source directory to the target directory. Existing files will be overwritten (if different). Additional files in the target folder will not be removed.
        /// </summary>
        public static void DirectoryCopy(string sourcePath, string targetPath, bool copySubdirectories)
        {
            // Check that the source directory is valid
            DirectoryInfo sourceDirectory = new(sourcePath);
            try
            {
                if (!sourceDirectory.Exists)
                {
                    EventLogger.Log($"Source directory may not exist and cannot be copied from: {sourceDirectory}", System.Diagnostics.EventLogEntryType.Warning);
                    return;
                }
            }
            catch
            {
                EventLogger.Log($"Error accessing source directory to copy from: {sourceDirectory}", System.Diagnostics.EventLogEntryType.Error);
                return;
            }

            Directory.CreateDirectory(targetPath); // Create the target directory if it does not already exist

            // Copy the new and different files from the current source directory.
            FileInfo[] sourceFiles = sourceDirectory.GetFiles();
            foreach (FileInfo sourceFile in sourceFiles)
            {
                string targetFilePath = Path.Combine(targetPath, sourceFile.Name);
                if (!File.Exists(targetFilePath))
                {
                    sourceFile.CopyTo(targetFilePath);
                }
                else
                {
                    bool different = AnyFileDifference(sourceFile, targetFilePath);
                    if (different)
                    {
                        sourceFile.CopyTo(targetFilePath, true);
                    }
                }
            }

            if (copySubdirectories)
            {
                try
                {
                    DirectoryInfo[] currentSourceSubdirectories = sourceDirectory.GetDirectories();
                    foreach (DirectoryInfo subdirectory in currentSourceSubdirectories)
                    {
                        string targetSubdirectory = Path.Combine(targetPath, subdirectory.Name);
                        DirectoryCopy(subdirectory.FullName, targetSubdirectory, copySubdirectories);
                    }
                }
                catch
                {
                    EventLogger.Log($"Error retrieving directory subfolders from: {sourceDirectory}", System.Diagnostics.EventLogEntryType.Error);
                }
            }
        }

        /// <summary>
        /// Copies from the source directory to the target directory. Existing files will be overwritten (if different). Additional files in the target folder will be removed.
        /// TODO: This can be made much more efficient
        /// </summary>
        public static void DirectoryReplicate(string sourcePath, string targetPath)
        {
            DirectoryDelete(targetPath);
            DirectoryCopy(sourcePath, targetPath, true);
        }

        public static void DirectoryDelete(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                }
                catch
                {
                    EventLogger.Log($"Attempt to delete directory failed: {directoryPath}", System.Diagnostics.EventLogEntryType.Warning);
                }
            }
        }
    }
}
