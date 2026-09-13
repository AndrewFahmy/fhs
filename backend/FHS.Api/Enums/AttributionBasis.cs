namespace FHS.Api.Enums;

/// <summary>
/// How an escape came to be linked to an internal defect. The distinction is load-bearing for
/// analytics: an assessed link is somebody's belief, a traced one is established by records.
/// </summary>
public enum AttributionBasis
{
    /// <summary>Someone reviewed the escape and believes it originates from this defect.</summary>
    Assessed,

    /// <summary>Established through records — serial number, batch or lot — linking the returned unit to the defect.</summary>
    Traced
}
