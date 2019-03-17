﻿using System;

// TSIterator - used to iterate through time series data

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
// TSIterator - used to iterate through time series data
// ----------------------------------------------------------------------------
// History:
//
// 05 Jul 2000	Steven A. Malers, RTi	Copy TSDateIterator and update to have
//					basic needed functionality.
// 27 Jul 2000	Michael Thiemann, RTi   Use TSIterator as base class and use
//					as the default for regular time series
//					but create the IrregularTSIterator
//					class to handle irregular time series.
//					The base class will work with irregular
//					time series, but is slower.
// 11 Oct 2000	SAM, RTi		Port to java.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//					Fix some potential bugs where internal
//					dates where not being set to copies
//					(therefore the original data may have
//					been getting modified).
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
// 2003-07-24	SAM, RTi		* Minor cleanup to synchronize with C++.
//					* Make protected data members use full
//					  javadoc.
//					* Add more examples to the class
//					  summary to illustate use of the
//					  iterator.
//					* Throw Exception in the constructor
//					  to more explicitly handle null time
//					  series and dates.
//					* In the copy constructor, clone the
//					  DateTime objects (previously just set
//					  the reference).
//					* Remove checks for year=0.  DateTime
//					  robustly handles creation, etc and
//					  should only have year=0 for valid data
//					  (e.g., monthly average time series).
//					* Remove call to TS.getDataDate() - this
//					  method was originally implemented for
//					  iteration other than TSIterator, which
//					  never panned out.  The method does not
//					  do anything anyhow as called.
//					* Add previous() as per the C++ code.
//					* Move getDuration() to
//					  IrregularTSIterator() since it does
//					  not make sense for regular interval
//					  time series.
// 2005-09-14	SAM, RTi		* Enable the previous() method.
//					* Add goTo(), goToNearestNext(),
//					  goToNearestPrevious().
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// The TSIterator allows iteration through a regular-interval time series.  Use the
	/// IrregularTSIterator for irregular-interval time series.  In general,
	/// this should be transparent because the proper iterator will be created from the TS.iterator() method.
	/// Use the iterator as follows:
	/// <pre>
	/// TS somets;	// Construct and initialize a time series or use an existing time series.
	/// TSIterator tsi = somets.iterator ( somets.getDate1(), somets.getDate2() );
	///		// Can construct with any set of dates (ideally matching the
	///		// time series interval) or no dates, in which case the full
	///		// time series period will be used.
	/// DateTime date;
	/// double value;
	/// TSData data;
	/// for ( ; (data = tsi.next()) != null; ) {
	///			// The first call will set the pointer to the
	///			// first data value in the period.  next() will return
	///			// null when the last date in the processing period has been passed.
	/// date = tsi.getDate();
	/// value = tsi.getDataValue();
	/// }
	/// </pre>
	/// Alternatively, operate on a TSData object as follows:
	/// <pre>
	/// TSData data;
	/// for ( data = tsi.next(); data != null; data = tsi.next() ) {
	/// date = data.getDate();
	/// value = data.getData();
	/// }
	/// </pre>
	/// The previous() method can be substituted for next() if iteration is to occur
	/// backwards (in this case the date/times are still specified with the earliest date/time first).
	/// </summary>
	public class TSIterator
	{

	/// <summary>
	/// Time series to iterate on.
	/// </summary>
	protected internal TS _ts = null;

	/// <summary>
	/// Interval base for time series, to optimize code.
	/// </summary>
	protected internal int _intervalBase = 0;

	/// <summary>
	/// Interval multiplier for time series, to optimize code.
	/// </summary>
	protected internal int _intervalMult = 0;

	/// <summary>
	/// Date/time to start iteration.
	/// </summary>
	protected internal DateTime _date1 = null;

	/// <summary>
	/// Date/time to end iteration.
	/// </summary>
	protected internal DateTime _date2 = null;

	/// <summary>
	/// Date/time for current position.
	/// </summary>
	protected internal DateTime _currentDate = null;

	/// <summary>
	/// Indicates whether the first date has been processed.
	/// Only next() and previous() should change the values of this data member.
	/// </summary>
	protected internal bool _firstDateProcessed = false;

	/// <summary>
	/// Indicates whether the last date has been processed.
	/// Only next() and previous() should change the values of this data member.
	/// </summary>
	protected internal bool _lastDateProcessed = false;

	/// <summary>
	/// The data object to return.  It is reused for each return to avoid numerous memory allocation operations.
	/// </summary>
	protected internal TSData _tsdata = null;

	/// <summary>
	/// Construct a time series iterator and set the period to be the full limits
	/// of the time series data.  The current date/time is set to the first date/time
	/// of the time series and will be returned by the first next() call. </summary>
	/// <param name="ts"> Time series to iterate on. </param>
	/// <exception cref="if"> the time series or date/times for the time series are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSIterator(TS ts) throws Exception
	public TSIterator(TS ts)
	{
		initialize();

		_ts = ts;
		_intervalBase = _ts.getDataIntervalBase();
		_intervalMult = _ts.getDataIntervalMult();
		if (ts == null)
		{
			throw new Exception("Null time series for TSIterator");
		}
		if (ts.getDate1() == null)
		{
			throw new Exception("Null starting date/time for TSIterator");
		}
		if (ts.getDate2() == null)
		{
			throw new Exception("Null ending date/time for TSIterator");
		}
		_date1 = new DateTime(ts.getDate1());
		_date2 = new DateTime(ts.getDate2());
		_currentDate = new DateTime(_date1);
	}

	/// <summary>
	/// Construct and set the period for the iterator to the specified dates.
	/// The current date/time for the iterator is set to the first specified date/time
	/// and will be returned with the first call to next(). </summary>
	/// <param name="ts"> Time series to iterate on. </param>
	/// <param name="date1"> First date/time in period to iterate through.  If null, the first
	/// date in the time series will be used. </param>
	/// <param name="date2"> Last date/time in period to iterate through.  If null, the last
	/// date in the time series will be used. </param>
	/// <exception cref="if"> the time series or dates for the time series are null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSIterator(TS ts, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	public TSIterator(TS ts, DateTime date1, DateTime date2)
	{
		initialize();

		if (ts == null)
		{
			throw new Exception("Null time series for TSIterator");
		}
		if ((date1 == null) && (ts.getDate1() == null))
		{
			throw new Exception("Null starting date/time for TSIterator");
		}
		if ((date2 == null) && (ts.getDate2() == null))
		{
			throw new Exception("Null ending date/time for TSIterator");
		}
		_ts = ts;
		_intervalBase = _ts.getDataIntervalBase();
		_intervalMult = _ts.getDataIntervalMult();

		if (date1 != null)
		{
			_date1 = new DateTime(date1);
		}
		else
		{
			_date1 = new DateTime(ts.getDate1());
		}

		if (date2 != null)
		{
			_date2 = new DateTime(date2);
		}
		else
		{
			_date2 = new DateTime(ts.getDate2());
		}

		_currentDate = new DateTime(_date1);

		/* FIXME SAM 2009-11-13 THIS CODE COMMENTED OUT ON 2009-11-13 - SEEMS TO BE A BUG AND NOT
		CLEAR WHY IT IS HERE.  Remove this code if tests are OK and no further fixes are needed.
		// It is possible that the date passed in does not agree with a date
		// in the time series.  For the initialization of the iterator, we want
		// the _current_date to be an actual date in the data...
		// TODO SAM 2009-01-15 Evaluate code
		if ( Message.isDebugOn ) {
			Message.printDebug ( 50, "TSIterator", "Requested start date is " + _currentDate );
		}
		if ( _currentDate.greaterThan(_ts.getDate2()) ) {
			if ( Message.isDebugOn ) {
				Message.printDebug ( 50, "TSIterator", "Initialized Current Date " + _currentDate +
				" is larger than last Date of TS " + _ts.getDate2().toString() + ". TS has only one value!" );
			}
			_currentDate = new DateTime (_ts.getDate2());
		}
	
		if ( Message.isDebugOn ) {
			Message.printDebug ( 50, "TSIterator", "After check, start date changed to " + _currentDate );
		}
		*/
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="i"> Iterator to copy. </param>
	public TSIterator(TSIterator i)
	{
		initialize();

		_ts = i._ts; // Don't need to make a deep copy because this is meant to be a reference.
		_intervalBase = i._intervalBase;
		_intervalMult = i._intervalMult;
		_date1 = (DateTime)i._date1.clone();
		_date2 = (DateTime)i._date2.clone();
		_currentDate = (DateTime)i._currentDate.clone();
	}

	/// <summary>
	/// Clone this TSIterator object.  The Object base class clone() method is called and then the TSIterator
	/// objects are cloned.  The result is a complete deep copy.  The time series that is being iterated is
	/// NOT cloned because only a reference is maintained by the TSIterator.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			// Clone the base class...
			TSIterator tsi = (TSIterator)base.clone();
			// Don't want to clone the TS reference, just set the reference...
			tsi._ts = _ts;
			// Now clone the mutable objects...
			if (_date1 != null)
			{
				tsi._date1 = (DateTime)_date1.clone();
			}
			if (_date2 != null)
			{
				tsi._date2 = (DateTime)_date2.clone();
			}
			if (_currentDate != null)
			{
				tsi._currentDate = (DateTime)_currentDate.clone();
			}
			return tsi;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TSIterator()
	{
		_ts = null;
		_date1 = null;
		_date2 = null;
		_currentDate = null;
		_tsdata = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Indicate if next() can be called to return data.
	/// If false is returned, then the iterator is positioned at the last date with data.
	/// </summary>
	public virtual bool hasNext()
	{
		// For a regular time series, add one interval to the current date and see if
		// it is past the end of the iterator
		DateTime dt = new DateTime(_currentDate);
		dt.addInterval(_intervalBase, _intervalMult);
		if (dt.greaterThan(_date2))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Return the current state of the isLastDateProcessed flag. </summary>
	/// <returns> the current state of isLastDateProcessed flag for the iterator. </returns>
	public virtual bool isLastDateProcessed()
	{
		return _lastDateProcessed;
	}

	/// <summary>
	/// Return the current date for the iterator. </summary>
	/// <returns> the current date for the iterator. </returns>
	public virtual DateTime getDate()
	{
		return _currentDate;
	}

	/// <summary>
	/// Return the data flag for the current date. </summary>
	/// <returns> the data flag for the current date. </returns>
	public virtual string getDataFlag()
	{
		return _tsdata.getDataFlag();
	}

	/// <summary>
	/// Return the data value for the current date. </summary>
	/// <returns> the data value for the current date. </returns>
	public virtual double getDataValue()
	{
		return _tsdata.getDataValue();
	}

	// TODO SAM 2005-09-14 Make this private for now since we call only when data should be available.
	// This method may be useful if public.
	/// <summary>
	/// Return the TSData for the current date/time of the iterator. </summary>
	/// <returns> the TSData for the current date/time of the iterator.  WARNING:  the contents of this object are
	/// volatile and change with each call.  Use the get*() methods in TSIterator to retrieve data directly prior
	/// to making subsequent calls. </returns>
	private TSData getCurrentTSData()
	{
		return _ts.getDataPoint(_currentDate, _tsdata);
	}

	/// <summary>
	/// Return the reference to the time series being iterated. </summary>
	/// <returns> reference to the TS. </returns>
	public virtual TS getTS()
	{
		return _ts;
	}

	/// <summary>
	/// Go to the specified date/time, returning the matching data as if next() or previous() had been called.
	/// The date/time in the time series MUST exactly match the date (dt.equals(date/time from time series) is called).
	/// If unable to go to the date/time, null is returned. </summary>
	/// <param name="dt"> Date/time to go to. </param>
	/// <param name="resetIfFail"> If true and the search fails, reset to the starting
	/// position, but still return null.  If false and the search fails, the current
	/// date/time of the iterator will be at the end of data and null will be returned. </param>
	/// <returns> the TSData for the requesting date/time.  WARNING:  the contents of this object are volatile and
	/// change with each iteration.  Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public virtual TSData goTo(DateTime dt, bool resetIfFail)
	{
		DateTime date = null;
		TSData data = null;
		// If the iterator has not fully initialized, call next() once to force it...
		if (!_firstDateProcessed && !_lastDateProcessed)
		{
			next();
		}
		DateTime currentDateOrig = null;
		bool lastDateProcessedOrig = _lastDateProcessed;
		bool firstDateProcessedOrig = _firstDateProcessed;
		if (dt.Equals(_currentDate))
		{
			// Just return...
			return getCurrentTSData();
		}
		else if (dt.greaterThan(_currentDate))
		{
			// Requested date/time is greater than current.
			// Need to move forward in time.
			if (resetIfFail)
			{
				// Save the starting conditions in case the search fails...
				if (_currentDate != null)
				{
					currentDateOrig = new DateTime(_currentDate);
				}
			}
			while ((data = next()) != null)
			{
				date = data.getDate();
				if (dt.Equals(date))
				{
					return data;
				}
				else if (dt.lessThan(date))
				{
					// Have passed the requested date/time.
					break;
				}
			}
			// If here the search failed.
			if (resetIfFail)
			{
				_currentDate = currentDateOrig;
				_firstDateProcessed = firstDateProcessedOrig;
				_lastDateProcessed = lastDateProcessedOrig;
			}
			return null;
		}
		else
		{
			// Requested date/time is earlier than the current date/time.  Need to move back in time...
			if (resetIfFail)
			{
				// Save the starting conditions in case the search fails...
				if (_currentDate != null)
				{
					currentDateOrig = new DateTime(_currentDate);
				}
			}
			while ((data = previous()) != null)
			{
				date = data.getDate();
				if (dt.Equals(date))
				{
					return data;
				}
				else if (dt.greaterThan(date))
				{
					// Have passed the requested date/time.
					break;
				}
			}
			// If here the search failed.
			if (resetIfFail)
			{
				_currentDate = currentDateOrig;
				_firstDateProcessed = firstDateProcessedOrig;
				_lastDateProcessed = lastDateProcessedOrig;
			}
			return null;
		}
	}

	/// <summary>
	/// Go to the specified date/time, returning the matching data as if next() or
	/// previous() had been called.  If an exact match for the requested date/time
	/// cannot be made, return the nearest next (future) data.  Return null if the
	/// search cannot find a matching date/time (e.g., due to the end of the period). </summary>
	/// <param name="resetIfFail"> If true and the search fails, reset to the starting
	/// position, but still return null.  If false and the search fails, the current
	/// date/time of the iterator will be at the end of data and null will be returned. </param>
	/// <returns> the TSData for the requesting date/time.  WARNING:  the contents of this object are volatile
	/// and change with each iteration.  Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public virtual TSData goToNearestNext(DateTime dt, bool resetIfFail)
	{
		DateTime date = null;
		TSData data = null;
		// If the iterator has not fully initialized, call next() once to force it...
		if (!_firstDateProcessed && !_lastDateProcessed)
		{
			next();
		}
		DateTime currentDateOrig = null;
		bool lastDateProcessedOrig = _lastDateProcessed;
		bool firstDateProcessedOrig = _firstDateProcessed;
		if (resetIfFail)
		{
			// Save the starting conditions in case the search fails...
			if (_currentDate != null)
			{
				currentDateOrig = new DateTime(_currentDate);
			}
		}
		if (dt.Equals(_currentDate))
		{
			// Just return...
			return getCurrentTSData();
		}
		else if (dt.greaterThanOrEqualTo(_currentDate))
		{
			// Requested date/time is greater than the iterator current time so need to move forward in time.
			while ((data = next()) != null)
			{
				date = data.getDate();
				if (dt.greaterThan(date))
				{
					// Still not there...
					continue;
				}
				else
				{
					// Have matched/passed the requested date/time...
					return data;
				}
			}
			// If here the search failed.
			if (resetIfFail)
			{
				_currentDate = currentDateOrig;
				_firstDateProcessed = firstDateProcessedOrig;
				_lastDateProcessed = lastDateProcessedOrig;
			}
			return null;
		}
		else
		{
			// Requested date/time is less than the current time so need to move back in time...
			while ((data = previous()) != null)
			{
				date = data.getDate();
				if (dt.lessThan(date))
				{
					// Still not there...
					continue;
				}
				else if (dt.Equals(date))
				{
					// Have matched the requested date/time so return it...
					return data;
				}
				else
				{
					// Have passed the requested date/time.  Return the next item (which should be after the
					// requested date/time...
					data = next();
					if (data != null)
					{
						return data;
					}
					else
					{
						break; // Return null below.
					}
				}
			}
			// If here the search failed.
			if (resetIfFail)
			{
				_currentDate = currentDateOrig;
				_firstDateProcessed = firstDateProcessedOrig;
				_lastDateProcessed = lastDateProcessedOrig;
			}
			return null;
		}
	}

	/// <summary>
	/// Go to the specified date/time, returning the matching data as if next() or
	/// previous() had been called.  If an exact match for the requested date/time
	/// cannot be made, return the nearest previous (past) data.  Return null if the
	/// search cannot find a matching date/time (e.g., due to the end of the period). </summary>
	/// <param name="resetIfFail"> If true and the search fails, reset to the starting
	/// position, but still return null.  If false and the search fails, the current
	/// date/time of the iterator will be at the end of data and null will be returned. </param>
	/// <returns> the TSData for the requesting date/time.  WARNING:  the contents of this object are volatile
	/// and change with each iteration.  Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public virtual TSData goToNearestPrevious(DateTime dt, bool resetIfFail)
	{
		DateTime date = null;
		TSData data = null;
		// If the iterator has not fully initialized, call previous() once to force it...
		if (!_firstDateProcessed && !_lastDateProcessed)
		{
			previous();
		}
		DateTime currentDateOrig = null;
		bool lastDateProcessedOrig = _lastDateProcessed;
		bool firstDateProcessedOrig = _firstDateProcessed;
		if (resetIfFail)
		{
			// Save the starting conditions in case the search fails...
			if (_currentDate != null)
			{
				currentDateOrig = new DateTime(_currentDate);
			}
		}
		if (dt.Equals(_currentDate))
		{
			// Just return...
			return getCurrentTSData();
		}
		else if (dt.greaterThan(_currentDate))
		{
			// Requested date/time is after current date/time.  Need to move forward in time.
			while ((data = next()) != null)
			{
				date = data.getDate();
				if (dt.Equals(date))
				{
					return data;
				}
				else if (dt.lessThan(date))
				{
					// Moved one past so return the previous...
					data = previous();
					if (data != null)
					{
						return data;
					}
					else
					{
						break; // return null below.
					}
				}
			}
			// If here the search failed.
			if (resetIfFail)
			{
				_currentDate = currentDateOrig;
				_firstDateProcessed = firstDateProcessedOrig;
				_lastDateProcessed = lastDateProcessedOrig;
			}
			return null;
		}
		else
		{
			// Requested date/time is less than current date/time.  Need to move back in time...
			while ((data = previous()) != null)
			{
				date = data.getDate();
				if (dt.lessThan(date))
				{
					// Still searching...
					continue;
				}
				else
				{
					// Have matched/passed the requested date/time...
					return data;
				}
			}
			// If here the search failed.
			if (resetIfFail)
			{
				_currentDate = currentDateOrig;
				_firstDateProcessed = firstDateProcessedOrig;
				_lastDateProcessed = lastDateProcessedOrig;
			}
			return null;
		}
	}

	/// <summary>
	/// Initialize data.
	/// </summary>
	private void initialize()
	{
		_ts = null;
		_firstDateProcessed = false;
		_lastDateProcessed = false;
		_tsdata = new TSData();
	}

	/// <summary>
	/// Advance the iterator one data point.  When called the first time, the first data
	/// point will be returned.  This method is used to advance forward through a time series. </summary>
	/// <returns> null if the time series has no data value or the date/time is past the
	/// end.  If the previous situation does not apply, return a pointer to an internal
	/// TSData object containing the data for the current time step (WARNING:  the
	/// contents of this object are volatile and change with each iteration).  Use the
	/// get*() methods in TSIterator to retrieve data directly. </returns>
	public virtual TSData next()
	{
		int dl = 30;

		if ((_ts == null) || (_ts.getDataSize() == 0))
		{
			return null;
		}
		if (!_lastDateProcessed)
		{
			// We only want to advance the date if we have not already gone past the end...
			if (!_firstDateProcessed)
			{
				// It is possible that the date specified as input does
				// not exactly align with a date in the time series.
				// But it is adjusted at construction.
				_firstDateProcessed = true;
			}
			else
			{
				// Only increment if have processed the first date...
				// The following should only be used for IrregularTS.  However, IrregularTS now should
				// return an IrregularTSIterator so the following should actually never get called...
				_currentDate.addInterval(_intervalBase, _intervalMult);
			}
			if (_currentDate.greaterThan(_date2))
			{
				// We are at the end or have exceeded the limits of the data...
				_lastDateProcessed = true;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, "TSIterator.next", "Have passed end date: " + _date2);
				}
			}
		}
		if (_lastDateProcessed)
		{
			return null;
		}
		else
		{
			// Set the values.  These are used by getDate, etc...
			return getCurrentTSData();
		}
	}

	/// <summary>
	/// Decrement the iterator one data point.  When called the first time, the last
	/// data point will be returned.  This method can be used to iterate backwards
	/// through a time series.  The previous() method can be used with next().
	/// Because the default construction of this class assumes that forward iteration
	/// with next() will occur, the first call to previous() will reset the current
	/// date/time to the end of the period (from the time series or as specified in
	/// the constructor).  This occurs only if next() has not been called. </summary>
	/// <returns> null if there are no data or the date/time is past the first date in the
	/// iteration period.  If the previous situation does not apply, return a pointer
	/// to an internal TSData object containing the data for the current time step
	/// (WARNING:  the contents of this object are volatile and change with each
	/// iteration).  Use the get*() methods in TSIterator to retrieve data directly. </returns>
	public virtual TSData previous()
	{
		int dl = 30;

		if ((_ts == null) || (_ts.getDataSize() == 0))
		{
			return null;
		}
		if (!_firstDateProcessed)
		{
			// We only want to decrement the date if we have not already gone past the end...
			if (!_firstDateProcessed && !_lastDateProcessed)
			{
				// Because at construction the current date/time is set
				// to the start, we need to override here and set to the
				// ending date/time.  Only do this if next() has not
				// been called (in this case _first_date_processed will be false).
				_currentDate = new DateTime(_date2);
				_lastDateProcessed = true;
			}
			else if (!_lastDateProcessed)
			{
				// It is possible that the date specified as input does
				// not exactly align with a date in the time series.
				// But it is adjusted at construction.
				_lastDateProcessed = true;
			}
			else
			{
				// Only decrement if have processed the last date...
				// The following should only be used for IrregularTS.
				// However, IrregularTS now should return an
				// IrregularTSIterator so the following should actually never get called...
				_currentDate.addInterval(_intervalBase, -_intervalMult);
			}
			if (_currentDate.lessThan(_date1))
			{
				// We are at the end or have exceeded the limits of the data...
				_firstDateProcessed = true;
				if (Message.isDebugOn)
				{
					Message.printDebug(dl, "TSIterator.previous", "Have passed start date: " + _date1);
				}
			}
		}
		if (_firstDateProcessed)
		{
			// FIXME SAM 2015-10-17 If next() is called to initialize iteration, then calling previous() always returns null
			return null;
		}
		else
		{
			// Set the values.  These are used by getDate, etc...
			return getCurrentTSData();
		}
	}

	/// <summary>
	/// Set the starting date/time for the iterator.  Use this to reset the start date.
	/// The iterator will be reset and a call to next() will return the first value. </summary>
	/// <param name="date1"> New starting date/time for iterator. </param>
	public virtual void setBeginTime(DateTime date1)
	{
		_date1 = new DateTime(date1);
		_firstDateProcessed = false;
		_lastDateProcessed = false;
		_currentDate = new DateTime(_date1); // Default for next()
	}

	/// <summary>
	/// Set the ending date/time for the iterator.
	/// The iterator will be reset.  A call to next() will not return the first value
	/// but will return the next value after the last value returned. </summary>
	/// <param name="date2"> New end date/time for iterator. </param>
	public virtual void setEndTime(DateTime date2)
	{
		_date2 = new DateTime(date2);
		_lastDateProcessed = false;
	}

	}

}