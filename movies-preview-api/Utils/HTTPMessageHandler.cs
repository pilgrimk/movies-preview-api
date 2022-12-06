using System;
using System.Text;
using Microsoft.VisualBasic;
using System.Net;
using System.Xml.Serialization;

namespace movies_preview_api.Utils
{
    public class HTTPMessageHandler
    {
        private const string GET_REQUEST_METHOD = "GET";
        private const string POST_REQUEST_METHOD = "POST";
        private const string PUT_REQUEST_METHOD = "PUT";
        private const string DELETE_REQUEST_METHOD = "DELETE";
        private const string FORM_URLENCODED = "application/x-www-form-urlencoded";
        private const string UNRECOGNIZED_METHOD_ERR_MSG = "Unrecognized HTTP method attempted: {0}";
        private const string DETAILED_ERR_MSG = "Error Msg: {0}, Method: {1}, URL: {2} Payload: {3}";
        private const int TIMEOUT = 20000;

        private const string HEADER_CONTENT_TYPE = "Content-Type";
        private const string HEADER_CONTENT_LENGTH = "Content-Length";
        private const string HEADER_TIMEOUT = "Timeout";
        private const string HEADER_AUTHORIZATION = "Authorization";
        private const string HEADER_AUTHTOKEN = "AuthToken";

        private readonly Logger logger = new Logger();

        public string SendHTTPMesssage(string url, string method, string postdata = "", WebHeaderCollection headers = null)
        {
            HttpWebRequest request;
            string responseMessage = null;
            UTF8Encoding enc;
            byte[] postdatabytes = null;

            try
            {
                if ((!string.IsNullOrEmpty(postdata)))
                {
                    enc = new System.Text.UTF8Encoding();
                    postdatabytes = enc.GetBytes(postdata);
                }

                request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                switch (method)
                {
                    case GET_REQUEST_METHOD:
                    case POST_REQUEST_METHOD:
                    case PUT_REQUEST_METHOD:
                    case DELETE_REQUEST_METHOD:
                        {
                            request.Method = method;
                            break;
                        }

                    default:
                        {
                            throw new Exception(string.Format(UNRECOGNIZED_METHOD_ERR_MSG, method));
                        }
                }

                if (Information.IsNothing(headers))
                    headers = new WebHeaderCollection();

                if (Information.IsNothing(headers.Get(HEADER_CONTENT_TYPE)))
                    headers.Add(HEADER_CONTENT_TYPE, FORM_URLENCODED);
                if (Information.IsNothing(headers.Get(HEADER_TIMEOUT)))
                    headers.Add(HEADER_TIMEOUT, TIMEOUT.ToString());

                SetWebRequestHeaders(ref request, headers);

                if ((!Information.IsNothing(postdatabytes)))
                {
                    if (Information.IsNothing(headers.Get(HEADER_CONTENT_LENGTH)))
                        headers.Add(HEADER_CONTENT_LENGTH, postdatabytes.Length.ToString());

                    System.IO.Stream dataStream = request.GetRequestStream();
                    dataStream.Write(postdatabytes, 0, postdatabytes.Length);
                    dataStream.Close();
                }

                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    var reader = new System.IO.StreamReader(response.GetResponseStream());
                    responseMessage = reader.ReadToEnd();
                }

                return responseMessage;
            }
            catch (Exception ex)
            {
                string msg = string.Format(DETAILED_ERR_MSG, ex.Message, method, url, postdata);
                logger.WriteToLog(msg, Logger.LogMessageType.ERROR);
                return msg;
            }
        }

        private void SetWebRequestHeaders(ref HttpWebRequest request, WebHeaderCollection headers)
        {
            try
            {
                foreach (string key in headers)
                {
                    switch (key)
                    {
                        case HEADER_CONTENT_TYPE:
                            {
                                request.ContentType = headers.Get(key);
                                break;
                            }

                        case HEADER_CONTENT_LENGTH:
                            {
                                request.ContentLength = System.Convert.ToInt64(headers.Get(key));
                                break;
                            }

                        case HEADER_TIMEOUT:
                            {
                                request.Timeout = System.Convert.ToInt32(headers.Get(key));
                                break;
                            }

                        case HEADER_AUTHORIZATION:
                            {
                                request.Headers.Add(HEADER_AUTHORIZATION, headers.Get(key));
                                break;
                            }

                        case HEADER_AUTHTOKEN:
                            {
                                request.Headers.Add(HEADER_AUTHTOKEN, headers.Get(key));
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("HTTPMessageHandler.SetWebRequestHeaders: " + ex.Message);
            }
        }

        private string SerializeRequestRecord(WebHeaderCollection request)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(WebHeaderCollection));

            serializer.Serialize(sw, request);
            string returnVal = sw.ToString();

            return returnVal;
        }
    }
}
