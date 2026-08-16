namespace giorgiokalmund.Dora.Saving
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