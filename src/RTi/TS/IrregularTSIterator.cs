using System.Collections.Generic;

// IrregularTSIterator - used to iterate through irregular time series data

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
// IrregularTSIterator - used to iterate through irregular time series data
// ----------------------------------------------------------------------------
// History:
//
// 05 Jul 2000	Steven A. Malers, RTi	Copy TSDateIterator and update to have
//					basic needed functionality.
// 27 Jul 2000	Michael Thiemann, RTi   Use TSIterator as base class and derive
//					this class to improve performance.
// 28 Jul 2000	SAM, RTi		Clean up code to remove redundant
//					code in derived class.
// 11 Oct 2000	SAM, RTi		Port to Java from C++.
// 10 Sep 2001	SAM, RTi		Change iterator to be more similar to
//					C++, which was updated after the initial
//					port.  Fix a problem in IrregularTS that
//					impacted iteration.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// 2003-07-24	SAM, RTi		* Synchronize with C++ code.
//					* Have the constructors throw an
//					  Exception if the time series or dates
//					  are null.
//					* Add getDuration().
//					* Add finalize().
// 2005-09-14	SAM, RTi		Change _last_date_encountered to
//					_last_date_processed, as per the
//					TSIterator base class.
// 2005-09-16	SAM, RTi		* Add skeleton goTo(),
//					  goToNearestNext(),
//					  goToNearestPrevious().
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{

	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// The IrregularTSIterator can be used to iterate through data in an IrregularTS.
	/// It is used similarly to the TSIterator.  See TSIterator for examples of use.
	/// </summary>
	public class IrregularTSIterator : TSIterator
	{

	/// <summary>
	/// Construct an iterator for the full period of the time series. </summary>
	/// <param name="ts"> Time series to iterate through. </param>
	/// <exception cref="Exception"> if the time series or its start and end dates are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IrregularTSIterator(IrregularTS ts) throws Exception
	public IrregularTSIterator(IrregularTS ts) : base(ts)
	{
	}

	/// <summary>
	/// Construct an iterator for the given period of the time series. </summary>
	/// <param name="ts"> Time series to iterate. </param>
	/// <param name="date1"> First date to iterate. </param>
	/// <param name="date2"> Last date to iterate. </param>
	/// <exception cref="Exception"> if the time series or its start and end dates are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IrregularTSIterator(IrregularTS ts, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	public IrregularTSIterator(IrregularTS ts, DateTime date1, DateTime date2) : base(ts, date1, date2)
	{
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~IrregularTSIterator()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the duration of the data value in seconds.  This method is only defined
	/// for the IrregularTSIterator and will return zero for regular TS. </summary>
	/// <returns> Data value duration in seconds. </returns>
	public virtual int getDuration()
	{
		return 0;
	}

	// TODO SAM 2005-09-16 Remove the warning when implemented
	/// <summary>
	/// Go to the specified date/time, returning the matching data as if next() or
	/// previous() had been called.  The date/time in the time series MUST exactly
	/// match the date (dt.equals(...)).  If unable to go to the date/time, null is returned. </summary>
	/// <param name="dt"> Date/time to go to.
	/// THIS METHOD IS NOT YET IMPLEMENTED FOR IrregularTSIterator.  null IS ALWAYS RETURNED. </param>
	/// <param name="reset_if_fail"> If true and the search fails, reset to the starting
	/// position, but still return null.  If false and the search fails, the current
	/// date/time of the iterator will be at the end of data and null will be returned. </param>
	/// <returns> the TSData for the requesting date/time.  WARNING:  the contents of this object are volatile
	/// and change with each iteration.  Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public override TSData goTo(DateTime dt, bool reset_if_fail)
	{
		Message.printWarning(1, "IrregularTSIterator.goTo", "The goTo() method is not yet implemented in IrregularTSIterator.");
		return null;
	}

	// TODO SAM 2005-09-16 Remove the warning when implemented
	/// <summary>
	/// Go to the specified date/time, returning the matching data as if next() or
	/// previous() had been called.  If an exact match for the requested date/time
	/// cannot be made, return the nearest next (future) data.  Return null if the
	/// search cannot find a matching date/time (e.g., due to the end of the period).
	/// THIS METHOD IS NOT YET IMPLEMENTED FOR IrregularTSIterator.  null IS ALWAYS RETURNED. </summary>
	/// <param name="reset_if_fail"> If true and the search fails, reset to the starting
	/// position, but still return null.  If false and the search fails, the current
	/// date/time of the iterator will be at the end of data and null will be returned. </param>
	/// <returns> the TSData for the requesting date/time.  WARNING:  the contents of this object are volatile
	/// and change with each iteration.  Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public override TSData goToNearestNext(DateTime dt, bool reset_if_fail)
	{
		Message.printWarning(1, "IrregularTSIterator.goToNearestNext", "The goToNearestNext() method is not yet implemented in IrregularTSIterator.");
		return null;
	}

	// TODO SAM 2005-09-16 Remove the warning when implemented
	/// <summary>
	/// Go to the specified date/time, returning the matching data as if next() or
	/// previous() had been called.  If an exact match for the requested date/time
	/// cannot be made, return the nearest previous (past) data.  Return null if the
	/// search cannot find a matching date/time (e.g., due to the end of the period). </summary>
	/// <param name="reset_if_fail"> If true and the search fails, reset to the starting
	/// position, but still return null.  If false and the search fails, the current
	/// date/time of the iterator will be at the end of data and null will be returned. </param>
	/// <returns> the TSData for the requesting date/time.
	/// THIS METHOD IS NOT YET IMPLEMENTED FOR IrregularTSIterator.  null IS ALWAYS RETURNED.
	/// WARNING:  the contents of this object are volatile and change with each iteration.
	/// Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public override TSData goToNearestPrevious(DateTime dt, bool reset_if_fail)
	{
		Message.printWarning(1, "IrregularTSIterator.goToNearestPrevious", "The goToNearestPrevious() method is not yet implemented in IrregularTSIterator.");
		return null;
	}

	/// <summary>
	/// Indicate if next() can be called to return data.
	/// If false is returned, then the iterator is positioned at the last date with data.
	/// </summary>
	public override bool hasNext()
	{
		// For an irregular time series, get the date/time for the next value and check whether it is past _date2
		TSData nextData = _tsdata.getNext();
		if (nextData == null)
		{
			// There is no next point
			return false;
		}
		else
		{
			DateTime dt = nextData.getDate();
			if (dt == null)
			{
				// Next point does not have a date
				return false;
			}
			else if (dt.greaterThan(_date2))
			{
				// Next point is past the end
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Advance the iterator.  When called the first time, the initial value will be returned. </summary>
	/// <returns> null if the data is past the end or a pointer to an internal
	/// TSData object containing the data for the current time step (WARNING:  the
	/// contents of this object are volatile and change with each iteration).  Use the
	/// get* methods in TSIterator to retrieve data directly. </returns>
	public override TSData next()
	{
		int dl = 30;
		TSData theData = null; // Use this for returns.

		if ((_ts == null) || (_ts.getDataSize() == 0))
		{
			return null;
		}
		if (_lastDateProcessed)
		{
			return null;
		}

		// Only want to advance the date if we have not already gone past the end...

		if (_firstDateProcessed)
		{
			theData = _tsdata.getNext();
		}
		else
		{
			IList<TSData> v = ((IrregularTS)_ts).getData();
			if ((v == null) || (v.Count == 0))
			{
				return null;
			}
			int size = v.Count;
			TSData ptr = null;
			for (int i = 0; i < size; i++)
			{
				ptr = v[i];
				if (ptr.getDate().Equals(_currentDate))
				{
					theData = ptr;
					break;
				}
			}
			_firstDateProcessed = true;
		}

		if (theData == null)
		{
			// We are at the end or have exceeded the limits the data...
			_lastDateProcessed = true;
		}
		else
		{
			_currentDate = theData.getDate();
			if (_currentDate.greaterThan(_date2))
			{
				// We are further than we want to go ...
				_lastDateProcessed = true;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, "IrregularTSIterator.next", "Have passed end date: " + _date2.ToString());
				}
			}
		}

		if (_lastDateProcessed)
		{
			return null;
		}
		else
		{
			_tsdata = theData;
			return theData;
		}
	}

	// TODO - need to add previous also need to add clone().

	}

}