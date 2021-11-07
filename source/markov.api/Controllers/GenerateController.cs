using markov.api.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace markov.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateController : ControllerBase
    {
        private readonly ILogger<GenerateController> _logger;
        private readonly MarkovQuery _markovQuery;

        public GenerateController(
            ILogger<GenerateController> logger
            , MarkovQuery markovQuery
            )
        {
            _logger = logger;
            _markovQuery = markovQuery;
        }


        [HttpGet]
        public IActionResult Get()
        {
            FakeReview res = _markovQuery.GetFakeReview();
            return Ok(res);
        }
    }
}
