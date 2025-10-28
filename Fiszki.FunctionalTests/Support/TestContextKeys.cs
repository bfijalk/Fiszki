namespace Fiszki.FunctionalTests.Support;

public static class TestContextKeys
{
    public const string Browser = nameof(Browser);
    public const string Page = nameof(Page);
    public const string BaseUrl = nameof(BaseUrl);
    
    // Test user management keys
    public const string TestUserEmail = nameof(TestUserEmail);
    public const string TestUserPassword = nameof(TestUserPassword);
    public const string TestUserId = nameof(TestUserId);
}
