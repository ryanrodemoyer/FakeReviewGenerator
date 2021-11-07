using markov.web.services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace markov.api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMarkov();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "markov.api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "markov.api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

namespace markov.web.services
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;

    public record FakeReview
    {
        // the regex will ensure that punctuation is spaced appropriately so that it's "touching" the word instead of a blank space
        public string review => Regex
                .Replace(string.Join(" ", _words), @" ([.!;?:,])", m => m.Groups[1].Value);

        public int rating => new Random().Next(1, 6);

        private IEnumerable<string> _words;

        public FakeReview(IEnumerable<string> words)
        {
            _words = words;
        }
    }

    public class MarkovQuery
    {
        private readonly Markov _markov;

        public MarkovQuery(Markov markov)
        {
            _markov = markov;
        }

        public FakeReview GetFakeReview()
        {
            var r = new Random();
            int max = r.Next(5, 100);

            var words = _markov.GetNextWord().Take(max).ToList();
            return new FakeReview(words);
        }
    }

    public static class MarkovExtensions
    {
        private class Review
        {
            public string reviewerID { get; set; }
            public string asin { get; set; }
            public string reviewerName { get; set; }
            public string reviewText { get; set; }
            public double overall { get; set; }
            public string summary { get; set; }
            public int unixReviewTime { get; set; }
            public string reviewTime { get; set; }
        }

        public static void AddMarkov(this IServiceCollection services)
        {
            var config = new MarkovConfig(() => {
                // this regex will split the input in to tokens of words as well as punctuation

                List<string> tokens = File.ReadLines("Health_and_Personal_Care_5.json")
                    .Select(x => JsonConvert.DeserializeObject<Review>(x))
                    .Select(x => Regex.Matches(x.reviewText, @"[\w'-]+|[.!?,:;]+").Cast<Match>())
                    .SelectMany(matchList => matchList)
                    .SelectMany(match => match.Groups.Cast<Group>())
                    .Select(group => group.Value)
                    .ToList()
                    ;

                return tokens;
            });

            services.AddSingleton(MarkovBuilder.Build(config));
            services.AddSingleton<MarkovQuery>();
        }
    }
}