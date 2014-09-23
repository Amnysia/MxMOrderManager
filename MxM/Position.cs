using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Internals;

namespace MxM
{
    class Position
    {
        public TradeType TradeType { get; set; }
        public Symbol Symbol { get; set; }
        public long Volume { get; set; }
        public double EntryPrice { get; set; }
        public double TargetInPips { get; set; }
        public double StopInPips { get; set; }
        public string PositionLabel { get; set; }


    }
}
