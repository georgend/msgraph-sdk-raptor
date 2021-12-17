// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "English only. No need for a resource table.", Scope = "member", Target = "~M:ApplicationPermissionsUpdater.ApplicationPermissionsUpdater.UpdatePermissions(MsGraphSDKSnippetsCompiler.PermissionManager)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "English only. No need for a resource table.", Scope = "member", Target = "~M:ApplicationPermissionsUpdater.ApplicationPermissionsUpdater.Main(System.String[])~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We are interested in the success or failure of the operation.", Scope = "member", Target = "~M:ApplicationPermissionsUpdater.ApplicationPermissionsUpdater.UpdatePermissions(MsGraphSDKSnippetsCompiler.PermissionManager)~System.Threading.Tasks.Task")]
