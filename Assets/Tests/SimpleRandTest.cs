using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using BuildingGen.Components;
using System.Text.RegularExpressions;
using AdaptiveGrid;

[TestFixture]
public class SimpleRandTest
{
    [Test]
    public void TestGetIterator()
    {
        int from = 0;
        int to = 999;
        int start = 511;
        int expectedCount = to - from;
        HashSet<int> results = new HashSet<int>();

        var iterator = SimpleRand.Sequence(from, to, start);
        foreach (var value in iterator)
        {
            results.Add(value);
        }

        Assert.AreEqual(expectedCount, results.Count);
        for (int i = from; i < to; i++)
        {
            Assert.IsTrue(results.Contains(i), $"Value {i} is missing in the results.");
        }
    }
}