using System;
using System.Text;
using Microsoft.VisualBasic;
using System.Net;
using System.Xml.Serialization;
using System.Net.Http;
using System.IO;

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

        private static readonly Logger logger = new();

        public string SendHTTPMesssage(string url, string method, string postdata = "", WebHeaderCollection? headers = null)
        {
            System.Net.Http.HttpMethod request_method =  System.Net.Http.HttpMethod.Get;
            UTF8Encoding enc;
            byte[]? postdatabytes = null;

            try
            {
                switch (method)
                {
                    case GET_REQUEST_METHOD:
                        {
                            request_method = System.Net.Http.HttpMethod.Get;
                            break;
                        }

                    case POST_REQUEST_METHOD:
                        {
                            request_method = System.Net.Http.HttpMethod.Post;
                            break;
                        }   

                    case PUT_REQUEST_METHOD:
                        {
                            request_method = System.Net.Http.HttpMethod.Put;
                            break;
                        }

                    case DELETE_REQUEST_METHOD:
                        {
                            request_method = System.Net.Http.HttpMethod.Delete;
                            break;
                        }

                    default:
                        {
                            throw new Exception(string.Format(UNRECOGNIZED_METHOD_ERR_MSG, method));
                        }
                }

                if (Information.IsNothing(headers))
                    headers = [];
                if (Information.IsNothing(headers?.Get(HEADER_CONTENT_TYPE)))
                    headers?.Add(HEADER_CONTENT_TYPE, FORM_URLENCODED);
                if (Information.IsNothing(headers?.Get(HEADER_TIMEOUT)))
                    headers?.Add(HEADER_TIMEOUT, TIMEOUT.ToString());
                if ((!Information.IsNothing(postdatabytes)) && 
                    (Information.IsNothing(headers?.Get(HEADER_CONTENT_LENGTH))))
                    headers?.Add(HEADER_CONTENT_LENGTH, postdatabytes?.Length.ToString());

                if ((!string.IsNullOrEmpty(postdata)))
                {
                    enc = new System.Text.UTF8Encoding();
                    postdatabytes = enc.GetBytes(postdata);
                }

                HttpRequestMessage request = new(request_method, url);
                HttpClient httpClient = new();
                SetHttpClientHeaders(ref httpClient, headers);

                var response = httpClient.Send(request);
                var reader = new StreamReader(response.Content.ReadAsStream());
                string? responseMessage = reader.ReadToEnd();
                reader.Close();

                return responseMessage;
            }
            catch (Exception ex)
            {
                string msg = string.Format(DETAILED_ERR_MSG, ex.Message, method, url, postdata);
                logger.WriteToLog(msg, Logger.LogMessageType.ERROR);
                return msg;
            }
        }

        private static void SetHttpClientHeaders(ref HttpClient client, WebHeaderCollection? headers)
        {
            try
            {
                foreach (string key in headers!)
                {
                    switch (key)
                    {
                        case HEADER_CONTENT_TYPE:
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(key, headers.Get(key));
                                break;
                            }

                        case HEADER_CONTENT_LENGTH:
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(key, Convert.ToString(Convert.ToInt64(headers.Get(key))));
                                break;
                            }

                        case HEADER_TIMEOUT:
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(key, Convert.ToString(Convert.ToInt32(headers.Get(key))));
                                break;
                            }

                        case HEADER_AUTHORIZATION:
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(key, headers.Get(key));
                                break;
                            }

                        case HEADER_AUTHTOKEN:
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(key, headers.Get(key));
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
                string msg = string.Format("HTTPMessageHandler.SetHttpClientHeaders: {0}", ex.Message);
                logger.WriteToLog(msg, Logger.LogMessageType.ERROR);
                throw new Exception(msg);
            }
        }

        private static string SerializeRequestRecord(WebHeaderCollection request)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlSerializer serializer = new(typeof(WebHeaderCollection));

            serializer.Serialize(sw, request);
            string returnVal = sw.ToString();

            return returnVal;
        }
    }
}
