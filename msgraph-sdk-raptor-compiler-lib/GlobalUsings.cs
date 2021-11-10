global using Azure.Identity;
global using System;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Tasks;
global using MsGraphSDKSnippetsCompiler.Models;
global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.Text;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading;
global using Microsoft.Graph;
global using System.Net.Http;
global using NUnit.Framework;
global using Microsoft.Extensions.Configuration;

// Microsoft.Graph has very generic names in the namespaces
// disambiguate collisions
global using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;
global using Location = Microsoft.CodeAnalysis.Location;

global using Process = System.Diagnostics.Process;

global using Directory = System.IO.Directory;
global using File = System.IO.File;
