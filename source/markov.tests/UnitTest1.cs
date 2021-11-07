using markov.api.services;
using NUnit.Framework;

namespace markov.tests
{
    public class FakeReviewTests
    {
        [Test]
        public void OutputsOnlyWholeSentences()
        {
            // arrange
            string input = "The Sadly, he can get the smell, I cannot tell you 8217; tied to keep it just a short in and my original only remove dust! in two automobile tire pressure at the rotating head";

            // act 
            var review = new FakeReview(input.Split(" "));

            // assert
            string expected = "The Sadly, he can get the smell, I cannot tell you 8217; tied to keep it just a short in and my original only remove dust!";
            Assert.AreEqual(expected, review.review);
        }
    }
}