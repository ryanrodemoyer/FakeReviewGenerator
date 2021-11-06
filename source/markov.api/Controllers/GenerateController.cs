using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace markov.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateController : ControllerBase
    {
        private readonly ILogger<GenerateController> _logger;
        private readonly Markov _markov;

        public GenerateController(
            ILogger<GenerateController> logger
            , Markov markov
            )
        {
            _logger = logger;
            _markov = markov;
        }


        [HttpGet]
        public FakeReview Get()
        {
            var r = new Random();
            int max = r.Next(5, 100);

            var words =_markov.GetNextWord().Take(max).ToList();
            return new FakeReview(words);
        }
    }

    public record FakeReview
    {
        public string review { get; set; }

        public int rating => new Random().Next(1, 6);

        public FakeReview(string review)
        {
            this.review = review;
        }

        public FakeReview(IEnumerable<string> words)
        {
            review = Regex
                .Replace(string.Join(" ", words), @" ([.!;?:,])", m => m.Groups[1].Value);
        }
    }
}
