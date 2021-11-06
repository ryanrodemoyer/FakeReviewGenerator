using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DataStructures.RandomSelector;
using Newtonsoft.Json;

namespace markov
{
	public class NextWord
    {

    }

    public class Markov
    {
        public Dictionary<string, Result> Model { get; }

		public Markov(Dictionary<string, Result> model)
		{
			Model = model;
		}

		public string Random => Selector.SelectRandomItem();

		public IEnumerable<string> GetNextWord()
        {
			bool first = true;

			string word = null;
			if (first)
            {
				var selec = new DynamicRandomSelector<string>();

				var startingWords = Model.Values.Where(x => x.IsStartingWord);
				int totalCount = startingWords.Sum(x => x.Counter);
				foreach (var record in startingWords)
				{
					float raw = (float)record.Counter / (float)totalCount;

					selec.Add(record.Value, raw);
				}

				selec.Build();

				word = selec.SelectRandomItem();
				yield return Model[word].Random;
			}

			while (true)
            {
				word = Model[word].Random;
				yield return word;
            }
        }

        private DynamicRandomSelector<string> _selector = null;
        private DynamicRandomSelector<string> Selector
        {
            // not thread safe (yet)
            get
            {
                if (_selector == null)
                {
                    _selector = new DynamicRandomSelector<string>();

                    foreach (KeyValuePair<string, Result> record in Model)
                    {
                        float raw = (float)record.Value.Counter / (float)Model.Keys.Count;

                        _selector.Add(record.Key, raw);
                    }

                    _selector.Build();
                }

                return _selector;
            }
        }
    }

	public static class MarkovBuilder {
		public static Markov Build(string filename)
		{
			string regex = @"[\w'-]+|[.!?,:;]+";

			List<string> tokens = File.ReadLines(filename)
				.Take(5000)
				.Select(x => JsonConvert.DeserializeObject<Review>(x))
				.Select(x => Regex.Matches(x.reviewText, regex).Cast<Match>())
				.SelectMany(x => x)
				.SelectMany(x => x.Groups.Cast<Group>())
				.Select(x => x.Value)
				.ToList()
				;

			var model = new Dictionary<string, Result>();

			string prev = null;

			for (int i = 0; i < tokens.Count; i++)
			{
				string token = tokens[i];

				if (model.ContainsKey(token))
				{
					model[token].Counter++;

					if (model[prev].NextToken.ContainsKey(token))
					{
						model[prev].NextToken[token]++;
					}
					else
					{
						model[prev].NextToken.Add(token, 1);
					}
				}
				else
				{
					if (prev == null)
					{
						model.Add(token, new Result(token));
					}
					else
					{
						model.Add(token, new Result(token));

						if (model[prev].NextToken.ContainsKey(token))
						{
							model[prev].NextToken[token]++;
						}
						else
						{
							model[prev].NextToken.Add(token, 1);
						}
					}
				}

				prev = token;
			}

			var markov = new Markov(model);
			return markov;
		}
	}

	public class Result
	{
		public string Value { get; set; }

        public bool IsStartingWord => Regex.IsMatch(Value, @"[.!?]");


        public int Counter { get; set; } = 1;
		public Dictionary<string, int> NextToken { get; set; } = new Dictionary<string, int>();

		public Result(string value)
		{
			Value = value;
		}

		public string Random => Selector.SelectRandomItem();

		private DynamicRandomSelector<string> _selector = null;
		private DynamicRandomSelector<string> Selector
		{
			// not thread safe (yet)
			get
			{
				if (_selector == null)
				{
					_selector = new DynamicRandomSelector<string>();

					foreach (KeyValuePair<string, int> record in NextToken)
					{
						float raw = (float)record.Value / (float)Counter;

						_selector.Add(record.Key, raw);
					}

					_selector.Build();
				}

				return _selector;
			}
		}
	}

	public class Review
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

}
