﻿using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Azure.Storage.Blobs;

namespace MsGraphSDKSnippetsCompiler.Models
{
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
        private readonly Regex idRegex = new Regex(@"{([A-Za-z\.]+)\-id}", RegexOptions.Compiled);

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
                const string blobName = "identifiers.json";

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
            return ReplaceIdsFromIdentifiersFile(ReplaceEdgeCases(input));
        }

        private string ReplaceEdgeCases(string input)
        {
            var edgeCases = new Dictionary<string, string>
            {
                // https://docs.microsoft.com/en-us/graph/api/drive-get-specialfolder?view=graph-rest-1.0&amp%3Btabs=csharp&tabs=csharp#http-request-1
                { "Special[\"{driveItem-id}\"]", "Special[\"music\"]" },
            };

            foreach (var (key, value) in edgeCases)
            {
                input = input.Replace(key, value);
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
                    input = input.Replace(id, currentIdNode.Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            return input;
        }
    }
}
