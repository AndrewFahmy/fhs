using System.Diagnostics;
using FHS.Chain.Contracts;
using FHS.Chain.Enums;
using FHS.Chain.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace FHS.Chain;

public sealed class ChainRunner(IServiceProvider services)
{
    public const string ActivitySourceName = "FHS.Chain";
    private static readonly ActivitySource Activities = new(ActivitySourceName);

    public Task<Result> RunAsync<TState>(Chain<TState> chain, TState state, CancellationToken ct)
        where TState : ChainState => RunCoreAsync(chain, state, ct);

    public async Task<Result<TResult>> RunAsync<TState, TResult>(
        Chain<TState, TResult> chain,
        TState state,
        CancellationToken ct
    )
        where TState : ChainState<TResult>
    {
        var result = await RunCoreAsync(chain, state, ct);

        return result.IsSuccess ? Result<TResult>.Success(state.Result) : Result<TResult>.Fail(result.Error);
    }

    private async Task<Result> RunCoreAsync<TState>(Chain<TState> chain, TState state, CancellationToken ct)
        where TState : ChainState
    {
        using var chainActivity = Activities.StartActivity($"chain {chain.Name}");
        chainActivity?.SetTag("chain.name", chain.Name);

        var current = string.Empty;

        try
        {
            foreach (var descriptor in chain.Links)
            {
                current = descriptor.Name;

                using var activity = Activities.StartActivity($"link {descriptor.Name}");
                activity?.SetTag("chain.name", chain.Name);
                activity?.SetTag("link.name", descriptor.Name);

                var link = (ILink<TState>)services.GetRequiredService(descriptor.Type);
                var outcome = await link.RunAsync(state, ct);

                activity?.SetTag("link.outcome", outcome.Kind.ToString());

                var result = ParseLinkResult(outcome, chainActivity);

                if (result != null)
                    return result.Value;
            }

            chainActivity?.SetTag("chain.outcome", "success");
            return Result.Success;
        }
        catch (Exception ex)
        {
            chainActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            chainActivity?.SetTag("chain.outcome", "threw");
            chainActivity?.SetTag("chain.failed_link", current);
            chainActivity?.AddException(ex);
            throw;
        }
    }

    private Result? ParseLinkResult(LinkResult outcome, Activity? chainActivity)
    {
        switch (outcome.Kind)
        {
            case LinkOutcome.Fail:
                chainActivity?.SetTag("chain.outcome", "fail");
                chainActivity?.SetTag("chain.error", outcome.Error!.Code);
                return Result.Fail(outcome.Error!);

            case LinkOutcome.Done:
                chainActivity?.SetTag("chain.outcome", "done");
                return Result.Success;

            default:
                return null;
        }
    }
}
