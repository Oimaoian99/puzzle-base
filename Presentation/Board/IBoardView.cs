using System;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Input;
using Puzzle.Presentation.Pieces;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Presentation boundary for observing a pure C# Board model.
    /// Synchronizes visual piece instances and routes player interactions
    /// into typed IInputCommand instances without executing puzzle rules.
    /// </summary>
    public interface IBoardView
    {
        Core.Gameplay.Board.Board Board { get; }
        int ActivePieceViewCount { get; }

        void Bind(Core.Gameplay.Board.Board board, Action<IInputCommand> inputReceiver = null);
        void Unbind();
        void Refresh();

        bool HasPieceView(int pieceId);
        IPieceView GetPieceView(int pieceId);

        event Action<IInputCommand> OnInputProduced;
    }
}
