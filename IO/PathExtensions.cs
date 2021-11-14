using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.MyServices;
using Microsoft.VisualBasic.FileIO;

using System.Collections;

using Paulus.Collections;
using Paulus.Common;

namespace Paulus.IO
{
    public static class PathExtensions //path extensions
    {
        //e.g. tesseract
        public static List<string> SearchForPathsInEnvironmentVariables(string partOfPathToSearch)
        {
            Hashtable list = (Hashtable)Environment.GetEnvironmentVariables();

            //Dictionary<string,string> dic = list.ToDictionary<string, string>();

            StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

            List<string> candidatePaths = new List<string>();
            foreach (DictionaryEntry entry in list)
            {
                if ((entry.Value as string).IndexOf(partOfPathToSearch, ignoreCase) >= 0)
                {
                    string[] paths = ((string)entry.Value).Split(';');
                    foreach (string path in paths)
                        if (path.IndexOf(partOfPathToSearch, ignoreCase) >= 0)
                            candidatePaths.Add(path);
                }
            }
            return candidatePaths;
        }

        //eg. tesseract,tesseract.exe 
        public static string SearchForFileInEnvironmentVariablePaths(string partOfPathToSearch, string fileNameWithExtension)
        {
            List<string> paths = SearchForPathsInEnvironmentVariables(partOfPathToSearch);
            foreach (string path in paths)
            {
                string candidatePath = Path.Combine(path, fileNameWithExtension);
                if (File.Exists(candidatePath)) return candidatePath;
            }
            return null;
        }

        public static string CombineWithExecutablePath(string filename)
        {
            return Path.Combine(Path.GetDirectoryName(ExecutablePath), filename);
        }

        public static string ExecutablePath //return the path of the executable
        {
            get { return global::System.Reflection.Assembly.GetCallingAssembly().Location; }
        }

        public static string[] GetCommandLineArguments()
        {
            string[] args = global::System.Environment.GetCommandLineArgs();
            //the first argument is always the path of the executable file
            //the executable filename is not included in the args argument in the Main function
            return args.Slice<string>(1, args.Length - 1);
        }

        public static bool IsFileLocked(string path)
        {
            try
            {
                FileStream s = File.OpenRead(path);
                s.Close();
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static string RemoveInvalidFileNameCharacters(string path, string replaceBy = "")
        {
            //23-11-2013: Modified to allow replacement of invalid character with a valid string.

            //avoid invalid characters by removing them
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (replaceBy.Length > 0 && !IsFileNameValid(replaceBy)) throw new ArgumentException("The replacement string contains invalid file name characters.", "replaceBy");

            string validName = path;
            foreach (char c in invalidChars) if (validName.Contains(c)) validName = validName.Replace(c.ToString(), replaceBy);

            return validName;

        }

        public static bool IsPathValid(string path)
        {
            //23-11-2013: Changed name to IsPathValid (from IsFilePathValid). Modified, because the : character should be only once defined and also to allow file:/// paths.
            //16-11-2013: Created.
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                //allow forward slash paths and file:/// cases
                return Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);
            }
        }

        public static bool IsFileNameValid(string fileName)
        {
            //23-11-2013: Created.
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars) if (fileName.Contains(c)) return false;

            return true;
        }


        //eg. returns c:\program files (x86) and c:\program files, or just one of them
        public static List<string> GetProgramFilesPaths()
        {
            //Environment.GetEnvironmentVariable returns null if the environment variable does not exist
            List<string> candidateProgramFilePaths = new List<string>(new string[]
            {
                Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                Environment.GetEnvironmentVariable("ProgramW6432"),
                Environment.GetEnvironmentVariable("ProgramFiles"),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) //in Framework 4.0
            });

            return candidateProgramFilePaths.ToUniqueValuesList<string>();
        }

        //eg. return c:\program files (x86)\[programDirectory] and c:\program files\[programDirectory]
        //only if the corresponding folders exist
        public static List<string> GetProgramPaths(string programDirectory)
        {
            List<string> programFilesPaths = PathExtensions.GetProgramFilesPaths();

            List<string> programPaths = new List<string>();
            foreach (string programFilePath in programFilesPaths)
            {
                string candidateAxiPath = Path.Combine(programFilePath, programDirectory);
                if (Directory.Exists(candidateAxiPath)) programPaths.Add(candidateAxiPath);
            }

            return programPaths;
        }

        public static bool IsPathAbsolute(string path)
        {
            //23-11-2013: Modified to allow file:// expressions.
            Regex reg = new Regex("^[a-zA-Z]{1}:\\.*");
            bool firstCheck = reg.IsMatch(path);
            if (firstCheck) return true;

            Uri result;
            return Uri.TryCreate(path, UriKind.Absolute, out result);

        }

        public static string GetWellFormedPath(string path)
        {
            //23-11-2013: Created and tested.
            if (!PathExtensions.IsPathValid(path)) throw new ArgumentException("The path is invalid.", "path");

            string wellFormedPath = path;
            bool isUriStyle = Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);
            if (isUriStyle)
            {
                Uri u = new Uri(path, UriKind.RelativeOrAbsolute);
                if (u.IsAbsoluteUri) wellFormedPath = u.LocalPath;
            }

            wellFormedPath = wellFormedPath.Replace('/', '\\');

            return wellFormedPath;
        }

        public static string[] GetSubPaths(string path)
        {
            //23-11-2013: Created and tested.
            if (!PathExtensions.IsPathValid(path)) throw new ArgumentException("The path is invalid.", "path");

            string p = GetWellFormedPath(path);

            string[] components = p.Split('\\');
            string[] subpaths = new string[components.Length];

            subpaths[0] = components[0];
            for (int i = 1; i < components.Length; i++)
                subpaths[i] = subpaths[i - 1] + "\\" + components[i];

            return subpaths;
        }


        /// <summary>
        /// Returns the absolute path of a relative path.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="path"></param>
        /// <returns>If the path is absolute then returns the path. If the path is relative then it returns the absolute path based on the basePath.</returns>
        public static string GetAbsolutePath(string basePath, string path)
        {
            if (IsPathAbsolute(path)) return path;
            else
            {
                Uri basePathUri = new Uri(basePath + (basePath.EndsWith("\\") ? "" : "\\"));
                return new Uri(basePathUri, path).LocalPath;
            }
        }

        /// <summary>
        /// Returns the absolute path of a relative path.
        /// </summary>
        /// <param name="baseFile"></param>
        /// <param name="file"></param>
        /// <returns>If the path is absolute then returns the path. If the path is relative then it returns the absolute path based on the basePath. If the file does not exists it is combined with the executable path.</returns>
        public static string GetAbsolutePath2(string baseFile, string file,bool implyExecutablePathIfFileDoesNotExist=true)
        {
            if (!IsPathAbsolute(file))
            {
                string candidatePath = Path.Combine(Path.GetDirectoryName(baseFile), file);
                if (File.Exists(candidatePath) ||
                    !File.Exists(candidatePath) && !implyExecutablePathIfFileDoesNotExist)
                    return Path.Combine(Path.GetDirectoryName(baseFile), file);
                else
                    return PathExtensions.CombineWithExecutablePath(file);
            }
            else
                return file;
        }


        public static string GetRelativeToExePath(string path, char? slashChar = '\\')
        {
            return GetRelativePath(ExecutablePath, path, slashChar);
        }

        //we assume that both paths are absolute!
        public static string GetRelativePath(string basePath, string path, char? slashChar = '\\')
        {
            //if a name is given then retrieve it's parent directory
            if (File.Exists(basePath)) basePath = Path.GetDirectoryName(basePath);

            //base path must contain a trailing slash to be recognized as a path
            bool endsWithBackSlash = basePath.EndsWith(@"\");
            bool endsWithSlash = basePath.EndsWith(@"/");
            if (!endsWithBackSlash && !endsWithSlash)
            {
                if (!slashChar.HasValue)
                {
                    bool useSlash = basePath.CharacterCount('/') > basePath.CharacterCount('\\');
                    basePath += useSlash ? @"/" : @"\";
                }
                else
                    basePath += slashChar.Value;
            }
            Uri basePathUri = new Uri(basePath);
            Uri pathUri = new Uri(path); //the path is relative to to the basepath
            return Uri.UnescapeDataString(basePathUri.MakeRelativeUri(pathUri).ToString()); //to string returns the canonical version of the string
        }

        public static void CopyDirectoryWithoutEmptyFolders(string source, string target)
        {
            string[] allFiles = Directory.GetFiles(source, "*.*", global::System.IO.SearchOption.AllDirectories);
            foreach (string srcFile in allFiles)
            {
                string targetFile = target + srcFile.Substring(source.Length);
                string targetDir = Path.GetDirectoryName(targetFile);
                if (Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                File.Copy(srcFile, targetFile);
            }
        }

        //showUI and allowCancel are used only if overwrite is false
        //accepts both files and folders as a source
        //unified copy function that allows the use of ui
        public static void Copy(string source, string target, bool overwrite = true, bool showUI = false, bool throwErrorOnCancel = true)
        {
            //var fs = new Microsoft.VisualBasic.Devices.Computer().FileSystem;
            //fs.CopyDirectory(source, target, true);
            //fs.CopyFile(axiSystemPath, Path.Combine(outputPath, "axisystem.axisys"), true);

            if (Directory.Exists(source))
                if (showUI) FileSystem.CopyDirectory(source, target, UIOption.AllDialogs, throwErrorOnCancel ? UICancelOption.ThrowException : UICancelOption.DoNothing);
                else FileSystem.CopyDirectory(source, target, overwrite);
            else if (File.Exists(source))
                if (showUI) FileSystem.CopyFile(source, target, UIOption.AllDialogs, throwErrorOnCancel ? UICancelOption.ThrowException : UICancelOption.DoNothing);
                else FileSystem.CopyFile(source, target, overwrite);
            else throw new FileOrDirectoryNotFoundException();
        }

        //throws OperationCanceledException if throwErrorOnCancel is true
        public static void Copy(this DirectoryInfo sourceDirectory, string targetDirectoryName, bool overwrite = true, bool showUI = false, bool throwErrorOnCancel = true)
        {
            string sourceDirectoryName = sourceDirectory.FullName;
            if (showUI) FileSystem.CopyDirectory(sourceDirectoryName, targetDirectoryName, UIOption.AllDialogs, throwErrorOnCancel ? UICancelOption.ThrowException : UICancelOption.DoNothing);
            else FileSystem.CopyDirectory(sourceDirectoryName, targetDirectoryName, overwrite);
        }

        public static bool FileStartsWith(string fullPath, string textToCheck)
        {
            using (StreamReader reader = new StreamReader(fullPath))
            {
                int bufferCount = textToCheck.Length;
                char[] buffer = new char[bufferCount];
                if (reader.ReadBlock(buffer, 0, bufferCount) == bufferCount)
                    return new string(buffer) == textToCheck;
            }
            return false;
        }

        public static void RenameFile(string file, string newFilenameWithExtension, bool overwrite = true)
        {
            string newPath = Path.Combine(Path.GetDirectoryName(file), newFilenameWithExtension + Path.GetExtension(file));
            File.Copy(file, newPath, overwrite);
            File.Delete(file);
        }

        public static string[] GetFilesExceptThoseStartingFrom(string path, string extension, string startingFrom)
        {
            string[] files = Directory.GetFiles(path, "*." + extension);

            return (from f in files where !Path.GetFileName(f).StartsWith(startingFrom) select f).ToArray();
        }

        [Serializable]
        public class FileOrDirectoryNotFoundException : IOException
        {
            public FileOrDirectoryNotFoundException() : base("Unable fo find the specified file or directory.") { }
            public FileOrDirectoryNotFoundException(string message) : base(message) { }
            public FileOrDirectoryNotFoundException(string message, Exception inner) : base(message, inner) { }

            public FileOrDirectoryNotFoundException(string message, string fileName) : base(message) { _fileName = fileName; }
            public FileOrDirectoryNotFoundException(string message, string fileName, Exception inner) : base(message, inner) { _fileName = fileName; }

            protected FileOrDirectoryNotFoundException(
              global::System.Runtime.Serialization.SerializationInfo info,
              global::System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }

            private string _fileName;
            public string FileName { get { return _fileName; } }
        }

        //copy a file to a new directory and keeps the same filename
        //and returns the target path
        public static string CopyFileWithTheSameNameTo(string filePath, string newDirectoryName, bool overwrite)
        {
            string target;
            target = Path.Combine(newDirectoryName, Path.GetFileName(filePath));
            File.Copy(filePath, target, overwrite);
            return target;
        }

        //moves a file to a new directory and keeps the same filename
        //and returns the target path
        public static string MoveFileWithTheSameNameTo(string filePath, string newDirectoryName)
        {
            string target;
            target = Path.Combine(newDirectoryName, Path.GetFileName(filePath));
            File.Move(filePath, target);
            return target;

        }

        public static string GetFilePathWithoutExtension(string filePath)
        {
            return Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
        }

    }
}
