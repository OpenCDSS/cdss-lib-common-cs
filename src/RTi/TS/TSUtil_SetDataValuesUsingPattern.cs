using System.Text;

// TSUtil_SetDataValuesUsingPattern - set values in a time series using a pattern.

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

namespace RTi.TS
{
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// Set values in a time series using a pattern.
	/// </summary>
	public class TSUtil_SetDataValuesUsingPattern
	{

	/// <summary>
	/// Constructor.
	/// </summary>
	public TSUtil_SetDataValuesUsingPattern()
	{
		// Does nothing.
	}

	/// <summary>
	/// Set the time series data to a repeating sequence of values. </summary>
	/// <param name="ts"> Time series to update. </param>
	/// <param name="startDate"> Date to start assignment. </param>
	/// <param name="endDate"> Date to stop assignment. </param>
	/// <param name="patternValues"> Data values to set as time series data.  NaN will cause the value to be set to the
	/// missing data value for the time series. </param>
	/// <param name="patternFlags"> String flag values to set as data, or null to not use flags. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDataValuesUsingPattern(TS ts, RTi.Util.Time.DateTime startDate, RTi.Util.Time.DateTime endDate, double [] patternValues, String [] patternFlags) throws Exception
	public virtual void setDataValuesUsingPattern(TS ts, DateTime startDate, DateTime endDate, double[] patternValues, string[] patternFlags)
	{ // Get valid dates because the ones passed in may have been null...
		TSLimits valid_dates = TSUtil.getValidPeriod(ts, startDate, endDate);
		DateTime start = valid_dates.getDate1();
		DateTime end = valid_dates.getDate2();

		// Replace NaN values in patternValues with the missing value for the time series.  Do it
		// here so that checks are minimized and the genesis information shows values relevant for the time series.
		double[] patternValues2 = new double[patternValues.Length];
		for (int i = 0; i < patternValues.Length; i++)
		{
			if (double.IsNaN(patternValues[i]))
			{
				patternValues2[i] = ts.getMissing();
			}
			else
			{
				patternValues2[i] = patternValues[i];
			}
		}

		TSIterator tsi = ts.iterator(start, end);
		int ipattern = 0;
		int iflag = 0;
		DateTime date;
		bool usingDataFlags = false;
		if ((patternFlags != null) && (patternFlags.Length > 0))
		{
			usingDataFlags = true;
		}
		while (tsi.next() != null)
		{
			// The first call will set the pointer to the first data value in the period.
			// next() will return null when the last date in the processing period has been passed.
			date = tsi.getDate();
			if (usingDataFlags)
			{
				ts.setDataValue(date, patternValues2[ipattern++], patternFlags[iflag++], 0);
			}
			else
			{
				ts.setDataValue(date, patternValues2[ipattern++]);
			}
			if (ipattern == patternValues2.Length)
			{
				// Reset to start at the beginning of the pattern...
				ipattern = 0;
			}
			if (usingDataFlags && (iflag == patternFlags.Length))
			{
				// Reset to start at the beginning of the flags...
			   iflag = 0;
			}
		}
		// Set the genesis information...
		ts.setDescription(ts.getDescription() + ", pattern");
		StringBuilder patternbuf = new StringBuilder();
		for (int i = 0; i < patternValues2.Length; i++)
		{
			if (i != 0)
			{
				patternbuf.Append(",");
			}
			patternbuf.Append(StringUtil.formatString(patternValues2[i],"%.3f"));
		}
		StringBuilder flagbuf = new StringBuilder();
		if (usingDataFlags)
		{
			for (int i = 0; i < patternFlags.Length; i++)
			{
				if (i != 0)
				{
					flagbuf.Append(",");
				}
				flagbuf.Append(patternFlags[i]);
			}
			if (flagbuf.Length > 0)
			{
				flagbuf.Insert(0, ", flags=");
			}
		}
		ts.addToGenesis("Set " + start + " to " + end + " to pattern=" + patternbuf.ToString() + flagbuf.ToString());
	}

	}

}