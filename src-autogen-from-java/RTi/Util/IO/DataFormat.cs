using System;
using System.Text;

// DataFormat - data format class

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
// DataFormat - data format class
// ----------------------------------------------------------------------------
// History:
//
// 12 Jan 1998	Steven A. Malers, RTi	Initial version.
// 13 Apr 1999	SAM, RTi		Add finalize.
// 18 May 2001	SAM, RTi		Change toString() to return
//					_format_string like C++ and make so
//					the format string is created whenever
//					a field is set.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
// 2001-12-09	SAM, RTi		Copy all TSUnits* classes to Data*
//					to allow general use.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The DataFormat is a simple class used to hold information about data
	/// formatting (e.g., precision for output).  It is primarily used by the DataUnits class. </summary>
	/// <seealso cref= DataUnits </seealso>
	public class DataFormat
	{

	private string _format_string; // C-style format string for data.
	private int _precision; // The number of digits of precision
						// after the decimal point on output.
	private int _width; // The total width of the format.

	/// <summary>
	/// Construct and set the output width to 10 digits, 2 after the decimal
	/// point, and use a %g format.
	/// </summary>
	public DataFormat()
	{
		initialize();
	}

	/// <summary>
	/// Copy constructor. </summary>
	/// <param name="format"> DataFormat to copy. </param>
	public DataFormat(DataFormat format)
	{
		initialize();
		setFormatString(format._format_string);
		setPrecision(format._precision);
		setWidth(format._width);
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DataFormat()
	{
		_format_string = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the format string to use for output. </summary>
	/// <returns> The format string to use for output.  This is a C-style format string
	/// (use with StringUtil.formatString()). </returns>
	/// <seealso cref= RTi.Util.String.StringUtil#formatString </seealso>
	public virtual string getFormatString()
	{
		return _format_string;
	}

	/// <summary>
	/// Return the precision (number of digits after the decimal point). </summary>
	/// <returns> The precision (number of digits after the decimal point) to use for
	/// formatting. </returns>
	public virtual int getPrecision()
	{
		return _precision;
	}

	/// <summary>
	/// Return the width of the output when formatting. </summary>
	/// <returns> The width of the output when formatting. </returns>
	public virtual int getWidth()
	{
		return _width;
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		_precision = 2;
		_width = 10;
		setFormatString();
	}

	/// <summary>
	/// Refresh the value of the format string based on the width and precision.
	/// </summary>
	private void setFormatString()
	{
		if (_width <= 0)
		{
			_format_string = "%." + _precision + "f";
		}
		else
		{
			_format_string = "%" + _width + "." + _precision + "f";
		}
	}

	/// <summary>
	/// Set the format string.  This is a C-style format string. </summary>
	/// <param name="format_string"> Format string to use for output. </param>
	/// <seealso cref= RTi.Util.String.StringUtil#formatString </seealso>
	public virtual void setFormatString(string format_string)
	{
		if (string.ReferenceEquals(format_string, null))
		{
			return;
		}
		_format_string = format_string;
		int dot_pos = format_string.IndexOf(".", StringComparison.Ordinal);
		if (dot_pos < 0)
		{
			// Like %f
			_width = 0;
			_precision = 0;
		}
		else
		{ // Get the width...
			_width = 0;
			StringBuilder b1 = new StringBuilder();
			if (dot_pos > 0)
			{
				for (int i = dot_pos - 1; (i >= 0) && char.IsDigit(format_string[i]); i++)
				{
					b1.Append(format_string[i]);
				}
				if (b1.Length > 0)
				{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET StringBuilder equivalent to the Java 'reverse' method:
					b1.reverse();
					_width = StringUtil.atoi(b1.ToString());
				}
			}
			b1 = null;
			// Get the precision...
			_precision = 0;
			StringBuilder b2 = new StringBuilder();
			int length = format_string.Length;
			for (int i = dot_pos + 1; (i < length) && char.IsDigit(format_string[i]); i++)
			{
				b2.Append(format_string[i]);
			}
			if (b2.Length > 0)
			{
				_precision = StringUtil.atoi(b2.ToString());
			}
			b2 = null;
		}
	}

	/// <summary>
	/// Set the number of digits after the decimal point to use for output. </summary>
	/// <param name="precision"> Number of digits after the decimal point. </param>
	public virtual void setPrecision(int precision)
	{
		_precision = precision;
		setFormatString();
	}

	/// <summary>
	/// Set the total number of characters to use for output. </summary>
	/// <param name="width"> Total number of characters for output. </param>
	public virtual void setWidth(int width)
	{
		_width = width;
		setFormatString();
	}

	/// <summary>
	/// Return string version. </summary>
	/// <returns> A string representation of the format (e.g., "%10.2f"). </returns>
	public override string ToString()
	{
		return _format_string;
	}

	} // End of DataFormat

}