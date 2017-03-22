using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finder
{
    public class Finder
    {
        private List<Cell> _cells;
        private List<Union> _unions; 

        public Finder(List<Cell> cells )
        {
            _unions = new List<Union>();
            _cells = cells;
        }

    }
}
