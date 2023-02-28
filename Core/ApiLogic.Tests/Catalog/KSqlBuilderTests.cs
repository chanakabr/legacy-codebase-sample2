using System.Collections.Generic;
using ApiLogic.Catalog;
using NUnit.Framework;
using TVinciShared;

namespace ApiLogic.Tests.Catalog
{
    [TestFixture]
    public class KSqlBuilderTests
    {
        [TestCaseSource(nameof(TestCaseData))]
        public void ValidateKsql(KsqlBuilder actualKsql, string expectedKsql)
        {
            Assert.That(actualKsql.Build(), Is.EqualTo(expectedKsql));
        }

        private static IEnumerable<TestCaseData> TestCaseData()
        {
            yield return new TestCaseData(new KsqlBuilder()
                    .And(x => x
                        .Or(y => y.Values(x.Equal, "asset_type", new[] { 1884, 1885 }))
                        .Equal("seriesId", "GOT")
                        .Or(y => y
                            .And(k => k.Equal("seasonNumber", 1).Greater("episodeNumber", 1))
                            .And(k => k.Greater("seasonNumber", 2).Greater("episodeNumber", 0)))),
                "(and (or asset_type='1884' asset_type='1885') seriesId='GOT' (or (and seasonNumber='1' episodeNumber>'1') (and seasonNumber>'2' episodeNumber>'0')))");
            yield return new TestCaseData(new KsqlBuilder()
                    .And(x => x
                        .Or(y => y.Values(x.Equal, "asset_type", new[] { 1884, 1885 }))
                        .Equal("seriesId", "GOT")
                        .Or(y => y
                            .And(k => k.NotExists("seasonNumber").Greater("episodeNumber", 1))
                            .And(k => k.Greater("seasonNumber", 0).Greater("episodeNumber", 0)))),
                "(and (or asset_type='1884' asset_type='1885') seriesId='GOT' (or (and seasonNumber!+'' episodeNumber>'1') (and seasonNumber>'0' episodeNumber>'0')))");
            yield return new TestCaseData(new KsqlBuilder()
                    .And(x => x
                        .Or(y => y.Values(x.Equal, "asset_type", new[] { 1884, 1885 }))
                        .Equal("seriesId", "GOT")),
                "(and (or asset_type='1884' asset_type='1885') seriesId='GOT')");
        }
    }
}