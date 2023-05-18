using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public enum VehicleType { Car, Firecar, None }
    public enum Direction {UP, LEFT, RIGHT, DOWN}
    public class Vehicle
    {
        private (int x, int y) _position;
        private VehicleType _type;
        private Direction _currentDirection;
        private Building _startBuilding;
        private bool _arrived;
        private Queue<(int x, int y)> _route;

        public Vehicle((int x, int y) position, Queue<(int x, int y)> route, Building startBuilding, VehicleType type = VehicleType.Car)
        {
            _position = position;
            _type = type;
            _route = route;
            _route.Dequeue();
            _startBuilding = startBuilding;
            _arrived = (_route.Count <= 1);
            _currentDirection = InitDirection();
        }
        public Direction CurrentDirection { get => _currentDirection; }
        public Building StartBuilding { get => _startBuilding; }
        public VehicleType Type { get => _type; }
        public bool Arrived { get => _arrived; }
        public (int x, int y) CurrentPosition { get => _position; }

        public bool FacingOpposite(Direction otherDir)
        {
            bool opposite = false;
            switch (_currentDirection)
            {
                case Direction.UP:
                    if (otherDir == Direction.DOWN) opposite = true;
                    break;
                case Direction.DOWN:
                    if (otherDir == Direction.UP) opposite = true;
                    break;
                case Direction.LEFT:
                    if (otherDir == Direction.RIGHT) opposite = true;
                    break;
                case Direction.RIGHT:
                    if (otherDir == Direction.LEFT) opposite = true;
                    break;
                default:
                    break;
            }

            return opposite;
        }

        private Direction InitDirection()
        {
            var direction = Direction.UP;
            (int x, int y) nextPos = PeekNextPos();
            if (_route.Count > 0)
            { 
                if(nextPos.x == _position.x)
                {
                    if (nextPos.y == _position.y - 1) direction = Direction.DOWN;
                    if (nextPos.y == _position.y + 1) direction = Direction.UP;
                }
                if (nextPos.y == _position.y)
                {
                    if (nextPos.x == _position.x - 1) direction = Direction.LEFT;
                    if (nextPos.x == _position.x + 1) direction = Direction.RIGHT;
                }
            }                       
            return direction;    
        }

        public Direction NextDirection()
        {
            var direction = CurrentDirection;
            (int x, int y) nextPos = PeekNextPos();
            if (_route.Count > 0)
            {
                if (nextPos.x == _position.x)
                {
                    if (nextPos.y == _position.y - 1) direction = Direction.DOWN;
                    if (nextPos.y == _position.y + 1) direction = Direction.UP;
                }
                if (nextPos.y == _position.y)
                {
                    if (nextPos.x == _position.x - 1) direction = Direction.LEFT;
                    if (nextPos.x == _position.x + 1) direction = Direction.RIGHT;
                }
            }
            return direction;
        }
        
        public (int x, int y) PeekNextPos()
        {
            (int x, int y) nextPos = (_position.x, _position.y);
            if (_route.Count > 0)
                nextPos = _route.Peek();
            
            return nextPos;
        }

        public void Move()
        {
            if (_route.Count > 1)
            {
                _currentDirection = NextDirection();
                _position = _route.Dequeue();
            }
            if (_route.Count <= 1)
            {
                _arrived = true;
            }
        }
    }
}
