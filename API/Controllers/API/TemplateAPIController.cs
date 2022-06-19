using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.API
{
    [ApiController]
    [Route("/api/template")]
    public class TemplateAPIController : ControllerBase
    {
        #region Public Methods

        [HttpGet()]
        public IActionResult Index()
        {
            return Ok(new
            {
                message = "This is the template",
                time = DateTime.Now.ToString("MM/dd/yyyy - HH:mm:ss.fff")
            });
        }

        #endregion Public Methods
    }
}