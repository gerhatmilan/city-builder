using System;
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
