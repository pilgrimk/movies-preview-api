using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace movies_preview_api.Utils
{
    public static class MyUtils
    {
        private const string MOVIE_API_KEY = "4c3a1f537f8907422488da3c434f96b2";

        public enum PublicKeyType
        {
            MOVIES
        }

        public static string SerializeObject_XML<T>(this T toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    return textWriter.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }

        }

        public static string SerializeObject_JSON<T>(this T toSerialize)
        {
            try
            {

                DataContractJsonSerializer js = new DataContractJsonSerializer(toSerialize.GetType());
                MemoryStream msObj = new MemoryStream();
                js.WriteObject(msObj, toSerialize);
                msObj.Position = 0;
                StreamReader sr = new StreamReader(msObj);

                // "{\"Description\":\"Share Knowledge\",\"Name\":\"C-sharpcorner\"}"  
                string json = sr.ReadToEnd();

                sr.Close();
                msObj.Close();

                return json;
            }
            catch
            {
                return string.Empty;
            }

        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string GetWorkingDirectory()
        {
            string workingDirectory;

            if (Assembly.GetEntryAssembly().Location.IndexOf("bin\\") > 0)
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\")));
            }
            else
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            return workingDirectory;
        }

        public static string GetAPIKey(PublicKeyType key)
        {
            switch (key)
            {
                case PublicKeyType.MOVIES:
                    return MOVIE_API_KEY;
                default:
                    return string.Empty;
            }
        }
    }
}
