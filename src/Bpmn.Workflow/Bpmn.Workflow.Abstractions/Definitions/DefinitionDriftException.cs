namespace Bpmn.Workflow.Definitions;

/// <summary>
/// Raised when an instance pins a definition (id + version) whose structural hash no
/// longer matches the one currently registered — i.e. the developer changed the
/// definition body without bumping the version.
/// </summary>
public class DefinitionDriftException : Exception
{
    public DefinitionDriftException(string definitionId, int version, string expectedHash, string actualHash)
        : base($"Definition drift for '{definitionId}' v{version}: expected hash '{expectedHash}', found '{actualHash}'.")
    {
        DefinitionId = definitionId;
        Version = version;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
    }

    public string DefinitionId { get; }

    public int Version { get; }

    public string ExpectedHash { get; }

    public string ActualHash { get; }
}
