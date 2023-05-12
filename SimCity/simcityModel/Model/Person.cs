using Newtonsoft.Json;
using System;

namespace simcityModel.Model
{
    public class Person
    {
        #region Private fields

        private const int INITIAL_HAPPINESS = 50;
        private double _happiness;
        private Field _home;
        private Field _work;
        private int _distanceToWork;

        #endregion

        #region Events

        public event EventHandler? HappinessChanged;

        #endregion

        #region Properties

        public double Happiness
        {
            get => _happiness;
            set
            {
                _happiness = value;
                if (_happiness < 0) _happiness = 0;
                else if (_happiness > 100) _happiness = 100;
                OnHappinessChanged();
            }
        }

        public Field Home
        {
            get => _home;
            set
            {
                if (value != null)
                {
                    _home = value;
                    HappinessChanged += new EventHandler(_home.OnPeopleHappinessChanged);
                }
            }
        }

        public Field Work
        {
            get => _work;
            set
            {
                if (value != null)
                {
                    _work = value;
                    HappinessChanged += new EventHandler(_work.OnPeopleHappinessChanged);
                }
            }
        }
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

        public void RefreshHappiness(SimCityModel model)
        {
            double distanceToWorkFactor = 0;
            double industrialBuildingNearbyFactor = 0;
            double negativeBudgetFactor = 0;
            double officeIndustrialRatioFactor = -1 * Math.Abs(model.NumberOfIndustrialBuildings - model.NumberOfOfficeBuildings);
            double fieldSafetyFactor = (Home.FieldSafety + Work.FieldSafety) - (model.Population / 10.0);

            // calculating distanceToWorkFactor
            if (_distanceToWork <= model.GameSize / 2)
                distanceToWorkFactor = 1.4 * (model.GameSize / 2 - _distanceToWork);
            else
                distanceToWorkFactor = -1.6 * (_distanceToWork - model.GameSize / 2);

            // calculating industralBuildingNearbyFactor
            List<FieldStat> homeFieldStats = Home.FieldStats;
            FieldStat? closestIndustrialBuildingStat = null;

            foreach (FieldStat stat in homeFieldStats)
            {
                if (stat.Type == FieldType.IndustrialZone && stat.HasBuilding)
                {
                    closestIndustrialBuildingStat = stat;
                    break;
                }
            }

            if (closestIndustrialBuildingStat != null && closestIndustrialBuildingStat.Distance != 0)
            {
                if (closestIndustrialBuildingStat.Distance >= model.GameSize / 2)
                    industrialBuildingNearbyFactor = 1.2 * (closestIndustrialBuildingStat.Distance - model.GameSize / 2);
                else
                    industrialBuildingNearbyFactor = -1.3 * (model.GameSize / 2 - closestIndustrialBuildingStat.Distance);
            }

            // calculating negativeBudgetFactor
            if (model.DaysPassedSinceNegativeBudget > 0)
            {
                
                negativeBudgetFactor = -1 * (model.DaysPassedSinceNegativeBudget * 50.0 / 365) * (Math.Pow(1.2, model.Money / -500.0));
            }           

            List<double> factors = new List<double>()
            {
                INITIAL_HAPPINESS,
                Home.FieldHappiness,
                Work.FieldHappiness,
                distanceToWorkFactor,
                industrialBuildingNearbyFactor,
                negativeBudgetFactor,
                officeIndustrialRatioFactor,
                fieldSafetyFactor
            };

            Happiness = factors.Sum();
        }

        public void MoveAway(SimCityModel model)
        {
            ((PeopleBuilding)Home.Building!).People.Remove(this);
            ((PeopleBuilding)Work.Building!).People.Remove(this);
            model.People.Remove(this);
            model.Population--;
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
