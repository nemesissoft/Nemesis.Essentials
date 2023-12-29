using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests.Design;

public class ComparerTests
{
    [Test]
    public void NaturalStringComparer_ShouldYieldResultsInNaturalOrder()
    {
        // arrange
        List<string> data = ["20string", "2string", "3string", "st20ring", "st2ring", "st3ring", "string2", "string20", "string3"];
        data = Shuffle(data);

        // act
        data.Sort(NaturalStringComparer.Default);

        // assert
        List<string> expected = [
            "2string",
            "3string",
            "20string",

            "st2ring",
            "st3ring",
            "st20ring",

            "string2",
            "string3",
            "string20"
        ];
        Assert.That(data, Is.EqualTo(expected));


        static List<string> Shuffle(List<string> list)
        {
            var rand = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                var k = rand.Next(i + 1);
                (list[i], list[k]) = (list[k], list[i]);
            }
            return list;
        }
    }
}