using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using movies_preview_api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace movies_preview_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogFileController : ControllerBase
    {
        private readonly Utils.Logger myLogger = new Utils.Logger();

        [Route("Test")]
        [HttpGet]
        public JsonResult Test()
        {
            return new JsonResult("Test");
        }

        // GET: api/<LogFileController>
        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                myLogger.WriteToLog("Retrieving log file data via Get request", Utils.Logger.LogMessageType.PROCESS);

                string[] result = myLogger.GetLogFile();
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                WriteToLogFile("LogFile Get, Error: " + ex.Message, Utils.Logger.LogMessageType.ERROR);
                return new JsonResult("LogFile Get, Error: " + ex.Message);
            }
        }

        // POST api/<LogFileController>
        [HttpPost]
        public string Post([FromBody] LogFileInput logfile)
        {
            try
            {
                myLogger.WriteToLog(logfile.Data, Utils.Logger.LogMessageType.PROCESS);
                return "Message added to log file";
            }
            catch (Exception ex)
            {
                WriteToLogFile("LogFile Post, Error:" + ex.Message, Utils.Logger.LogMessageType.ERROR);
                return "Failed to add message to log file";
            }
        }

        // DELETE api/<LogFileController>
        public JsonResult Delete()
        {
            try
            {
                myLogger.DeleteLogFile();
                return new JsonResult("Log file deleted");
            }
            catch (Exception ex)
            {
                WriteToLogFile("LogFile Delete, Error:" + ex.Message, Utils.Logger.LogMessageType.ERROR);
                return new JsonResult("LogFile Delete, Error:" + ex.Message);
            }
        }

        private void WriteToLogFile(string msg, Utils.Logger.LogMessageType msgType)
        {
            myLogger.WriteToLog(msg, msgType);
        }
    }
}
