using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace micro_email.Controllers
{
    public class EmailData
    {
        public string EmailAddress { get;set; }
        public string Message { get;set; }
    }

    [Route("email")]
    public class EmailController: Controller
    {
        private ILogger _logger;

        public EmailController(ILogger<EmailController> logger)
        {
           _logger = logger; 
        }

        [HttpPost]
        public string Email([FromBody]EmailData data)
        {
            _logger.LogInformation($"Sending email to - {data.EmailAddress} - with message - {data.Message}");
            return "Email sent succesfully";
        }
    }
}