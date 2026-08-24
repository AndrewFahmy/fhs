using FHS.Chain.Primitives;

namespace FHS.Api.Extensions;

public static class ResultExtensions
{
    extension<TValue>(Result<TValue> result)
    {
        public IResult Match(Func<TValue, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Error.ToProblem();

        public IResult Match(Func<IResult> onSuccess) =>
            result.IsSuccess ? onSuccess() : result.Error.ToProblem();

        public IResult ToOk() =>
            result.Match(Results.Ok);

        public IResult ToNoContent() =>
            result.Match(Results.NoContent);

        public IResult ToCreated(Func<TValue, string> location) =>
            result.Match(value => Results.Created(location(value), value));
    }
}