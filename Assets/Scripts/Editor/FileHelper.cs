using System.IO;
using UnityEngine;

namespace FrameworkEditor
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
        
        public static string[] GetAllChildFiles(string path, string suffix = "", SearchOption option = SearchOption.AllDirectories)
        {
            string strPattner = "*";
            if (suffix.Length > 0 && suffix[0] != '.')
            {
                strPattner += "." + suffix;
            }
            else
            {
                strPattner += suffix;
            }

            string[] files = Directory.GetFiles(path, strPattner, option);
            var count = 0;
            for (int i = 0; i < files.Length; i++)
            {
                var name = files[i];
                if (name.Contains(".DS_Store"))
                {
                    files[i] = null;
                }
                else
                {
                    count++;
                }
            }

            string[] filesWithoutDSStore = new string[count];
            var index = 0;
            foreach (var name in files)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    filesWithoutDSStore[index] = name;
                    index++;
                }
            }
            if (index != count)
            {
                Debug.LogError("错啦！");
            }
            return filesWithoutDSStore;
        }
    }
    
}