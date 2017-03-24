using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;
using HoMM.Engine;
using HommFinder;

namespace Homm.Client.Interfaces
{
	public interface IOutPutPrinter
	{
		void PrintMap(IEnumerable<MapObjectData> objects, int width, int height);
		void PrintPath(IEnumerable<MapObjectData> objects, IEnumerable<Cell> direction, int width, int height);
	}
}
