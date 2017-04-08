using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homm.Client.Interfaces;
using HommFinder;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client.AILogic
{
    internal class SinglePlayAiLogic : AiLogic
	{
	    public SinglePlayAiLogic(HommClient client, HommSensorData sensorData) : base(client, sensorData)
	    {
	    }

	    public sealed override void MakeDecisions()
		{

            workingWithMines();

            workingWithDwellings();
        }

		public sealed override void Act(List<Cell> path)
		{
			
		}

	    public sealed override void IncreaseGamingPoints()
	    {
	        
	    }
	}
}
