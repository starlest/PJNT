using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PutraJayaNT.Reports.Dataset_Objects
{
    class StockMovement
    {
        public string ID { get; set; }

        public string Date { get; set; }

        public string FromWarehouse { get; set; }
        
        public string ToWarehouse { get; set; }
    }
}
