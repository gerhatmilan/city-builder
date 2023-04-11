using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    /// <summary>
    /// Game event arguments class for SimCityModel events.
    /// </summary>
    public class GameEventArgs : EventArgs
    {
        /// <summary>
        /// Represents the game time.
        /// </summary>
        public DateTime GameTime { get; private set; }

        /// <summary>
        /// Represents the money.
        /// </summary>
        public Int32 Money { get; private set; }

        /// <summary>
        /// Represents the population.
        /// </summary>
        public Int32 Population { get; private set; }

        /// <summary>
        /// Initializes a new instance of GameEventArgs class.
        /// </summary>
        /// <param name="gameTime">Current date in the game.</param>
        /// <param name="money">Current money in the game.</param>
        /// <param name="population">Current population in the game.</param>
        public GameEventArgs(DateTime gameTime, Int32 money, Int32 population)
        {
            GameTime = gameTime;
            Money = money;
            Population= population;
        }
    }
}
