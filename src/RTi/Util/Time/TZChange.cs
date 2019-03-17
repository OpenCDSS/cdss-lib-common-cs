// TZChange - time-zone change information

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

//------------------------------------------------------------------------------
// TZChange - time-zone change information
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 2001-12-19	Steven A. Malers, RTi	Initial version.
//------------------------------------------------------------------------------

namespace RTi.Util.Time
{
	/// <summary>
	/// The TZChange class provides storage for a time zone change data (dates when
	/// time zones change from standard to daylight savings) for the USA.  An array of
	/// data is maintained in the TZ class.  TZChange typically only need to be
	/// accessed from the TZ class.  Note that there are many exceptions to these
	/// general parameters.  In order to provide complete support, it is likely that
	/// a database of time zone parameters would need to be read or a more complex way
	/// to add time zone change rules needs to be added.
	/// The time changes occur at 2AM. </summary>
	/// <seealso cref= TZ  </seealso>
	public class TZChange
	{

	/// <summary>
	/// Year of interest.
	/// </summary>
	public int year;

	/// <summary>
	/// Day in April when the time changes from standard to daylight savings time.
	/// </summary>
	public int apr_tods;

	/// <summary>
	/// Day in October when the time changes from daylight savings to standard time.
	/// </summary>
	public int oct_tost;

	/// <summary>
	/// Construct using the individual data items.  See the description of public data
	/// for more information. </summary>
	/// <param name="y"> Year. </param>
	/// <param name="a_tods"> Day in April to switch to daylight savings time. </param>
	/// <param name="o_tost"> Day in October to switch to standard time. </param>
	public TZChange(int y, int a_tods, int o_tost)
	{
		year = y;
		apr_tods = a_tods;
		oct_tost = o_tost;
	}

	}

}