using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;
using MxM;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.CentralEuropeanStandardTime, AccessRights = AccessRights.None)]
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

        private ITrader cTraderPlatform { get; set; }


        protected override void OnStart()
        {
            this.cTraderPlatform = new PlatformTrader(this, this.GetTradeType(), this.RiskPercentage, this.EntryPrice, this.StopPrice);
            

            Positions.Closed += PositionsOnClosed;

            if (!this.HasOpenPositions() && !this.HasPendingOrders())
            {
                long Volume = this.cTraderPlatform.CalculateVolume();

                double TargetOneInPips = this.CalculateTargetInPips(this.TargetPriceOne);
                double TargetTwoInPips = this.CalculateTargetInPips(this.TargetPriceTwo);

                double StopInPips = this.CalculateStopInPips();

                long VolumeOne = this.cTraderPlatform.AdjustVolume(Volume / 2);
                long VolumeTwo = this.cTraderPlatform.AdjustVolume(Volume / 2);

                TradeResult ResultOne = PlaceStopOrder(this.GetTradeType(), Symbol, VolumeOne, this.EntryPrice, this.PositionNameOne(), StopInPips, TargetOneInPips);
                TradeResult ResultTwo = PlaceStopOrder(this.GetTradeType(), Symbol, VolumeTwo, this.EntryPrice, this.PositionNameTwo(), StopInPips, TargetTwoInPips);

                if (!ResultOne.IsSuccessful || !ResultTwo.IsSuccessful)
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


        private double CalculateTargetInPips(double TargetPrice)
        {
            double TargetInPips = 0;
            if (TargetPrice != 0)
            {
                double TargetDistance = Math.Abs(this.EntryPrice - TargetPrice);
                TargetInPips = TargetDistance / Symbol.PipSize;
            }


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
            ChartObjects.DrawHorizontalLine("EntryLine", this.EntryPrice, Colors.RoyalBlue, 2, LineStyle.Dots);
            ChartObjects.DrawHorizontalLine("StopLine", this.StopPrice, Colors.Red, 2, LineStyle.Dots);
            ChartObjects.DrawHorizontalLine("TargetOneLine", this.TargetPriceOne, Colors.LimeGreen, 2, LineStyle.Dots);
            ChartObjects.DrawHorizontalLine("TargetTwoLine", this.TargetPriceTwo, Colors.LimeGreen, 2, LineStyle.Dots);
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var position = args.Position;
            Position remainingPosition = Positions.Find(this.PositionNameTwo(), Symbol, this.GetTradeType());
            if (remainingPosition != null)
            {
                TradeResult Result = ModifyPosition(remainingPosition, remainingPosition.EntryPrice, remainingPosition.TakeProfit);
                if (!Result.IsSuccessful)
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
