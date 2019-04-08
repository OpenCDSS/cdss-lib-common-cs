using System;
using System.Collections.Generic;
using System.Text;

// TS - base class from which all time series are derived

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
// TS - base class from which all time series are derived
// see C++ version for difference notes.
// ----------------------------------------------------------------------------
// This is the abstract base class for all time series.
// The derived class hierarchy is then:
//
//              -----------------------TS-----------------------------
//              |           |        |          |                    |
//              V           V        V          V                    V
//           MinuteTS     HourTS   DayTS     MonthTS             IrregularTS
//              |           |                   |
//
//         ~~~~~~~~~~~~~~~~~~~Below have static read/write methods~~~~~~~~~~
//              |           |                   |
//              V           V                   V
//       HECDSSMinuteTS NWSCardTS          DateValueTS		etc.
//
// In general, the majority of the data members will be used for all derived
// time series.  However, some will not be appropriate.  The decision has been
// made to keep them here to make it less work to manage the first layer of
// derived classes.  For example, the irregular time series does not need to
// store _interval_mult.  Most of the other data still apply.
//
// Because this is a base class, anything derived from the class can only be as
// specific as the base class.  Therefore, routines to do conversions between
// time series types will have to be done outside of this class library.  Using
// the TS base class allows polymorphism when used by complicated objects.
//
// It is assumed that the first layer will define the best way to store the
// data.  In other words, different data intervals will have different storage
// methods.  The goal is not to conceptualize time series so much that they all
// consist of an array of date and data values.  For constant interval time
// series, the best performance will be to store the values in an ordered array
// that is indexed by month, etc.  If this assumption is wrong, then maybe the
// middle layer gets folded into the base class.
//
// It may be desirable to build some streaming control into this class.  For
// example, read multiple time series from the same file (for certain time
// series types) or may want to read one block of data from multiple files
// (e.g., when running a memory-sensitive application that only needs to have
// in memory one month of data for every time series in the system and reuses
// that space).  For now assume that we will always read the entire time series
// in but be aware that more control may be added later.
//
// New code does not have 3 layers.  Instead, I/O classes should have static
// methods like readTimeSeries, writeTimeSeries that operate on time series
// instances.  See DateValueTS for an example.  Older code may not follow this
// approach.
// ----------------------------------------------------------------------------
// History:
//
// Apr, May 96	Steven A. Malers, RTi	Start developing the library based on
//					the C version of the TS library.
// 10 Sep 96	SAM, RTi		Formalize the class enough so that we
//					can begin to use with the Operation
//					class to work on the DSS.
// 05 Feb 97	SAM, RTi		Add allocateDataSpace and setDataValue
//					virtual functions.  Add _dirty
//					so that we know when data has been
//					set (to indicate that we need to redo
//					calcMaxMin).  Add getDataPosition and
//					freeDataSpace.
// 26 May 97	SAM, RTi		Add writePersistentHeader.  To increase
//					performance in derived classes, make
//					more data members protected.  Also add
//					_data_date1 and _data_date2 to hold the
//					dates where we actually have data.
// 06 Jun 1997	SAM, RTi		Add a third position argument to
//					getDataPosition to work with the
//					MinuteTS data.  Other intervals will not
//					use the 2nd or 3rd positions.
//					Add TSIntervalFromString.
// 16 Jun 1997	MJR, RTi		Overloaded calcMaxMinValues to 
//					find and return max and min between
//					two dates that are passed in.
// 03 Nov 1997  Daniel Weiler, RTi	Added GetPeriodFromTS function
// 26 Nov 1997	SAM, DKW, RTi		Add getValidPeriod.
// 14 Dec 1997	SAM, RTi		Add copyHeader.
// 06 Jan 1998	SAM, RTi		Update to use getDataLimits,
//					setDataLimits, refresh(), and change
//					data limits to _data_limits.  Put all
//					but the original dates and overal date
//					limits in _data_limits.
//					Add _sequence_number.
// 22 Feb 1998	SAM, RTi		Add _data_size to hold the total number
//					of elements allocated for data.
// 31 Mar 1998	SAM, DLG, RTi		Add _legend for use with output plots,
//					reports, etc.
// 28 Apr 1998	SAM, RTi		Add _status to allow general use by
//					other programs (e.g., to indicate that
//					the TS should not be used in a later
//					computation).
// 01 May 1998	SAM, RTi		Add _extended_legend for use with output
//					reports, etc.  Change so that
//					setComments resets the comments.
// 07 May 1998	SAM, RTi		Add formatHeader.
// 13 Jul 1998	SAM, RTi		Update copyHeader documentation.
// 23 Jul 1998	SAM, RTi		Add changePeriodOfRecord as "virtual"
//					function.  This needs to be implemented
//					at the storage level (next level of
//					extension).
// 06 Aug 1998	SAM, RTi		Remove getDataPosition, getDataPointer
//					from this class.  Those routines are
//					often not needed and should be private
//					to the derived classes.
// 20 Aug 1998	SAM, RTi		OK, realized that getDataPosition is
//					valuable for derived classes, but change
//					to return an array of integers with the
//					positions.  Make class abstract to
//					pass compile with 1.2.
// 18 Nov 1998	SAM, RTi		Add copyData method.
// 11 Jan 1998	SAM, RTi		Add routine name to virtual functions
//					so we can track down problems.
// 13 Apr 1999	SAM, RTi		Add finalize.  Add genesis format flag.
//					Change so addToGenesis does not
//					include routine name.
// 28 Jan 2000	SAM, RTi		Add setMissingRange() to allow handling
//					of -999 and -998 missing data values
//					in NWS work.
// 11 Oct 2000	SAM, RTi		Add iterator(), getDataPoint().
// 16 Oct 2000	SAM, RTi		Add _enabled, _selected, and _plot*
//					data to work with visualization.
// 13 Nov 2000	SAM, RTi		copyHeader() was not copying the
//					interval base.
// 20 Dec 2000	SAM, RTi		Add _data_limits_original, which is
//					currently just a convenience for code
//					(like tstool) so the original data
//					limits can be saved for use with
//					filling.  This may actually be a good
//					way to compare before and after data
//					statistics.  This data item is not a
//					copy of the limits (whereas the current
//					data limits object is a copy - the old
//					convention may be problematic).
// 28 Jan 2001	SAM, RTi		Update javadoc to not rely on @return.
//					Add checks for null strings when adding
//					to comments/genesis.  Add
//					allocateDataSpace() that takes period,
//					consistent with C++.  Make sure methods
//					are alphabetized.  Change so setDate*
//					methods set the precision of the date
//					to be appropriate for the time series
//					interval.
// 21 Feb 2001	SAM, RTi		Implement clone().  Add getInputName()
//					and setInputName().
// 31 May 2001	SAM, RTi		Use TSDate.setPrecision(TS) to ensure
//					start and end dates are the correct
//					precision in set*Date() methods.
// 28 Aug 2001	SAM, RTi		Fix the clone() method to be a deep
//					copy.
// 2001-11-05	SAM, RTi		Full review of javadoc.  Verify that
//					variables are set to null when no longer
//					used.  Change methods to have return
//					type of void where appropriate.
//					Change calculateDataSize() to be a
//					static method.  Remove the deprecated
//					readPersistent(), writePersistent()
//					methods.
// 2002-01-21	SAM, RTi		Remove the plot data.  This is now
//					handled in the TSGraph* code.  By
//					removing here, we decouple the plot
//					properties from the TS, eliminating
//					problems.
// 2002-01-30	SAM, RTi		Add _has_data_flags, _data_flag_length,
//					hasDataFlags(), and setDataValue(with
//					data flag and duration) to support data
//					flags and duration.  Flags should be
//					returned by using the getDataPoint()
//					method in derived classes.  Remove the
//					input stream flags and data - this has
//					never been used.  Fix copyHeader() to
//					do a deep copy on the TSIdent.
// 2002-02-17	SAM, RTi		Change the sequence number initial value
//					to -1.
// 2002-04-17	SAM, RTi		Update setGenesis() to have append flag.
// 2002-04-23	SAM, RTi		Deprecated getSelected() and
//					setSelected() in favor of isSelected().
//					Add %z to formatLegend() to use the
//					sequence number.
// 2002-06-03	SAM, RTi		Add support for NaN as missing data
//					value.
// 2002-06-16	SAM, RTi		Add isDirty() to help IrregularTS.
// 2002-08-12	J. Thomas Sapienza, RTi	Added calcDataDate for use with JTable
//					models.
// 2002-09-04	SAM, RTi		Remove calcDataDate() - same effect
//					can occur by a call to a TSDate.
//					Add getDataFlagLength() to allow
//					DateValueTS to output the flags.
//					Update javadoc to explain that
//					allocateDataSpace() and
//					changePeriodOfRecord() should handle the
//					data flag - previously hasDataFlag.
// 2002-11-25	SAM, RTi		Change getDate*() methods to return null
//					if the requested data are null.
// 2003-01-08	SAM, RTi		Add a hasData() method to indicate
//					whether the time series has data.
// 2003-06-02	SAM, RTi		Upgrade to use generic classes.
//					* Change TSDate to DateTime.
//					* Change TSUnits to DataUnits.
//					* Remove INTERVAL_* - TS package classes
//					  should use TimeInterval.* instead.
//					* Remove _date_type data member and
//					  associated DATES_* - they have never
//					  been used.
//					* Remove FORMAT_ since other classes
//					  handle formatting themselves.
//					* Remove the _data_type because it is
//					  stored in the TSIdent.
// 2003-07-24	SAM, RTi		* Fully remove commented out code for
//					  getDataDate() - it is not used by
//					  TSIterator any more and no other code
//					  uses.
//					* TSIterator constructor now throws an
//					  exception so declare throws here.
// 2003-08-21	SAM, RTi		* Change isSelected(boolean) back to
//					  setSelected() - ARG!
//					* Add isEditable() and setEditable().
//					* Remove deprecated constructor to take
//					  a String (filename) - extended classes
//					  should handle I/O.
//					* Remove deprecated addToGenesis() that
//					  took a routine name.
//					* Remove deprecated INFINITY - not used.
// 2004-01-28	SAM, RTi		* Change wording in text format header
//					  to more clearly identify original and
//					  current data period.
// 2004-03-04	J. Thomas Sapienza, RTi	* Class now implements serializable.
//					* Class now implements transferable.
// 2004-04-14	SAM, RTi		* Fix so that when setting the dates
//					  the original time zone precision
//					  information is not clobbered.
// 2004-11-23	SAM, RTi		* Move the sequence number to TSIdent
//					  since it is now part of the TSID.
//					* In formatLegend(), use instance data
//					  members instead of calling get()
//					  methods - performance increases.
// 2005-05-12	SAM, RTi		* Add allocateDataFlagSpace() to support
//					  enabling data flags after the initial
//					  allocation.
// 2006-10-03	SAM, RTi		* Add %p to formatLegend() for period.
// 2006-11-22	SAM, RTi		Fix but in addToComments() where the
//					wrong Vector was being used.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.TS
{


	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/// <summary>
	/// This class is the base class for all time series classes.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TS implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
	[Serializable]
	public class TS : ICloneable, Transferable
	{

	// FIXME SAM 2007-12-13 Need to move transfer objects to wrapper around this class.
	/// <summary>
	/// Data flavor for transferring this object.
	/// </summary>
	public static DataFlavor tsFlavor = new DataFlavor(typeof(RTi.TS.TS),"RTi.TS.TS");

	/// <summary>
	/// General string to use for status of the time series (use as appropriate by
	/// high-level code).  This value is volatile - do not assume its value will remain
	/// for long periods.  This value is not used much now that the GRTS package has been updated.
	/// </summary>
	protected internal string _status;

	/// <summary>
	/// Beginning date/time for data, at a precision appropriate for the data.
	/// Missing data may be included in the period.
	/// </summary>
	protected internal DateTime _date1;

	/// <summary>
	/// Ending date/time for data, at a precision appropriate for the data.
	/// Missing data may be included in the period.
	/// </summary>
	protected internal DateTime _date2;

	/// <summary>
	/// Original starting date/time for data, at a precision appropriate for the data.
	/// For example, this may be used to indicate the period in a database, which is
	/// different than the period that was actually queried and saved in memory.
	/// </summary>
	protected internal DateTime _date1_original;

	/// <summary>
	/// Original ending date/time for data, at a precision appropriate for the data.
	/// For example, this may be used to indicate the period in a database, which is
	/// different than the period that was actually queried and saved in memory.
	/// </summary>
	protected internal DateTime _date2_original;

	/// <summary>
	/// The data interval base.  See TimeInterval.HOUR, etc.
	/// </summary>
	protected internal int _data_interval_base;

	/// <summary>
	/// The base interval multiplier (what to multiply _interval_base by to get the
	/// real interval).  For example 15-minute data would have
	/// _interval_base = TimeInterval.MINUTE and _interval_mult = 15.
	/// </summary>
	protected internal int _data_interval_mult;

	/// <summary>
	/// The data interval in the original data source (for example, the source may be
	/// in days but the current time series is in months).
	/// </summary>
	protected internal int _data_interval_base_original;

	/// <summary>
	/// The data interval multiplier in the original data source.
	/// </summary>
	protected internal int _data_interval_mult_original;

	/// <summary>
	/// Number of data values inclusive of _date1 and _date2.  Set in the
	/// allocateDataSpace() method.  This is useful for general information.
	/// </summary>
	protected internal int _data_size;

	/// <summary>
	/// Data units.  A list of units and conversions is typically maintained in the DataUnits* classes.
	/// </summary>
	protected internal string _data_units;

	/// <summary>
	/// Units in the original data source (e.g., the current data may be in CFS and the
	/// original data were in CMS).
	/// </summary>
	protected internal string _data_units_original;

	/// <summary>
	/// Indicates whether data flags are being used with data.  If enabled, the derived
	/// classes that store data should override the allocateDataSpace(boolean, int)
	/// method to create a data array to track the data flags.  It is recommended to
	/// save space that the flags be handled using String.intern().
	/// </summary>
	protected internal bool _has_data_flags = false;

	/// <summary>
	/// Indicate whether data flags should use String.intern().
	/// </summary>
	protected internal bool _internDataFlagStrings = true;

	// FIXME SAM 2007-12-13 Need to phase this out in favor of handling in DAO code.
	/// <summary>
	/// Version of the data format (mainly for use with files).
	/// </summary>
	protected internal string _version;

	// FIXME SAM 2007-12-13 Need to evaluate renaming to avoid confusion with TSIdent input name.
	// Implementing a DataSource concept for input/output may help (but also have data source in TSIdent!).
	/// <summary>
	/// Input source information.  Filename if read from file or perhaps a database
	/// name and table (e.g., HydroBase.daily_flow).  This is the actual location read,
	/// which should not be confused with the TSIdent storage name (which may not be fully expanded).
	/// </summary>
	protected internal string _input_name;

	/// <summary>
	/// Time series identifier, which provides a unique and absolute handle on the time series.
	/// An alias is provided within the TSIdent class.
	/// </summary>
	protected internal TSIdent _id;

	/// <summary>
	/// Indicates whether the time series data have been modified by calling
	/// setDataValue().  Call refresh() to update the limits.  This is not used with header data.
	/// </summary>
	protected internal bool _dirty;

	/// <summary>
	/// Indicates whether the time series is editable.  This primarily applies to the
	/// data (not the header information).  UI components can check to verify whether
	/// users should be able to edit the time series.  It is not intended to be checked
	/// by low-level code (manipulation is always granted).
	/// </summary>
	protected internal bool _editable = false;

	/// <summary>
	/// A short description (e.g, "XYZ gage at ABC river").
	/// </summary>
	protected internal string _description;

	/// <summary>
	/// Comments that describe the data.  This can be anything from an original data
	/// source.  Sometimes the comments are created on the fly to generate a standard
	/// header (e.g., describe drainage area).
	/// </summary>
	protected internal IList<string> _comments;

	/// <summary>
	/// List of metadata about data flags.  This provides a description about flags
	/// encountered in the time series.
	/// </summary>
	private IList<TSDataFlagMetadata> __dataFlagMetadataList = new List<TSDataFlagMetadata>();

	/// <summary>
	/// History of time series.  This is not the same as the comments but instead
	/// chronicles how the time series is manipulated in memory.  For example the first
	/// genesis note may be about how the time series was read.  The second may
	/// indicate how it was filled.  Many TSUtil methods add to the genesis.
	/// </summary>
	protected internal IList<string> _genesis;

	/// <summary>
	/// TODO SAM 2010-09-21 Evaluate whether generic "Attributable" interface should be implemented instead.
	/// Properties for the time series beyond the built-in properties.  For example, location
	/// information like county and state can be set as a property.
	/// </summary>
	private LinkedHashMap<string, object> __property_HashMap = null;

	/// <summary>
	/// The missing data value.  Default for some legacy formats is -999.0 but increasingly Double.NaN is used.
	/// </summary>
	protected internal double _missing;

	/// <summary>
	/// Lower bound on the missing data value (for quick comparisons and when missing data ranges are used).
	/// </summary>
	protected internal double _missingl;

	/// <summary>
	/// Upper bound on the missing data value (for quick comparisons and when missing data ranges are used).
	/// </summary>
	protected internal double _missingu;

	/// <summary>
	/// Limits of the data.  This also contains the date limits other than the original dates.
	/// </summary>
	protected internal TSLimits _data_limits;

	/// <summary>
	/// Limits of the original data.  Currently only used by apps like TSTool.
	/// </summary>
	protected internal TSLimits _data_limits_original;

	//TODO SAM 2007-12-13 Evaluate need now that GRTS is available.
	/// <summary>
	/// Legend to show when plotting or tabulating a time series.  This is generally a short legend.
	/// </summary>
	protected internal string _legend;

	// TODO SAM 2007-12-13 Evaluate need now that GRTS is available.
	/// <summary>
	/// Legend to show when plotting or tabulating a time series.  This is usually a
	/// long legend.  This may be phased out now that the GRTS package has been phased in for visualization.
	/// </summary>
	protected internal string _extended_legend;

	/// <summary>
	/// Indicates whether time series is enabled (used to "comment" out of plots, etc).
	/// This may be phased out.
	/// </summary>
	protected internal bool _enabled;

	/// <summary>
	/// Indicates whether time series is selected (e.g., as result of a query).
	/// Often time series might need to be programmatically selected (e.g., with TSTool
	/// selectTimeSeries() command) to simplify output by other commands.
	/// </summary>
	protected internal bool _selected;

	/// <summary>
	/// Construct a time series and initialize the member data.  Derived classes should
	/// set the _data_interval_base.
	/// </summary>
	public TS()
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(50, "TS.TS", "Constructing");
		}
		init();
	}

	/// <summary>
	/// Copy constructor.  Only the header is copied since derived classes should copy the data. </summary>
	/// <seealso cref= #copyHeader </seealso>
	public TS(TS ts)
	{
		copyHeader(this);
	}

	/// <summary>
	/// Add a TSDataFlagMetadata instance to the list maintained with the time series, to explain flag meanings. </summary>
	/// <param name="dataFlagMetadata"> instance of TSDataFlagMetadata to add to time series. </param>
	public virtual void addDataFlagMetadata(TSDataFlagMetadata dataFlagMetadata)
	{
		__dataFlagMetadataList.Add(dataFlagMetadata);
	}

	/// <summary>
	/// Add a String to the comments associated with the time series (e.g., station remarks). </summary>
	/// <param name="comment"> Comment string to add. </param>
	public virtual void addToComments(string comment)
	{
		if (!string.ReferenceEquals(comment, null))
		{
			_comments.Add(comment);
		}
	}

	/// <summary>
	/// Add a list of String to the comments associated with the time series (e.g., station remarks). </summary>
	/// <param name="comments"> Comments strings to add. </param>
	public virtual void addToComments(IList<string> comments)
	{
		if (comments == null)
		{
			return;
		}
		foreach (string comment in comments)
		{
			if (!string.ReferenceEquals(comment, null))
			{
				_comments.Add(comment);
			}
		}
	}

	/// <summary>
	/// Add a string to the genesis string list.  The genesis is a list of comments
	/// indicating how the time series was read and manipulated.  Genesis information
	/// should be added by methods that, for example, fill data and change the period. </summary>
	/// <param name="genesis"> Comment string to add to genesis information. </param>
	public virtual void addToGenesis(string genesis)
	{
		if (!string.ReferenceEquals(genesis, null))
		{
			_genesis.Add(genesis);
		}
	}

	/// <summary>
	/// Allocate the data flag space for the time series.  This requires that the data
	/// interval base and multiplier are set correctly and that _date1 and _date2 have been set.
	/// The allocateDataSpace() method will allocate the data flags if appropriate.
	/// Use this method when the data flags need to be allocated after the initial allocation.
	/// This method is meant to be overridden in derived
	/// classes (e.g., MinuteTS, MonthTS) that are optimized for data storage for different intervals. </summary>
	/// <param name="initialValue"> Initial value (null is allowed and will result in the flags being initialized to spaces). </param>
	/// <param name="retainPreviousValues"> If true, the array size will be increased if necessary, but
	/// previous data values will be retained.  If false, the array will be reallocated and initialized to spaces. </param>
	/// <exception cref="Exception"> if there is an error allocating the memory. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void allocateDataFlagSpace(String initialValue, boolean retainPreviousValues) throws Exception
	public virtual void allocateDataFlagSpace(string initialValue, bool retainPreviousValues)
	{
		Message.printWarning(1, "TS.allocateDataFlagSpace", "TS.allocateDataFlagSpace() is virtual, define in derived classes.");
	}

	/// <summary>
	/// Allocate the data space for the time series.  This requires that the data
	/// interval base and multiplier are set correctly and that _date1 and _date2 have
	/// been set.  If data flags are used, hasDataFlags() should also be called before
	/// calling this method.  This method is meant to be overridden in derived classes
	/// (e.g., MinuteTS, MonthTS) that are optimized for data storage for different intervals. </summary>
	/// <returns> 0 if successful allocating memory, non-zero if failure. </returns>
	public virtual int allocateDataSpace()
	{
		Message.printWarning(1, "TS.allocateDataSpace", "TS.allocateDataSpace() is virtual, define in derived classes.");
		return 1;
	}

	/// <summary>
	/// Allocate the data space for the time series using given dates.  This requires
	/// that the data interval base and multiplier are set correctly in the derived
	/// class (the allocateDataSpace(void) method of the derived class will be called). </summary>
	/// <param name="date1"> Start of period. </param>
	/// <param name="date2"> End of period. </param>
	/// <returns> 1 if there is an error allocating the data space, 0 if success. </returns>
	public virtual int allocateDataSpace(DateTime date1, DateTime date2)
	{ //Set data
		setDate1(date1);
		setDate1Original(date1);
		setDate2(date2);
		setDate2Original(date2);

	   // Now allocate memory for the new data space...
		if (allocateDataSpace() != 0)
		{
			Message.printWarning(3, "TS.allocateDataSpace(DateTime,DateTime)", "Error allocating data space for " + date1 + " to " + date2);
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// Calculate the data size (number of data points) for a time series, given the
	/// interval multiplier and start and end dates.  This method is meant to be
	/// overridden in derived classes (in which case the interval base will be known). </summary>
	/// <param name="date1"> Start of period. </param>
	/// <param name="date2"> End of period. </param>
	/// <param name="multiplier"> Multiplier for interval base. </param>
	/// <returns> the number of data points in a period, given the interval mulitplier. </returns>
	public static int calculateDataSize(DateTime date1, DateTime date2, int multiplier)
	{
		Message.printWarning(1, "TS.calculateDataSize", "TS.calculateDataSize() is virtual, define in derived classes.");
		return 0;
	}

	/// <summary>
	/// Change the period of record for the data.  If the period is lengthened, fill
	/// with missing data.  If shortened, truncate the data.  This method should be
	/// implemented in a derived class and should handle resizing of the data space and data flag array. </summary>
	/// <param name="date1"> Start date for new period. </param>
	/// <param name="date2"> End date for new period. </param>
	/// <exception cref="RTi.TS.TSException"> if this method is called in the base class (or if
	/// an error in the derived class). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void changePeriodOfRecord(RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws TSException
	public virtual void changePeriodOfRecord(DateTime date1, DateTime date2)
	{
		string routine = "TS.changePeriodOfRecord";

		Message.printWarning(1, routine, "TS.changePeriodOfRecord is a virtual function, redefine in derived classes");

		throw new TSException(routine + ": changePeriodOfRecord needs to be implemented in derived class!");
	}

	/// <summary>
	/// Clone the object.  The Object base class clone() method is called and then the
	/// TS objects are cloned.  The result is a complete deep copy.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			// Clone the base class...
			TS ts = (TS)base.clone();
			// Now clone mutable objects..
			if (_date1 != null)
			{
				ts._date1 = (DateTime)_date1.clone();
			}
			if (_date2 != null)
			{
				ts._date2 = (DateTime)_date2.clone();
			}
			if (_date1_original != null)
			{
				ts._date1_original = (DateTime)_date1_original.clone();
			}
			if (_date2_original != null)
			{
				ts._date2_original = (DateTime)_date2_original.clone();
			}
			if (_id != null)
			{
				ts._id = (TSIdent)_id.clone();
			}
			int size = 0;
			int i = 0;
			if (_comments != null)
			{
				ts._comments = new List<string>(_comments.Count);
				size = _comments.Count;
				for (i = 0; i < size; i++)
				{
					ts._comments.Add(_comments[i]);
				}
			}
			if (_genesis != null)
			{
				ts._genesis = new List<string>(_genesis.Count);
				size = _genesis.Count;
				for (i = 0; i < size; i++)
				{
					ts._genesis.Add(_genesis[i]);
				}
				ts.addToGenesis("Made a copy of TSID=\"" + getIdentifier() + "\" Alias=\"" + getAlias() + "\" (previous history information is for original)");
			}
			if (_data_limits != null)
			{
				ts._data_limits = (TSLimits)_data_limits.clone();
			}
			if (_data_limits_original != null)
			{
				ts._data_limits_original = (TSLimits)_data_limits_original.clone();
			}
			return ts;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Copy the data array from one time series to another.  This method should be defined in a derived class. </summary>
	/// <param name="ts"> The time series to copy the data from. </param>
	/// <param name="start_date"> The time to start copying the data (if null, use the first date from this instance). </param>
	/// <param name="end_date"> The time to end copying the data (if null, use the last date from this instance). </param>
	/// <param name="copy_missing"> If true, copy missing data (including out-of-period missing
	/// data).  If false, ignore missing data. </param>
	/// <exception cref="TSException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void copyData(TS ts, RTi.Util.Time.DateTime start_date, RTi.Util.Time.DateTime end_date, boolean copy_missing) throws TSException
	public virtual void copyData(TS ts, DateTime start_date, DateTime end_date, bool copy_missing)
	{
		string message = "This method needs to be implemented in a derived class (e.g., MonthTS)";
		Message.printWarning(3, "TS.copyData", message);
		throw new TSException(message);
	}

	/// <summary>
	/// Copy the data array from one time series to another.  This method should be defined in a derived class. </summary>
	/// <param name="ts"> The time series to copy the data from. </param>
	/// <param name="copy_missing"> If true, copy missing data (including out-of-period missing
	/// data).  If false, ignore missing data. </param>
	/// <exception cref="TSException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void copyData(TS ts, boolean copy_missing) throws TSException
	public virtual void copyData(TS ts, bool copy_missing)
	{
		string message = "This method needs to be implemented in a derived class (e.g., MonthTS)";
		Message.printWarning(3, "TS.copyData", message);
		throw new TSException(message);
	}

	/// <summary>
	/// Copy one time series header to this one.  This copies everything except data
	/// related to the data space.  Note that the dates are also copied and
	/// allocateDataSpace() should be called if necessary to reset the data space.  The
	/// following data is copied (the associated set method is shown to allow individual
	/// changes to be applied after the copy, if appropriate).  See also the second
	/// table that indicates what is NOT copied.  This method may need to be overloaded
	/// in the future to allow only a partial copy of the header.
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Data Member</b></td>	<td><b>Set Method</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_comments</td>		<td>SetComments()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_flag_length</td>		<td>hasDataFlags()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_interval_base_original</td>	<td>setDataIntervalBaseOriginal()</td>
	/// </tr>
	/// <tr>
	/// <td>_data_interval_mult_original</td>	<td>setDataIntervalMultOriginal()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_interval_mult</td>	<td>setDataIntervalMult()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_units</td>		<td>setDataUnits()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_units_original</td>	<td>setDataUnitsOriginal()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_date1</td>			<td>setDate1()</td>
	/// </tr>
	/// <tr>
	/// <td>_date2</td>			<td>setDate2()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_date1_original</td>	<td>setDate1Original()</td>
	/// </tr>
	/// <tr>
	/// <td>_date2_original</td>	<td>setDate2Original()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_date_type</td>		<td>Not implemented.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_description</td>		<td>SetDescription()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_extended_legend</td>	<td>setExtendedLegend()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_genesis</td>		<td>SetGenesis()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_has_data_flags</td>	<td>hasDataFlags()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_id</td>			<td>setIdentifier()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_input_name</td>		<td>setInputName()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_legend</td>		<td>setLegend()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_missing</td>		<td>setMissing() and setMissingRange()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_sequence_number</td>	<td>setSequenceNumber()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_status</td>		<td>setStatus()</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_version</td>	<td>setVersion()</td>
	/// </tr>
	/// 
	/// </table>
	/// 
	/// The following data are not copied:
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Data Member</b></td>	<td><b>Set Method</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_size</td>		<td>Set in allocateDataSpace().</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_interval_base</td>	<td>Set in the specific constructor.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_data_limits</td>		<td>Associated with data.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>_dirty</td>			<td>Associated with data.</td>
	/// </tr>
	/// 
	/// </table>
	/// </summary>
	public virtual void copyHeader(TS ts)
	{
		setVersion(ts.getVersion());
		setStatus(ts.getStatus());

		setInputName(ts.getInputName());

		// Copy TSIdent...

		try
		{
			setIdentifier(new TSIdent(ts.getIdentifier()));
		}
		catch (Exception)
		{
			// Should not happen.
		}

		// Need to initialize DateTime somehow, but how do you pick defaults?

		// THIS IS DATA RELATED 13 JUL 1998
		//_data_limits = new TSLimits ( ts.getDataLimits() );
		_date1 = new DateTime(ts.getDate1());
		_date2 = new DateTime(ts.getDate2());
		_date1_original = new DateTime(ts.getDate1Original());
		_date2_original = new DateTime(ts.getDate2Original());

		setDataType(ts.getDataType());

		_data_interval_base = ts.getDataIntervalBase();
		_data_interval_mult = ts.getDataIntervalMult();
		_data_interval_base_original = ts.getDataIntervalBaseOriginal();
		_data_interval_mult_original = ts.getDataIntervalMultOriginal();

		setDescription(ts.getDescription());

		_comments = new List<string>(2,1);
		_comments = StringUtil.addListToStringList(_comments, ts.getComments());
		_genesis = new List<string>(2,1);
		_genesis = StringUtil.addListToStringList(_genesis, ts.getGenesis());

		setDataUnits(ts.getDataUnits());
		setDataUnitsOriginal(ts.getDataUnitsOriginal());

		// First set the missing data value...
		setMissing(ts.getMissing());
		// Now set the range itself in case it has been reset...
		setMissingRange(ts.getMissingRange());

		// THIS IS DATA RELATED 13 Jul 1998
		//_dirty = true;// We need to recompute limits when we get the chance

		// Copy legend information...
		_legend = ts.getLegend();
		_extended_legend = ts.getExtendedLegend();

		// Data flags...

		_has_data_flags = ts._has_data_flags;
	}

	/// <summary>
	/// Return a formatted extended legend string without changing saved legend. </summary>
	/// <returns> A formatted extended legend string for the time series but do not
	/// update the time series extended legend data.
	/// The format is the same as for formatLegend(). </returns>
	/// <param name="format"> Format string. </param>
	/// <seealso cref= #formatLegend </seealso>
	public virtual string formatExtendedLegend(string format)
	{
		return formatExtendedLegend(format, false);
	}

	/// <summary>
	/// Return a formatted extended legend string and optionally save the changed legend. </summary>
	/// <returns> A formatted extended legend string for the time series.
	/// The format is the same as for formatLegend(). </returns>
	/// <param name="format"> Format string. </param>
	/// <param name="flag"> If true, the extended legend data member will be updated after formatting. </param>
	/// <seealso cref= #formatLegend </seealso>
	public virtual string formatExtendedLegend(string format, bool flag)
	{ // First format using the normal legend string...
		string extended_legend = formatLegend(format);
		// Now, if desired, save the legend...
		if (flag)
		{
			setExtendedLegend(extended_legend);
		}
		return extended_legend;
	}

	/// <summary>
	/// Format a standard time series header, for use with formatOutput. </summary>
	/// <returns> A list of strings containing the header.  Blank lines at the start
	/// and end of the header are not included. </returns>
	public virtual IList<string> formatHeader()
	{
		IList<string> header = new List<string>(10, 5);

		header.Add("Time series alias       = " + getAlias());
		header.Add("Time series identifier  = " + getIdentifier());
		header.Add("Description             = " + getDescription());
		header.Add("Data source             = " + getIdentifier().getSource());
		header.Add("Data type               = " + getIdentifier().getType());
		header.Add("Data interval           = " + getIdentifier().getInterval());
		header.Add("Data units              = " + _data_units);
		if ((_date1 != null) && (_date2 != null))
		{
			header.Add("Period                  = " + _date1 + " to " + _date2);
			//StringUtil.formatString(_date1.getYear(), "%04d") + "-" +
			//StringUtil.formatString(_date1.getMonth(), "%02d") + " to " +
			//StringUtil.formatString(_date2.getYear(), "%04d") + "-" +
			//StringUtil.formatString(_date2.getMonth(), "%02d") ) );
		}
		else
		{
			header.Add("Period                  = N/A");
		}
		if ((_date1_original != null) && (_date2_original != null))
		{
			header.Add("Orig./Avail. period     = " + _date1_original + " to " + _date2_original);
			//StringUtil.formatString(
			//_date1_original.getYear(), "%04d") + "-" +
			//StringUtil.formatString(
			//_date1_original.getMonth(), "%02d") + " to " +
			//StringUtil.formatString(
			//_date2_original.getYear(), "%04d") + "-" +
			//StringUtil.formatString(
			//_date2_original.getMonth(), "%02d") ) );
		}
		else
		{
			header.Add("Orig./Avail. period     = N/A");
		}
		return header;
	}

	/// <summary>
	/// Return a formatted legend string, without changing the legend in memory. </summary>
	/// <returns> A formatted legend string for the time series but do not update the
	/// time series legend data.  See the version that accepts a flag for a description of format characters. </returns>
	/// <param name="format"> Format string. </param>
	public virtual string formatLegend(string format)
	{
		return formatLegend(format, false);
	}

	// FIXME SAM This code seems redundant with similar code in TSUtil.  Need to refactor and consolidate.
	/// <summary>
	/// Return a formatted legend string, optionally changing the legend in memory. </summary>
	/// <returns> A formatted legend string for the time series but do not update the time series legend data. </returns>
	/// <param name="format"> Format string containing normal characters and formatting strings
	/// which will be replaced with time series header information.  Format specifiers can reference time series properties
	/// using ${ts:PropertyName} notation, or use the % specifiers as follows (grouped in categories):
	/// <para>
	/// 
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Format specifier</b></td>	<td><b>Description</b></td>
	/// </tr
	/// 
	/// <tr>
	/// <td><b>%%</b></td>
	/// <td>Literal percent character.</td>.
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%A</b></td>
	/// <td>Alias (from TSIdent).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%D</b></td>
	/// <td>Description (e.g., "RED RIVER BELOW MY TOWN").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%F</b></td>
	/// <td>Full time series identifier (e.g.,
	/// "12345678_FREE.CRDSS_USGS.QME.24Hour.Trace1")</td>.
	/// </tr>
	/// 
	/// <tr>
	/// </para>
	/// <td><b>%I</b><br><para>
	/// </para>
	/// <b>%b</b><para>
	/// <b>%m</b></td>
	/// </para>
	/// <td>The full data interval part of the identifier (e.g., "24Hour").<para>
	/// </para>
	/// Interval multiplier part of the identifier (e.g., "24").<para>
	/// Base data interval part of the identifier (e.g., "Hour").</td>
	/// </tr>
	/// 
	/// <tr>
	/// </para>
	/// <td><b>%i</b><br><para></td>
	/// <td>The input name part of the identifier (e.g., the file name or database table
	/// from which the time series was read.</td>
	/// </tr>
	/// 
	/// <tr>
	/// </para>
	/// <td><b>%L</b><br><para>
	/// </para>
	/// <b>%l</b><para>
	/// <b>%w</b></td>
	/// </para>
	/// <td>The full location part of the identifier (e.g., "12345678_FREE").<para>
	/// </para>
	/// Main location part of the identifier (e.g., "12345678").<para>
	/// Sub-location part of the identifier (e.g., "FREE").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%p</b></td>
	/// <td>The time series period using date/times corresponding to the precision of
	/// the data (e.g., "2006-10-01 18 Z - 2006-10-03 18 Z").
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// </para>
	/// <td><b>%S</b><br><para>
	/// </para>
	/// <b>%s</b><para>
	/// <b>%x</b></td>
	/// </para>
	/// <td>The full data source part of the identifier (e.g., "CRDSS_USGS").<para>
	/// </para>
	/// Main data source part of the identifier (e.g., "CRDSS").<para>
	/// Sub-source part of the identifier (e.g., "USGS").</td>
	/// </tr>
	/// 
	/// <tr>
	/// </para>
	/// <td><b>%T</b><br><para>
	/// </para>
	/// <b>%t</b><para>
	/// <b>%k</b></td>
	/// </para>
	/// <td>The full data type part of the identifier (e.g., "QME").<para>
	/// Main data source part of the identifier (reserved for future use and currently
	/// </para>
	/// the same as the full data type).<para>
	/// Sub-type part of the identifier.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%U</b></td>
	/// <td>Data units (e.g., "CFS"; see DataUnits).</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%Z</b></td>
	/// <td>Scenario part of the identifier (e.g., "Trace1").</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>%z</b></td>
	/// <td>Time series sequence ID (e.g., year for traces).</td>
	/// </tr>
	/// </table>
	/// </para>
	/// <para>
	/// </para>
	/// </param>
	/// <param name="update_ts"> If true, the legend data member in the time series will be
	/// updated.  Otherwise a formatted string is returned but the internal data member is left unchanged. </param>
	/// <seealso cref= TSIdent </seealso>
	/// <seealso cref= RTi.Util.IO.DataUnits </seealso>
	public virtual string formatLegend(string format, bool update_ts)
	{
		StringBuilder buffer = new StringBuilder();

		//Message.printStatus ( 2, "", "Legend format is " + format );
		// Loop through the format string and if format strings are found,
		// append to the buffer.  Otherwise, transfer all characters as given.
		if (string.ReferenceEquals(format, null))
		{
			return "";
		}
		int len = format.Length;
		char c;
		for (int i = 0; i < len; i++)
		{
			c = format[i];
			if (c == '%')
			{
				// Format modifier.  Get the next character...
				++i;
				if (i >= len)
				{
					break;
				}
				c = format[i];
				if (c == '%')
				{
					// Literal %...
					buffer.Append(c);
				}
				else if (c == 'A')
				{
					// Alias from TSIdent...
					buffer.Append(_id.getAlias());
				}
				else if (c == 'b')
				{
					// Data interval base...
					buffer.Append(TimeInterval.getName(_id.getIntervalBase(),1));
				}
				else if (c == 'D')
				{
					// Description...
					buffer.Append(_description);
				}
				else if (c == 'F')
				{
					// Full identifier...
					//buffer.append ( _id.getIdentifier() );
					buffer.Append(_id.ToString());
				}
				else if (c == 'I')
				{
					// Full interval...
					buffer.Append(_id.getInterval());
				}
				else if (c == 'i')
				{
					// Input name...
					buffer.Append(_id.getInputName());
				}
				else if (c == 'k')
				{
					// Sub source...
					buffer.Append(_id.getSubSource());
				}
				else if (c == 'L')
				{
					// Full location...
					buffer.Append(_id.getLocation());
				}
				else if (c == 'l')
				{
					// Main location...
					buffer.Append(_id.getMainLocation());
				}
				else if (c == 'm')
				{
					// Data interval multiplier...
					buffer.Append(_id.getIntervalMult());
				}
				else if (c == 'p')
				{
					// Period...
					buffer.Append("" + _date1 + " - " + _date2);
				}
				else if (c == 'S')
				{
					// Full source...
					buffer.Append(_id.getSource());
				}
				else if (c == 's')
				{
					// Main source...
					buffer.Append(_id.getMainSource());
				}
				else if (c == 'U')
				{
					// Units...
					buffer.Append(_data_units);
				}
				else if (c == 'T')
				{
					// Data type...
					buffer.Append(_id.getType());
				}
				else if (c == 't')
				{
					// Data main type (reserved for future use - for
					// now return the total)...
					buffer.Append(_id.getType());
				}
				else if (c == 'y')
				{
					// Data sub type (reserved for future use)...
				}
				else if (c == 'w')
				{
					// Sub-location...
					buffer.Append(_id.getSubLocation());
				}
				else if (c == 'x')
				{
					// Sub source...
					buffer.Append(_id.getSubSource());
				}
				else if (c == 'Z')
				{
					// Scenario...
					buffer.Append(_id.getScenario());
				}
				else if (c == 'z')
				{
					// Sequence ID (old sequence number)...
					buffer.Append(_id.getSequenceID());
				}
				else
				{
					// No match.  Add the % and the character...
					buffer.Append("%");
					buffer.Append(c);
				}
			}
			else
			{
				// Just add the character...
				buffer.Append(c);
			}
		}
		//Message.printStatus(2, routine, "After formatLegend(), string is \"" + s2 + "\"" );
		// Now replace ${ts:Property} strings with properties from the time series
		// Put the most specific first so it is matched first
		string startString = "${ts:";
		int startStringLength = 5;
		string endString = "}";
		object propO;
		int start = 0; // Start at the beginning of the string
		int pos2 = 0;
		string s2 = buffer.ToString();
		while (pos2 < s2.Length)
		{
			int pos1 = StringUtil.indexOfIgnoreCase(s2, startString, start);
			if (pos1 >= 0)
			{
				// Find the end of the property
				pos2 = s2.IndexOf(endString, pos1, StringComparison.Ordinal);
				if (pos2 > 0)
				{
					// Get the property...
					string propname = s2.Substring(pos1 + startStringLength, pos2 - (pos1 + startStringLength));
					//Message.printStatus(2, routine, "Property=\"" + propname + "\" isTSProp=" + isTsProp + " pos1=" + pos1 + " pos2=" + pos2 );
					// By convention if the property is not found, keep the original string so can troubleshoot property issues
					string propvalString = s2.Substring(pos1, (pos2 + 1) - pos1);
					// Get the property out of the time series
					propO = getProperty(propname);
					if (propO != null)
					{
						// This handles conversion of integers to strings
						propvalString = "" + propO;
					}
					// Replace the string and continue to evaluate s2
					s2 = s2.Substring(0, pos1) + propvalString + s2.Substring(pos2 + 1);
					// Next search will be at the end of the expanded string (end delimiter will be skipped in any case)
					start = pos1 + propvalString.Length;
				}
				else
				{
					// No closing character so leave the property string as is and march on...
					start = pos1 + startStringLength;
					if (start > s2.Length)
					{
						break;
					}
				}
			}
			else
			{
				// No more ${} property strings so done processing properties.
				// If checking time series properties will then check global properties in next loop
				break;
			}
		}
		if (update_ts)
		{
			setLegend(buffer.ToString());
		}
		return s2;
	}

	/// <summary>
	/// Format the time series into a general output format.  This method should be
	/// overridden in the derived class.  For example, all MonthTS should be have a
	/// general text report format.  The properties can be used to customize the
	/// output.  This method is meant as a general output format.  Specific formats
	/// should be implemented as classes with static readTimeSeries()/writeTimeSeries() methods. </summary>
	/// <returns> list of strings containing formatted output. </returns>
	/// <param name="props"> Modifiers for output. </param>
	/// <exception cref="RTi.TS.TSException"> If low-level formatting code throws an exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(RTi.Util.IO.PropList props) throws TSException
	public virtual IList<string> formatOutput(PropList props)
	{
		Message.printWarning(3, "TS.formatOutput", "TS.formatOutput(PropList) is virtual, redefine in derived classes.");
		return null;
	}

	/// <summary>
	/// Format the time series into a general output format and write to an output file.
	/// This method should be overridden in the derived class. </summary>
	/// <returns> Formatted output in a Vector, or null. </returns>
	/// <param name="out"> PrintWriter to receive output. </param>
	/// <param name="props"> Modifiers for output. </param>
	/// <exception cref="RTi.TS.TSException"> Thrown if low-level formatting code throws an exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(java.io.PrintWriter out, RTi.Util.IO.PropList props) throws TSException
	public virtual IList<string> formatOutput(PrintWriter @out, PropList props)
	{
		Message.printWarning(3, "TS.formatOutput", "TS.formatOutput(" + "PrintWriter,PropList) is virtual, redefine in derived classes");
		return null;
	}

	/// <summary>
	/// Format the time series into a general output format.  This method should be
	/// overridden in the derived class. </summary>
	/// <returns> List containing formatted output, or null. </returns>
	/// <param name="out"> Name of the output file. </param>
	/// <param name="props"> Modifiers for output. </param>
	/// <exception cref="RTi.TS.TSException"> Thrown if low-level formatting code throws an exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> formatOutput(String out, RTi.Util.IO.PropList props) throws TSException
	public virtual IList<string> formatOutput(string @out, PropList props)
	{
		string routine = "TS.formatOutput(string,int,long)";
		Message.printWarning(1, routine, "TS.formatOutput(String,PropList)" + " function is virtual, redefine in derived classes");
		return null;
	}

	/// <summary>
	/// Return the time series alias from the TSIdent. </summary>
	/// <returns> The alias part of the time series identifier. </returns>
	public virtual string getAlias()
	{
		return _id.getAlias();
	}

	/// <summary>
	/// Return the time series comments. </summary>
	/// <returns> The comments list. </returns>
	public virtual IList<string> getComments()
	{
		return _comments;
	}

	/// <summary>
	/// Return the time series data flag meta-data list. </summary>
	/// <returns> The data flag meta-data list. </returns>
	public virtual IList<TSDataFlagMetadata> getDataFlagMetadataList()
	{
		return __dataFlagMetadataList;
	}

	/// <summary>
	/// Return the data interval base. </summary>
	/// <returns> The data interval base (see TimeInterval.*). </returns>
	public virtual int getDataIntervalBase()
	{
		return _data_interval_base;
	}

	/// <summary>
	/// Return the original data interval base. </summary>
	/// <returns> The data interval base of the original data. </returns>
	public virtual int getDataIntervalBaseOriginal()
	{
		return _data_interval_base_original;
	}

	/// <summary>
	/// Return the data interval multiplier. </summary>
	/// <returns> The data interval multiplier. </returns>
	public virtual int getDataIntervalMult()
	{
		return _data_interval_mult;
	}

	/// <summary>
	/// Return the original data interval multiplier. </summary>
	/// <returns> The data interval multiplier of the original data. </returns>
	public virtual int getDataIntervalMultOriginal()
	{
		return _data_interval_mult_original;
	}

	/// <summary>
	/// Return the time series data limits (a new copy is returned).  If necessary, the
	/// limits are refreshed.  The refresh() method should be defined in the derived class. </summary>
	/// <returns> The time series data limits. </returns>
	/// <seealso cref= TSLimits </seealso>
	public virtual TSLimits getDataLimits()
	{ // Make sure that the limits have been set...
		refresh();
		if (_data_limits == null)
		{
			return null;
		}
		else
		{
			// Create a new copy to protect.
			return (new TSLimits(_data_limits));
		}
	}

	/// <summary>
	/// Return the original time series data limits.  The reference to the original
	/// data is returned so that MonthTSLimits or others can be returned. </summary>
	/// <returns> The original time series data limits. </returns>
	/// <seealso cref= TSLimits </seealso>
	public virtual TSLimits getDataLimitsOriginal()
	{
		if (_data_limits_original == null)
		{
			return null;
		}
		else
		{
			return (_data_limits_original);
		}
	}

	/// <summary>
	/// Return a TSData for a date.  This method should be defined in derived classes,
	/// especially if data flags are being used. </summary>
	/// <param name="date"> date/time to get data. </param>
	/// <param name="tsdata"> if null, a new instance of TSData will be returned.  If non-null, the provided
	/// instance will be used (this is often desirable during iteration to decrease memory use and
	/// increase performance). </param>
	/// <returns> a TSData for the specified date/time. </returns>
	public virtual TSData getDataPoint(DateTime date, TSData tsdata)
	{
		Message.printWarning(3, "TS.getDataPoint", "This is a virtual function, redefine in child classes");
		// Empty point...
		TSData data = new TSData();
		return data;
	}

	/// <summary>
	/// Return the number of data points that are allocated in memory.
	/// Zero will be returned if allocateDataSpace() has not been called. </summary>
	/// <returns> The number of data points included in the period. </returns>
	public virtual int getDataSize()
	{
		return _data_size;
	}

	/// <summary>
	/// Return the data type from the TSIdent or an empty string if no TSIdent has been set. </summary>
	/// <returns> The data type abbreviation. </returns>
	public virtual string getDataType()
	{
		if (_id == null)
		{
			return "";
		}
		else
		{
			return _id.getType();
		}
	}

	/// <summary>
	/// Return the data units. </summary>
	/// <returns> The data units. </returns>
	/// <seealso cref= RTi.Util.IO.DataUnits </seealso>
	public virtual string getDataUnits()
	{
		return _data_units;
	}

	/// <summary>
	/// Return the original data units. </summary>
	/// <returns> The data units in the original data. </returns>
	/// <seealso cref= RTi.Util.IO.DataUnits </seealso>
	public virtual string getDataUnitsOriginal()
	{
		return _data_units_original;
	}

	/// <summary>
	/// Return the data value for a date. </summary>
	/// <returns> The data value associated with a date.  This should be
	/// overridden in derived classes (always returns the missing data value here). </returns>
	/// <param name="date"> Date corresponding to the data value. </param>
	public virtual double getDataValue(DateTime date)
	{
		Message.printWarning(3, "TS.getDataValue", "TS.getDataValue is a virtual function, redefine in derived classes");
		return _missing;
	}

	/// <summary>
	/// Return the first date in the period of record (returns a copy). </summary>
	/// <returns> The first date in the period of record, or null if the date is null. </returns>
	public virtual DateTime getDate1()
	{
		if (_date1 == null)
		{
			return null;
		}
		return new DateTime(_date1);
	}

	/// <summary>
	/// Return the first date in the original period of record (returns a copy). </summary>
	/// <returns> The first date of the original data source (generally equal to or
	/// earlier than the time series that is actually read), or null if the date is null. </returns>
	public virtual DateTime getDate1Original()
	{
		if (_date1_original == null)
		{
			return null;
		}
		return new DateTime(_date1_original);
	}

	/// <summary>
	/// Return the last date in the period of record (returns a copy). </summary>
	/// <returns> The last date in the period of record, or null if the date is null. </returns>
	public virtual DateTime getDate2()
	{
		if (_date2 == null)
		{
			return null;
		}
		return new DateTime(_date2);
	}

	/// <summary>
	/// Return the last date in the original period of record (returns a copy). </summary>
	/// <returns> The last date of the original data source (generally equal to or
	/// later than the time series that is actually read), or null if the date is null. </returns>
	public virtual DateTime getDate2Original()
	{
		if (_date2_original == null)
		{
			return null;
		}
		return new DateTime(_date2_original);
	}

	/// <summary>
	/// Return the time series description. </summary>
	/// <returns> The time series description. </returns>
	public virtual string getDescription()
	{
		return _description;
	}

	/// <summary>
	/// Return value of flag indicating whether time series is enabled. </summary>
	/// <returns> true if the time series is enabled, false if disabled. </returns>
	public virtual bool getEnabled()
	{
		return _enabled;
	}

	/// <summary>
	/// Return the extended time series legend. </summary>
	/// <returns> Time series extended legend. </returns>
	public virtual string getExtendedLegend()
	{
		return _extended_legend;
	}

	/// <summary>
	/// Return the genesis information. </summary>
	/// <returns> The genesis comments. </returns>
	public virtual IList<string> getGenesis()
	{
		return _genesis;
	}

	/// <summary>
	/// Return the time series identifier as a TSIdent. </summary>
	/// <returns> the time series identifier as a TSIdent. </returns>
	/// <seealso cref= TSIdent </seealso>
	public virtual TSIdent getIdentifier()
	{
		return _id;
	}

	/// <summary>
	/// Return the time series identifier as a String.  This returns TSIdent.getIdentifier(). </summary>
	/// <returns> The time series identifier as a string. </returns>
	/// <seealso cref= TSIdent </seealso>
	public virtual string getIdentifierString()
	{
		return _id.getIdentifier();
	}

	/// <summary>
	/// Return the input name (file or database table) for the time series. </summary>
	/// <returns> the input name. </returns>
	public virtual string getInputName()
	{
		return _input_name;
	}

	/// <summary>
	/// Return whether data flag strings use String.intern(). </summary>
	/// <returns> True if data flag strings use String.intern(), false otherwise. </returns>
	public virtual bool getInternDataFlagStrings()
	{
		return _internDataFlagStrings;
	}

	/// <summary>
	/// Return the time series legend. </summary>
	/// <returns> Time series legend. </returns>
	public virtual string getLegend()
	{
		return _legend;
	}

	/// <summary>
	/// Return the location part of the time series identifier.  Does not include location type. </summary>
	/// <returns> The location part of the time series identifier (from TSIdent). </returns>
	public virtual string getLocation()
	{
		return _id.getLocation();
	}

	/// <summary>
	/// Return the missing data value used for the time series (single value). </summary>
	/// <returns> The value used for missing data. </returns>
	public virtual double getMissing()
	{
		return _missing;
	}

	/// <summary>
	/// Return the missing data range (2 values). </summary>
	/// <returns> The range of values for missing data.  The first value is the lowest
	/// value, the second the highest.  A new array instance is returned. </returns>
	public virtual double[] getMissingRange()
	{
		double[] missing_range = new double[2];
		missing_range[0] = _missingl;
		missing_range[1] = _missingu;
		return missing_range;
	}

	/// <summary>
	/// Return the date for the first non-missing data value. </summary>
	/// <returns> The first date where non-missing data occurs (a copy is returned). </returns>
	public virtual DateTime getNonMissingDataDate1()
	{
		return new DateTime(_data_limits.getNonMissingDataDate1());
	}

	/// <summary>
	/// Return the date for the last non-missing data value. </summary>
	/// <returns> The last date where non-missing data occurs (a copy is returned). </returns>
	public virtual DateTime getNonMissingDataDate2()
	{
		return new DateTime(_data_limits.getNonMissingDataDate2());
	}

	/// <summary>
	/// Get the hashtable of properties, for example to allow display. </summary>
	/// <returns> the hashtable of properties, for example to allow display, may be null. </returns>
	public virtual Dictionary<string, object> getProperties()
	{
		if (__property_HashMap == null)
		{
			__property_HashMap = new LinkedHashMap<string, object>(); // Initialize to non-null for further use
		}
		return __property_HashMap;
	}

	/// <summary>
	/// Get a time series property's contents (case-specific). </summary>
	/// <param name="propertyName"> name of property being retrieved. </param>
	/// <returns> property object corresponding to the property name. </returns>
	public virtual object getProperty(string propertyName)
	{
		if (__property_HashMap == null)
		{
			return null;
		}
		return __property_HashMap.get(propertyName);
	}

	/// <summary>
	/// Return the sequence identifier (old sequence number) for the time series. </summary>
	/// <returns> The sequence identifier for the time series.  This is meant to be used
	/// when an array of time series traces is maintained. </returns>
	/// <returns> time series trace ID. </returns>
	public virtual string getSequenceID()
	{
		return _id.getSequenceID();
	}

	/// <summary>
	/// Return the time series status. </summary>
	/// <returns> The status flag for the time series.  This is a general purpose flag. </returns>
	/// <seealso cref= #setStatus </seealso>
	public virtual string getStatus()
	{
		return _status;
	}

	// TODO SAM 2013-09-21 Need to move this out of this class to separate concerns
	/// <summary>
	/// Returns the data in the specified DataFlavor, or null if no matching flavor
	/// exists.  From the Transferable interface.
	/// Supported dataflavors are:<br>
	/// </ul>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor in which to return the data. </param>
	/// <returns> the data in the specified DataFlavor, or null if no matching flavor exists. </returns>
	public virtual object getTransferData(DataFlavor flavor)
	{
		if (flavor.Equals(tsFlavor))
		{
			return this;
		}
		else if (flavor.Equals(TSIdent.tsIdentFlavor))
		{
			return _id;
		}
		else
		{
			return null;
		}
	}

	//TODO SAM 2013-09-21 Need to move this out of this class to separate concerns
	/// <summary>
	/// Returns the flavors in which data can be transferred.  From the Transferable
	/// interface.  The order of the dataflavors that are returned are:<br>
	/// <ul>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <returns> the flavors in which data can be transferred. </returns>
	public virtual DataFlavor[] getTransferDataFlavors()
	{
		DataFlavor[] flavors = new DataFlavor[2];
		flavors[0] = tsFlavor;
		flavors[1] = TSIdent.tsIdentFlavor;
		return flavors;
	}

	/// <summary>
	/// Return the time series input format version. </summary>
	/// <returns> The time series version, to be used to indicate input file formats. </returns>
	public virtual string getVersion()
	{
		return _version;
	}

	/// <summary>
	/// Indicate whether the time series has data, which is determined by checking to
	/// see whether memory has been allocated for the data space (which implies that
	/// the dates have been set).  This method can be checked after data are read rather
	/// than checking the dates.  This method should be defined in derived classes with specific data storage schemes. </summary>
	/// <returns> true if the time series has data, false if not. </returns>
	public virtual bool hasData()
	{
		Message.printWarning(1, "TS.getDataValue", "TS.hasData() is a virtual function, redefine in derived classes");
		return false;
	}

	/// <summary>
	/// Indicate whether the time series has data flags. </summary>
	/// <returns> true if data flags are enabled for the time series. </returns>
	public virtual bool hasDataFlags()
	{
		return _has_data_flags;
	}

	// FIXME SAM 2010-08-20 Evaluate phasing this out.  setDataValue() now automatically turns on
	// data flags when a flag is passed in.  Also, using intern strings improves memory management so
	// allocating memory is not such a big deal as it was in the past.
	/// <summary>
	/// Set whether the time series has data flags.  This method should be called before
	/// allocateDataSpace() is called.  If data flags are enabled, allocateDataSpace()
	/// will allocate memory for the data and data flags. </summary>
	/// <param name="hasDataFlags"> Indicates whether data flags will be associated with each data value.  The data flag is a String. </param>
	/// <param name="internDataFlagStrings"> if true, then String.intern() will be used to manage the data flags.  This generally
	/// is advised because flags often consist of the same strings used repeatedly.  However, if unique
	/// string values are used, then false can be specified so as to not bloat the global String table. </param>
	/// <returns> true if data flags are enabled for the time series, after the set. </returns>
	public virtual bool hasDataFlags(bool hasDataFlags, bool internDataFlagStrings)
	{
		_has_data_flags = hasDataFlags;
		_internDataFlagStrings = internDataFlagStrings;
		return _has_data_flags;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void init()
	{
		_version = "";

		_input_name = "";

		// Need to initialize an empty TSIdent...

		_id = new TSIdent();
		_legend = "";
		_extended_legend = "";
		_data_size = 0;
		// DateTime need to be initialized somehow...
		setDataType("");
		_data_interval_base = 0;
		_data_interval_mult = 1;
		_data_interval_base_original = 1;
		_data_interval_mult_original = 0;
		setDescription("");
		_comments = new List<string>(2,2);
		_genesis = new List<string>(2,2);
		setDataUnits("");
		setDataUnitsOriginal("");
		setMissing(-999.0);
		_data_limits = new TSLimits();
		_dirty = true; // We need to recompute limits when we get the chance
		_enabled = true;
		_selected = false; // Let other code select, e.g., as query result
		_editable = false;
	}

	/// <summary>
	/// Determines whether the specified flavor is supported as a transfer flavor.
	/// From the Transferable interface.
	/// Supported dataflavors are:<br>
	/// <ul>
	/// <li>TS - TS.class / RTi.TS.TS</li>
	/// <li>TSIdent - TSIdent.class / RTi.TS.TSIdent</li></ul> </summary>
	/// <param name="flavor"> the flavor to check. </param>
	/// <returns> true if data can be transferred in the specified flavor, false if not. </returns>
	public virtual bool isDataFlavorSupported(DataFlavor flavor)
	{
		if (flavor.Equals(tsFlavor))
		{
			return true;
		}
		else if (flavor.Equals(TSIdent.tsIdentFlavor))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determine if a data value for the time series is missing.  The missing value can
	/// be set to a range of values or a single value, using setMissing().
	/// There is no straightforward way to check to see if a value is equal to NaN
	/// (the code: if ( value == Double.NaN ) will always return false if one or both
	/// values are NaN).  Consequently there is no way to see know if only one or both
	/// values is NaN, using the standard operators.  Instead, we assume that NaN
	/// should be interpreted as missing and do the check if ( value != value ), which
	/// will return true if the value is NaN.  Consequently, code that uses time series
	/// data should not check for missing and treat NaN differently because the TS class treats NaN as missing. </summary>
	/// <returns> true if the data value is missing, false if not. </returns>
	/// <param name="value"> Value to check. </param>
	public virtual bool isDataMissing(double value)
	{
		if (double.IsNaN(value))
		{
			return true;
		}
		if ((value >= _missingl) && (value <= _missingu))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Indicate whether the time series is dirty (data have been modified). </summary>
	/// <returns> true if the time series is dirty, false if not. </returns>
	public virtual bool isDirty()
	{
		return _dirty;
	}

	/// <summary>
	/// Indicate whether the time series is editable. </summary>
	/// <returns> true if the time series is editable, false if not. </returns>
	public virtual bool isEditable()
	{
		return _editable;
	}

	/// <summary>
	/// Indicate whether the time series is selected. </summary>
	/// <returns> true if the time series is selected, false if not. </returns>
	public virtual bool isSelected()
	{
		return _selected;
	}

	/// <summary>
	/// Create and return an iterator for the time series using the full period for the
	/// time series.  For regular interval time series,
	/// the iterator is TSIterator.  IrregularTS use the IrregularTSIterator. </summary>
	/// <returns> an iterator for the time series. </returns>
	/// <exception cref="if"> the dates for the iterator are not set (they are not set in the time series). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSIterator iterator() throws Exception
	public virtual TSIterator iterator()
	{
		return new TSIterator(this);
	}

	/// <summary>
	/// Return an iterator for the time series using the specified period.
	/// For regular interval time series, the iterator is that same. IrregularTS use the IrregularTSIterator. </summary>
	/// <param name="date1"> Start of data iteration.  If null, the first date/time will be used. </param>
	/// <param name="date2"> End of data iteration.  If null, the last date/time will be used. </param>
	/// <returns> an iterator for the time series. </returns>
	/// <exception cref="if"> the dates for the iterator are not set (they are not set in the time series). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSIterator iterator(RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2) throws Exception
	public virtual TSIterator iterator(DateTime date1, DateTime date2)
	{
		return new TSIterator(this, date1, date2);
	}

	/// <summary>
	/// Refresh the secondary data (e.g., data limits).  This should be overruled in the derived class.
	/// </summary>
	public virtual void refresh()
	{
		Message.printWarning(3, "TS.refresh", "TS.refresh is virtual.  Define in the derived class!");
	}

	/// <summary>
	/// Set the time series identifier alias. </summary>
	/// <param name="alias"> Alias of time series. </param>
	public virtual void setAlias(string alias)
	{
		if (!string.ReferenceEquals(alias, null))
		{
			_id.setAlias(alias);
		}
	}

	/// <summary>
	/// Set the comments string list. </summary>
	/// <param name="comments"> Comments to set. </param>
	public virtual void setComments(IList<string> comments)
	{
		if (comments != null)
		{
			_comments = comments;
		}
	}

	/// <summary>
	/// Set the data interval. </summary>
	/// <param name="base"> Base interval (see TimeInterval.*). </param>
	/// <param name="mult"> Base interval multiplier. </param>
	public virtual void setDataInterval(int @base, int mult)
	{
		_data_interval_base = @base;
		_data_interval_mult = mult;
	}

	/// <summary>
	/// Set the data interval for the original data. </summary>
	/// <param name="base"> Base interval (see TimeInterval.*). </param>
	/// <param name="mult"> Base interval multiplier. </param>
	public virtual void setDataIntervalOriginal(int @base, int mult)
	{
		_data_interval_base_original = @base;
		_data_interval_mult_original = mult;
	}

	/// <summary>
	/// Set the data limits for the time series.  This is generally only called by
	/// internal routines.  A copy is saved. </summary>
	/// <param name="limits"> Time series limits. </param>
	/// <seealso cref= TSLimits </seealso>
	public virtual void setDataLimits(TSLimits limits)
	{
		_data_limits = new TSLimits(limits);
	}

	/// <summary>
	/// Set the original data limits for the time series.  This is currently typically
	/// only called by application code (like TSTool).  The reference to the limits are
	/// saved so that derived class data can be stored and returned with
	/// getDataLimitsOriginal().  Make a copy of the data if it needs to be protected. </summary>
	/// <param name="limits"> Time series data limits. </param>
	/// <seealso cref= TSLimits </seealso>
	public virtual void setDataLimitsOriginal(TSLimits limits)
	{
		if (limits == null)
		{
			_data_limits_original = null;
		}
		_data_limits_original = limits;
	}

	/// <summary>
	/// Set the number of data points including the full period.  This should be called by refresh(). </summary>
	/// <param name="data_size"> Number of data points in the time series. </param>
	protected internal virtual void setDataSize(int data_size)
	{
		_data_size = data_size;
	}

	/// <summary>
	/// Set the data type. </summary>
	/// <param name="data_type"> Data type abbreviation. </param>
	public virtual void setDataType(string data_type)
	{
		if ((!string.ReferenceEquals(data_type, null)) && (_id != null))
		{
			_id.setType(data_type);
		}
	}

	/// <summary>
	/// Set the data units. </summary>
	/// <param name="data_units"> Data units abbreviation. </param>
	/// <seealso cref= RTi.Util.IO.DataUnits </seealso>
	public virtual void setDataUnits(string data_units)
	{
		if (!string.ReferenceEquals(data_units, null))
		{
			_data_units = data_units;
		}
	}

	/// <summary>
	/// Set the data units for the original data. </summary>
	/// <param name="units"> Data units abbreviation. </param>
	/// <seealso cref= RTi.Util.IO.DataUnits </seealso>
	public virtual void setDataUnitsOriginal(string units)
	{
		if (!string.ReferenceEquals(units, null))
		{
			_data_units_original = units;
		}
	}

	/// <summary>
	/// Set a data value for the specified date. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <param name="val"> Data value for date. </param>
	/// <seealso cref= RTi.Util.Time.DateTime </seealso>
	public virtual void setDataValue(DateTime date, double val)
	{
		Message.printWarning(1, "TS.setDataValue", "TS.setDataValue is " + "virtual and should be redefined in derived classes");
	}

	// TODO SAM 2010-08-03 if flag is null, should it be treated as empty string?  What about append?
	/// <summary>
	/// Set a data value and associated information for the specified date.  This method
	/// should be defined in derived classes. </summary>
	/// <param name="date"> Date of interest. </param>
	/// <param name="val"> Data value for date. </param>
	/// <param name="data_flag"> Data flag associated with the data value. </param>
	/// <param name="duration"> Duration (seconds) for the data value (specify as 0 if not relevant). </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setDataValue(DateTime date, double val, string data_flag, int duration)
	{
		Message.printWarning(3, "TS.setDataValue", "TS.setDataValue is " + "virtual and should be implemented in derived classes");
	}

	/// <summary>
	/// Set the first date in the period.  A copy is made.
	/// The date precision is set to the precision appropriate for the time series. </summary>
	/// <param name="t"> First date in period. </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setDate1(DateTime t)
	{
		if (t != null)
		{
			_date1 = new DateTime(t);
			if (_data_interval_base != TimeInterval.IRREGULAR)
			{
				// For irregular, rely on the DateTime precision
				_date1.setPrecision(_data_interval_base);
			}
		}
	}

	/// <summary>
	/// Set the first date in the period in the original data.  A copy is made.
	/// The date precision is set to the precision appropriate for the time series. </summary>
	/// <param name="t"> First date in period in the original data. </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setDate1Original(DateTime t)
	{
		if (t != null)
		{
			_date1_original = new DateTime(t);
			if (_data_interval_base != TimeInterval.IRREGULAR)
			{
				// For irregular, rely on the DateTime precision
				_date1_original.setPrecision(_data_interval_base);
			}
		}
	}

	/// <summary>
	/// Set the last date in the period.  A copy is made.
	/// The date precision is set to the precision appropriate for the time series. </summary>
	/// <param name="t"> Last date in period. </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setDate2(DateTime t)
	{
		if (t != null)
		{
			_date2 = new DateTime(t);
			if (_data_interval_base != TimeInterval.IRREGULAR)
			{
				// For irregular, rely on the DateTime precision
				_date2.setPrecision(_data_interval_base);
			}
		}
	}

	/// <summary>
	/// Set the last date in the period in the original data. A copy is made.
	/// The date precision is set to the precision appropriate for the time series. </summary>
	/// <param name="t"> Last date in period in the original data. </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setDate2Original(DateTime t)
	{
		if (t != null)
		{
			_date2_original = new DateTime(t);
			if (_data_interval_base != TimeInterval.IRREGULAR)
			{
				// For irregular, rely on the DateTime precision
				_date2_original.setPrecision(_data_interval_base);
			}
		}
	}

	/// <summary>
	/// Set the description. </summary>
	/// <param name="description"> Time series description (this is not the comments). </param>
	public virtual void setDescription(string description)
	{
		if (!string.ReferenceEquals(description, null))
		{
			_description = description;
		}
	}

	/// <summary>
	/// Set whether the time series is dirty (data have been modified). </summary>
	/// <param name="dirty"> true if the time series is dirty/edited, false if not. </param>
	public virtual void setDirty(bool dirty)
	{
		_dirty = dirty;
	}

	/// <summary>
	/// Set whether the time series is editable. </summary>
	/// <param name="editable"> true if the time series is editable. </param>
	public virtual void setEditable(bool editable)
	{
		_editable = editable;
	}

	/// <summary>
	/// Indicate whether the time series is enabled.  This is being phased in and is
	/// currently only used by graphics code. </summary>
	/// <param name="enabled"> true if the time series is enabled or false if disabled. </param>
	public virtual void setEnabled(bool enabled)
	{
		_enabled = enabled;
	}

	/// <summary>
	/// Set the time series extended legend.  This was used at one point to set a
	/// secondary legend because the Java plotting package did not allow a long legend
	/// in the graph.  Currently, this data item is being used to set the legend for
	/// the table time series view. </summary>
	/// <param name="extended_legend"> Time series extended legend (can be used for labels on 
	/// graphs, etc.). </param>
	/// <seealso cref= #formatExtendedLegend </seealso>
	public virtual void setExtendedLegend(string extended_legend)
	{
		if (!string.ReferenceEquals(extended_legend, null))
		{
			_extended_legend = extended_legend.Trim();
		}
	}

	/// <summary>
	/// Set the genesis information.  The original is lost. </summary>
	/// <param name="genesis"> Genesis comments. </param>
	public virtual void setGenesis(IList<string> genesis)
	{
		setGenesis(genesis, false);
	}

	/// <summary>
	/// Set the genesis information. </summary>
	/// <param name="genesis"> Genesis comments. </param>
	/// <param name="append"> Indicates whether genesis information should be appended. </param>
	public virtual void setGenesis(IList<string> genesis, bool append)
	{
		if (!append)
		{
			// Don't call removeAllElements() because the genesis may have
			// been retrieved and then reset using the same Vector!
			_genesis = new List<string>();
		}
		_genesis = StringUtil.addListToStringList(_genesis, genesis);
	}

	/// <summary>
	/// Set the time series identifier using a TSIdent.
	/// Note that this only sets the identifier but
	/// does not set the separate data fields (like data type). </summary>
	/// <param name="id"> Time series identifier. </param>
	/// <seealso cref= TSIdent </seealso>
	/// <exception cref="Exception"> If there is an error setting the identifier. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setIdentifier(TSIdent id) throws Exception
	public virtual void setIdentifier(TSIdent id)
	{
		if (id != null)
		{
			_id = new TSIdent(id);
		}
	}

	/// <summary>
	/// Set the time series identifier using a string.
	/// Note that this only sets the identifier but
	/// does not set the separate data fields (like data type). </summary>
	/// <param name="identifier"> Time series identifier. </param>
	/// <exception cref="Exception"> If there is an error setting the identifier. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setIdentifier(String identifier) throws Exception
	public virtual void setIdentifier(string identifier)
	{
		if (!string.ReferenceEquals(identifier, null))
		{
			_id.setIdentifier(identifier);
		}
	}

	/// <summary>
	/// Set the time series identifier using a individual string parts. </summary>
	/// <param name="location"> Location of the time series. </param>
	/// <param name="source"> Source of the data. </param>
	/// <param name="type"> Data type. </param>
	/// <param name="interval"> Data interval, including base and possibly multiplier. </param>
	/// <param name="scenario"> Scenario for data. </param>
	/// <exception cref="Exception"> If there is an error setting the identifier (e.g., interval
	/// is not recognized). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setIdentifier(String location, String source, String type, String interval, String scenario) throws Exception
	public virtual void setIdentifier(string location, string source, string type, string interval, string scenario)
	{
		_id.setLocation(location);
		_id.setSource(source);
		_id.setType(type);
		_id.setInterval(interval);
		_id.setScenario(scenario);
	}

	/// <summary>
	/// Set the input name (file or database table).
	/// </summary>
	public virtual void setInputName(string input_name)
	{
		if (!string.ReferenceEquals(input_name, null))
		{
			_input_name = input_name;
		}
	}

	/// <summary>
	/// Set the time series legend. </summary>
	/// <param name="legend"> Time series legend (can be used for labels on graphs, etc.). </param>
	/// <seealso cref= #formatLegend </seealso>
	public virtual void setLegend(string legend)
	{
		if (!string.ReferenceEquals(legend, null))
		{
			_legend = legend.Trim();
		}
	}

	/// <summary>
	/// Set the time series identifier location. </summary>
	/// <param name="location"> Location of time series. </param>
	public virtual void setLocation(string location)
	{
		if (!string.ReferenceEquals(location, null))
		{
			_id.setLocation(location);
		}
	}

	/// <summary>
	/// Set the missing data value for the time series.  The upper and lower bounds
	/// of missing data are set to this value +.001 and -.001, to allow for precision truncation.
	/// The value is constrained to Double.MAX and Double.Min. </summary>
	/// <param name="missing"> Missing data value for time series. </param>
	public virtual void setMissing(double missing)
	{
		_missing = missing;
		if (double.IsNaN(missing))
		{
			// Set the bounding limits also just to make sure that values like -999 are not treated as missing.
			_missingl = Double.NaN;
			_missingu = Double.NaN;
			return;
		}
		if (missing == double.MaxValue)
		{
			_missingl = missing - .001;
			_missingu = missing;
		}
		else
		{
			// Set a range on the missing value check that is slightly on each side of the value
			_missingl = missing - .001;
			_missingu = missing + .001;
		}
	}

	/// <summary>
	/// Set the missing data range for the time series.  The value returned from
	/// getMissing() is computed as the average of the values.  Two values must be
	/// specified, neither of which can be a NaN. </summary>
	/// <param name="missing"> Missing data range for time series. </param>
	public virtual void setMissingRange(double[] missing)
	{
		if (missing == null)
		{
			return;
		}
		if (missing.Length != 2)
		{
			return;
		}
		_missing = (missing[0] + missing[1]) / 2.0;
		if (missing[0] < missing[1])
		{
			_missingl = missing[0];
			_missingu = missing[1];
		}
		else
		{
			_missingl = missing[1];
			_missingu = missing[0];
		}
	}

	/// <summary>
	/// Set the date for the first non-missing data value. </summary>
	/// <param name="t"> Date for the first non-missing data value. </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setNonMissingDataDate1(DateTime t)
	{
		if (t != null)
		{
			_data_limits.setNonMissingDataDate1(t);
		}
	}

	/// <summary>
	/// Set the date for the last non-missing data value. </summary>
	/// <param name="t"> Date for the last non-missing data value. </param>
	/// <seealso cref= DateTime </seealso>
	public virtual void setNonMissingDataDate2(DateTime t)
	{
		if (t != null)
		{
			_data_limits.setNonMissingDataDate2(t);
		}
	}

	/// <summary>
	/// Set a time series property's contents (case-specific). </summary>
	/// <param name="propertyName"> name of property being set. </param>
	/// <param name="property"> property object corresponding to the property name. </param>
	public virtual void setProperty(string propertyName, object property)
	{
		if (__property_HashMap == null)
		{
			__property_HashMap = new LinkedHashMap<string, object>();
		}
		__property_HashMap.put(propertyName, property);
	}

	/// <summary>
	/// Indicate whether the time series is selected.  This is being phased in and is
	/// currently only used by graphics code.
	/// </summary>
	public virtual void setSelected(bool selected)
	{
		_selected = selected;
	}

	/// <summary>
	/// Set the sequence identifier (old sequence number), used with ensembles. </summary>
	/// <param name="sequenceID"> sequence identifier for the time series. </param>
	public virtual void setSequenceID(string sequenceID)
	{
		_id.setSequenceID(sequenceID);
	}

	/// <summary>
	/// Set the time series identifier source. </summary>
	/// <param name="source"> Source for time series. </param>
	public virtual void setSource(string source)
	{
		if (!string.ReferenceEquals(source, null))
		{
			_id.setSource(source);
		}
	}

	/// <summary>
	/// Set the status flag for the time series.  This is used by high-level code when
	/// manipulating time series.  For example, a Vector of time series might be
	/// passed to a routine for graphing.  Additionally, another display component may
	/// list an extended legend.  The status allows the first component to disable some
	/// time series because of incompatibility so the second component can detect.
	/// This feature may be phased out. </summary>
	/// <seealso cref= #getStatus </seealso>
	public virtual void setStatus(string status)
	{
		if (!string.ReferenceEquals(status, null))
		{
			_status = status;
		}
	}

	/// <summary>
	/// Set the time series version, to be used with input file formats. </summary>
	/// <param name="version"> Version number for time series file. </param>
	public virtual void setVersion(string version)
	{
		if (!string.ReferenceEquals(version, null))
		{
			_version = version;
		}
	}

	}

}