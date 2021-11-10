using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MsGraphSDKSnippetsCompiler.Models;

public record CompilationResultsModel(bool IsSuccess, IEnumerable<Diagnostic> Diagnostics, string MarkdownFileName);
#pragma warning disable CA1801 // Review unused parameters (false positive, seems to be fixed in vNext of dotnet: https://github.com/dotnet/roslyn-analyzers/pull/4499/files)
public record ExecutionResultsModel(CompilationResultsModel CompilationResult, bool Success, string ExceptionMessage = null);
