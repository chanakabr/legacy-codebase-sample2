using ElasticSearch.Searcher;
using NUnit.Framework;

namespace ElasticSearch.Test
{
    [TestFixture]
    public class ESMatchQueryTests
    {
        [Test]
        public void ToString_ForQueryWithBackSlash_EscapesSpecialCharacter()
        {
            var exactSearchValue = @"Back\Slash";

            // same initialization as in IndexManagerV2 line 3979: matchQuery = new ESMatchQuery
            var esMatchQuery = new ESMatchQuery
            {
                Field = "name",
                Query = exactSearchValue,
            };

            var actualResult = esMatchQuery.ToString();

            var wrongResult = @"{ ""match"": { ""name"":{""query"": ""Back\Slash"", ""operator"": ""OR"" }}}";
            Assert.AreNotEqual(wrongResult, actualResult);
            
            var correctResult = @"{ ""match"": { ""name"":{""query"": ""Back\\Slash"", ""operator"": ""OR"" }}}";
            Assert.AreEqual(correctResult, actualResult);
        }
    }
}