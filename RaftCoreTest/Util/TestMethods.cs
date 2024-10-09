using System;
using Xunit;
using System.Collections.Generic;
using RaftCore;
using RaftCore.StateMachine.Implementations;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;

namespace RaftCoreTest.Util;

internal enum SM { Numeral, Dictionary };

public static class TestMethods
{
    internal const int WAIT_MS = 1000;

    // Creates a single-node cluster with a numeral state machine and returns the node
    static internal RaftNode<uint, string> CreateNode()
    {
        var node = new RaftNode<uint, string>(1, new NumeralStateMachine());
        var c = new RaftCluster<uint, string>();
        c.AddNode(new ObjectRaftConnector<uint, string>(node.NodeId, node));
        node.Configure(c);
        return node;
    }

    // Creates and returns a configured array of raftnodes using the test cluster
    static internal RaftNode<uint, string>[] ConfigureRaftCluster(int numberOfNodes, SM sm)
    {
        var nodes = new RaftNode<uint, string>[numberOfNodes];

        // Create nodes
        for (uint i = 0; i < numberOfNodes; i++)
        {
            if (sm == SM.Numeral)
            {
                nodes[i] = new RaftNode<uint, string>(i, new NumeralStateMachine());
            }
            else
            {
                nodes[i] = new RaftNode<uint, string>(i, new DictionaryStateMachine());
            }
        }

        // Adding them to a cluster and configuring them
        foreach (var node in nodes)
        {
            var c = new RaftCluster<uint, string>();
            Array.ForEach(nodes, x => c.AddNode(new ObjectRaftConnector<uint, string>(x.NodeId, x)));
            node.Configure(c);
        }

        return nodes;
    }

    static internal RaftNode<uint, string>[] ConfigureAndRunRaftCluster(int numberOfNodes, SM sm)
    {
        var nodes = ConfigureRaftCluster(numberOfNodes, sm);
        Array.ForEach(nodes, node => node.Run());
        return nodes;
    }
}