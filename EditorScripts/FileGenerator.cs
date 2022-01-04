using System.IO;
using pbuddy.StringUtility.RuntimeScripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace pbuddy.ShaderUtility.EditorScripts
{
    public static class FileGenerator
    {
        private static readonly string ParentDirectory = Path.Combine(Application.dataPath, typeof(DebugAndTestGPUCodeUtility).NamespaceAsPath());
        private static readonly string GeneratedFilesRootDirectory = Path.Combine(ParentDirectory, "GENERATED");
        
        private const string MetaExtension = ".meta";

        public static string GetPathToSubDirectory(string nestedDirectoryLocation)
        {
            string fullPath = Path.Combine(GeneratedFilesRootDirectory, nestedDirectoryLocation);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }
        
        public static string GetPathToSubFile(string nestedFileLocation)
        {
            GetPathToSubDirectory(Path.GetDirectoryName(nestedFileLocation));
            return Path.Combine(GeneratedFilesRootDirectory, nestedFileLocation);
        }
        
        public static void GenerateArbitraryFile(string contents, string pathRelativeToGeneratedFilesRoot)
        {
            string fullPathToFile = GetPathToSubFile(pathRelativeToGeneratedFilesRoot);
            using var file = new StreamWriter(fullPathToFile);
            file.Write(contents);
        }

        public static string[] GetGeneratedFileContents(string path)
        {
            string fullPathToFile = path.Contains(GeneratedFilesRootDirectory) ? path : GetPathToSubFile(path);
            return File.Exists(fullPathToFile) ? File.ReadAllLines(fullPathToFile) : null;
        }
        
        public static void DeleteGeneratedFile(string fullPathToFile)
        {
            void SafeDeleteFile(string fileLocation)
            {
                if (File.Exists(fileLocation))
                {
                    File.Delete(fileLocation);
                    SafeDeleteFile($"{fileLocation}{MetaExtension}");
                }
            }

            void SafeDeleteDirectory(string directoryLocation)
            {
                if (Directory.Exists(directoryLocation) && Directory.GetFiles(directoryLocation).Length == 0)
                {
                    Directory.Delete(directoryLocation);
                    SafeDeleteFile($"{directoryLocation}{MetaExtension}");
                }
            }
            
            string directory = Path.GetDirectoryName(fullPathToFile);
            Assert.IsTrue(directory.Contains(GeneratedFilesRootDirectory));
            SafeDeleteFile(fullPathToFile);
            SafeDeleteDirectory(directory);
            SafeDeleteDirectory(GeneratedFilesRootDirectory);
        }
    }
}