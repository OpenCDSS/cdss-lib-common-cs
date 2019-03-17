using System;

// InvalidConfigurationException - exception thrown when an error parsing an XML configuration file

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

//------------------------------------------------------------------------------------
// InvaildConfigurationException - class to build an exception handler that gets 
// thrown when parsing an XML configuration file encounters an invalid configuration.
//------------------------------------------------------------------------------------
// History:
//
//      2006-06-16      Scott Townsend, RTi     Create initial version of this
//                                              Adapter exception class. This
//						class is necessary since it does
//						handle specific error messages
//						and specific config file clean-up.
//						This clean-up includes closing any
//						open connections to the config
//						file or server which holds the file.
//------------------------------------------------------------------------------------
// Endheader

namespace RTi.DataServices.Adapter
{

	/// <summary>
	/// Exception which occurs when the ConfigurationFile object can not parse the 
	/// given XML configuration file.
	/// 
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class InvalidConfigurationException extends Exception
	public class InvalidConfigurationException : Exception
	{

	/// <summary>
	/// private Adapter instance
	/// </summary>
	private ConfigurationFile __IOConfiguration = null;

	/// <summary>
	/// <para>Default constructor to build this exception.</para>
	/// </summary>
	public InvalidConfigurationException() : base("An Error occured: Parse error happened when " + "parsing the Data Services configuration file")
	{
		if (__IOConfiguration != null)
		{
			//__IOConfiguration.finalize();
		}
	}

	/// <summary>
	/// <para>Constructor to build this exception with a developer defined error message.</para> </summary>
	/// <param name="exceptionMessage"> a String holding the developer defined error message. </param>
	public InvalidConfigurationException(string exceptionMessage) : base(exceptionMessage)
	{
		if (__IOConfiguration != null)
		{
			//__IOConfiguration.finalize();
		}
	}

	/// <summary>
	/// <para>Constructor to build this exception with the ConfigurationFile specified.</para> </summary>
	/// <param name="DSConfiguration"> a ConfigurationFile object that will be finalized when
	/// this constructor is called. </param>
	public InvalidConfigurationException(ConfigurationFile DSConfiguration) : base("An Error occured: Parse error happened when " + "parsing the Data Services configuration file")
	{

		// Set the ConfigurationFile
		__IOConfiguration = DSConfiguration;

		if (__IOConfiguration != null)
		{
			//__IOConfiguration.finalize();
		}
	}

	/// <summary>
	/// <para>Constructor to build this exception with a developer defined error message
	/// and the ConfigurationFile.</para> </summary>
	/// <param name="exceptionMessage"> a String holding the developer defined error message. </param>
	/// <param name="DSConfiguration"> a ConfigurationFile object that will be finalized when
	/// this constructor is called. </param>
	public InvalidConfigurationException(ConfigurationFile DSConfiguration, string exceptionMessage) : base(exceptionMessage)
	{

		// Set the ConfigurationFile
		__IOConfiguration = DSConfiguration;

		if (__IOConfiguration != null)
		{
			//__IOConfiguration.finalize();
		}
	}

	/// <summary>
	/// A get accessor method to get the ConfigurationFile specified by the Exception. </summary>
	/// <returns> The ConfigurationFile utilized in theis Exception. </returns>
	public virtual ConfigurationFile getConfigurationFile()
	{
		return __IOConfiguration;
	}
	}

}