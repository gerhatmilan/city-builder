using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    
    public class Field
    {
        public const int RESIDENTAL_CAPACITY = 20;
        public const int INDUSTRIAL_CAPACITY = 30;
        public const int OFFICE_CAPACITY = 40;

        private int _x;
        private int _y;
        private FieldType _type;
        private Building? _building;
        private int _capacity;
        private int _numberOfPeople;

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

        public int NumberOfPeople { get => _numberOfPeople; set => _numberOfPeople = value; }

        public Field(int x, int y)
        {
            _x = x;
            _y = y;
            _type = FieldType.GeneralField;
            _building = null;
        }
    }
}
