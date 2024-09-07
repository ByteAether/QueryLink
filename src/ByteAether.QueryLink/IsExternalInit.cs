using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Bug workaround for .net standard
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public record IsExternalInit;