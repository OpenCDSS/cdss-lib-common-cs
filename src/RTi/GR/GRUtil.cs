// GRUtil - utility methods and data for the entire GR package

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
// GRUtil - Utility methods and data for the entire GR package
// ----------------------------------------------------------------------------
// GRConvertUnits - convert a value from one set of units to another
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-05-02	J. Thomas Sapienza, RTi	Initial version.
// 2003-05-07	JTS, RTi		More constants moved in here.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	public class GRUtil
	{

	/// <summary>
	/// Status of device or drawing area is closed.
	/// </summary>
	public const int STATUS_CLOSED = 0;
	/// <summary>
	/// Status of device or drawing area is open.
	/// </summary>
	public const int STATUS_OPEN = 1;
	/// <summary>
	/// Status of device or drawing area is active (and open).
	/// </summary>
	public const int STATUS_ACTIVE = 2;

	/// <summary>
	/// Close a device or drawing area and free its resources.
	/// </summary>
	public const int CLOSE_HARD = 0;
	/// <summary>
	/// Close a device or drawing area but allow its resources to be reused.
	/// </summary>
	public const int CLOSE_SOFT = 1;

	/// <summary>
	/// Draw to the device.
	/// </summary>
	public const int MODE_DRAW = 0x1;
	/// <summary>
	/// Record drawing to a file.
	/// </summary>
	public const int MODE_RECORD = 0x2;
	/// <summary>
	/// Print help for drawing command.
	/// </summary>
	public const int MODE_HELP = 0x4;

	/// <summary>
	/// Indicator of Device (or Drawing Area) being closed.
	/// </summary>
	public const int STAT_CLOSED = 0;
	/// <summary>
	/// Indicator of Device (or Drawing Area) being open.
	/// </summary>
	public const int STAT_OPEN = 1;
	/// <summary>
	/// Indicator of Device (or Drawing Area) being active.
	/// </summary>
	public const int STAT_ACTIVE = 2;

	/// <summary>
	/// Convert the internal orientation number flag to the string representation. </summary>
	/// <param name="orient"> Orientation as GRDeviceUtil.ORIENTATION_*. </param>
	/// <returns> String orientation. </returns>
	/// <exception cref="GRException"> if the orientation cannot be determined. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected static String getStringOrientation(int orient) throws GRException
	protected internal static string getStringOrientation(int orient)
	{
		if (orient == GRDeviceUtil.ORIENTATION_LANDSCAPE)
		{
			return "landscape";
		}
		else if (orient == GRDeviceUtil.ORIENTATION_PORTRAIT)
		{
			return "portrait";
		}
		else
		{
			throw new GRException("Orientation " + orient + " cannot be converted to string");
		}
	}

	/// <summary>
	/// Get size of page as a string. </summary>
	/// <param name="pagesize">	Page size as internal integer (see GRDeviceUtil.SIZE_*). </param>
	/// <returns> page size as string (e.g., "A"). </returns>
	/// <exception cref="GRException"> if the page size cannot be determined. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String getStringPageSize(int pagesize) throws GRException
	public static string getStringPageSize(int pagesize)
	{
		if (pagesize == GRDeviceUtil.SIZE_A)
		{
			return "A";
		}
		else if (pagesize == GRDeviceUtil.SIZE_B)
		{
			return "B";
		}
		else if (pagesize == GRDeviceUtil.SIZE_C)
		{
			return "C";
		}
		else if (pagesize == GRDeviceUtil.SIZE_D)
		{
			return "D";
		}
		else
		{
			throw new GRException("Cannot convert page size " + pagesize + " to string");
		}
	}

	}

}