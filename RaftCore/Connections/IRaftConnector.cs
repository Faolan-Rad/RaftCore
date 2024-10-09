using System;
using System.Collections.Generic;
using System.Numerics;
using RaftCore.Components;

namespace RaftCore.Connections;

/// <summary>
/// Defines how to connect and interact with a node.
/// </summary>
public interface IRaftConnector
{
}

/// <summary>
/// Defines how to connect and interact with a node.
/// using <typeparamref name="TID" /> as the node id
/// </summary>
public interface IRaftConnector<TID> : IRaftConnector
        where TID : unmanaged, IEqualityOperators<TID, TID, bool>
{

    /// <summary>
    /// ID uniquely identifying the node this <see cref="IRaftConnector{TID}"/> connects to.
    /// </summary>
    TID NodeId { get; }

    /// <summary>
    /// Calls the RequestVote method on the node.
    /// </summary>
    /// <param name="term">Term of the candidate</param>
    /// <param name="candidateId">Node ID of the candidate</param>
    /// <param name="lastLogIndex">Index of candidate's last log entry</param>
    /// <param name="lastLogTerm">Term of candidate's last log entry</param>
    /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
    Result<bool> RequestVote(int term, TID candidateId, int lastLogIndex, int lastLogTerm);

    /// <summary>
    /// Calls the TestConnection method on the node.
    /// </summary>
    void TestConnection();
}

/// <summary>
/// Defines how to connect and interact with a node.
/// using <typeparamref name="TCommand" /> as the message data and <typeparamref name="TID" /> as the node id
/// </summary>
public interface IRaftConnector<TID, TCommand> : IRaftConnector<TID>
        where TID : unmanaged, IEqualityOperators<TID, TID, bool>
{

    /// <summary>
    /// Calls the MakeRequest method on the node.
    /// </summary>
    /// <param name="command"><typeparamref name="TCommand" /> containing the request to send to the node</param>
    void MakeRequest(TCommand command);

    /// <summary>
    /// Calls the AppendEntries method on the node.
    /// </summary>
    /// <param name="term">Leader's current term number</param>
    /// <param name="leaderId">ID of the node invoking this method</param>
    /// <param name="prevLogIndex">Index of log immediately preceding new ones</param>
    /// <param name="prevLogTerm">Term of prevLogIndex entry</param>
    /// <param name="entries">List of entries sent to be replicated. null for heartbeat</param>
    /// <param name="leaderCommit">Leader's CommitIndex</param>
    /// <returns>Returns a Result object containing the current term of the node and whether the request worked</returns>
    Result<bool> AppendEntries(int term, TID leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry<TCommand>> entries, int leaderCommit);

}
