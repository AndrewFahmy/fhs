namespace FHS.Api.Primitives;

public readonly record struct Paging(int Number, int Size)
{
    public const int DefaultSize = 20;
    public const int MaxSize = 100;

    public int Skip => (Number - 1) * Size;

    public static Paging From(int? number, int? size) =>
        new(Math.Max(number ?? 1, 1), Math.Clamp(size ?? DefaultSize, 1, MaxSize));
}
