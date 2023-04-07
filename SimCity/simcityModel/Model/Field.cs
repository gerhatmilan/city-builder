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

        public FieldType Type
        {
            get { return _type; }
        }

        public Building? Building
        {
            get { return _building; }
        }

        public Field()
        {
            _type = FieldType.GeneralField;
            _building = null;
        }
    }
}
