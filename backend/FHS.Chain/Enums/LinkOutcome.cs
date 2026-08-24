namespace FHS.Chain.Enums;

public enum LinkOutcome
{
    Continue = 0,   // run the next link — also the default, so default(LinkResult) is harmless

    Done,           // stop early, successfully

    Fail            // stop, map to a problem response
}