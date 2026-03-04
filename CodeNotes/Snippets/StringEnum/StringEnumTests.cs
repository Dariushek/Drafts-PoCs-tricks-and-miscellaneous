namespace Snippets;

public class StringEnumTests
{
    [Test]
    public void Test()
    {
        StringEnum optionA = StringEnum.OptionA;
        StringEnum optionA2 = StringEnum.OptionA;
        StringEnum optionB = StringEnum.OptionB;

        TestContext.Out.WriteLine(optionA.ToString());
        TestContext.Out.WriteLine(optionB.ToString());
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(optionA2, Is.EqualTo(optionA));
            Assert.That(optionA2, Is.SameAs(optionA));

            Assert.That(optionB, Is.Not.EqualTo(optionA));

            Assert.That(optionA, Is.EqualTo(optionA2));
        }
        Assert.That(optionA, Is.Not.EqualTo(optionB));
    }
}