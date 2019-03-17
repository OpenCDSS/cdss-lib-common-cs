// Timer - a timer class to help time processing

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

// ----------------------------------------------------------------------------
// Timer - a timer class to help time processing
// ----------------------------------------------------------------------------
// History:
//
// ?		Steven A. Malers, RTi	Initial version to help diagnose
//					performance issues.
// 2001-02-06	SAM, RTi		Clean up javadoc to simplify port to
//					C++.  Add finalize() method and make
//					sure values are set to null when not
//					used.
// 2001-05-04	SAM, RTi		Deprecate this class in favor of the
//					StopWatch class.
// ----------------------------------------------------------------------------

namespace RTi.Util.Time
{

	/// <summary>
	/// This class provides a way to track execution time similar to a stopwatch.  To
	/// use the class, declare an instance and then call "start" and "stop" as necessary
	/// to add to the time.  Use "clear" to reset the timer to zero.  The timer amounts
	/// are tracked internally in milliseconds.  Note that the timer process does
	/// introduce overhead into program execution because it requests the system time
	/// and should only be used when debugging or in
	/// cases where the performance issues are not large.  For example, put start/stop
	/// calls outside of loops, or, if in loops, consider only using if wrapped in
	/// Message.isDebugOn() checks. </summary>
	/// @deprecated Use StopWatch class. 
	public class Timer
	{

	// Data members...

	private long _total_milliseconds; // Total time in milliseconds.
	private System.DateTime _start_date; // Start date for a timer session.
	private bool _start_set; // Indicates if the start time has been
						// set.
	private System.DateTime _stop_date; // Stop date for a timer session.

	/// <summary>
	/// Constructor and initialize the timer count to zero milliseconds. </summary>
	/// @deprecated Use StopWatch class. 
	public Timer()
	{
		initialize(0);
	}

	/// <summary>
	/// Construct given an initial time count (for example, use if a second time is
	/// storing an initial time plus new accumulations of time). </summary>
	/// <param name="total"> Total time to initialize timer to, milliseconds. </param>
	/// @deprecated Use StopWatch class. 
	public Timer(long total)
	{
		initialize(total);
	}

	/// <summary>
	/// Reset the timer to zero.
	/// </summary>
	public virtual void clear()
	{
		_total_milliseconds = 0;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~Timer()
	{
		_start_date = null;
		_stop_date = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the accumulated milliseconds. </summary>
	/// <returns> The number of milliseconds accumulated in the timer. </returns>
	public virtual long getMilliseconds()
	{
		return _total_milliseconds;
	}

	/// <summary>
	/// Return the accumulated seconds. </summary>
	/// <returns> The number of seconds accumulated in the timer (as a double so that
	/// milliseconds are also reflected). </returns>
	public virtual double getSeconds()
	{
		return (double)_total_milliseconds / (double)1000.0;
	}

	/// <summary>
	/// Initialize timer. </summary>
	/// <param name="initial"> timer value in milliseconds. </param>
	private void initialize(long total)
	{
		_total_milliseconds = total;
		_start_date = null;
		_start_set = false;
		_stop_date = null;
	}

	/// <summary>
	/// Start accumulating time in the timer.
	/// </summary>
	public virtual void start()
	{
		_start_set = true;
		_start_date = System.DateTime.Now;
	}

	/// <summary>
	/// Stop accumulating time in the timer.  This does not clear the timer and
	/// subsequent calls to "start" can be made to continue adding to the timer.
	/// </summary>
	public virtual void stop()
	{
		_stop_date = System.DateTime.Now;
		// now compute the difference and add to the
		if (_start_set)
		{
			long add = _stop_date.Ticks - _start_date.Ticks;
			_total_milliseconds += add;
		}
		_start_set = false;
	}

	/// <summary>
	/// Print the timer value as seconds.
	/// </summary>
	public override string ToString()
	{
		return "Timer(seconds)=" + getSeconds();
	}

	} // End class Timer

}