﻿using System.Collections.Generic;
using Martiscoin.NBitcoin;
using Martiscoin.P2P.Protocol.Payloads;

namespace Martiscoin.Features.PoA.Payloads
{
    /// <summary>
    /// Block headers received as a response to getheaders messages.
    /// </summary>
    [Payload("poahdr")]
    public class PoAHeadersPayload : Payload
    {
        private List<PoABlockHeader> headers;

        public List<PoABlockHeader> Headers
        {
            get { return this.headers; }
        }

        public PoAHeadersPayload()
        {
            this.headers = new List<PoABlockHeader>();
        }

        public PoAHeadersPayload(IList<PoABlockHeader> headers)
        {
            this.headers = new List<PoABlockHeader>(headers);
        }

        public override void ReadWriteCore(BitcoinStream stream)
        {
            stream.ReadWrite(ref this.headers);
        }
    }
}