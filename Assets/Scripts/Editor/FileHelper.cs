using System.IO;

namespace Editor
{
    public class FileHelper
    {
        public static string ReadTextFromFile(string path, string defaultValue = "")
        {
            string result = defaultValue;

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                using (StreamReader reader = fileInfo.OpenText())
                {
                    result = reader.ReadToEnd();
                    reader.Close();
                }
            }

            return result;
        }
    }
}