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
    public class DualHardAiLogic : AiLogic
    {
        private double _decisionTime;
        private Hero _enemyHero;
        private Cell _enemyHeroLocation;

        public DualHardAiLogic(HommClient client, HommSensorData sensorData) : base(client, sensorData)
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
                scoutMap();
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

        private void scoutMap()
        {
            var scoutCells = Finder.Cells.Where(c=> !c.Value.Equals(Single.MaxValue)).ToList();
            var scoutCell = scoutCells.Find(cell=> cell.Value.Equals(scoutCells.Max(c => c.Value)));
            var path = Finder.GetSmartPath(scoutCell);
            _decisionTime = SensorData.WorldCurrentTime;
            Act(path);
        }
    }
}
