using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HommFinder
{
    public class ObjectCellType
    {
        public MainCellType MainType { get; set; } = MainCellType.None;
        public SubCellType SubCellType { get; set; } = SubCellType.None;

    }
}
