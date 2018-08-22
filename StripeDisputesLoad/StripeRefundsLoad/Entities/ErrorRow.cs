﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StripeRefundsLoad.Entities
{
    public class ErrorRow
    {

        public string APITransactionId { get; set; }
        public string Source { get; set; }
        public string TransactionJson { get; set; }

        public string ExceptionMessage { get; set; }
    }
}
