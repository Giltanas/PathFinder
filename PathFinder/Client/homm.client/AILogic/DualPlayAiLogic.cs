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

            workingWithMines();
            
            workingWithDwellings();
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
            var steps = Converter.ConvertCellPathToDirection(path);
            foreach (var direction in steps)
            {
                if (SensorData.WorldCurrentTime - _decisionTime >=2)
                {
                    MakeDecisions();
                }
                moveOneStep(direction);
            }
        }
    }
}
