using System;

// StopWatch - this class provides a way to track execution time similar to a physical stopwatch

/* NoticeStart

CDSS Common Java Library
CDSS Common Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Common Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Common Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Common Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */
namespace RTi.Util.Time
{
    using Message = Message.Message;

    /// <summary>
	/// This class provides a way to track execution time similar to a physical stopwatch.  To
	/// use the class, declare an instance and then call "start" and "stop" as necessary
	/// to add to the time.  Use "clear" to reset the timer to zero.  The time amounts
	/// are tracked internally in milliseconds.  Note that the StopWatch features do
	/// introduce overhead into program execution because it requests the system time
	/// and should only be used when debugging or in
	/// cases where the performance issues are not large.  For example, put start/stop
	/// calls outside of loops, or, if in loops, consider only using if wrapped in
	/// Message.isDebugOn() checks.
	/// </summary>
    public class StopWatch
    {

        /**
        Total elapsed running time in milliseconds.
        */
        private double _total_milliseconds;
        /**
        Start date for a StopWatch session.
        */
        private System.DateTime? _start_date;
        /**
        Indicates if the start time has been set.
        */
        private bool _start_set;
        /**
        Stop date for a StopWatch session.
        */
        private System.DateTime? _stop_date;

        /// <summary>
        /// Constructor and initialize the StopWatch count to zero milliseconds.
        /// </summary>
        public StopWatch()
        {
            initialize(0);
        }

        /// <summary>
        /// Construct given an initial time count (for example, use if a second time is
        /// storing an initial time plus new accumulations of time). </summary>
        /// <param name="total"> Total time to initialize StopWatch to, milliseconds. </param>
        public StopWatch(long total)
        {
            initialize(total);
        }

        /// <summary>
        /// Add the time from another stopwatch to the elapsed time for this stopwatch. </summary>
        /// <param name="sw"> the StopWatch from which to get additional time. </param>
        public virtual void add(StopWatch sw)
        {
            _total_milliseconds += sw.getMilliseconds();
        }

        /// <summary>
        /// Reset the StopWatch to zero.
        /// </summary>
        public virtual void clear()
        {
            _total_milliseconds = 0;
        }

        /// <summary>
        /// Reset the StopWatch to zero and call start().
        /// </summary>
        public virtual void clearAndStart()
        {
            _total_milliseconds = 0;
            start();
        }

        /// <summary>
        /// Return the accumulated milliseconds. </summary>
        /// <returns> The number of milliseconds accumulated in the StopWatch. </returns>
        public virtual double getMilliseconds()
        {
            return _total_milliseconds;
        }

        /// <summary>
        /// Return the accumulated seconds. </summary>
        /// <returns> The number of seconds accumulated in the StopWatch (as a double so that
        /// milliseconds are also reflected). </returns>
        public virtual double getSeconds()
        {
            return (double)_total_milliseconds / (double)1000.0;
        }

        /// <summary>
        /// Initialize StopWatch. </summary>
        /// <param name="initial"> StopWatch value in milliseconds. </param>
        private void initialize(long total)
        {
            _total_milliseconds = total;
            _start_date = null;
            _start_set = false;
            _stop_date = null;
        }

        /// <summary>
        /// Start accumulating time in the StopWatch.
        /// </summary>
        public virtual void start()
        {
            _start_set = true;
            _start_date = System.DateTime.Now;
        }

        /// <summary>
        /// Stop accumulating time in the StopWatch.  This does not clear the StopWatch and
        /// subsequent calls to "start" can be made to continue adding to the StopWatch.
        /// </summary>
        public virtual void stop()
        {
            string routine = "Message.stop";
            _stop_date = System.DateTime.Now;
            // Compute the difference and add to the elapsed time.
            if (_start_set)
            {
                double add = (_stop_date.Value - _start_date.Value).TotalMilliseconds;
                _total_milliseconds += add;
            }
            _start_set = false;
        }

        /**
        Print the StopWatch value as seconds.
        */
        public string ToString()
        {
            return "StopWatch(seconds)=" + getSeconds();
        }
    }
}
