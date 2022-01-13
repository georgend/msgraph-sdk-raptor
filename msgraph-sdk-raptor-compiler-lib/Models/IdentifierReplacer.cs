using Azure.Storage.Blobs;

namespace MsGraphSDKSnippetsCompiler.Models;

public class IdentifierReplacer
{
    /// <summary>
    /// singleton lazy instance
    /// </summary>
    public static IdentifierReplacer Instance => lazy.Value;

    private static readonly Lazy<IdentifierReplacer> lazy = new(() => new IdentifierReplacer());

    /// <summary>
    /// tree of IDs that appear in sample Graph URLs
    /// </summary>
    private readonly IDTree tree;

    /// <summary>
    /// regular expression to match strings like {namespace.type-id}
    /// also extracts namespace.type part separately so that we can use it as a lookup key.
    /// </summary>
    private readonly Regex idRegex = new Regex(@"{([A-Za-z0-9\.]+)\-id}", RegexOptions.Compiled);

    /// <summary>
    /// Regular Expr mapped to replacement texts
    /// </summary>
    internal static readonly List<(Regex SearchRegex, string ReplacementContent)> EdgeCaseRegexes = new()
    {
        // https://docs.microsoft.com/en-us/graph/api/driveitem-delta?view=graph-rest-1.0&tabs=csharp#request-3
        // Swap out this date for to now, which is similar to latest.
        // For new tenants, we cannot pick an arbitrary date since we won't know the state of the tenant.
        new ValueTuple<Regex, string>(
            new Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]+)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?", RegexOptions.Compiled),
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
        )
    };

    public IdentifierReplacer(IDTree tree)
    {
        this.tree = tree;
    }

    /// <summary>
    /// Default constructor which builds the tree from Azure blob storage
    /// </summary>
    private IdentifierReplacer()
    {
        var config = TestsSetup.Config.Value;

        string json;
        if (config.IsLocalRun)
        {
            json = File.ReadAllText(@"identifiers.json");
        }
        else
        {
            const string blobContainerName = "raptoridentifiers";
            var getDomain = (string email) => email.Split('@')[1].Split('.')[0]; // DOMAIN from admin@DOMAIN.onmicrosoft.com
            var blobName = $"identifiers_{getDomain(config.Username)}_{getDomain(config.EducationUsername)}.json";

            var raptorStorageConnectionString = config.RaptorStorageConnectionString;
            var blobClient = new BlobClient(raptorStorageConnectionString, blobContainerName, blobName);

            using var stream = new MemoryStream();
            blobClient.DownloadTo(stream);
            json = new UTF8Encoding().GetString(stream.ToArray());
        }

        tree = JsonSerializer.Deserialize<IDTree>(json);
    }

    public string ReplaceIds(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }
        // Start with Regex Edge Cases replacement.
        var regexEdgeCases = RegexReplaceEdgeCases(EdgeCaseRegexes, input);
        var baseEdgeCases = ReplaceEdgeCases(regexEdgeCases);
        return ReplaceIdsFromIdentifiersFile(baseEdgeCases);
    }

    private static string ReplaceEdgeCases(string input)
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        var edgeCases = new Dictionary<string, string>
            {
                // https://docs.microsoft.com/en-us/graph/api/drive-get-specialfolder?view=graph-rest-1.0&amp%3Btabs=csharp&tabs=csharp#http-request-1
                { "Special[\"{driveItem-id}\"]", "Special[\"music\"]" },
                // https://docs.microsoft.com/en-us/graph/api/driveitem-get-content-format?view=graph-rest-1.0&tabs=csharp
                { "QueryOption(\"format\", \"{format}\")", "QueryOption(\"format\", \"pdf\")"},
                // https://docs.microsoft.com/en-us/graph/api/event-delta?view=graph-rest-1.0&tabs=csharp
                { "new QueryOption(\"startdatetime\", \"{start_datetime}\"", $"new QueryOption(\"startdatetime\", \"{yesterday}\"" },
                { "new QueryOption(\"enddatetime\", \"{end_datetime}\"", $"new QueryOption(\"enddatetime\", \"{now}\"" },
                // https://docs.microsoft.com/en-us/graph/api/driveitem-delta?view=graph-rest-1.0&amp%3Btabs=csharp&tabs=http#request-1
                // Replace 1230919asd190410jlka with driveDelta-id which will allow Replacement downstream
                { ".Delta(\"1230919asd190410jlka\")", ".Delta(\"{driveDelta-id}\")"},
                // https://docs.microsoft.com/en-us/graph/api/directory-deleteditems-get?view=graph-rest-1.0&tabs=csharp
                // Replace directoryObject-id with deletedDirectoryObject-id which will allow Replacement downstream
                { ".Directory.DeletedItems[\"{directoryObject-id}\"]", ".Directory.DeletedItems[\"{deletedDirectoryObject-id}\"]"},
                // https://docs.microsoft.com/en-us/graph/api/rbacapplication-list-roleassignments?view=graph-rest-1.0&tabs=http#example-2-request-using-a-filter-on-principalid
                // PrincipalId is tenant specific data, replace hardcoded id with tenant specific id
                { ".Filter(\" principalId eq 'f1847572-48aa-47aa-96a3-2ec61904f41f'\")", ".Filter(\" principalId eq '{roleAssignmentPrincipal-id}'\")"},
                // this query seems to only work with fully qualified name of the type
                { "Expand(\"eventMessage/event\")", "Expand(\"microsoft.graph.eventMessage/event\")"},
            };

        // each tuple in the list represents:
        // 1. a condition defined on the input (e.g. containing some substring)
        // 2. string to be replaced
        // 3. string to replace with
        var conditionalEdgeCases = new List<Tuple<Func<string, bool>, string, string>>();
        conditionalEdgeCases.Add(new Tuple<Func<string, bool>, string, string>(
            (input) => input.Contains("Expand(\"microsoft.graph.eventMessage/event\")", StringComparison.Ordinal),
            "{message-id}",
            "{eventMessageRequest-id}"
        ));

        foreach (var (key, value) in edgeCases)
        {
            input = input.Replace(key, value, StringComparison.Ordinal);
        }

        foreach (var (condition, oldValue, newValue) in conditionalEdgeCases)
        {
            if (condition(input))
            {
                input = input.Replace(oldValue, newValue, StringComparison.Ordinal);
            }
        }

        return input;
    }

    /// <summary>
    /// Replace text found in input, by Search Regex with ReplacementContent
    /// </summary>
    /// <param name="edgeCaseRegexes"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static string RegexReplaceEdgeCases(List<(Regex SearchRegex, string ReplacementContent)> edgeCaseRegexes, string input)
    {
        foreach (var (searchRegex, replacementContent) in edgeCaseRegexes)
        {
            input = searchRegex.Replace(input, replacementContent);
        }

        return input;
    }

    /// <summary>
    /// Replaces ID placeholders of the form {name-id} by looking up name in the IDTree.
    /// If there is more than one placeholder, it traverses through the tree, e.g.
    /// For input string "sites/{site-id}/lists/{list-id}",
    ///   {site-id} is replaced by root->site entry from the IDTree.
    ///   {list-id} is replaced by root->site->list entry from the IDTree.
    /// </summary>
    /// <param name="input">String containing ID placeholders</param>
    /// <returns>input string, but its placeholders replaced from IDTree</returns>
    private string ReplaceIdsFromIdentifiersFile(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var matches = idRegex.Matches(input);
        IDTree currentIdNode = tree;
        foreach (Match match in matches)
        {
            var id = match.Groups[0].Value;     // e.g. {site-id}
            var idType = match.Groups[1].Value; // e.g. extract site from {site-id}

            currentIdNode.TryGetValue(idType, out IDTree localTree);
            if (localTree == null
                || localTree.Value.EndsWith($"{idType}>", StringComparison.Ordinal)) // placeholder value, e.g. <teamsApp_teamsAppDefinition> for teamsApp->teamsAppDefinition
            {
                throw new InvalidDataException($"no data found for id: {id} in identifiers.json file");
            }
            else
            {
                currentIdNode = localTree;
                input = ReplaceFirst(input, id, currentIdNode.Value);
            }
        }

        return input;
    }

    /// <summary>
    /// Replaces first instance of a substring with a replacement string.
    /// </summary>
    /// <param name="input">String to be modified</param>
    /// <param name="substring">Substring to be replaced</param>
    /// <param name="replacement">Replacement string</param>
    /// <returns>input string with first instance of substring replaced</returns>
    private static string ReplaceFirst(string input, string substring, string replacement)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrEmpty(substring))
        {
            throw new ArgumentNullException(nameof(substring));
        }

        if (replacement == null)
        {
            throw new ArgumentNullException(nameof(replacement));
        }

        var index = input.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return input;
        }

        return string.Concat(input.AsSpan(0, index), replacement, input.AsSpan(index + substring.Length));
    }
}
