namespace Puzzle.Core.Input
{
    public interface IInputCommand
    {
        string CommandName { get; }
        float Timestamp { get; }
    }

    public interface IInputReceiver
    {
        void ReceiveCommand(IInputCommand command);
    }
}
