namespace FHS.Chain.Contracts;

/// <summary>
/// Empty base. Its only jobs are to anchor the generic constraint and to give
/// zero-surface shared links something to target (<c>SaveChanges : ILink&lt;ChainState&gt;</c>).
/// </summary>
public abstract class ChainState;
