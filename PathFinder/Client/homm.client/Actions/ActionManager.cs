using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using Homm.Client.AILogic;
using Homm.Client.Helpers;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.Actions
{
	public class ActionManager
	{
		public AiLogic AiLogic { get; private set; }
		public ActionManager(HommClient client, HommSensorData sensorData)
		{
			var startCell = sensorData.Location.CreateCell();

			var enemyRespawn =
				startCell.SameLocation(new Cell(0, 0)) ?
				sensorData.Map.Objects.SingleOrDefault(o => o.Location.X == sensorData.Map.Width - 1 && o.Location.Y == sensorData.Map.Height - 1) :
				sensorData.Map.Objects.SingleOrDefault(o => o.Location.X == 0 && o.Location.Y == 0);
			var mapType = MapType.Single;

			if (sensorData.Map.Objects.Count < sensorData.Map.Height * sensorData.Map.Width)
			{
				mapType = MapType.DualHard;
			}
			else if (enemyRespawn.Hero != null)
			{
				mapType = MapType.Dual;
			}
			switch (mapType)
			{
				case MapType.Single:
				{
					AiLogic = new SinglePlayAiLogic(){ };
					break;
				}
			}
		}
		//TODO: change signature of this method
		public void Play()
		{
			AiLogic.Act();
		}
	}

	public enum MapType
	{
		Single,
		Dual,
		//mode without open map
		DualHard
	}
}
