using UnityEngine;

namespace giorgiokalmund.Dora.Saving.StorageProviders
{
    public class PlayerPrefsStorageProvider : IStorageProvider
    {
        public void Store(string path, string data)
        {
            PlayerPrefs.SetString(path, data);
        }

        public string Load(string path)
        {
            return PlayerPrefs.GetString(path);
        }

        public bool TryLoad(string path, out string data)
        {
            if (Exists(path))
            {
                data = Load(path);
                return true;
            }

            data = null;
            return false;
        }

        public bool Exists(string path)
        {
            return PlayerPrefs.HasKey(path);
        }

        public bool Delete(string path)
        {
            bool contains = Exists(path);
            PlayerPrefs.DeleteKey(path);
            return contains;
        }
    }
}