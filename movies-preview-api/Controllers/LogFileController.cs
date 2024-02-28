using Microsoft.AspNetCore.Mvc;
using movies_preview_api.Models;
using movies_preview_api.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace movies_preview_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogFileController : ControllerBase
    {
        private static readonly Logger logger = new();

        // GET: api/<LogFileController>
        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                logger.WriteToLog("Retrieving log file data via Get request", Utils.Logger.LogMessageType.PROCESS);

                string[] result = logger.GetLogFile();
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format("LogFile GET, Error: {0}", ex.Message);
                logger.WriteToLog(msg, Logger.LogMessageType.ERROR);
                return new JsonResult(msg);
            }
        }

        // POST api/<LogFileController>
        [HttpPost]
        public string Post([FromBody] LogFileInput logfile)
        {
            try
            {
                string? post_data = logfile.Data;

                if (!string.IsNullOrEmpty(post_data)) 
                {
                    logger.WriteToLog(post_data, Logger.LogMessageType.PROCESS);
                    return "Message added to log file";
                }
                else 
                {
                    return "Data is NULL, no update to log file performed";
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("LogFile POST, Error: {0}", ex.Message);
                logger.WriteToLog(msg, Logger.LogMessageType.ERROR);
                return msg;
            }
        }

        // DELETE api/<LogFileController>/5
        [HttpDelete]
        public JsonResult Delete()
        {
            try
            {
                logger.DeleteLogFile();
                return new JsonResult("Log file deleted");
            }
            catch (Exception ex)
            {
                string msg = string.Format("LogFile DELETE, Error: {0}", ex.Message);
                logger.WriteToLog(msg, Logger.LogMessageType.ERROR);
                return new JsonResult(msg);
            }
        }
    }
}
