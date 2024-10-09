using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using RaftCore.Components;
using System.Numerics;

namespace RaftCore.Connections;

/// <summary>
/// Represents a cluster of Raft nodes. Contains one <see cref="IRaftConnector{TID,TCommand}"/> for each node.
/// using <typeparamref name="TCommand" /> as the message data and <typeparamref name="TID" /> as the node id
/// </summary>
public class RaftCluster<TID, TCommand>
    where TID : unmanaged, IEqualityOperators<TID, TID, bool>
{

    private readonly List<IRaftConnector<TID, TCommand>> _nodes = [];

    /// <summary>
    /// Number of nodes in the cluster.
    /// </summary>
    public int Size
    {
        get
        {
            return _nodes.Count;
        }
    }

    /// <summary>
    /// Adds a node's <see cref="IRaftConnector{TID,TCommand}"/> to the cluster.
    /// </summary>
    /// <param name="node"></param>
    public void AddNode(IRaftConnector<TID, TCommand> node)
    {
        this._nodes.Add(node);
    }

    /// <summary>
    /// Redirects a user request to a specified node.
    /// </summary>
    /// <param name="command">User request to redirect</param>
    /// <param name="leaderId">Node to redirect the request to</param>
    public void RedirectRequestToNode(TCommand command, TID? leaderId)
    {
        _nodes.Find(x => x.NodeId == leaderId.Value).MakeRequest(command);
    }

    /// <summary>
    /// Randomly calculates the election timeout for a node in the cluster.
    /// This calculation is based on the return value of <see cref="CalculateBroadcastTimeMS"/>
    /// The election timeout will be a random number between 12 and 24 times that of the broadcast time.
    /// </summary>
    /// <returns>A randomized election timeout appropiate to the cluster size and characteristics</returns>
    public int CalculateElectionTimeoutMS()
    {
        int broadcastTime = CalculateBroadcastTimeMS();
        // Ensures the election timeout is one order of magnitude bigger than the broadcast time
        return Random.Shared.Next(broadcastTime * 12, broadcastTime * 24);
    }

    /// <summary>
    /// Calculates the broadcast time: That is, the average time it takes a server to:
    /// <list type="number">
    /// <item>
    /// <description>send RPCs in parallel to every server in the cluster</description>
    /// </item>
    /// <item>
    /// <description>receive their responses.</description>
    /// </item>
    /// </list>
    /// The method uses the node's <see cref="RaftNode{TID,TCommand}.TestConnection"/>
    /// The broadcast time will always be at least 25 ms.
    /// </summary>
    /// <returns>The estimated broadcast time of the cluster, or 25. Whichever is higher.</returns>
    public int CalculateBroadcastTimeMS()
    {
        var stopWatch = Stopwatch.StartNew();

        Parallel.ForEach(_nodes, x => x.TestConnection());

        stopWatch.Stop();

        int elapsedMS = (int)stopWatch.ElapsedMilliseconds;

        return Math.Max(25, elapsedMS);
    }

    /// <summary>
    /// Returns the ids of all the nodes in the cluster except for the specified one.
    /// </summary>
    /// <param name="nodeId">Node ID to exclude</param>
    /// <returns>List of node IDs without the specified one</returns>
    public List<TID> GetNodeIdsExcept(TID nodeId)
    {
        return _nodes.Where(x => x.NodeId != nodeId).Select(x => x.NodeId).ToList();
    }

    /// <summary>
    /// Sends the AppendEntry message to the specified <see cref="IRaftConnector"/>
    /// </summary>
    /// <param name="nodeId">Node that will receive the request</param>
    /// <param name="term">Leader's current term number</param>
    /// <param name="leaderId">ID of the node invoking this method</param>
    /// <param name="prevLogIndex">Index of log immediately preceding new ones</param>
    /// <param name="prevLogTerm">Term of prevLogIndex entry</param>
    /// <param name="entries">List of entries sent to be replicated. null for heartbeat</param>
    /// <param name="leaderCommit">Leader's CommitIndex</param>
    /// <returns>Returns a Result object containing the current term of the node and whether the request worked</returns>
    public Result<bool> SendAppendEntriesTo(TID nodeId, int term, TID leaderId, int prevLogIndex,
                                   int prevLogTerm, List<LogEntry<TCommand>> entries, int leaderCommit)
    {
        return _nodes.Find(x => x.NodeId == nodeId).AppendEntries(term, leaderId, prevLogIndex,
                                   prevLogTerm, entries, leaderCommit);
    }

    /// <summary>
    /// Sends the RequestVote message to the specified <see cref="IRaftConnector"/>
    /// </summary>
    /// <param name="nodeId">Node that will receive the request</param>
    /// <param name="term">Term of the candidate</param>
    /// <param name="candidateId">Node ID of the candidate</param>
    /// <param name="lastLogIndex">Index of candidate's last log entry</param>
    /// <param name="lastLogTerm">Term of candidate's last log entry</param>
    /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
    public Result<bool> RequestVoteFrom(TID nodeId, int term, TID candidateId, int lastLogIndex, int lastLogTerm)
    {
        return _nodes.Find(x => x.NodeId == nodeId).RequestVote(term, candidateId, lastLogIndex, lastLogTerm);
    }
}
