using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers
{
    [ApiController]
    [Route("tags")]
    public class TagController : ControllerBase
    {
        #region Public Methods

        [HttpGet("{tag?}")]
        public JsonResult Index([FromRoute] string? tag)
        {
            if (tag == null)
                return new JsonResult(Tags.GetTags());
            return new JsonResult(
                Tags.GetTags().FirstOrDefault(i =>
                {
                    if (int.TryParse(tag, out int id))
                    {
                        return i.Key == id;
                    }
                    else
                    {
                        return i.Value == tag;
                    }
                }, new KeyValuePair<int, string>())
            );
        }

        #endregion Public Methods
    }
}