using System;
using System.Collections.Generic;
using System.IO;

// DataUnits - class to provide capabilities for reading and storing data units and conversion between units

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

namespace RTi.Util.IO
{

    using Message = RTi.Util.Message.Message;
    using StringUtil = RTi.Util.String.StringUtil;

    /// <summary>
	/// The DataUnits class provides capabilities for reading and storing 
	/// data units and conversion between units.  Units are maintained internally using a list of DataUnits.
	/// </summary>
	public class DataUnits
    {
        /// <summary>
        /// Indicates that the units system is unknown.
        /// </summary>
        public const int SYSTEM_UNKNOWN = 0;
        /// <summary>
        /// Indicates that the units are for the English System.
        /// </summary>
        public const int SYSTEM_ENGLISH = 1;
        /// <summary>
        /// Indicates that the units are for the International System.
        /// </summary>
        public const int SYSTEM_SI = 2;
        /// <summary>
        /// Indicates that the units are for both English and International System.
        /// </summary>
        public const int SYSTEM_ALL = 3;

        // Data members...

        /// <summary>
        /// The units abbreviation (e.g., "AF").
        /// </summary>
        private string __abbreviation;
        /// <summary>
        /// The long name (e.g., "ACRE-FOOT").
        /// </summary>
        private string __long_name;
        /// <summary>
        /// The dimension (e.g., "L3").
        /// </summary>
        //private DataDimension __dimension;
        /// <summary>
        /// Indicates whether it the base unit in the dimension.
        /// </summary>
        private int __base_flag;
        /// <summary>
        /// The number of digits of precision after the decimal point on output.
        /// </summary>
        private int __output_precision;
        /// <summary>
        /// Units system (SYSTEM_SI, SYSTEM_ENGLISH, SYSTEM_ALL, or SYSTEM_UNKNOWN).
        /// </summary>
        private int __system;
        /// <summary>
        /// Multiplier for conversion (relative to base).
        /// </summary>
        private double __mult_factor;
        /// <summary>
        /// Add factor for conversion (relative to base).
        /// </summary>
        private double __add_factor;
        /// <summary>
        /// Behavior flag (e.g., whether to output in upper-case).
        /// </summary>
        private int __behavior_mask;
        /// <summary>
        /// Note indicating source of the data units.
        /// </summary>
        private string __source;

        /// <summary>
        /// List of internally-maintained available units, make sure to be non-null.
        /// </summary>
        private static IList<DataUnits> __units_Vector = new List<DataUnits>(20);

        /// <summary>
        /// Construct and set all data members to empty strings and zeros.
        /// </summary>
        public DataUnits()
        {
            initialize();
        }

        /// <summary>
        /// Return the units abbreviation string. </summary>
        /// <returns> The units abbreviation string. </returns>
        public virtual string getAbbreviation()
        {
            return __abbreviation;
        }

        /// <summary>
        /// Determine the format for output based on the units and precision.  A default precision of 2 is used. </summary>
        /// <returns> the printing format for data of a units type. </returns>
        /// <param name="units_string"> Units of data. </param>
        /// <param name="width"> Width of output (if zero, no width will be used in the format). </param>
        public static DataFormat getOutputFormat(string units_string, int width)
        {
            return getOutputFormat(units_string, width, 2);
        }

        /// <summary>
        /// Determine the format for output based on the units and precision. </summary>
        /// <returns> the printing format for data of a units type. </returns>
        /// <param name="units_string"> Units of data. </param>
        /// <param name="width"> Width of output (if zero, no width will be used in the format). </param>
        /// <param name="default_precision"> Default precision if precision cannot be determined
        /// from the units.  If not specified, 2 will be used. </param>
        public static DataFormat getOutputFormat(string units_string, int width, int default_precision)
        {
            string routine = "DataUnits.getOutputFormat";

            // Initialize the DataFormat for return...

            DataFormat format = new DataFormat();
            format.setWidth(width);
            format.setPrecision(default_precision);

            // Check for valid units request...

            if ((string.ReferenceEquals(units_string, null)) || (units_string.Length == 0))
            {
                // No units are specified...
                Message.printWarning(3, routine, "No units abbreviation specified.  Using precision " + default_precision);
                return format;
            }

            // Get the units...

            try
            {
                DataUnits units = lookupUnits(units_string);
                format.setPrecision(units.getOutputPrecision());
            }
            catch (Exception)
            {
                Message.printWarning(3, "DataUnits.getOutputFormat", "Unable to find data for units \"" + units_string + "\".  Using format \"" + format.ToString() + "\"");
            }
            return format;
        }

        /// <summary>
        /// Return the output precision for the units. </summary>
        /// <returns> The output precision for the units (the number of digits after the decimal point). </returns>
        public virtual int getOutputPrecision()
        {
            return __output_precision;
        }

        /// <summary>
        /// Initialize data members.
        /// </summary>
        private void initialize()
        {
            setAbbreviation("");
            setLongName("");

            // _dimension is initialized in its class

            __base_flag = 0;
            __output_precision = 2;
            __system = SYSTEM_UNKNOWN;
            __mult_factor = 0.0; // This will cause obvious errors to show up if units are not defined correctly.
            __add_factor = 0.0;
            __behavior_mask = 0;
            __source = "";
        }

        /// <summary>
        /// Return a DataUnits instance, given the units abbreviation.  A copy is NOT made. </summary>
        /// <returns> A DataUnits instance, given the units abbreviation. </returns>
        /// <param name="units_string"> The units abbreviation to look up. </param>
        /// <exception cref="Exception"> If there is a problem looking up the units abbreviation. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static DataUnits lookupUnits(String units_string) throws Exception
        public static DataUnits lookupUnits(string units_string)
        {
            string routine = "DataUnits.lookupUnits";

            // First see if the units are already in the list...

            int size = __units_Vector.Count;
            DataUnits pt = null;
            for (int i = 0; i < size; i++)
            {
                pt = __units_Vector[i];
                if (Message.isDebugOn)
                {
                    Message.printDebug(20, routine, "Comparing \"" + units_string + "\" and \"" + pt.getAbbreviation() + "\"");
                }
                if (units_string.Equals(pt.getAbbreviation(), StringComparison.OrdinalIgnoreCase))
                {
                    // The requested units match something that is in the list.  Return the matching DataUnits...
                    return pt;
                }
            }
            // Throw an exception...
            throw new Exception("\"" + units_string + "\" units not found");
        }

        /// <summary>
        /// Set the abbreviation string for the units. </summary>
        /// <param name="abbreviation"> Units abbreviation (e.g., "CFS"). </param>
        public virtual void setAbbreviation(string abbreviation)
        {
            if (string.ReferenceEquals(abbreviation, null))
            {
                return;
            }
            __abbreviation = abbreviation;
        }

        /// <summary>
        /// Set the long name for the units (e.g., "cubic feet per second"). </summary>
        /// <param name="long_name"> Long name for the units. </param>
        public virtual void setLongName(string long_name)
        {
            if (string.ReferenceEquals(long_name, null))
            {
                return;
            }
            __long_name = long_name;
        }
    }
}
