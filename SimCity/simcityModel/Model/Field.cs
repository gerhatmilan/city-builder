using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class Field
    {
        private FieldType _type;
        private Building? _building;
        private int _capacity;
        private int _numberOfPeople;

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
                    case FieldType.ResidentalZone: return 20;
                    case FieldType.IndustrialZone: return 30;
                    case FieldType.OfficeZone: return 40;
                    default: return 0;
                }
            }
        }

        public int NumberOfPeople { get => _numberOfPeople; set => _numberOfPeople = value; }

        public Field()
        {
            _type = FieldType.GeneralField;
            _building = null;
        }
    }
}
