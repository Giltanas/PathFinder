using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homm.Client.Interfaces;

namespace Homm.Client.AILogic
{
    internal class SinglePlayAiLogic : AiLogic
	{
		public override void MakeDecisions()
		{
			throw new NotImplementedException();
		}

		public override void Act()
		{
			UpdateMap();
			workingWithMines();
			workingWithDwellings();
			figthForResource();
		}

	    public override bool CanIncreaseGamingPoints()
	    {
	        throw new NotImplementedException();
	    }
	}
}
