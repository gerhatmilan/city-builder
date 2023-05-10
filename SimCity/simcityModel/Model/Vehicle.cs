using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VehicleType { Car, Firecar, None }
    public enum Direction {UP, LEFT, RIGHT, DOWN}
    public class Vehicle
    {
        private (int x, int y) _position;
        private Direction _currentDirection;
        private Queue<(int x, int y)> _route;

        public Vehicle((int x, int y) position, Queue<(int x, int y)> route)
        {
            _position = position;
            _route = route;
            _currentDirection = InitDirection();
        }


        public Direction CurrentDirection {get => _currentDirection;}
        
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

        private Direction NextDirection()
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
            if (_route.Count > 0)
            {
                _position = _route.Dequeue();
                _currentDirection = NextDirection();
            }
        }

    }
}
