# FakeReviewGenerator
This project generates a fake product review based on a dataset from http://jmcauley.ucsd.edu/data/amazon/. Specifically, the http://snap.stanford.edu/data/amazon/productGraph/categoryFiles/reviews_Health_and_Personal_Care_5.json.gz dataset.

# Run
Takes about 30 seconds to generate the model and then the website will expose a Swagger page at `/swagger`.

1. `cd source/markov.api`
1. `dotnet run`

# Project Structure
## markov
Contains the actual logic for generating the Markov chain. There is a dependency included named WeightedRandomSelector that was sourced from https://github.com/viliwonka/WeightedRandomSelector. This dependency enables us to select a random word based on a weight which is important characteristic of a Markov chain. I attempted to implement something on my own but I struggled so I picked this dependency in the interest of time.

The general process is to tokenize every `reviewText` field so that we get a list of words and punctuation. For example, `the quick brown fox; jumped over the fence!` is turned to `['the', 'quick', 'brown', 'fox', ';', 'jumped', 'over', 'the', 'fence', '!']`. Next, we build a frequency table by iterating over each word to track the occurence of the word as well as the word it procedes. With this data we now know how often each word was used as well as the probability that given a word `W` - the chance of the next word. This is where we leverage `WeightedRandomSelector` to help choose the next word based on the frequency data. 

It takes roughly 30 seconds to generate the model based on the Health and Personal Care dataset. I'm positive there are optimizations to make to this algorithm but defered based on the interest of a time commitment for this project. In the API, the model is generated at startup and then cached so that subsequent calls are quick.

## markov.api
The ASP.NET Core website (based on .NET 5) that creates the model based on the training data and exposes the review through the `/api/generate` HTTP GET endpoint.

In the interest of storage space, the data set was not included as part of source control. The user will need to download a dataset and update `Startup.cs`=>line 127 with their filename.

Example Review
```json
{
  "rating": 5,
  "review": "Sharing the inside of tasks done after 4 FL OZ, viseral fat. It's just this model on my u can work they are glass of KMF solves my amazement that I'd accidentally take it in for such as for a favor and kidney function. At the way so I tried, especially."
}
```

## markov.tests
Project unit tests.
