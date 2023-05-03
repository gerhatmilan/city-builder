using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class Person
    {
        #region Private fields

        private const int INITIAL_HAPPINESS = 50;
        private int _happiness;
        private Field _home;
        private Field _work;
        private int _distanceToWork;

        #endregion

        #region Events

        public event EventHandler? HappinessChanged;

        #endregion

        #region Properties

        public int Happiness { get => _happiness; set { _happiness = value; OnHappinessChanged(); } }
        public Field Home { get => _home; set => _home = value; }
        public Field Work { get => _work; set => _work = value; }
        public int DistanceToWork { get => _distanceToWork; set => _distanceToWork = value; }

        #endregion

        #region Constructor

        public Person(Field home, Field work, int distanceToWork)
        {
            _happiness = INITIAL_HAPPINESS;
            _distanceToWork = distanceToWork;
            _home = home;
            _work = work;

            HappinessChanged += new EventHandler(_home.OnPeopleHappinessChanged);
            HappinessChanged += new EventHandler(_work.OnPeopleHappinessChanged);
        }

        #endregion

        #region Public methods

        public void RefreshHappiness(int gameSize)
        {
            int distanceToWorkFactor = 0;
            int industrialBuildingNearbyFactor = 0;

            // calculating distanceToWorkFactor
            if (distanceToWorkFactor <= gameSize / 2)
                distanceToWorkFactor = gameSize / 2 - (_distanceToWork + 3); // positive case
            else
                distanceToWorkFactor = -1 * (_distanceToWork + 3 - gameSize / 2); // negative case

            // calculating industralBuildingNearbyFactor
            int distanceToClosestIndustralBuilding = 0;
            List<FieldStat> homeFieldStats = Home.FieldStats;

            FieldStat closestIndustrialBuildingStat = null;
            foreach (FieldStat stat in homeFieldStats)
            {
                if (stat.Type == FieldType.IndustrialZone && stat.HasBuilding)
                {
                    closestIndustrialBuildingStat = stat;
                    break;
                }
            }

            if (closestIndustrialBuildingStat != null)
            {
                distanceToClosestIndustralBuilding = closestIndustrialBuildingStat.Distance;
            }
            if (distanceToClosestIndustralBuilding != 0)
            {
                if (distanceToClosestIndustralBuilding >= gameSize / 2)
                    industrialBuildingNearbyFactor = distanceToClosestIndustralBuilding - gameSize / 2;
                else
                    industrialBuildingNearbyFactor = -1 * (gameSize / 2 - distanceToClosestIndustralBuilding);
            }              

            Dictionary<String, Int32> factors = new Dictionary<String, Int32>()
            {
                { "InitalHappiness", INITIAL_HAPPINESS },
                { "DistanceToWork", distanceToWorkFactor},
                { "IndustrialBuildingNearby", industrialBuildingNearbyFactor},
                { "HomeFieldHappiness", Home.FieldHappiness},
                { "WorkFieldHappiness", Work.FieldHappiness}
            };


            if (factors.Values.Sum() < 0) Happiness = 0;
            else if (factors.Values.Sum() > 100) Happiness = 100;
            else Happiness = factors.Values.Sum();
        }

        #endregion

        #region Private event triggers

        private void OnHappinessChanged()
        {
            HappinessChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
