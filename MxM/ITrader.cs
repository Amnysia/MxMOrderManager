﻿using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MxM
{
    public interface ITrader
    {
        long CalculateVolume();
        long AdjustVolume(long Volume);
        TradeResult ExecuteMarketOrderWithNoTarget();
    }
}
