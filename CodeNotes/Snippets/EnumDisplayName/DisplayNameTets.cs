namespace Snippets;

public class DisplayNameTets
{
    [Test]
    public void TestGetDisplayName()
    {
        string displayName = UserRole.SystemAdmin.GetDisplayName();
        TestContext.Out.WriteLine(displayName);
    }
}