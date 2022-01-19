using FluentAssertions;

namespace UnitTests;

public class LanguageTestDataTests
{
    [TestCase]
    public void JavaClassNameNullFileName()
    {
        var languageTestData = GetMockLanguageTestData(Versions.V1, null);
        languageTestData.JavaClassName.Should().BeNull();
    }

    [TestCase]
    public void JavaClassNameForV1()
    {
        var languageTestData = GetMockLanguageTestData(Versions.V1, "get-user-java-snippets.md");
        languageTestData.JavaClassName.Should().Be("GetUserV1");
    }


    [TestCase]
    public void JavaClassNameForBeta()
    {
        var languageTestData = GetMockLanguageTestData(Versions.Beta, "get-user-java-snippets.md");
        languageTestData.JavaClassName.Should().Be("GetUserBeta");
    }

    private static LanguageTestData GetMockLanguageTestData(Versions version, string fileName)
    {
        return new LanguageTestData
        (
            version,
            false,
            false,
            null,
            null,
            null,
            fileName,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
    }
}
