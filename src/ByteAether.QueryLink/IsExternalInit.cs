using System.ComponentModel;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Bug workaround for .net standard
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public record IsExternalInit;