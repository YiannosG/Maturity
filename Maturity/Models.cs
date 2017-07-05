using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maturity
{
    public class Models
    {
        public class Policy
        {
            public string PolicyNumber { get; set; }
            public DateTime PolicyStartDate { get; set; }
            public int Premiums { get; set; }
            public bool Membership { get; set; }
            public int DiscretionaryBonus { get; set; }
            public decimal UpliftPerecentage { get; set; }
        }
    }
}
