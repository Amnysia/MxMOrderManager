using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MxMOrderManager : Robot
    {

        [Parameter("Risk %", DefaultValue = 5)]
        public int RiskPercentage { get; set; }

        [Parameter("Buy", DefaultValue = true)]
        public bool IsBuyOrder { get; set; }

        [Parameter("Entry Price")]
        public double EntryPrice { get; set; }

        [Parameter("Stop Price")]
        public double StopPrice { get; set; }

        [Parameter("Target One Price")]
        public double TargetPriceOne { get; set; }

        [Parameter("Target Two Price")]
        public double TargetPriceTwo { get; set; }


        protected override void OnStart()
        {
            Positions.Closed += PositionsOnClosed;

            if (!this.HasOpenPositions() && !this.HasPendingOrders())
            {
                long Volume = this.CalculateVolume();

                double TargetOneInPips = this.CalculateTargetInPips(this.TargetPriceOne);
                double TargetTwoInPips = this.CalculateTargetInPips(this.TargetPriceTwo);

                double StopInPips = this.CalculateStopInPips();
                
                long VolumeOne = this.AdjustVolume(Volume / 2);
                long VolumeTwo = this.AdjustVolume(Volume / 2);

                TradeResult ResultOne = PlaceStopOrder(this.GetTradeType(), Symbol, VolumeOne, this.EntryPrice, this.PositionNameOne(), StopInPips, TargetOneInPips);
                TradeResult ResultTwo = PlaceStopOrder(this.GetTradeType(), Symbol, VolumeTwo, this.EntryPrice, this.PositionNameTwo(), StopInPips, TargetTwoInPips);
                
                if(!ResultOne.IsSuccessful || !ResultTwo.IsSuccessful)
                {
                    ClosePosition(ResultOne.Position);
                    ClosePosition(ResultTwo.Position);
                }
            }
            this.DrawLines();
        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        private string PositionNameOne()
        {
            return string.Format("{0}{1}One", Symbol.Code, this.GetTradeType());
        }

        private string PositionNameTwo()
        {
            return string.Format("{0}{1}Two", Symbol.Code, this.GetTradeType());
        }

        private long CalculateVolume()
        {
            double MaxRisquedAmount = (Account.Balance / 100) * this.RiskPercentage;
            double StopValue = this.CalculateStopInPips() * Symbol.PipValue;
            long Volume = Convert.ToInt64(MaxRisquedAmount / StopValue);
            return this.AdjustVolume(Volume);
        }

        private long AdjustVolume(long Volume)
        {
            long AdjustedVolume = 0;
            long VolumeStepDifference = Volume % Symbol.VolumeStep;
            if (VolumeStepDifference > 500)
            {
                long differenceToAdd = Symbol.VolumeStep - VolumeStepDifference;
                AdjustedVolume = Volume + differenceToAdd;
            }
            else
            {
                AdjustedVolume = Volume - VolumeStepDifference;
            }
            return AdjustedVolume;
        }

        private double CalculateTargetInPips(double TargetPrice)
        {
            double TargetDistance = Math.Abs(this.EntryPrice - TargetPrice);
            double TargetInPips = TargetDistance / Symbol.PipSize;

            return TargetInPips;
        }

        private double CalculateStopInPips()
        {
            double StopDistance = Math.Abs(this.EntryPrice - this.StopPrice);
            double StopInPips = StopDistance / Symbol.PipSize;

            return StopInPips;
        }

        private TradeType GetTradeType()
        {
            return IsBuyOrder ? TradeType.Buy : TradeType.Sell;
        }

        private void DrawLines()
        {
            ChartObjects.DrawHorizontalLine("EntryLine", this.EntryPrice, Colors.RoyalBlue, 2, LineStyle.LinesDots);
            ChartObjects.DrawHorizontalLine("StopLine", this.StopPrice, Colors.Red, 2, LineStyle.LinesDots);
            ChartObjects.DrawHorizontalLine("TargetOneLine", this.TargetPriceOne, Colors.LimeGreen, 2, LineStyle.LinesDots);
            ChartObjects.DrawHorizontalLine("TargetTwoLine", this.TargetPriceTwo, Colors.LimeGreen, 2, LineStyle.LinesDots);
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var position = args.Position;
            Position remainingPosition = Positions.Find(this.PositionNameTwo(), Symbol, this.GetTradeType());
            if(remainingPosition != null)
            {
                TradeResult Result = ModifyPosition(remainingPosition, remainingPosition.EntryPrice, remainingPosition.TakeProfit);
                if(!Result.IsSuccessful)
                {
                    Notifications.SendEmail("maxlecomte@gmail.com", "maxlecomte@gmail.com", "Unable to move stop to break even", "Stop is not moved to break even");
                    Print("Email sent to maxlecomte@gmail.com");
                }
            }
        }

        private bool HasOpenPositions()
        {
            bool StillPositionsOpen = false;
            foreach (Position position in Positions)
            {
                if (position.Label == this.PositionNameOne() || position.Label == this.PositionNameTwo())
                {
                    StillPositionsOpen = true;
                    break;
                }
            }
            return StillPositionsOpen;
        }

        private bool HasPendingOrders()
        {
            bool StillPendingOrders = false;
            foreach (PendingOrder pendingOrder in PendingOrders)
            {
                if (pendingOrder.Label == this.PositionNameOne() || pendingOrder.Label == this.PositionNameTwo())
                {
                    StillPendingOrders = true;
                    break;
                }
            }
            return StillPendingOrders;
        }
    }
}
