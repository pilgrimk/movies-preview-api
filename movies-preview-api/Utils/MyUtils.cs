using System.Xml.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace movies_preview_api.Utils
{
    public static class MyUtils
    {
        private static readonly Logger logger = new();
        private const string MOVIE_API_KEY = "4c3a1f537f8907422488da3c434f96b2";

        public enum PublicKeyType
        {
            MOVIES
        }

        public static string SerializeObject_XML<T>(this T toSerialize)
        {
            try
            {
                if (toSerialize?.GetType() is not null)
                {
                    XmlSerializer xmlSerializer = new(toSerialize.GetType());

                    using (StringWriter textWriter = new())
                    {
                        xmlSerializer.Serialize(textWriter, toSerialize);
                        return textWriter.ToString();
                    }
                }
                else 
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog(string.Format("SerializeObject_XML, error: {0}", ex.Message), Logger.LogMessageType.ERROR);
                return string.Empty;
            }

        }

        public static string SerializeObject_JSON<T>(this T toSerialize)
        {
            try
            {
                if (toSerialize?.GetType() is not null)
                {
                    DataContractJsonSerializer js = new(toSerialize.GetType());
                    MemoryStream msObj = new MemoryStream();
                    js.WriteObject(msObj, toSerialize);
                    msObj.Position = 0;
                    StreamReader sr = new(msObj);

                    // "{\"Description\":\"Share Knowledge\",\"Name\":\"C-sharpcorner\"}"  
                    string json = sr.ReadToEnd();

                    sr.Close();
                    msObj.Close();

                    return json;
                }
                else 
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                logger.WriteToLog(string.Format("SerializeObject_JSON, error: {0}", ex.Message), Logger.LogMessageType.ERROR);
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
            string? string_path = string.Empty;

            try
            {
                string? entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;

                if (!string.IsNullOrEmpty(entryAssemblyLocation))
                {
                    int binIndex = entryAssemblyLocation.IndexOf("bin", StringComparison.OrdinalIgnoreCase);

                    if (binIndex > 0)
                    {
                        string_path = Path.GetDirectoryName(entryAssemblyLocation[..binIndex]);
                    }
                    else
                    {
                        string_path = Path.GetDirectoryName(entryAssemblyLocation);
                    }
                }

                if (string.IsNullOrEmpty(string_path)) 
                {
                    return string.Empty;
                }
                else
                {
                    return string_path;
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog(string.Format("GetWorkingDirectory, error: {0}", ex.Message), Logger.LogMessageType.ERROR);
                return string.Empty;
            }
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
