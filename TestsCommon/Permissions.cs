using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestsCommon
{
    public class Permissions
    {
        public List<string> Application;
        public List<string> DelegatedWork;
        public List<string> DelegatedPersonal;

        private enum ParseState
        {
            Searching,
            FoundTitle,
            Processing,
            PermissionLine
        }

        private const string TitleLine = "|Permissiontype|Permissions(fromleasttomostprivileged)"; // TODO
        private const string DashLine = "|:--"; // TODO
        private const string ApplicationStart = "|Application|"; // TODO
        private const string DelegatedWorkStart = "|Delegated(workorschoolaccount)|";
        private const string DelegatedPersonalStart = "|Delegated(personalMicrosoftaccount)|";
        private const string NotSupported = "Notsupported.";

        public Permissions()
        {
            Application = new List<string>();
            DelegatedPersonal = new List<string>();
            DelegatedWork = new List<string>();
        }

        public static Permissions CreateFromFileContents(string[] fileContent)
        {
            if (fileContent == null)
            {
                throw new ArgumentNullException(nameof(fileContent));
            }

            var permissions = new Permissions();
            var state = ParseState.Searching;
            foreach (var line in fileContent)
            {
                var trimmedLine = line.Trim().Replace(" ", "");
                switch (state)
                {
                    case ParseState.Searching:
                        if (trimmedLine.StartsWith(TitleLine, StringComparison.OrdinalIgnoreCase))
                        {
                            state = ParseState.FoundTitle;
                        }
                        break;
                    case ParseState.FoundTitle:
                        if (!trimmedLine.Contains(DashLine))
                        {
                            throw new InvalidDataException("malformed markdown table for permissions");
                        }
                        state = ParseState.Processing;
                        break;
                    case ParseState.Processing:
                        if (trimmedLine.StartsWith(ApplicationStart, StringComparison.OrdinalIgnoreCase))
                        {
                            permissions.Application = GetPermissionList(trimmedLine, ApplicationStart);
                        }
                        else if (trimmedLine.StartsWith(DelegatedPersonalStart, StringComparison.OrdinalIgnoreCase))
                        {
                            permissions.DelegatedPersonal = GetPermissionList(trimmedLine, DelegatedPersonalStart);
                        }
                        else if (trimmedLine.StartsWith(DelegatedWorkStart, StringComparison.OrdinalIgnoreCase))
                        {
                            permissions.DelegatedWork = GetPermissionList(trimmedLine, DelegatedWorkStart);
                        }
                        else if (string.Empty == trimmedLine.Trim())
                        {
                            return permissions;
                        }
                        else
                        {
                            throw new InvalidDataException("Unexpected row in the permissions list!");
                        }
                        break;
                }
            }

            throw new InvalidDataException("No permissions found in the file!");
        }

        public static List<string> GetPermissionList(string line, string key)
        {
            return line?.Substring(key?.Length ?? 0)
                .Replace("|", "")   // remove trailing markdown table border
                .Replace(" ", "")   // remove spaces
                .Split(",")
                .Where(x => x != NotSupported)
                .ToList();
        }
    }
}
