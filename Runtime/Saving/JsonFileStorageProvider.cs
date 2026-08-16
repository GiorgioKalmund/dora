namespace giorgiokalmund.Dora.Saving
{
    public class JsonFileStorageProvider : SimpleFileStorageProvider
    {
        public JsonFileStorageProvider(string folderPath) : base(folderPath)
        {
            
        }
        
        protected override string GetFileExtension()
        {
            return "json";
        }
    }
}