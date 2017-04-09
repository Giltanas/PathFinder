using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homm.Client.Helpers;
using HommFinder;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client.AILogic
{
    public class DualPlayAiLogic : AiLogic
    {
        private double _decisionTime;
        private Hero _enemyHero;
        private Cell _enemyHeroLocation;

        public DualPlayAiLogic(HommClient client, HommSensorData sensorData) : base(client, sensorData)
        {
        }

        public override void IncreaseGamingPoints()
        {
            var a = workingWithMines();
            UpdateMap();
            var b = workingWithDwellings();
            UpdateMap();
            if (a.Count == 0 && b.Count == 0)
            {
                Client.Wait(2);
            }
        }

        public sealed override void MakeDecisions()
        {
            var pathToEnemyHero = Finder.GetMovesStraightToCell(_enemyHeroLocation);
            if (checkCanDefeatAllPathEnemies(pathToEnemyHero))
            {
                _decisionTime = SensorData.WorldCurrentTime;
                Act(pathToEnemyHero);
            }
            else
            {
                IncreaseGamingPoints();
            }
        }

        public sealed override void Act(List<Cell> path)
        {
            if (path.Count == 0) return;
            var steps = Converter.ConvertCellPathToDirection(path);
            for (var index = 0; index < steps.Count; index++)
            {
                if (SensorData.WorldCurrentTime - _decisionTime >= 2.0f)
                {
                    UpdateMap();
                    _decisionTime = SensorData.WorldCurrentTime;
                    return;
                }
                var containsArmy = path[index + 1].EnemyArmy != null;
                SensorData = Client.Move(steps[index]);
                if (containsArmy)
                {
                    UpdateMap();
                    _decisionTime = SensorData.WorldCurrentTime;
                    return;
                }
            }
        }
    }
}
