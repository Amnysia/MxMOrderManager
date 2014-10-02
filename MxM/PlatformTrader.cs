using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace MxM
{
    public class PlatformTrader : ITrader
    {

        public PlatformTrader(Robot aRobot, TradeType aTradeType, int aRiskPercentage, double aEntryPrice, double aStopPrice)
        {
            this.Robot = aRobot;
            this.EntryPrice = aEntryPrice;
            this.StopPrice = aStopPrice;
            this.TradeType = aTradeType;
            this.RiskPercentage = aRiskPercentage;
        }

        private Robot Robot { get; set; }
        private double EntryPrice { get; set; }
        private double StopPrice { get; set; }
        private TradeType TradeType { get; set; }
        private int RiskPercentage { get; set; }

        public long CalculateVolume()
        {
            double MaxRisquedAmount = (this.Robot.Account.Balance / 100) * this.RiskPercentage;
            double StopValue = this.CalculateStopInPips() * this.Robot.Symbol.PipValue;
            long Volume = Convert.ToInt64(MaxRisquedAmount / StopValue);
            return this.AdjustVolume(Volume);
        }


        private double CalculateStopInPips()
        {
            double StopDistance = Math.Abs(this.EntryPrice - this.StopPrice);
            double StopInPips = StopDistance / this.Robot.Symbol.PipSize;

            return StopInPips;
        }

        public long AdjustVolume(long Volume)
        {
            long AdjustedVolume = 0;
            long VolumeStepDifference = Volume % this.Robot.Symbol.VolumeStep;
            if (VolumeStepDifference > this.Robot.Symbol.VolumeStep / 2)
            {
                long differenceToAdd = this.Robot.Symbol.VolumeStep - VolumeStepDifference;
                AdjustedVolume = Volume + differenceToAdd;
            }
            else
            {
                AdjustedVolume = Volume - VolumeStepDifference;
            }
            return AdjustedVolume;

        }

       
        public TradeResult ExecuteMarketOrderWithNoTarget()
        {
            if(this.TradeType == TradeType.Buy)
            {
                this.EntryPrice = this.Robot.Symbol.Ask;
            }
            else
            {
                this.EntryPrice = this.Robot.Symbol.Bid;
            }

            return this.Robot.ExecuteMarketOrder(this.TradeType, this.Robot.Symbol, this.CalculateVolume(), "MarketOrder", this.CalculateStopInPips(), 0);
        }

    }//CLASS

}
