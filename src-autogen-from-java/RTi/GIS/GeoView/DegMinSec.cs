using System.Text;

// DegMinSec - class to hold degrees, minutes, and seconds

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

namespace RTi.GIS.GeoView
{
	/// <summary>
	/// Class to hold degrees, minutes, and seconds, in particular useful for conversion from decimal degrees in a way
	/// that holds discrete values.
	/// </summary>
	public class DegMinSec
	{

	/// <summary>
	/// Degrees as integer.
	/// </summary>
	private int __deg = 0;

	/// <summary>
	/// Minutes as integer.
	/// </summary>
	private int __min = 0;

	/// <summary>
	/// Seconds as double because remainder is 100s of second.
	/// </summary>
	private double __sec = 0.0;

	/// <summary>
	/// Decimal degrees from original data, if available.
	/// </summary>
	private double? __decdeg = null;

	/// <summary>
	/// Construct from parts.
	/// </summary>
	public DegMinSec(int deg, int min, double sec)
	{
		__deg = deg;
		__min = min;
		__sec = sec;
	}

	/// <summary>
	/// Construct from decimal degrees. </summary>
	/// <param name="decdeg"> decimal degrees to parse </param>
	/// <returns> new DegMinSec instance corresponding to specified decimal degrees </returns>
	public DegMinSec(double decdeg)
	{
		parseDecdeg(decdeg, this);
		__decdeg = new double?(decdeg);
	}

	/// <summary>
	/// Return the decimal degrees corresponding to the instance.
	/// </summary>
	public virtual double getDecDegrees()
	{
		if (__decdeg != null)
		{
			// Have the value from the constructor
			return __decdeg.Value;
		}
		else
		{
			// Calculate from the parts
			double decdeg = __deg + __min / 60.0 + __sec / 3600.0;
			return decdeg;
		}
	}

	/// <summary>
	/// Return the degrees.
	/// </summary>
	public virtual int getDeg()
	{
		return __deg;
	}

	/// <summary>
	/// Return the minutes.
	/// </summary>
	public virtual int getMin()
	{
		return __min;
	}

	/// <summary>
	/// Return the seconds.
	/// </summary>
	public virtual double getSec()
	{
		return __sec;
	}

	/// <summary>
	/// Create from a string.
	/// </summary>
	public static DegMinSec parseDegMinSec(string dms, DegMinSecFormatType format)
	{
		DegMinSec dmso = new DegMinSec(0,0,0.0);
		if (format == DegMinSecFormatType.DEGMMSS)
		{
			// Format is DegMMSS with no partial seconds.  Degrees can be 2 or 3 digits, zero padded.
			int len = dms.Length;
			string ss = dms.Substring(len - 2);
			string mm = dms.Substring(len - 4, (len - 2) - (len - 4));
			string dd = dms.Substring(0,len - 4);
			dmso.__deg = int.Parse(dd);
			dmso.__min = int.Parse(mm);
			dmso.__sec = double.Parse(ss);
		}
		return dmso;
	}

	/// <summary>
	/// Create from decimal degrees.  The seconds are not truncated/rounded. </summary>
	/// <param name="decdeg"> decimal degrees </param>
	/// <param name="dms"> if non-null, use the object to set the parsed values (useful to improve performance by not
	/// creating a new object); if null, return a new object </param>
	/// <returns> DegMinSec object corresponding to decimal degrees </returns>
	public static DegMinSec parseDecdeg(double decdeg, DegMinSec dms)
	{
		if (dms == null)
		{
			dms = new DegMinSec(0,0,0.0);
		}
		dms.__deg = (int)decdeg;
		int frac = (int)((decdeg * 3600.0) % 3600.0);
		dms.__min = frac / 60;
		dms.__sec = frac % 60.0;
		return dms;
	}

	/// <summary>
	/// Return a string representation of the object. </summary>
	/// <param name="format"> the format to use for the string:
	/// <ol>
	/// <li>    DEGMMSS corresponds to "DegMSS", where minutes and seconds
	///        are padded with zeros.  Fractions of seconds are truncated (no attempt to round).
	///        </li> </param>
	public virtual string ToString(DegMinSecFormatType format)
	{
		if (format == DegMinSecFormatType.DEGMMSS)
		{
			StringBuilder b = new StringBuilder("");
			// Do not round the arc-seconds because there is no way to go the other direction with
			// only 4 decimals of output.
			int deg = getDeg();
			b.Append("" + (int)deg);
			string min = "" + getMin();
			if (min.Length == 1)
			{
				min = "0" + min;
			}
			b.Append(min);
			string sec = "" + (int)getSec();
			if (sec.Length == 1)
			{
				sec = "0" + sec;
			}
			b.Append(sec);
			return b.ToString();
		}
		else
		{
			return "";
		}
	}

	}

}