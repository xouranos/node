﻿using Martiscoin.Consensus.BlockInfo;
using Martiscoin.Consensus.Rules;
using Microsoft.Extensions.Logging;

namespace Martiscoin.Features.PoA.BasePoAFeatureConsensusRules
{
    /// <summary>
    /// Checks that signature from header we wanted to download block data for
    /// is equal to signature in block we've received.
    /// </summary>
    public class PoAIntegritySignatureRule : IntegrityValidationConsensusRule
    {
        /// <inheritdoc />
        public override void Run(RuleContext context)
        {
            BlockSignature expectedSignature = (context.ValidationContext.ChainedHeaderToValidate.Header as PoABlockHeader).BlockSignature;
            BlockSignature actualSignature = (context.ValidationContext.BlockToValidate.Header  as PoABlockHeader).BlockSignature;

            if (expectedSignature != actualSignature)
            {
                this.Logger.LogTrace("(-)[INVALID_SIGNATURE]");
                PoAConsensusErrors.InvalidBlockSignature.Throw();
            }
        }
    }
}
