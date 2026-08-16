namespace giorgiokalmund.Dora.Saving
{
    public interface IStorageProvider
    {
        public void Store(string path, string data);
        public string Load(string path);
        public bool TryLoad(string path, out string data);
        public bool Exists(string path);
        public bool Delete(string path);
    }
}