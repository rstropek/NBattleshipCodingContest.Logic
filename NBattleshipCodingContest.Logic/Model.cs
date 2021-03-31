using System;

namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}

namespace NBattleshipCodingContest.Logic
{
    // Note the use of records here. Read more at
    // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#records

    public record ShotRequest(Guid GameId, int Shooter, int Opponent, IReadOnlyBoard BoardShooterView, BoardIndex? LastShot = null);

    public record ShotRequestManagerView(Guid GameId, int Shooter, int Opponent, BoardContent BoardShooterView, IReadOnlyBoard SolutionBoard);

    public record ShotResponse(Guid GameId, BoardIndex Index);

    public record ShotResult(Guid GameId, SquareContent SquareContent);

}
