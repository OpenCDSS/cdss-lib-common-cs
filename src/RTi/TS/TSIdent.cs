using System;
using System.Collections.Generic;
using System.Text;

// TSIdent - class to store and manipulate a time series identifier, or TSID string

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
    using Message = Util.Message.Message;
    using StringUtil = Util.String.StringUtil;
    using TimeInterval = Util.Time.TimeInterval;

    /// <summary>
    /// The TSIdent class stores and manipulates a time series identifier, or
    /// TSID string. The TSID string consists of the following parts:
    /// <para>
    /// <pre>
    /// [LocationType:]Location[-SubLoc].Source.Type[-Subtype].Interval.Scenario[SeqID]~InputType~InputName
    /// </pre>
    /// <pre>
    /// [LocationType:]Location[-SubLoc].Source.Type[-Subtype].Interval.Scenario[SeqID]~DataStoreName
    /// </pre>
    /// </para>
    /// <para>
    /// TSID's as TSIdent objects or strings can be used to pass unique time series
    /// identifiers and are used throughout the time series package.  Some TS object
    /// data, including data type, are stored only in the TSIdent, to avoid redundant data.
    /// </para>
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @SuppressWarnings("serial") public class TSIdent implements Cloneable, java.io.Serializable, java.awt.datatransfer.Transferable
    [Serializable]
    public class TSIdent // : ICloneable
    {
        /// <summary>
        /// Mask indicating that no sub-location should be allowed (treat as part of the main location), used by setLocation().
        /// </summary>
        public const int NO_SUB_LOCATION = 0x1;

        /// <summary>
        /// Mask indicating that no sub-source should be allowed (treat as part of the main source), used by setSource().
        /// </summary>
        public const int NO_SUB_SOURCE = 0x2;

        /// <summary>
        /// Mask indicating that no sub-type should be allowed (treat as part of the main type), used by setType().
        /// </summary>
        public const int NO_SUB_TYPE = 0x4;

        /// <summary>
        /// Mask indicating that no validation of data should occur.  This is useful for storing
        /// identifier parts during manipulation (e.g., use wildcards, or specify parts of identifiers).
        /// </summary>
        public const int NO_VALIDATION = 0x8;

        /// <summary>
        /// Separator string for TSIdent string parts.
        /// </summary>
        public const string SEPARATOR = ".";

        /// <summary>
        /// Separator string between the TSIdent location type and start of the location ID.
        /// </summary>
        public const string LOC_TYPE_SEPARATOR = ":";

        /// <summary>
        /// Separator string for TSIdent location parts.
        /// </summary>
        public const string LOCATION_SEPARATOR = "-";

        /// <summary>
        /// Separator string for TSIdent source parts.
        /// </summary>
        public const string SOURCE_SEPARATOR = "-";

        /// <summary>
        /// Separator string for TSIdent data type parts.
        /// </summary>
        public const string TYPE_SEPARATOR = "-";

        /// <summary>
        /// Start of sequence identifier (old sequence number).
        /// </summary>
        public const string SEQUENCE_NUMBER_LEFT = "[";

        /// <summary>
        /// End of sequence identifier (old sequence number).
        /// </summary>
        public const string SEQUENCE_NUMBER_RIGHT = "]";

        /// <summary>
        /// Separator string for input type and datastore at end of TSID.
        /// </summary>
        public const string INPUT_SEPARATOR = "~";

        /// <summary>
        /// The quote can be used to surround TSID parts that have periods, so as to protect the part.
        /// </summary>
        public const string PERIOD_QUOTE = "'";

        /// <summary>
        /// The DataFlavor for transferring this specific class.
        /// </summary>
        //public static DataFlavor tsIdentFlavor = new DataFlavor(typeof(TSIdent), "TSIdent");

        // Data members...

        /// <summary>
        /// The whole identifier, including the input type.
        /// </summary>
        private string __identifier;

        /// <summary>
        /// A comment that can be used to describe the TSID, for example one-line TSTool software comment.
        /// </summary>
        private string __comment = "";

        /// <summary>
        /// A short alias for the time series identifier.
        /// </summary>
        private string __alias;

        /// <summary>
        /// The location (combining the main location and the sub-location).
        /// </summary>
        private string __full_location;

        /// <summary>
        /// Location type (optional).
        /// </summary>
        private string __locationType = "";

        /// <summary>
        /// The main location.
        /// </summary>
        private string __main_location;

        /// <summary>
        /// The sub-location (2nd+ parts of the location, using the LOCATION_SEPARATOR.
        /// </summary>
        private string __sub_location;

        /// <summary>
        /// The time series data source (combining the main source and the sub-source).
        /// </summary>
        private string __full_source;

        /// <summary>
        /// The main source.
        /// </summary>
        private string __main_source;

        /// <summary>
        /// The sub-source.
        /// </summary>
        private string __sub_source;

        /// <summary>
        /// The time series data type (combining the main and sub types).
        /// </summary>
        private string __full_type;

        /// <summary>
        /// The main data type.
        /// </summary>
        private string __main_type;

        /// <summary>
        /// The sub data type.
        /// </summary>
        private string __sub_type;

        /// <summary>
        /// The time series interval as a string.
        /// </summary>
        private string __interval_string;

        /// <summary>
        /// The base data interval.
        /// </summary>
        private int __interval_base;

        /// <summary>
        /// The data interval multiplier.
        /// </summary>
        private int __interval_mult;

        /// <summary>
        /// The time series scenario.
        /// </summary>
        private string __scenario;

        /// <summary>
        /// Identifier used for ensemble trace (e.g., if a list of time series is
        /// grouped as a set of traces in an ensemble, the trace ID can be the year that the trace starts).
        /// </summary>
        private string __sequenceID;

        /// <summary>
        /// Type of input (e.g., "DateValue", "RiversideDB")
        /// </summary>
        private string __input_type;

        /// <summary>
        /// Name of input (e.g., a file, data store, or database connection name).
        /// </summary>
        private string __input_name;

        /// <summary>
        /// Mask that controls behavior (e.g., how sub-fields are handled).
        /// </summary>
        private int __behavior_mask;

        /// <summary>
        /// Construct and initialize each part to empty strings.  Do handle sub-location and sub-source.
        /// </summary>
        public TSIdent()
        {
            init();
        }

        /// <summary>
        /// Construct using modifiers, indicating how to handle sub-location, etc. </summary>
        /// <param name="mask"> Behavior mask (see NO_SUB*). </param>
        public TSIdent(int mask)
        {
            init();
            setBehaviorMask(mask);
        }

        /// <summary>
        /// Construct using a full string identifier, which will be parsed and the
        /// individual parts of the identifier set. </summary>
        /// <param name="identifier"> Full string identifier (optionally, with right-most fields omitted). </param>
        /// <exception cref="if"> the identifier is invalid (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public TSIdent(String identifier) throws Exception
        public TSIdent(string identifier)
        {
            init();
            setIdentifier(identifier);
        }

        /// <summary>
        /// Construct using a full string identifier, which will be parsed and the
        /// individual parts of the identifier set. </summary>
        /// <param name="identifier"> Full string identifier (optionally, with right-most fields omitted). </param>
        /// <param name="mask"> Modifier to control behavior of TSIdent. </param>
        /// <exception cref="if"> the identifier is invalid (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public TSIdent(String identifier, int mask) throws Exception
        public TSIdent(string identifier, int mask)
        {
            init();
            setBehaviorMask(mask);
            setIdentifier(identifier);
        }

        /// <summary>
        /// Construct using each identifier part. </summary>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="full_type"> Full data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <exception cref="if"> the identifier is invalid (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public TSIdent(String full_location, String full_source, String full_type, String interval_string, String scenario) throws Exception
        public TSIdent(string full_location, string full_source, string full_type, string interval_string, string scenario)
        {
            init();
            setIdentifier(full_location, full_source, full_type, interval_string, scenario, "", "");
        }

        /// <summary>
        /// Construct using each identifier part. </summary>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="full_type"> Full data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <param name="mask"> Modifier to control behavior of TSIdent. </param>
        /// <exception cref="if"> the identifier is invalid (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public TSIdent(String full_location, String full_source, String full_type, String interval_string, String scenario, int mask) throws Exception
        public TSIdent(string full_location, string full_source, string full_type, string interval_string, string scenario, int mask)
        {
            init();
            setBehaviorMask(mask);
            setIdentifier(full_location, full_source, full_type, interval_string, scenario, "", "");
        }

        /// <summary>
        /// Construct using each identifier part. </summary>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="full_type"> Full data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <param name="input_type"> Input type. </param>
        /// <param name="input_name"> Input name. </param>
        /// <exception cref="if"> the identifier is invalid (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public TSIdent(String full_location, String full_source, String full_type, String interval_string, String scenario, String input_type, String input_name) throws Exception
        public TSIdent(string full_location, string full_source, string full_type, string interval_string, string scenario, string input_type, string input_name)
        {
            init();
            setIdentifier(full_location, full_source, full_type, interval_string, scenario, input_type, input_name);
        }

        /// <summary>
        /// Copy constructor. </summary>
        /// <param name="tsident"> TSIdent to copy. </param>
        /// <exception cref="if"> the identifier is invalid (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public TSIdent(TSIdent tsident) throws Exception
        public TSIdent(TSIdent tsident)
        { // Identifier will get set from its parts
            init();
            setAlias(tsident.getAlias());
            setBehaviorMask(tsident.getBehaviorMask());
            // Do not use the following!  It triggers infinite recursion!
            //setIdentifier ( tsident.__identifier );
            setLocationType(tsident.getLocationType());
            setIdentifier(tsident.getLocation(), tsident.getSource(), tsident.getType(), tsident.getInterval(), tsident.getScenario(), tsident.getSequenceID(), tsident.getInputType(), tsident.getInputName());
            __interval_base = tsident.getIntervalBase();
            __interval_mult = tsident.getIntervalMult();
        }

        /// <summary>
        /// Return the time series alias. </summary>
        /// <returns> The alias for the time series. </returns>
        public virtual string getAlias()
        {
            return __alias;
        }

        /// <summary>
        /// Return the behavior mask. </summary>
        /// <returns> The behavior mask. </returns>
        public virtual int getBehaviorMask()
        {
            return __behavior_mask;
        }

        /// <summary>
        /// Return the full identifier String. </summary>
        /// <returns> The full identifier string. </returns>
        public virtual string getIdentifier()
        {
            return ToString(false);
        }

        /// <summary>
        /// Return the full identifier String. </summary>
        /// <param name="include_input">  If true, the input type and name will be included in the
        /// identifier.  If false, only the 5-part TSID will be included. </param>
        /// <returns> The full identifier string. </returns>
        public virtual string getIdentifier(bool include_input)
        {
            return ToString(include_input);
        }

        /// <summary>
        /// Return the full identifier given the parts.  This method may be called
        /// internally.  Null fields are treated as empty strings. </summary>
        /// <returns> The full identifier string given the parts. </returns>
        /// <param name="locationType"> location type </param>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="full_type"> Full data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <param name="sequenceID"> sequence identifier for the time series (in an ensemble). </param>
        /// <param name="input_type"> Input type.  If blank, the input type will not be added. </param>
        /// <param name="input_name"> Input name.  If blank, the input name will not be added. </param>
        public virtual string getIdentifierFromParts(string locationType, string full_location, string full_source, string full_type, string interval_string, string scenario, string sequenceID, string input_type, string input_name)
        {
            StringBuilder full_identifier = new StringBuilder();

            if ((!string.ReferenceEquals(locationType, null)) && (locationType.Length > 0))
            {
                full_identifier.Append(locationType + LOC_TYPE_SEPARATOR);
            }
            if (!string.ReferenceEquals(full_location, null))
            {
                full_identifier.Append(full_location);
            }
            full_identifier.Append(SEPARATOR);
            if (!string.ReferenceEquals(full_source, null))
            {
                full_identifier.Append(full_source);
            }
            full_identifier.Append(SEPARATOR);
            if (!string.ReferenceEquals(full_type, null))
            {
                full_identifier.Append(full_type);
            }
            full_identifier.Append(SEPARATOR);
            if (!string.ReferenceEquals(interval_string, null))
            {
                full_identifier.Append(interval_string);
            }
            if ((!string.ReferenceEquals(scenario, null)) && (scenario.Length != 0))
            {
                full_identifier.Append(SEPARATOR);
                full_identifier.Append(scenario);
            }
            if ((!string.ReferenceEquals(sequenceID, null)) && (sequenceID.Length != 0))
            {
                full_identifier.Append(SEQUENCE_NUMBER_LEFT + sequenceID + SEQUENCE_NUMBER_RIGHT);
            }
            if ((!string.ReferenceEquals(input_type, null)) && (input_type.Length != 0))
            {
                full_identifier.Append("~" + input_type);
            }
            if ((!string.ReferenceEquals(input_name, null)) && (input_name.Length != 0))
            {
                full_identifier.Append("~" + input_name);
            }
            return full_identifier.ToString();
        }

        /// <summary>
        /// Return the input name. </summary>
        /// <returns> The input name. </returns>
        public virtual string getInputName()
        {
            return __input_name;
        }

        /// <summary>
        /// Return the input type. </summary>
        /// <returns> The input type. </returns>
        public virtual string getInputType()
        {
            return __input_type;
        }

        /// <summary>
        /// Return the full interval string. </summary>
        /// <returns> The full interval string. </returns>
        public virtual string getInterval()
        {
            return __interval_string;
        }

        /// <summary>
        /// Return the data interval base as an int. </summary>
        /// <returns> The data interval base (see TimeInterval.*). </returns>
        public virtual int getIntervalBase()
        {
            return __interval_base;
        }

        /// <summary>
        /// Return the data interval multiplier. </summary>
        /// <returns> The data interval multiplier. </returns>
        public virtual int getIntervalMult()
        {
            return __interval_mult;
        }

        /// <summary>
        /// Return the full location. </summary>
        /// <returns> The full location string. </returns>
        public virtual string getLocation()
        {
            return __full_location;
        }

        /// <summary>
	    /// Return the location type. </summary>
	    /// <returns> The location type string. </returns>
	    public virtual string getLocationType()
	    {
		    return __locationType;
	    }

        /// <summary>
        /// Return the scenario string. </summary>
        /// <returns> The scenario string. </returns>
        public virtual string getScenario()
        {
            return __scenario;
        }

        /// <summary>
        /// Return the sequence identifier for the time series. </summary>
        /// <returns> The sequence identifier for the time series.  This is meant to be used
        /// when an array of time series traces is maintained, for example in an ensemble. </returns>
        /// <returns> time series sequence identifier. </returns>
        public virtual string getSequenceID()
        {
            if (string.ReferenceEquals(__sequenceID, null))
            {
                return "";
            }
            else
            {
                return __sequenceID;
            }
        }

        /// <summary>
        /// Return the full source string. </summary>
        /// <returns> The full source string. </returns>
        public virtual string getSource()
        {
            return __full_source;
        }

        /// <summary>
        /// Return the data type. </summary>
        /// <returns> The full data type string. </returns>
        public virtual string getType()
        {
            return __full_type;
        }

        /// <summary>
        /// Initialize data members.
        /// </summary>
        private void init()
        {
            __behavior_mask = 0; // Default is to process sub-location and sub-source

            // Initialize to null strings so that there are not problems with the recursive logic...

            __identifier = null;
            __full_location = null;
            __main_location = null;
            __sub_location = null;
            __full_source = null;
            __main_source = null;
            __sub_source = null;
            __full_type = null;
            __main_type = null;
            __sub_type = null;
            __interval_string = null;
            __scenario = null;
            __sequenceID = null;
            __input_type = null;
            __input_name = null;

            setAlias("");

            // Initialize the overall identifier to an empty string...

            setFullIdentifier("");

            // Initialize the location components...

            setMainLocation("");

            setSubLocation("");

            // Initialize the source...

            setMainSource("");
            setSubSource("");

            // Initialize the data type...

            setType("");
            setMainType("");
            setSubType("");

            // Initialize the interval...

            __interval_base = TimeInterval.UNKNOWN;
            __interval_mult = 0;

            try
            {
                setInterval("");
            }
            catch (Exception)
            {
                // Can ignore here.
            }

            // Initialize the scenario...

            setScenario("");

            // Initialize the input...

            setInputType("");
            setInputName("");
        }

        /// <summary>
        /// Parse a TSIdent instance given a String representation of the identifier.
        /// The behavior flag is assumed to be zero. </summary>
        /// <returns> A TSIdent instance given a full identifier string. </returns>
        /// <param name="identifier"> Full identifier as string. </param>
        /// <exception cref="if"> an error occurs (usually a bad interval). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static TSIdent parseIdentifier(String identifier) throws Exception
        public static TSIdent parseIdentifier(string identifier)
        {
            return parseIdentifier(identifier, 0);
        }

        /// <summary>
        /// Parse a TSIdent instance given a String representation of the identifier. </summary>
        /// <returns> A TSIdent instance given a full identifier string. </returns>
        /// <param name="identifier"> Full identifier as string. </param>
        /// <param name="behavior_flag"> Behavior mask to use when creating instance. </param>
        /// <exception cref="if"> an error occurs (usually a bad interval). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static TSIdent parseIdentifier(String identifier, int behavior_flag) throws Exception
        public static TSIdent parseIdentifier(string identifier, int behavior_flag)
        {
            string routine = "TSIdent.parseIdentifier";
            int dl = 100;

            // Declare a TSIdent which we will fill and return...

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Declare TSIdent within this routine...");
            }
            TSIdent tsident = new TSIdent(behavior_flag);
            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "...done declaring TSIdent");
            }

            // First parse the datastore and input type information...

            string identifier0 = identifier;
            IList<string> list = StringUtil.breakStringList(identifier, "~", 0);
            int i, nlist1;
            if (list != null)
            {
                nlist1 = list.Count;
                // Reset to first part for checks below...
                identifier = list[0];
                if (nlist1 == 2)
                {
                    tsident.setInputType(list[1]);
                }
                else if (nlist1 >= 3)
                {
                    tsident.setInputType(list[1]);
                    // File name may have a ~ so find the second instance
                    // of ~ and use the remaining string...
                    int pos = identifier0.IndexOf("~", StringComparison.Ordinal);
                    if ((pos >= 0) && identifier0.Length > (pos + 1))
                    {
                        // Have something at the end...
                        string sub = identifier0.Substring(pos + 1);
                        pos = sub.IndexOf("~", StringComparison.Ordinal);
                        if ((pos >= 0) && (sub.Length > (pos + 1)))
                        {
                            // The rest is the file...
                            tsident.setInputName(sub.Substring(pos + 1));
                        }
                    }
                }
            }

            // Now parse the 5-part identifier...

            string full_location = "", full_source = "", interval_string = "", scenario = "", full_type = "";

            // TODO SAM 2013-06-14 Need to evaluate how to handle parts that include periods - single quotes around?
            // Figure out whether we are using the new or old conventions.  First
            // check to see if the number of fields is small.  Then check to see if
            // the data type and interval are combined.

            int posQuote = identifier.IndexOf("'", StringComparison.Ordinal);
            if (posQuote >= 0)
            {
                // Have at least one quote so assume TSID something like:
                // LocaId.Source.'DataType-some.parts.with.periods'.Interval
                list = parseIdentifier_SplitWithQuotes(identifier);
            }
            else
            {
                // No quote in TSID so do simple parse
                list = StringUtil.breakStringList(identifier, ".", 0);
            }
            nlist1 = list.Count;
            for (i = 0; i < nlist1; i++)
            {
                if (Message.isDebugOn)
                {
                    Message.printDebug(dl, routine, "TS ID list[" + i + "]:  \"" + list[i] + "\"");
                }
            }

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Full TS ID:  \"" + identifier + "\"");
            }

            // Parse out location and split the rest of the ID...
            //
            // FIXME SAM 2008-04-28 Don't think quotes are valid anymore given command parameter
            // use.  Evaluate removing quote handling.
            // FIXME SAM 2013-06-16 Actually, may need quotes for more new use cases where periods are in identifier parts
            // This field is allowed to be surrounded by quotes since some
            // locations cannot be identified by a simple string.  Allow
            // either ' or " to be used and bracket it.

            int locationTypeSepPos = -1;
            if ((identifier[0] != '\'') && (identifier[0] != '\"'))
            {
                // There is not a quoted location string so there is the possibility of having a location type
                // This logic looks at the full string.  If the separator is after a period, then the colon is being
                // detected other than at the start in the location
                locationTypeSepPos = identifier.IndexOf(LOC_TYPE_SEPARATOR, StringComparison.Ordinal);
                if (locationTypeSepPos > identifier.IndexOf(SEPARATOR, StringComparison.Ordinal))
                {
                    locationTypeSepPos = -1;
                }
            }
            string locationType = "";
            if (locationTypeSepPos >= 0)
            {
                // Have a location type so split out and set, then treat the rest of the identifier
                // as the location identifier for further processing
                locationType = identifier.Substring(0, locationTypeSepPos);
                identifier = identifier.Substring(locationTypeSepPos + 1);
            }
            if ((identifier[0] == '\'') || (identifier[0] == '\"'))
            {
                full_location = StringUtil.readToDelim(identifier.Substring(1), identifier[0]);
                // Get the 2nd+ fields...
                int posQuote2 = identifier.IndexOf("'", StringComparison.Ordinal);
                if (posQuote2 >= 0)
                {
                    // Have at least one quote so assume TSID something like:
                    // LocaId.Source.'DataType-some.parts.with.periods'.Interval
                    list = parseIdentifier_SplitWithQuotes(identifier.Substring(full_location.Length + 1));
                }
                else
                {
                    list = StringUtil.breakStringList(identifier.Substring(full_location.Length + 1), ".", 0);
                }
                nlist1 = list.Count;
            }
            else
            {
                int posQuote2 = identifier.IndexOf("'", StringComparison.Ordinal);
                if (posQuote2 >= 0)
                {
                    // Have at least one quote so assume TSID something like:
                    // LocaId.Source.'DataType-some.parts.with.periods'.Interval
                    list = parseIdentifier_SplitWithQuotes(identifier);
                }
                else
                {
                    list = StringUtil.breakStringList(identifier, ".", 0);
                }
                nlist1 = list.Count;
                if (nlist1 >= 1)
                {
                    full_location = list[0];
                }
            }
            // Data source...
            if (nlist1 >= 2)
            {
                full_source = list[1];
            }
            // Data type...
            if (nlist1 >= 3)
            {
                full_type = list[2];
            }
            // Data interval...
            string sequenceID = null;
            if (nlist1 >= 4)
            {
                interval_string = list[3];
                // If no scenario is used, the interval string may have the sequence ID on the end, so search for the [ and split the
                // sequence ID out of the interval string...
                int intervalStringIndex = interval_string.IndexOf(SEQUENCE_NUMBER_LEFT, StringComparison.Ordinal);
                // Get the sequence ID...
                if (intervalStringIndex >= 0)
                {
                    if (interval_string.EndsWith(SEQUENCE_NUMBER_RIGHT, StringComparison.Ordinal))
                    {
                        // Should be a properly-formed sequence ID, but need to remove the brackets...
                        sequenceID = interval_string.Substring(intervalStringIndex + 1, (interval_string.Length - 1) - (intervalStringIndex + 1)).Trim();
                    }
                    if (intervalStringIndex == 0)
                    {
                        // There is no interval, just the sequence ID (should not happen)...
                        interval_string = "";
                    }
                    else
                    {
                        interval_string = interval_string.Substring(0, intervalStringIndex);
                    }
                }
            }
            // Scenario...  It is possible that the scenario has delimiters
            // in it.  Therefore, we need to concatenate all the remaining
            // fields to compose the complete scenario...
            if (nlist1 >= 5)
            {
                StringBuilder buffer = new StringBuilder();
                buffer.Append(list[4]);
                for (i = 5; i < nlist1; i++)
                {
                    buffer.Append(".");
                    buffer.Append(list[i]);
                }
                scenario = buffer.ToString();
            }
            // The scenario may now have the sequence ID on the end, search for the [ and split out of the scenario...
            int index = scenario.IndexOf(SEQUENCE_NUMBER_LEFT, StringComparison.Ordinal);
            // Get the sequence ID...
            if (index >= 0)
            {
                if (scenario.EndsWith(SEQUENCE_NUMBER_RIGHT, StringComparison.Ordinal))
                {
                    // Should be a properly-formed sequence ID...
                    sequenceID = scenario.Substring(index + 1, (scenario.Length - 1) - (index + 1)).Trim();
                }
                if (index == 0)
                {
                    // There is no scenario, just the sequence ID...
                    scenario = "";
                }
                else
                {
                    scenario = scenario.Substring(0, index);
                }
            }
            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "After split: fullloc=\"" + full_location + "\" fullsrc=\"" + full_source + "\" type=\"" + full_type + "\" int=\"" + interval_string + "\" scen=\"" + scenario + "\"");
            }

            // Now set the identifier component parts...

            tsident.setLocationType(locationType);
            tsident.setLocation(full_location);
            tsident.setSource(full_source);
            tsident.setType(full_type);
            tsident.setInterval(interval_string);
            tsident.setScenario(scenario);
            tsident.setSequenceID(sequenceID);

            // Return the TSIdent object for use elsewhere...

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Returning local TSIdent...");
            }
            return tsident;
        }

        /// <summary>
        /// Parse a TSID that has quoted part with periods in one or more parts. </summary>
        /// <param name="identifier"> TSID main part (no ~). </param>
        /// <returns> list of parts for TSID </returns>
        private static IList<string> parseIdentifier_SplitWithQuotes(string identifier)
        {
            // Process by getting one token at a time.
            // -tokens are between periods
            // -if first character of part is single quote, get to the next single quote
            IList<string> parts = new List<string>();
            bool inPart = true; // should always have a part at the front
            bool inQuote = false;
            char c;
            StringBuilder b = new StringBuilder();
            int ilen = identifier.Length;
            // Use debug messages for now but code seems to be OK
            // - remove debug messages later.
            for (int i = 0; i < ilen; i++)
            {
                c = identifier[i];
                if (Message.isDebugOn)
                {
                    Message.printDebug(1, "", "Character is: " + c);
                }
                if (c == '.')
                {
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(1, "", "Found period");
                    }
                    if (inQuote)
                    {
                        // In a quote so just keep adding characters
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(1, "", "In quote");
                        }
                        b.Append(c);
                    }
                    else
                    {
                        // Not in quote
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(1, "", "Not in quote");
                        }
                        if (inPart)
                        {
                            // Between periods.  Already in part so end it without adding period
                            if (Message.isDebugOn)
                            {
                                Message.printDebug(1, "", "In part, ending part");
                            }
                            parts.Add(b.ToString());
                            b.Length = 0;
                            // Will be in part at top of loop because current period will be skipped
                            // - but if last period treat the following part as empty string
                            if (i == (ilen - 1))
                            {
                                // Add an empty string
                                parts.Add("");
                            }
                            else
                            {
                                // Keep processing
                                // Set to not be in part
                                inPart = false;
                                --i; // Re-process period to trigger in a part in next iteration
                            }
                        }
                        else
                        {
                            // Was not in a part so start it
                            if (Message.isDebugOn)
                            {
                                Message.printDebug(1, "", "Not in part, starting part");
                            }
                            inPart = true;
                            // Don't add period to part.
                        }
                    }
                }
                else if (c == '\'')
                {
                    // Found a quote, which will surround a point, as in:  .'some.part'.
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(1, "", "Found quote");
                    }
                    if (inQuote)
                    {
                        // At the end of the quoted part.
                        // Always include the quote in the part
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(1, "", "In quote, ending quote");
                        }
                        b.Append(c);
                        parts.Add(b.ToString());
                        b.Length = 0;
                        // Next period immediately following will cause next part to be added, even if period at end of string
                        inQuote = false;
                        inPart = false;
                    }
                    else
                    {
                        // Need to start a part
                        if (Message.isDebugOn)
                        {
                            Message.printDebug(1, "", "Not in quote, starting quote");
                        }
                        b.Append(c); // Keep the quote
                        inQuote = true;
                    }
                }
                else
                {
                    // Character to add to part
                    b.Append(c);
                    if (i == (ilen - 1))
                    {
                        // Last part
                        parts.Add(b.ToString());
                    }
                }
            }
            if (Message.isDebugOn)
            {
                foreach (string s in parts)
                {
                    Message.printDebug(1, "xxx", "TSID part is \"" + s + "\"");
                }
            }
            return parts;
        }

        /// <summary>
        /// Set the time series alias. </summary>
        /// <param name="alias"> Alias for the time series. </param>
        public virtual void setAlias(string alias)
        {
            if (!string.ReferenceEquals(alias, null))
            {
                __alias = alias;
            }
        }

        /// <summary>
        /// Set the behavior mask.  The behavior mask controls how identifier sub-parts are
        /// joined into the full identifier.   Currently this routine does a full reset (not bit-wise). </summary>
        /// <param name="behavior_mask"> Behavior mask that controls how sub-fields are handled. </param>
        public virtual void setBehaviorMask(int behavior_mask)
        {
            __behavior_mask = behavior_mask;
        }

        /// <summary>
        /// Set the full identifier (this does not result in a parse).  It is normally only
        /// called from within this class. </summary>
        /// <param name="full_identifier"> Full identifier string. </param>
        private void setFullIdentifier(string full_identifier)
        {
            if (string.ReferenceEquals(full_identifier, null))
            {
                return;
            }
            __identifier = full_identifier;
            // DO NOT call setIdentifier() from here!
        }

        /// <summary>
        /// Set the full location (this does not result in a parse).  It is normally only
        /// called from within this class. </summary>
        /// <param name="full_location"> Full location string. </param>
        private void setFullLocation(string full_location)
        {
            if (string.ReferenceEquals(full_location, null))
            {
                return;
            }
            __full_location = full_location;
            // DO NOT call setIdentifier() from here!
        }

        /// <summary>
        /// Set the full source (this does not result in a parse).  It is normally only
        /// called from within this class. </summary>
        /// <param name="full_source"> Full source string. </param>
        private void setFullSource(string full_source)
        {
            if (string.ReferenceEquals(full_source, null))
            {
                return;
            }
            __full_source = full_source;
            // DO NOT call setIdentifier() from here!
        }

        /// <summary>
        /// Set the full data type (this does not result in a parse).  It is normally only
        /// called from within this class. </summary>
        /// <param name="full_type"> Full data type string. </param>
        private void setFullType(string full_type)
        {
            if (string.ReferenceEquals(full_type, null))
            {
                return;
            }
            __full_type = full_type;
            // DO NOT call setIdentifier() from here!
        }

        /// <summary>
        /// Set the full identifier from its parts.
        /// </summary>
        public virtual void setIdentifier()
        {
            string routine = "TSIdent.setIdentifier(void)";
            int dl = 100;

            // Assume that all the individual set routines have handled the
            // __behavior_mask accordingly and therefore we can just concatenate
            // strings here...

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Setting full identifier from parts: \"" + __full_location + "." + __full_source + "." + __full_type + "." + __interval_string + "." + __scenario + "~" + __input_type + "~" + __input_name);
            }

            string full_identifier;
            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Calling getIdentifierFromParts...");
            }
            full_identifier = getIdentifierFromParts(__locationType, __full_location, __full_source, __full_type, __interval_string, __scenario, __sequenceID, __input_type, __input_name);
            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "...successfully called getIdentifierFromParts...");
            }

            setFullIdentifier(full_identifier);

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "ID...");
                Message.printDebug(dl, routine, "\"" + __identifier + "\"");
            }
        }

        /// <summary>
        /// Set the identifier by parsing the given string. </summary>
        /// <param name="identifier"> Full identifier string. </param>
        /// <exception cref="if"> the identifier cannot be set (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setIdentifier(String identifier) throws Exception
        public virtual void setIdentifier(string identifier)
        {
            string routine = "TSIdent.setIdentifier";
            int dl = 100;

            if (string.ReferenceEquals(identifier, null))
            {
                return;
            }

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Trying to set identifier to \"" + identifier + "\"");
            }

            if (identifier.Length == 0)
            {
                // Cannot parse the identifier because doing so would result in
                // an infinite loop.  If this routine is being called with an
                // empty string, it is a mistake.  The initialization code will
                // call setFullIdentifier() directly.
                if (Message.isDebugOn)
                {
                    Message.printDebug(dl, routine, "Identifier string is empty, not processing!");
                }
                return;
            }

            // Parse the identifier using the public static function to create a temporary identifier object...

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Done declaring temp TSIdent.");
                Message.printDebug(dl, routine, "Parsing identifier...");
            }
            TSIdent tsident = parseIdentifier(identifier, __behavior_mask);
            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "...back from parsing identifier");
            }

            // Now copy the temporary copy into this instance...

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Setting the individual parts...");
            }
            setLocationType(tsident.getLocationType());
            setLocation(tsident.getLocation());
            setSource(tsident.getSource());
            setType(tsident.getType());
            setInterval(tsident.getInterval());
            setScenario(tsident.getScenario());
            setSequenceID(tsident.getSequenceID());
            setInputType(tsident.getInputType());
            setInputName(tsident.getInputName());
            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "... done setting the individual parts");
            }
        }

        /// <summary>
        /// Set the identifier given the main parts, but no sequence ID, input type, or input name. </summary>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="full_type"> Full data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <exception cref="if"> the identifier cannot be set (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setIdentifier(String full_location, String full_source, String full_type, String interval_string, String scenario) throws Exception
        public virtual void setIdentifier(string full_location, string full_source, string full_type, string interval_string, string scenario)
        {
            setLocation(full_location);
            setSource(full_source);
            setType(full_type);
            setInterval(interval_string);
            setScenario(scenario);
        }

        /// <summary>
        /// Set the identifier given the parts, but not including the sequence ID. </summary>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="type"> Data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <param name="input_type"> Input type. </param>
        /// <param name="input_name"> Input name. </param>
        /// <exception cref="if"> the identifier cannot be set (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setIdentifier(String full_location, String full_source, String type, String interval_string, String scenario, String input_type, String input_name) throws Exception
        public virtual void setIdentifier(string full_location, string full_source, string type, string interval_string, string scenario, string input_type, string input_name)
        {
            setLocation(full_location);
            setSource(full_source);
            setType(type);
            setInterval(interval_string);
            setScenario(scenario);
            setInputType(input_type);
            setInputName(input_name);
        }

        /// <summary>
        /// Set the identifier given the parts, including sequence ID. </summary>
        /// <param name="full_location"> Full location string. </param>
        /// <param name="full_source"> Full source string. </param>
        /// <param name="type"> Data type. </param>
        /// <param name="interval_string"> Data interval string. </param>
        /// <param name="scenario"> Scenario string. </param>
        /// <param name="sequenceID"> sequence identifier (for time series in ensemble). </param>
        /// <param name="input_type"> Input type. </param>
        /// <param name="input_name"> Input name. </param>
        /// <exception cref="if"> the identifier cannot be set (usually the interval is incorrect). </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setIdentifier(String full_location, String full_source, String type, String interval_string, String scenario, String sequenceID, String input_type, String input_name) throws Exception
        public virtual void setIdentifier(string full_location, string full_source, string type, string interval_string, string scenario, string sequenceID, string input_type, string input_name)
        {
            setLocation(full_location);
            setSource(full_source);
            setType(type);
            setInterval(interval_string);
            setScenario(scenario);
            setSequenceID(sequenceID);
            setInputType(input_type);
            setInputName(input_name);
        }

        /// <summary>
        /// Set the input name.
        /// The input name.
        /// </summary>
        public virtual void setInputName(string input_name)
        {
            if (!string.ReferenceEquals(input_name, null))
            {
                __input_name = input_name;
            }
        }

        /// <summary>
        /// Set the input type.
        /// The input type.
        /// </summary>
        public virtual void setInputType(string input_type)
        {
            if (!string.ReferenceEquals(input_type, null))
            {
                __input_type = input_type;
            }
        }

        /// <summary>
        /// Set the interval given the interval string. </summary>
        /// <param name="interval_string"> Data interval string. </param>
        /// <exception cref="if"> there is an error parsing the interval string. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setInterval(String interval_string) throws Exception
        public virtual void setInterval(string interval_string)
        {
            string routine = "TSIdent.setInterval(String)";
            int dl = 100;
            TimeInterval tsinterval = null;

            if (string.ReferenceEquals(interval_string, null))
            {
                return;
            }

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Setting interval to \"" + interval_string + "\"");
            }

            if (!interval_string.Equals("*") && interval_string.Length > 0)
            {
                // First split the string into its base and multiplier...

                if ((__behavior_mask & NO_VALIDATION) == 0)
                {
                    try
                    {
                        tsinterval = TimeInterval.parseInterval(interval_string);
                    }
                    catch (Exception)
                    {
                        // Not validating so let this pass...
                    }
                }
                else
                {
                    // Want to validate so let this throw an exception...
                    tsinterval = TimeInterval.parseInterval(interval_string);
                }

                // Now set the base and multiplier...
                if (tsinterval != null)
                {
                    __interval_base = tsinterval.getBase();
                    __interval_mult = tsinterval.getMultiplier();
                    if (Message.isDebugOn)
                    {
                        Message.printDebug(dl, routine, "Setting interval base to " + __interval_base + " mult: " + __interval_mult);
                    }
                }
            }
            // Else, don't do anything (leave as zero initialized values).

            // Now set the interval string.  Use the given interval base string
            // because we need to preserve existing file names, etc.

            setIntervalString(interval_string);
            setIdentifier();
        }

        /// <summary>
        /// Set the interval string.  This is normally only called from this class. </summary>
        /// <param name="interval_string"> Interval string. </param>
        private void setIntervalString(string interval_string)
        {
            if (!string.ReferenceEquals(interval_string, null))
            {
                __interval_string = interval_string;
            }
        }

        /// <summary>
        /// Set the full location from its parts.  This method is generally called from
        /// setMainLocation() and setSubLocation() methods to reset __full_location.
        /// </summary>
        public virtual void setLocation()
        {
            string routine = "TSIdent.setLocation";
            int dl = 100;

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Resetting full location from parts...");
            }
            if ((__behavior_mask & NO_SUB_LOCATION) != 0)
            {
                // Just use the main location as the full location...
                if (!string.ReferenceEquals(__main_location, null))
                {
                    // There should always be a main location after the object is initialized...
                    setFullLocation(__main_location);
                }
            }
            else
            {
                // Concatenate the main and sub-locations to get the full location.
                StringBuilder full_location = new StringBuilder();
                // We may want to check for __main_location[] also...
                if (!string.ReferenceEquals(__main_location, null))
                {
                    // This should always be the case after the object is initialized...
                    full_location.Append(__main_location);
                    if (!string.ReferenceEquals(__sub_location, null))
                    {
                        // We only want to add the sublocation if it is
                        // not an empty string (it will be an empty
                        // string after the object is initialized).
                        if (__sub_location.Length > 0)
                        {
                            // Have a sub_location so append it to the main location...
                            full_location.Append(LOCATION_SEPARATOR);
                            full_location.Append(__sub_location);
                        }
                    }
                    setFullLocation(full_location.ToString());
                }
            }
            // Now reset the full identifier...
            setIdentifier();
        }

        /// <summary>
        /// Set the full location from its full string. </summary>
        /// <param name="full_location"> The full location string. </param>
        public virtual void setLocation(string full_location)
        {
            string routine = "TSIdent.setLocation(String)";
            int dl = 100;

            if (string.ReferenceEquals(full_location, null))
            {
                return;
            }

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Trying to set location to \"" + full_location + "\"");
            }

            if ((__behavior_mask & NO_SUB_LOCATION) != 0)
            {
                // The entire string passed in is used for the main location...
                setMainLocation(full_location);
            }
            else
            {
                // Need to split the location into main and sub-location...
                IList<string> list;
                StringBuilder sub_location = new StringBuilder();
                int nlist;
                list = StringUtil.breakStringList(full_location, LOCATION_SEPARATOR, 0);
                nlist = list.Count;
                if (nlist >= 1)
                {
                    // Set the main location...
                    setMainLocation(list[0]);
                }
                if (nlist >= 2)
                {
                    // Now set the sub-location.  This allows for multiple
                    // delimited parts (everything after the first delimiter is treated as the sublocation).
                    int iend = nlist - 1;
                    for (int i = 1; i <= iend; i++)
                    {
                        if (i != 1)
                        {
                            sub_location.Append(LOCATION_SEPARATOR);
                        }
                        sub_location.Append(list[i]);
                    }
                    setSubLocation(sub_location.ToString());
                }
                else
                {
                    // Since only setting the main location need to set the sub-location to an empty string...
                    setSubLocation("");
                }
            }
        }

        /// <summary>
        /// Set the location type. </summary>
        /// <param name="locationType"> location type. </param>
        public virtual void setLocationType(string locationType)
        {
            if (string.ReferenceEquals(locationType, null))
            {
                return;
            }
            __locationType = locationType;
            setIdentifier();
        }

        /// <summary>
        /// Set the main location string (and reset the full location). </summary>
        /// <param name="main_location"> The main location string. </param>
        public virtual void setMainLocation(string main_location)
        {
            if (string.ReferenceEquals(main_location, null))
            {
                return;
            }
            __main_location = main_location;
            setLocation();
        }

        /// <summary>
        /// Set the main source string (and reset the full source). </summary>
        /// <param name="main_source"> The main source string. </param>
        public virtual void setMainSource(string main_source)
        {
            if (string.ReferenceEquals(main_source, null))
            {
                return;
            }
            __main_source = main_source;
            setSource();
        }

        /// <summary>
        /// Set the main data type string (and reset the full type). </summary>
        /// <param name="main_type"> The main data type string. </param>
        public virtual void setMainType(string main_type)
        {
            if (string.ReferenceEquals(main_type, null))
            {
                return;
            }
            __main_type = main_type;
            setType();
        }

        /// <summary>
        /// Set the full source from its parts.
        /// </summary>
        public virtual void setSource()
        {
            if ((__behavior_mask & NO_SUB_SOURCE) != 0)
            {
                // Just use the main source as the full source...
                if (!string.ReferenceEquals(__main_source, null))
                {
                    // There should always be a main source after the object is initialized...
                    setFullSource(__main_source);
                }
            }
            else
            {
                // Concatenate the main and sub-sources to get the full source.
                StringBuilder full_source = new StringBuilder();
                if (!string.ReferenceEquals(__main_source, null))
                {
                    // We only want to add the subsource if it is not an
                    // empty string (it will be an empty string after the
                    // object is initialized).
                    full_source.Append(__main_source);
                    if (!string.ReferenceEquals(__sub_source, null))
                    {
                        // We have sub_source so append it to the main source...
                        // We have a sub_source so append it to the main source...
                        if (__sub_source.Length > 0)
                        {
                            full_source.Append(SOURCE_SEPARATOR);
                            full_source.Append(__sub_source);
                        }
                    }
                    setFullSource(full_source.ToString());
                }
            }
            // Now reset the full identifier...
            setIdentifier();
        }

        /// <summary>
        /// Set the full source from a full string. </summary>
        /// <param name="source"> The full source string. </param>
        public virtual void setSource(string source)
        {
            if (string.ReferenceEquals(source, null))
            {
                return;
            }

            if (source.Equals(""))
            {
                setMainSource("");
                setSubSource("");
            }
            else if ((__behavior_mask & NO_SUB_SOURCE) != 0)
            {
                // The entire string passed in is used for the main source...
                setMainSource(source);
            }
            else
            {
                // Need to split the source into main and sub-source...
                IList<string> list;
                StringBuilder sub_source = new StringBuilder();
                int nlist;
                list = StringUtil.breakStringList(source, SOURCE_SEPARATOR, 0);
                nlist = list.Count;
                if (nlist >= 1)
                {
                    // Set the main source...
                    setMainSource(list[0]);
                }
                if (nlist >= 2)
                {
                    // Now set the sub-source...
                    int iend = nlist - 1;
                    for (int i = 1; i <= iend; i++)
                    {
                        sub_source.Append(list[i]);
                        if (i != iend)
                        {
                            sub_source.Append(SOURCE_SEPARATOR);
                        }
                    }
                    setSubSource(sub_source.ToString());
                }
                else
                {
                    // Since we are only setting the main location we
                    // need to set the sub-location to an empty string...
                    setSubSource("");
                }
            }
        }

        /// <summary>
        /// Set the scenario string. </summary>
        /// <param name="scenario"> The scenario string. </param>
        public virtual void setScenario(string scenario)
        {
            if (string.ReferenceEquals(scenario, null))
            {
                return;
            }
            __scenario = scenario;
            setIdentifier();
        }

        /// <summary>
        /// Set the sequence identifier, for example when the time series is part of an ensemble. </summary>
        /// <param name="sequenceID"> sequence identifier for the time series. </param>
        public virtual void setSequenceID(string sequenceID)
        {
            __sequenceID = sequenceID;
            setIdentifier();
        }

        /// <summary>
        /// Set the sub-location string (and reset the full location). </summary>
        /// <param name="sub_location"> The sub-location string. </param>
        public virtual void setSubLocation(string sub_location)
        {
            if (string.ReferenceEquals(sub_location, null))
            {
                return;
            }
            __sub_location = sub_location;
            setLocation();
        }

        /// <summary>
        /// Set the sub-source string (and reset the full source). </summary>
        /// <param name="sub_source"> The sub-source string. </param>
        public virtual void setSubSource(string sub_source)
        {
            if (string.ReferenceEquals(sub_source, null))
            {
                return;
            }
            __sub_source = sub_source;
            setSource();
        }

        /// <summary>
        /// Set the sub-type string (and reset the full data type). </summary>
        /// <param name="sub_type"> The sub-type string. </param>
        public virtual void setSubType(string sub_type)
        {
            if (string.ReferenceEquals(sub_type, null))
            {
                return;
            }
            __sub_type = sub_type;
            setType();
        }

        /// <summary>
        /// Set the full data type from its parts.  This method is generally called from
        /// setMainType() and setSubType() methods to reset __full_type.
        /// </summary>
        public virtual void setType()
        {
            string routine = "TSIdent.setType";
            int dl = 100;

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Resetting full type from parts...");
            }
            if ((__behavior_mask & NO_SUB_TYPE) != 0)
            {
                // Just use the main type as the full type...
                if (!string.ReferenceEquals(__main_type, null))
                {
                    // There should always be a main type after the object is initialized...
                    setFullType(__main_type);
                }
            }
            else
            {
                // Concatenate the main and sub-types to get the full type.
                StringBuilder full_type = new StringBuilder();
                if (!string.ReferenceEquals(__main_type, null))
                {
                    // This should always be the case after the object is initialized...
                    full_type.Append(__main_type);
                    if (!string.ReferenceEquals(__sub_type, null))
                    {
                        // We only want to add the subtype if it is
                        // not an empty string (it will be an empty
                        // string after the object is initialized).
                        if (__sub_type.Length > 0)
                        {
                            // We have a sub type so append it to the main type...
                            full_type.Append(TYPE_SEPARATOR);
                            full_type.Append(__sub_type);
                        }
                    }
                    setFullType(full_type.ToString());
                }
            }
            // Now reset the full identifier...
            setIdentifier();
        }

        /// <summary>
        /// Set the full data type from its full string. </summary>
        /// <param name="type"> The full data type string. </param>
        public virtual void setType(string type)
        {
            string routine = "TSIdent.setType";
            int dl = 100;

            if (string.ReferenceEquals(type, null))
            {
                return;
            }

            if (Message.isDebugOn)
            {
                Message.printDebug(dl, routine, "Trying to set data type to \"" + type + "\"");
            }

            if ((__behavior_mask & NO_SUB_TYPE) != 0)
            {
                // The entire string passed in is used for the main data type...
                setMainType(type);
            }
            else
            {
                // Need to split the data type into main and sub-location...
                IList<string> list;
                StringBuilder sub_type = new StringBuilder();
                int nlist;
                list = StringUtil.breakStringList(type, TYPE_SEPARATOR, 0);
                nlist = list.Count;
                if (nlist >= 1)
                {
                    // Set the main type...
                    setMainType(list[0]);
                }
                if (nlist >= 2)
                {
                    // Now set the sub-type...
                    int iend = nlist - 1;
                    for (int i = 1; i <= iend; i++)
                    {
                        sub_type.Append(list[i]);
                        if (i != iend)
                        {
                            sub_type.Append(TYPE_SEPARATOR);
                        }
                    }
                    setSubType(sub_type.ToString());
                }
                else
                {
                    // Since we are only setting the main type we
                    // need to set the sub-type to an empty string...
                    setSubType("");
                }
            }
        }

        /// <summary>
        /// Return a string representation of the TSIdent. </summary>
        /// <returns> A string representation of the TSIdent. </returns>
        /// <param name="include_input"> If true, the input type and name are included in the
        /// identifier.  If false, the 5-part TSID is returned. </param>
        public virtual string ToString(bool include_input)
        {
            string locationType = "";
            string scenario = "";
            string sequenceID = "";
            string input_type = "";
            string input_name = "";
            if ((!string.ReferenceEquals(__locationType, null)) && (__locationType.Length > 0))
            {
                locationType = __locationType + LOC_TYPE_SEPARATOR;
            }
            if ((!string.ReferenceEquals(__scenario, null)) && (__scenario.Length > 0))
            {
                // Add the scenario if it is not blank...
                scenario = "." + __scenario;
            }
            if ((!string.ReferenceEquals(__sequenceID, null)) && (__sequenceID.Length > 0))
            {
                // Add the sequence ID if it is not blank...
                sequenceID = SEQUENCE_NUMBER_LEFT + __sequenceID + SEQUENCE_NUMBER_RIGHT;
            }
            if (include_input)
            {
                if ((!string.ReferenceEquals(__input_type, null)) && (__input_type.Length > 0))
                {
                    input_type = "~" + __input_type;
                }
                if ((!string.ReferenceEquals(__input_name, null)) && (__input_name.Length > 0))
                {
                    input_name = "~" + __input_name;
                }
            }
            return (locationType + __full_location + "." + __full_source + "." + __full_type + "." + __interval_string + scenario + sequenceID + input_type + input_name);
        }

        /// <summary>
        /// Clone the object.  The Object base class clone() method is called, which clones
        /// all the TSIdent primitive data.  The result is a complete deep copy.
        /// </summary>
        //public virtual object Clone()
        //{
        //    try
        //    {
        //        TSIdent tsident = (TSIdent)base.Clone();
        //        return tsident;
        //    }
        //    catch (CloneNotSupportedException)
        //    {
        //        // Should not happen because everything is cloneable.
        //        throw new InternalError();
        //    }
        //}
    }
}
