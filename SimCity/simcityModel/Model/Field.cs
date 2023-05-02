﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    
    public enum FieldType { IndustrialZone, OfficeZone, ResidentalZone, GeneralField }

    public class FieldStat
    {
        private FieldType _type;
        private int _distance;
        private (int x, int y) _coordinates;

        public FieldStat(FieldType type, int distance, (int x, int y) coordinates)
        {
            _type = type;
            _distance = distance;
            _coordinates = coordinates;
        }

        public FieldType Type { get => _type; }
        public int Distance { get => _distance; }
        public (int x, int y) Coordinates { get => _coordinates; }
    }

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
        private int _numberOfPeople;


        #endregion

        protected List<FieldStat> _stats;
        public void updateFieldStats(SimCityModel model)
        {
            (bool[,] routeExists, bool allBuildingsFound, (int, int)[,] parents, int[,] distance) = model.BreadthFirst((_x,_y), true);
            for (int i = 0; i < model.GameSize; i++)
            {
                for (int j = 0; j < model.GameSize; j++)
                {
                    if (model.Fields[i, j].Building == null && model.Fields[i,j].Type != FieldType.GeneralField)
                    {
                        var fType = model.Fields[i, j].Type;
                        int dist = distance[i, j];
                        var stat = new FieldStat(fType, dist, (i, j));
                        _stats.Add(stat);
                    }
                }
            }
            _stats.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        }

        #region Events

        public event EventHandler<(int x, int y)> NumberOfPeopleChanged;
        public event EventHandler<(int x, int y)> PeopleHappinessChanged;

        #endregion

        #region Properties

        public int X { get => _x; }
        public int Y { get => _y; }

        public FieldType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        
        public List<FieldStat> FieldStats { get => _stats; }

        public Building? Building
        {
            get { return _building; }
            set
            {
                _building = value;
                if (_building != null && _building.GetType() == typeof(PeopleBuilding))
                {
                    ((PeopleBuilding)_building).NumberOfPeopleChanged += new EventHandler(OnNumberOfPeopleChanged);
                }
            }
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
            _numberOfPeople = 0;


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
                        sum += person.Happiness;
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

        #region Private event triggers

        private void OnNumberOfPeopleChanged(object? sender, EventArgs e)
        {
            NumberOfPeopleChanged?.Invoke(this, (X, Y));
        }

        #endregion
    }
}
