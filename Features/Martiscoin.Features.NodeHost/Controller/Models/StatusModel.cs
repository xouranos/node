﻿using System;
using System.Collections.Generic;
using Martiscoin.Controllers.Converters;
using Newtonsoft.Json;

namespace Martiscoin.Controllers.Models
{
    /// <summary>
    /// Class representing the status of the currently running node.
    /// </summary>
    public class StatusModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusModel"/> class.
        /// </summary>
        public StatusModel()
        {
            this.InboundPeers = new List<ConnectedPeerModel>();
            this.OutboundPeers = new List<ConnectedPeerModel>();
            this.FeaturesData = new List<FeatureData>();
        }

        /// <summary>The node's user agent that will be shared with peers in the version handshake.</summary>
        public string Agent { get; set; }

        /// <summary>The node's version.</summary>
        public string Version { get; set; }

        /// <summary>The public IP address of the node, either specified in config or ascertained by the endpoint tracker.</summary>
        public string ExternalAddress { get; set; }

        /// <summary>The network the current node is running on.</summary>
        public string Network { get; set; }

        /// <summary>The coin ticker to use with external applications.</summary>
        public string CoinTicker { get; set; }

        /// <summary>System identifier of the node's process.</summary>
        public int ProcessId { get; set; }

        /// <summary>The height of the consensus.</summary>
        public int? ConsensusHeight { get; set; }

        /// <summary>Height of the most recent block in persistent storage.</summary>
        /// <seealso cref="Martiscoin.Features.BlockRepository.HighestPersistedBlock.Height"/>
        public int BlockStoreHeight { get; set; }

        /// <summary>The tip of the most advanced peer our node is connected to or <c>null</c> if no peer is connected.</summary>
        public int? BestPeerHeight { get; set; }

        /// <summary>A collection of inbound peers.</summary>
        public List<ConnectedPeerModel> InboundPeers { get; set; }

        /// <summary>A collection of outbound peers.</summary>
        public List<ConnectedPeerModel> OutboundPeers { get; set; }

        /// <summary>A collection of all the features enabled by this node.</summary>
        public List<FeatureData> FeaturesData { get; set; }

        /// <summary>The path to the directory where the data is saved.</summary>
        public string DataDirectoryPath { get; set; }

        /// <summary>Time this node has been running.</summary>
        public TimeSpan RunningTime { get; set; }

        /// <summary>The current network difficulty target.</summary>
        public double Difficulty { get; set; }

        /// <summary>The node's protocol version</summary>
        public uint ProtocolVersion { get; set; }

        /// <summary>Is the node on the testnet.</summary>
        public bool Testnet { get; set; }

        /// <summary>The current transaction relay fee.</summary>
        [JsonConverter(typeof(BtcDecimalJsonConverter))]
        public decimal RelayFee { get; set; }

        /// <summary>Returns the status of the node.</summary>
        public string State { get; set; }
    }

    /// <summary>
    /// Class containing some details about the features loaded by this node.
    /// </summary>
    public class FeatureData
    {
        /// <summary>
        /// The namespace of the feature.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// The state in which the feature currently is.
        /// </summary>
        public string State { get; set; }
    }
}
