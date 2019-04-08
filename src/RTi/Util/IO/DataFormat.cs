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
        /// Return the precision (number of digits after the decimal point). </summary>
        /// <returns> The precision (number of digits after the decimal point) to use for
        /// formatting. </returns>
        public virtual int getPrecision()
        {
            return _precision;
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
    }
}
