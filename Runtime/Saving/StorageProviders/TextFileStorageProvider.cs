namespace giorgiokalmund.Dora.Saving.StorageProviders
{
    public class TextFileStorageProvider : SimpleFileStorageProvider
    {
        public TextFileStorageProvider(string folderPath) : base(folderPath)
        {
            
        }
        
        protected override string GetFileExtension()
        {
            return "txt";
        }
    }
}