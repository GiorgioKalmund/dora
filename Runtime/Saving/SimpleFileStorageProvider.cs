using System.IO;
using UnityEngine;

namespace giorgiokalmund.Dora.Saving
{
    public abstract class SimpleFileStorageProvider : IStorageProvider
    {
        private string _folderPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath">The path of the folder to save at</param>
        public SimpleFileStorageProvider(string folderPath)
        {
            _folderPath = folderPath;
        }
        
        public SimpleFileStorageProvider() : this(Application.persistentDataPath)
        {
            
        }

        /// <summary>
        /// Returns the signature of the file extension of the file to store.
        /// <b>Do not prefix with a '.', only return the name of the file extension suffix.</b>
        /// </summary>
        /// <returns>The name of the file extension.</returns>
        protected abstract string GetFileExtension();

        private string CombinedPath(string name) =>
            $"{Path.Combine(_folderPath, name)}.{GetFileExtension()}";
        
        public void Store(string path, string data)
        {
            var combined = CombinedPath(path);
            
            var dirName = Path.GetDirectoryName(combined);
            if (dirName == null)
                return;
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            
            Debug.Log($"savefile written at {combined}");
            File.WriteAllText(CombinedPath(path), data);
        }

        public string Load(string path)
        {
            var combined = CombinedPath(path);
            if (!File.Exists(combined))
                return null;
            
            Debug.Log($"savefile loaded at {combined}");
            return File.ReadAllText(combined);
        }

        public bool TryLoad(string path, out string data)
        {
            var combined = CombinedPath(path);
            if (!File.Exists(combined))
            {
                data = null;
                return false;
            }

            data = Load(path);
            return true;
        }

        public bool Exists(string path)
        {
            return File.Exists(CombinedPath(path));
        }

        public bool Delete(string path)
        {
            var combined = CombinedPath(path);
            if (!File.Exists(combined))
                return false;
            
            File.Delete(combined);
            return true;
        }
    }
}