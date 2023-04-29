using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    
    public enum FieldType { IndustrialZone, OfficeZone, ResidentalZone, GeneralField }
    public class Field
    {
        #region Private fields

        public const int RESIDENTAL_CAPACITY = 20;
        public const int INDUSTRIAL_CAPACITY = 40;
        public const int OFFICE_CAPACITY = 40;

        private int _x;
        private int _y;
        private FieldType _type;
        private Building? _building;
        private int _fieldHappiness;

        #endregion

        #region Properties

        public int X { get => _x; }
        public int Y { get => _y; }

        public FieldType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public Building? Building
        {
            get { return _building; }
            set { _building = value; }
        }

        public int Capacity
        {
            get
            {
                switch (_type)
                {
                    case FieldType.ResidentalZone: return RESIDENTAL_CAPACITY;
                    case FieldType.IndustrialZone: return INDUSTRIAL_CAPACITY;
                    case FieldType.OfficeZone: return OFFICE_CAPACITY;
                    default: return 0;
                }
            }
        }

        public int NumberOfPeople { get => CalculateNumberOfPeople(); }

        public int FieldHappiness { get => _fieldHappiness; set => _fieldHappiness = value; }

        public int PeopleHappiness { get => CalculatePeopleHappiness(); }

        #endregion

        #region Constructor

        public Field(int x, int y)
        {
            _x = x;
            _y = y;
            _type = FieldType.GeneralField;
            _building = null;
            _fieldHappiness = 0;
        }

        #endregion

        #region Private methods

        private int CalculatePeopleHappiness()
        {
            if (_building != null)
            {
                if (_building.GetType() == typeof(PeopleBuilding))
                {
                    int sum = 0;

                    foreach (Person person in ((PeopleBuilding)_building).People)
                    {
                        sum += person.happyness;
                    }

                    return sum / ((PeopleBuilding)_building).People.Count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private int CalculateNumberOfPeople()
        {
            if (_building != null)
            {
                if (_building.GetType() == typeof(PeopleBuilding))
                {
                    return ((PeopleBuilding)_building).People.Count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}
