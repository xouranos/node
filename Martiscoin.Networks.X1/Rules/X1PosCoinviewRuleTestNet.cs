using Martiscoin.Consensus;
using Martiscoin.Consensus.TransactionInfo;
using Martiscoin.Features.Consensus;
using Martiscoin.NBitcoin;
using Martiscoin.Utilities;
using Microsoft.Extensions.Logging;

namespace Martiscoin.Networks.X1.Rules
{
    /// <summary>
    /// The XDSTest network uses a pre-mine to catch up with the coin supply
    /// on XDSMain (XDSTest was created almost a year later).
    /// </summary>
    public class X1PosCoinviewRuleTestNet : X1PosCoinviewRule
    {
        /// <inheritdoc />
        public override Money GetProofOfWorkReward(int height)
        {
            if (height == this.consensus.PremineHeight)
                return this.consensus.PremineReward;

            return base.GetProofOfWorkReward(height);
        }

        /// <inheritdoc />
        public override Money GetProofOfStakeReward(int height)
        {
            if (height == this.consensus.PremineHeight)
                return this.consensus.PremineReward;

            return base.GetProofOfStakeReward(height);
        }

        protected override Money GetTransactionFee(UnspentOutputSet view, Transaction tx)
        {
            Money fee = base.GetTransactionFee(view, tx);

            if (!tx.IsProtocolTransaction())
            {
                if (fee < ((X1Main)this.Parent.Network).AbsoluteMinTxFee)
                {
                    this.Logger.LogTrace($"(-)[FAIL_{nameof(X1RequireWitnessRule)}]".ToUpperInvariant());
                    X1ConsensusErrors.FeeBelowAbsoluteMinTxFee.Throw();
                }
            }

            return fee;
        }

        protected override void CheckInputValidity(Transaction transaction, UnspentOutput coins)
        {
            return;
        }

        /// <inheritdoc />
        public override void CheckMaturity(UnspentOutput coins, int spendHeight)
        {
            base.CheckCoinbaseMaturity(coins, spendHeight);

            if (coins.Coins.IsCoinstake)
            {
                if ((spendHeight - coins.Coins.Height) < this.consensus.CoinbaseMaturity)
                {
                    if (coins.OutPoint.Hash == new uint256("29e5636769fec7a173d4351c2a6241b2d9d02bccd1b4a865c996d24c85f189ef"))
                    {
                        // There is a special case trx in the chain that was allowed immature trx to be spent before its time.
                        // After the issue was fixed we allowed the trx to pass
                        return;
                    }

                    this.Logger.LogDebug("Coinstake transaction height {0} spent at height {1}, but maturity is set to {2}.", coins.Coins.Height, spendHeight, this.consensus.CoinbaseMaturity);
                    this.Logger.LogTrace("(-)[COINSTAKE_PREMATURE_SPENDING]");
                    ConsensusErrors.BadTransactionPrematureCoinstakeSpending.Throw();
                }
            }
        }
    }
}