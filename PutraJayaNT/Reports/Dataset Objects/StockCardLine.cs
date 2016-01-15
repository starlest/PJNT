using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PutraJayaNT.Reports.Dataset_Objects
{
    class StockCardLine
    {
        public string Date { get; set; }

        public string Documentation { get; set; }

        public string Description { get; set; }

        public string CustomerSupplier { get; set; }

        public string InQuantity { get; set; }

        public string OutQuantity { get; set; }

        public string Balance { get; set; }
    }
}
