namespace RaftCore.Components;

/// <summary>
/// Represents a node's log entry.
/// </summary>
public sealed record class LogEntry<TCommand>
{

    /// <summary>
    /// Term in which the entry was received by the leader
    /// </summary>
    public int TermNumber { get; }

    /// <summary>
    /// Index in the log of the entry.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Command associated with the entry.
    /// </summary>
    public TCommand Command { get; }

    /// <summary>
    /// Initializes a log entry given a term number, index and a command
    /// </summary>
    /// <param name="termNumber">Parameter representing the entry's term</param>
    /// <param name="index">Parameter representing the entry's index</param>
    /// <param name="command">Parameter representing the entry's command</param>
    public LogEntry(int termNumber, int index, TCommand command)
    {
        this.TermNumber = termNumber;
        this.Index = index;
        this.Command = command;
    }
}
