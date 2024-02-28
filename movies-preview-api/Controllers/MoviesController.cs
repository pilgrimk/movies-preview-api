using Microsoft.AspNetCore.Mvc;
using movies_preview_api.Models;
using movies_preview_api.Utils;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace movies_preview_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private const int LOOK_AHEAD_MONTHS = 3;
        private const int MAX_PROCESS_PAGES = 3;
        private const double MIN_POPULARITY = 20.0;
        private const string THE_MOVIEDB_BASE_URL = "https://api.themoviedb.org";
        private const string DISCOVER_MOVIES_BY_DATE_STRING = "/3/discover/movie?primary_release_date.gte={0}&primary_release_date.lte={1}&api_key={2}&page={3}";

        private readonly Logger logger = new();

        // GET: api/<MoviesController>/GetUpcomingMovies
        [HttpGet("GetUpcomingMovies")]
        public List<UpcomingMovies> GetUpcomingMovies()
        {
            return GetUpcomingMovies("en");
        }

        // GET api/<MoviesController>/GetUpcomingMovies/en
        [HttpGet("GetUpcomingMovies/{lang}")]
        public List<UpcomingMovies> GetUpcomingMovies(string lang = "en")
        {
            List<UpcomingMovies> response = [];
            List<UpcomingMovies> paged_response = [];

            int page_number = 1;
            int total_pages = -1;
            string date_string = DateTime.Now.ToString("yyyy-MM-dd");
            string future_date_string = DateTime.Today.AddMonths(LOOK_AHEAD_MONTHS).ToString("yyyy-MM-dd");
            string pub_key = MyUtils.GetAPIKey(MyUtils.PublicKeyType.MOVIES);
            string url = THE_MOVIEDB_BASE_URL + string.Format(DISCOVER_MOVIES_BY_DATE_STRING, date_string, future_date_string, pub_key, page_number.ToString());

            paged_response = GetMovies(url, lang, ref total_pages);
            if (paged_response.Count > 0)
            {
                response.AddRange(paged_response);

                if (total_pages > 0)
                {
                    while ((page_number < total_pages) && (page_number < MAX_PROCESS_PAGES))
                    {
                        // increment our page counter and update our URL string
                        page_number += 1;
                        url = THE_MOVIEDB_BASE_URL + string.Format(DISCOVER_MOVIES_BY_DATE_STRING, date_string, future_date_string, pub_key, page_number.ToString());

                        // get the new specified page
                        paged_response = GetMovies(url, lang, ref total_pages);
                        if (paged_response.Count > 0)
                        {
                            response.AddRange(paged_response);
                        }
                    }

                    logger.WriteToLog(string.Format("GetUpcomingMovies, processed {0} of {1} total pages", page_number, total_pages), Logger.LogMessageType.PROCESS);
                }
            }

            return response;
        }


        // PRIVATE functions -----------------------------------------------------------------------------
        private List<UpcomingMovies> GetMovies(string url, string filter, ref int total_pages)
        {
            HTTPMessageHandler handler = new HTTPMessageHandler();
            List<UpcomingMovies> filtered_response = new List<UpcomingMovies>();

            string response = handler.SendHTTPMesssage(url, "GET");

            if (MyUtils.IsValidJson(response))
            {
                filtered_response = FilterUpcomingMovies(response, filter);

                if (total_pages < 0)
                {
                    total_pages = GetTotalPages(response);
                }
            }

            return filtered_response;
        }

        private int GetTotalPages(string JSON_string)
        {
            try
            {
                JObject obj_JSON = new JObject();
                obj_JSON = JObject.Parse(JSON_string);

                return obj_JSON.Value<int?>("total_pages") ?? 1;
            }
            catch (Exception ex)
            {
                logger.WriteToLog(string.Format("GetTotalPages, error: {0}", ex.Message), Logger.LogMessageType.ERROR);
                return 1;
            }
        }

        private List<UpcomingMovies> FilterUpcomingMovies(string JSON_string, string filter)
        {
            List<UpcomingMovies> response = new List<UpcomingMovies>();
            JObject obj_JSON = new JObject();
            obj_JSON = JObject.Parse(JSON_string);

            IEnumerable<JToken> result_json = obj_JSON.SelectTokens("$..results");
            foreach (var child in result_json.Children())
            {
                if (((double)child["popularity"] >= MIN_POPULARITY) && ((string)child["original_language"] == filter))
                {
                    // add the child to the response list
                    response.Add(child.ToObject<UpcomingMovies>());
                }
            }

            return response;
        }
        // -----------------------------------------------------------------------------------------------
    }
}
