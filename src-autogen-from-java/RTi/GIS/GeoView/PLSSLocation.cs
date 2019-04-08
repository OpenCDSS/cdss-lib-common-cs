using System;
using System.Collections.Generic;

// PLSSLocation - class for storing PLSS (Public Land Survey System) location data.

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

// -----------------------------------------------------------------------------
// PLSSLocation - class for storing PLSS (Public Land Survey System) 
//	location data.
// -----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file
// -----------------------------------------------------------------------------
// History:
//
// 2005-01-12	J. Thomas Sapienza, RTi	Initial version.
// 2005-01-14	JTS, RTi		* Removed enumerations.
//					* Converted some data from ints to 
//					  Strings.
// 2005-02-01	JTS, RTi		* Can now parse CDSS PLSS Locations.
//					* Removed the verbose location.
//					* Added half-section.
// 2005-02-04	JTS, RTi		Changed to support the SPFlex location
//					syntax in StateView.
// 2005-04-27	JTS, RTi		Added finalize().
// 2005-07-06	JTS, RTi		Removed spaces from the location string.
//					They were causing issues with querying
//					from HydroBase.  This is for new format
//					CDSS format strings.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// -----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class represents a PLSS (public land survey system) location.  
	/// Currently baselines are not fully supported.
	/// </summary>
	public class PLSSLocation
	{

	/// <summary>
	/// The value to use when unsetting a range, section, or township value in a 
	/// location.
	/// </summary>
	public const int UNSET = -999;

	/// <summary>
	/// The location baseline from which grids are numbered.  Runs East-West.
	/// </summary>
	private int __baseline = UNSET;

	/// <summary>
	/// The half section.
	/// </summary>
	private string __halfSection = null;

	/// <summary>
	/// The principal meridian from which grids are numbered.  Runs North-South.
	/// </summary>
	private string __pm = null;

	/// <summary>
	/// The 160-acre 1/4 section.
	/// </summary>
	private string __q = null;

	/// <summary>
	/// The 40-acre 1/4 1/4 section.
	/// </summary>
	private string __qq = null;

	/// <summary>
	/// The 10-acre 1/4 1/4 1/4 section.
	/// </summary>
	private string __qqq = null;

	/// <summary>
	/// The grid location E or W of the principal meridian.
	/// </summary>
	private int __range = UNSET;

	/// <summary>
	/// The direction of the grid location.  Must be either EAST or WEST.
	/// </summary>
	private string __rangeDirection = null;

	/// <summary>
	/// The section of the grid location found by the range and township.  The grid
	/// location is divided into 36 squares, each of which is a section.
	/// </summary>
	private int __section = UNSET;

	/// <summary>
	/// The grid location N or S of the baseline.
	/// </summary>
	private int __township = UNSET;

	/// <summary>
	/// The direction of the grid location.  Must be either NORTH or SOUTH.
	/// </summary>
	private string __townshipDirection = null;

	/// <summary>
	/// Constructor.
	/// </summary>
	public PLSSLocation()
	{
	}

	/// <summary>
	/// Constructor.  Parses the specified location string (see toString(false))
	/// and fills the values from the string. </summary>
	/// <param name="locationString"> the concise location string to parse. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PLSSLocation(String locationString) throws Exception
	public PLSSLocation(string locationString) : this(locationString, false)
	{
	}

	/// <summary>
	/// Constructor.  Parses the specified location string and fills the values 
	/// from the string. </summary>
	/// <param name="locationString"> the concise location string to parse. </param>
	/// <param name="cdssFormat"> whether the location String is in CDSS format (true) or not. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PLSSLocation(String locationString, boolean cdssFormat) throws Exception
	public PLSSLocation(string locationString, bool cdssFormat)
	{
		PLSSLocation location = PLSSLocation.parse(locationString, cdssFormat);

		setPM(location.getPM());
		setBaseline(location.getBaseline());
		setTownship(location.getTownship());
		setTownshipDirection(location.getTownshipDirection());
		setRange(location.getRange());
		setRangeDirection(location.getRangeDirection());
		setSection(location.getSection());
		setHalfSection(location.getHalfSection());
		setQ(location.getQ());
		setQQ(location.getQQ());
		setQQQ(location.getQQQ());
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~PLSSLocation()
	{
		__halfSection = null;
		__pm = null;
		__q = null;
		__qq = null;
		__qqq = null;
		__rangeDirection = null;
		__townshipDirection = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the baseline used for the plss location. </summary>
	/// <returns> the baseline used for the plss location. </returns>
	public virtual int getBaseline()
	{
		return __baseline;
	}

	/// <summary>
	/// Returns the half section. </summary>
	/// <returns> the half section. </returns>
	public virtual string getHalfSection()
	{
		return __halfSection;
	}

	/// <summary>
	/// Returns the principal meridian used for the plss location. </summary>
	/// <returns> the principal meridian used for the plss location. </returns>
	public virtual string getPM()
	{
		return __pm;
	}

	/// <summary>
	/// Returns the 160-acre quarter of the section. </summary>
	/// <returns> the 160-acre quarter of the section. </returns>
	public virtual string getQ()
	{
		return __q;
	}

	/// <summary>
	/// Returns the 40-acre quarter of the section. </summary>
	/// <returns> the 40-acre quarter of the section. </returns>
	public virtual string getQQ()
	{
		return __qq;
	}

	/// <summary>
	/// Returns the 10-acre quarter of the section. </summary>
	/// <returns> the 10-acre quarter of the section. </returns>
	public virtual string getQQQ()
	{
		return __qqq;
	}

	/// <summary>
	/// Returns the range value, relative to the principal meridian. </summary>
	/// <returns> the range value, relative to the principal meridian. </returns>
	public virtual int getRange()
	{
		return __range;
	}

	/// <summary>
	/// Returns the range direction. </summary>
	/// <returns> the range direction. </returns>
	public virtual string getRangeDirection()
	{
		return __rangeDirection;
	}

	/// <summary>
	/// Returns the section. </summary>
	/// <returns> the section. </returns>
	public virtual int getSection()
	{
		return __section;
	}

	/// <summary>
	/// Returns the township, relative to the baseline. </summary>
	/// <returns> the township, relative to the baseline. </returns>
	public virtual int getTownship()
	{
		return __township;
	}

	/// <summary>
	/// Returns the township direction. </summary>
	/// <returns> the township direction. </returns>
	public virtual string getTownshipDirection()
	{
		return __townshipDirection;
	}

	/// <summary>
	/// Parses a representation (see toString(false)) of a plss location and
	/// returns a PLSSLocation object with the location information filled in. </summary>
	/// <param name="locationString"> a concise representation of a plss location. </param>
	/// <returns> a PLSSLocation object with the location information filled in. </returns>
	/// <exception cref="Exception"> if there is an eror parsing the location string or if there
	/// is a problem filling data in the location object. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static PLSSLocation parse(String locationString) throws Exception
	public static PLSSLocation parse(string locationString)
	{
		return parse(locationString, false);
	}

	/// <summary>
	/// Parses a representation (see toString()) of a plss location and
	/// returns a PLSSLocation object with the location information filled in. </summary>
	/// <param name="locationString"> a concise representation of a plss location. </param>
	/// <param name="cdssFormat"> whether the location representation is in CDSS Format. </param>
	/// <returns> a PLSSLocation object with the location information filled in. </returns>
	/// <exception cref="Exception"> if there is an error parsing the location string or if there
	/// is a problem filling data in the location object. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static PLSSLocation parse(String locationString, boolean cdssFormat) throws Exception
	public static PLSSLocation parse(string locationString, bool cdssFormat)
	{
		IList<string> v = StringUtil.breakStringList(locationString, ",", 0);

		if (cdssFormat)
		{
			return parseCDSSLocation(locationString);
		}

		// REVISIT (JTS - 2005-01-12)
		// written but not yet tested!

		int index = -1;
		PLSSLocation location = new PLSSLocation();
		string s = null;
		string key = null;
		string value = null;
		string temp = null;

		for (int i = 0; i < v.Count; i++)
		{
			s = ((string)v[i]).Trim();
			index = s.IndexOf("=", StringComparison.Ordinal);

			if (index == -1)
			{
				throw new Exception("Invalid location string ('" + locationString + "').  " + "One of the comma-delimited parameters does " + "not have an equals sign.");
			}

			key = s.Substring(0, index);

			index = s.IndexOf("\"", StringComparison.Ordinal);

			if (index == -1)
			{
				throw new Exception("Invalid location string.  ('" + locationString + "').  " + key + " does not have values in quotation " + "marks.");
			}

			value = s.Substring(index + 1, (s.Length - 1) - (index + 1));

			if (key.Equals("PM", StringComparison.OrdinalIgnoreCase))
			{
				location.setPM(value);
			}
			else if (key.Equals("TS", StringComparison.OrdinalIgnoreCase))
			{
				index = value.IndexOf(" ", StringComparison.Ordinal);
				if (index == -1)
				{
					throw new Exception("Invalid TS value " + "(no direction).");
				}
				temp = value.Substring(0, index);
				location.setTownship(StringUtil.atoi(temp));
				temp = value.Substring(index).Trim();
				location.setTownshipDirection(temp);
			}
			else if (key.Equals("Range", StringComparison.OrdinalIgnoreCase))
			{
				index = value.IndexOf(" ", StringComparison.Ordinal);
				if (index == -1)
				{
					throw new Exception("Invalid Range value " + "(no direction).");
				}
				temp = value.Substring(0, index);
				location.setRange(StringUtil.atoi(temp));
				temp = value.Substring(index).Trim();
				location.setRangeDirection(temp);
			}
			else if (key.Equals("Section", StringComparison.OrdinalIgnoreCase))
			{
				index = value.IndexOf(" ", StringComparison.Ordinal);
				if (index == -1)
				{
					throw new Exception("Invalid section value " + "(no half section).");
				}
				temp = value.Substring(0, index);
				location.setSection(StringUtil.atoi(temp));
				temp = value.Substring(index).Trim();
				location.setHalfSection(temp);
			}
			else if (key.Equals("Q", StringComparison.OrdinalIgnoreCase))
			{
				location.setQ(value);
			}
			else if (key.Equals("QQ", StringComparison.OrdinalIgnoreCase))
			{
				location.setQQ(value);
			}
			else if (key.Equals("QQQ", StringComparison.OrdinalIgnoreCase))
			{
				location.setQQQ(value);
			}
			else
			{
				throw new Exception("Unknown key: " + key);
			}
		}

		return location;
	}

	/// <summary>
	/// Parses a representation (see toString(true)) of a plss location and
	/// returns a PLSSLocation object with the location information filled in. </summary>
	/// <param name="locationString"> a concise representation of a plss location. </param>
	/// <exception cref="Exception"> if there is an eror parsing the location string or if there
	/// is a problem filling data in the location object. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static PLSSLocation parseCDSSLocation(String locationString) throws Exception
	public static PLSSLocation parseCDSSLocation(string locationString)
	{
		IList<string> v = StringUtil.breakStringList(locationString, ",", 0);

		if (v.Count == 7)
		{
			return parseOldCDSSLocation(locationString);
		}
		else if (v.Count == 12)
		{
			// continue on	
		}
		else
		{
			throw new Exception("Improperly-formed CDSS PLSS location " + "string: " + locationString);
		}

		string s = null;

		PLSSLocation location = new PLSSLocation();

		s = ((string)v[0]).Trim();
		if (s.Equals("*"))
		{
			location.setPM(null);
		}
		else
		{
			location.setPM(s);
		}

		s = ((string)v[1]).Trim();
		if (s.Equals("*"))
		{
			location.setTownship(UNSET);
		}
		else
		{
			location.setTownship(StringUtil.atoi(s));
		}

		// half-township (2) is ignored

		s = ((string)v[3]).Trim();
		if (s.Equals("*"))
		{
			location.setTownshipDirection(null);
		}
		else
		{
			location.setTownshipDirection(s);
		}

		s = ((string)v[4]).Trim();
		if (s.Equals("*"))
		{
			location.setRange(UNSET);
		}
		else
		{
			location.setRange(StringUtil.atoi(s));
		}

		// half-range (5) is ignored

		s = ((string)v[6]).Trim();
		if (s.Equals("*"))
		{
			location.setRangeDirection(null);
		}
		else
		{
			location.setRangeDirection(s);
		}

		s = ((string)v[7]).Trim();
		if (s.Equals("*"))
		{
			location.setSection(UNSET);
		}
		else
		{
			location.setSection(StringUtil.atoi(s));
		}

		s = ((string)v[8]).Trim();
		if (s.Equals("*"))
		{
			location.setHalfSection(null);
		}
		else
		{
			location.setHalfSection(s);
		}

		s = ((string)v[9]).Trim();
		if (s.Equals("*"))
		{
			location.setQ(null);
		}
		else
		{
			location.setQ(s);
		}

		s = ((string)v[10]).Trim();
		if (s.Equals("*"))
		{
			location.setQQ(null);
		}
		else
		{
			location.setQQ(s);
		}

		s = ((string)v[11]).Trim();
		if (s.Equals("*"))
		{
			location.setQQQ(null);
		}
		else
		{
			location.setQQQ(s);
		}

		return location;
	}

	/// <summary>
	/// Parses a representation of a plss location and
	/// returns a PLSSLocation object with the location information filled in.  This
	/// parses the old CDSS Location, in which directions were combined with their
	/// areas.  For instance, "10 W" could be a range.  In the new CDSS location
	/// Strings, every piece is in a separate comma-delimited part of the String. </summary>
	/// <param name="locationString"> a concise representation of a plss location. </param>
	/// <returns> a PLSSLocation object with the location information filled in. </returns>
	/// <exception cref="Exception"> if there is an eror parsing the location string or if there
	/// is a problem filling data in the location object. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static PLSSLocation parseOldCDSSLocation(String locationString) throws Exception
	public static PLSSLocation parseOldCDSSLocation(string locationString)
	{
		IList<string> v = StringUtil.breakStringList(locationString, ",", 0);
		string s = null;

		PLSSLocation location = new PLSSLocation();

		s = ((string)v[0]).Trim();
		if (s.Equals("*"))
		{
			location.setPM(null);
		}
		else
		{
			location.setPM(s);
		}

		s = ((string)v[1]).Trim();
		int index = -1;
		if (s.Equals("*"))
		{
			location.setTownship(UNSET);
			location.setTownshipDirection(null);
		}
		else
		{
			index = s.IndexOf(" ", StringComparison.Ordinal);
			if (index > -1)
			{
				location.setTownship(StringUtil.atoi(s.Substring(0, index)));
				s = s.Substring(index + 1).Trim();
				if (s.Equals("*"))
				{
					location.setTownshipDirection(null);
				}
				else
				{
					location.setTownshipDirection(s);
				}
			}
			else
			{
				location.setTownship(UNSET);
				location.setTownshipDirection(s);
			}
		}

		s = ((string)v[2]).Trim();
		if (s.Equals("*"))
		{
			location.setRange(UNSET);
			location.setRangeDirection(null);
		}
		else
		{
			index = s.IndexOf(" ", StringComparison.Ordinal);
			if (index > -1)
			{
				location.setRange(StringUtil.atoi(s.Substring(0, index)));
				s = s.Substring(index + 1).Trim();
				if (s.Equals("*"))
				{
					location.setRangeDirection(null);
				}
				else
				{
					location.setRangeDirection(s);
				}
			}
			else
			{
				location.setRange(UNSET);
				location.setRangeDirection(s);
			}
		}

		s = ((string)v[3]).Trim();
		if (s.Equals("*"))
		{
			location.setSection(UNSET);
			location.setHalfSection(null);
		}
		else
		{
			index = s.IndexOf(" ", StringComparison.Ordinal);
			if (index > -1)
			{
				location.setSection(StringUtil.atoi(s.Substring(0, index)));
				s = s.Substring(index + 1).Trim();
				if (s.Equals("*"))
				{
					location.setHalfSection(null);
				}
				else
				{
					location.setHalfSection(s);
				}
			}
			else
			{
				location.setSection(UNSET);
				location.setHalfSection(s);
			}
		}

		s = ((string)v[4]).Trim();
		if (s.Equals("*"))
		{
			location.setQ(null);
		}
		else
		{
			location.setQ(s);
		}

		s = ((string)v[5]).Trim();
		if (s.Equals("*"))
		{
			location.setQQ(null);
		}
		else
		{
			location.setQQ(s);
		}

		s = ((string)v[6]).Trim();
		if (s.Equals("*"))
		{
			location.setQQQ(null);
		}
		else
		{
			location.setQQQ(s);
		}

		return location;
	}

	/// <summary>
	/// Sets the baseline. </summary>
	/// <param name="baseline"> the baseline to set. </param>
	/// <exception cref="Exception"> if there is an error setting the baseline. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setBaseline(int baseline) throws Exception
	public virtual void setBaseline(int baseline)
	{
		// REVISIT (JTS - 2005-01-12)
		// what are valid baselines?
		__baseline = baseline;
	}

	/// <summary>
	/// Sets the half section. </summary>
	/// <param name="halfSection"> the half section to set. </param>
	public virtual void setHalfSection(string halfSection)
	{
		__halfSection = halfSection;
	}

	/// <summary>
	/// Sets the principal meridian. </summary>
	/// <param name="pm"> the principal meridian to set.  Must be one of B, C, N, S, or U. </param>
	public virtual void setPM(string pm)
	{
		__pm = pm;
	}

	/// <summary>
	/// Sets the 160-acre section quarter. </summary>
	/// <param name="q"> the quarter.  Must be one of NORTHWEST, NORTHEAST, SOUTHWEST, 
	/// or SOUTHEAST. </param>
	public virtual void setQ(string q)
	{
		__q = q;
	}

	/// <summary>
	/// Sets the 40-acre section quarter. </summary>
	/// <param name="qq"> the quarter.  Must be one of NORTHWEST, NORTHEAST, SOUTHWEST, 
	/// or SOUTHEAST. </param>
	public virtual void setQQ(string qq)
	{
		__qq = qq;
	}

	/// <summary>
	/// Sets the the 10-acre section quarter. </summary>
	/// <param name="qqq"> the quarter.  Must be one of NORTHWEST, NORTHEAST, SOUTHWEST, 
	/// or SOUTHEAST. </param>
	public virtual void setQQQ(string qqq)
	{
		__qqq = qqq;
	}

	/// <summary>
	/// Sets the range. </summary>
	/// <param name="range"> the range.  Must be greater than 0. </param>
	/// <exception cref="Exception"> if an invalid range is set. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setRange(int range) throws Exception
	public virtual void setRange(int range)
	{
		if (range < 1 && range != UNSET)
		{
			throw new Exception("Range must be greater than 0");
		}

		__range = range;
	}

	/// <summary>
	/// Sets the range direction. </summary>
	/// <param name="direction"> the range direction.  Must be either WEST or EAST. </param>
	public virtual void setRangeDirection(string direction)
	{
		__rangeDirection = direction;
	}

	/// <summary>
	/// Sets the section number. </summary>
	/// <param name="section"> the section number to set.  Must be &gt;= 1 and &lt;= 36. </param>
	/// <exception cref="Exception"> if an invalid section is set. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setSection(int section) throws Exception
	public virtual void setSection(int section)
	{
		if (section < 1 && section != UNSET)
		{
			throw new Exception("Section must be greater than 0");
		}

		if (section > 36)
		{
			throw new Exception("Section must be less than 37");
		}

		__section = section;
	}

	/// <summary>
	/// Sets the township. </summary>
	/// <param name="township"> the township to set.  Must be greater than 0. </param>
	/// <exception cref="Exception"> if an invalid township is set. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTownship(int township) throws Exception
	public virtual void setTownship(int township)
	{
		if (township < 1 && township != UNSET)
		{
			throw new Exception("Township must be greater than 0");
		}

		__township = township;
	}

	/// <summary>
	/// Sets the township direction. </summary>
	/// <param name="direction"> the township direction to set.  Must be either NORTH or SOUTH. </param>
	public virtual void setTownshipDirection(string direction)
	{
		__townshipDirection = direction;
	}

	/// <summary>
	/// Returns a representation of this location as a String. </summary>
	/// <returns> a representation of this location as a String. </returns>
	public override string ToString()
	{
		return ToString(false);
	}

	/// <summary>
	/// Returns this location as a String.  If any of the data values are unset 
	/// (null Strings or ints == UNSET) then they are not included in the output. </summary>
	/// <param name="cdssFormat"> whether to display the location in CDSS format 
	/// ("*, *, *, *, *, *, *") or in normal PLSS Location. </param>
	public virtual string ToString(bool cdssFormat)
	{
		return ToString(cdssFormat, true);
	}

	/// <summary>
	/// Returns this location as a String.  If any of the data values are unset 
	/// (null Strings or ints == UNSET) then they are not included in the output. </summary>
	/// <param name="cdssFormat"> whether to display the location in CDSS format 
	/// ("*, *, *, *, *, *, *") or in normal PLSS Location. </param>
	/// <param name="odlFormat"> whether to display the location in the old CDSS format,
	/// which combined areas and their directions (for instance "... TS TDIR, ...")
	/// or in the new one, in which all values are in a separate comma-delimited 
	/// area. </param>
	public virtual string ToString(bool cdssFormat, bool oldFormat)
	{
		string s = "";
	// REVISIT (JTS - 2005-01-12)
	// baseline is not yet supported fully

		int count = 0;
		if (cdssFormat)
		{
			if (oldFormat)
			{
				if (string.ReferenceEquals(getPM(), null))
				{
					s += "*, ";
				}
				else
				{
					s += getPM() + ", ";
				}

				if (getTownship() == UNSET && string.ReferenceEquals(getTownshipDirection(), null))
				{
					s += "*, ";
				}
				else if (getTownship() != UNSET && string.ReferenceEquals(getTownshipDirection(), null))
				{
						s += getTownship() + " *, ";
				}
				else if (getTownship() == UNSET && !string.ReferenceEquals(getTownshipDirection(), null))
				{
						s += getTownshipDirection() + ", ";
				}
				else
				{
					s += getTownship() + " "
						+ getTownshipDirection() + ", ";
				}

				if (getRange() == UNSET && string.ReferenceEquals(getRangeDirection(), null))
				{
					s += "*, ";
				}
				else if (getRange() != UNSET && string.ReferenceEquals(getRangeDirection(), null))
				{
						s += getRange() + " *, ";
				}
				else if (getRange() == UNSET && !string.ReferenceEquals(getRangeDirection(), null))
				{
						s += getRangeDirection() + ", ";
				}
				else
				{
					s += getRange() + " " + getRangeDirection() + ", ";
				}

				if (getSection() == UNSET && string.ReferenceEquals(getHalfSection(), null))
				{
					s += "*, ";
				}
				else if (getSection() != UNSET && string.ReferenceEquals(getHalfSection(), null))
				{
						s += getSection() + " *, ";
				}
				else if (getSection() == UNSET && !string.ReferenceEquals(getHalfSection(), null))
				{
						s += getHalfSection() + ", ";
				}
				else
				{
					s += getSection() + " " + getHalfSection() + ", ";
				}

				if (string.ReferenceEquals(getQ(), null))
				{
					s += "*, ";
				}
				else
				{
					s += getQ() + ", ";
				}

				if (string.ReferenceEquals(getQQ(), null))
				{
					s += "*, ";
				}
				else
				{
					s += getQQ() + ", ";
				}

				if (string.ReferenceEquals(getQQQ(), null))
				{
					s += "*";
				}
				else
				{
					s += getQQQ();
				}
			}
			else
			{
				if (string.ReferenceEquals(getPM(), null))
				{
					s += "*,";
				}
				else
				{
					s += getPM() + ",";
				}

				if (getTownship() == UNSET)
				{
					s += "*,";
				}
				else
				{
					s += "" + getTownship() + ",";
				}

				// half-township is not supported now
				s += "*,";

				if (string.ReferenceEquals(getTownshipDirection(), null))
				{
					s += "*,";
				}
				else
				{
					s += "" + getTownshipDirection() + ",";
				}

				if (getRange() == UNSET)
				{
					s += "*,";
				}
				else
				{
					s += "" + getRange() + ",";
				}

				// half-range is not supported now
				s += "*,";

				if (string.ReferenceEquals(getRangeDirection(), null))
				{
					s += "*,";
				}
				else
				{
					s += "" + getRangeDirection() + ",";
				}

				if (getSection() == UNSET)
				{
					s += "*,";
				}
				else
				{
					s += "" + getSection() + ",";
				}

				if (string.ReferenceEquals(getHalfSection(), null))
				{
					s += "*,";
				}
				else
				{
					s += "" + getHalfSection() + ",";
				}

				if (string.ReferenceEquals(getQ(), null))
				{
					s += "*,";
				}
				else
				{
					s += getQ() + ",";
				}

				if (string.ReferenceEquals(getQQ(), null))
				{
					s += "*,";
				}
				else
				{
					s += getQQ() + ",";
				}

				if (string.ReferenceEquals(getQQQ(), null))
				{
					s += "*";
				}
				else
				{
					s += getQQQ();
				}
			}
		}
		else
		{
			if (!string.ReferenceEquals(__pm, null))
			{
				s += "PM=\"" + __pm + "\"";
				count++;
			}

			if (__township != UNSET && !string.ReferenceEquals(__townshipDirection, null))
			{
				if (count > 0)
				{
					s += ", ";
				}
				s += "TS=\"" + __township + " " + __townshipDirection + "\"";
				count++;
			}

			if (__range != UNSET && !string.ReferenceEquals(__rangeDirection, null))
			{
				if (count > 0)
				{
					s += ", ";
				}
				s += "Range=\"" + __range + " " + __rangeDirection + "\"";
				count++;
			}

			if (__section != UNSET)
			{
				if (count > 0)
				{
					s += ", ";
				}
				s += "Section=\"" + __section + " " + __halfSection + "\"";
				count++;
			}

			if (!string.ReferenceEquals(__q, null))
			{
				if (count > 0)
				{
					s += ", ";
				}
				s += "Q=\"" + __q + "\"";
				count++;
			}

			if (!string.ReferenceEquals(__qq, null))
			{
				if (count > 0)
				{
					s += ", ";
				}
				s += "QQ=\"" + __qq + "\"";
				count++;
			}

			if (!string.ReferenceEquals(__qqq, null))
			{
				if (count > 0)
				{
					s += ", ";
				}
				s += "QQQ=\"" + __qqq + "\"";
			}
		}
		return s;
	}

	// REVISIT (JTS - 2006-05-22)
	// This may not be necessary anymore.
	public static bool unset(string val)
	{
		if (string.ReferenceEquals(val, null))
		{
			return true;
		}
		return false;
	}

	// REVISIT (JTS - 2006-05-22)
	// This may not be necessary anymore.
	public static bool unset(int val)
	{
		if (val == UNSET)
		{
			return true;
		}
		return false;
	}

	}

}