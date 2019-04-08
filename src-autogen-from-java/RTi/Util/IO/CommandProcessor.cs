using System.Collections.Generic;

// CommandProcessor - this interface is implemented by classes that can process string commands

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

	/// <summary>
	/// This interface is implemented by classes that can process string commands.
	/// The command processor should perform the following:
	/// <ol>
	/// <li>	Implement the methods in this interface, in order to to be able to
	/// process and interact with commands.</li>
	/// <li>	Maintain important data.  For example, the CommandProcessor should
	/// maintain the list of data objects relevant to a data set, as an
	/// intermediary between commands.   Depending on the complexity of the
	/// data set, the CommandProcessor may be very simple, or may need extensive
	/// knowledge about the data set.</li>
	/// </ol>
	/// </summary>
	public interface CommandProcessor
	{

	/// <summary>
	/// Return a list of commands being managed by the processor.  This allows generic utility code to
	/// further process the commands. </summary>
	/// <returns> the list of commands being managed by the processor. </returns>
	IList<Command> getCommands();

	/// <summary>
	/// Return a property given the property name (request).  Return null if not found. </summary>
	/// <param name="prop"> Name of property being requested. </param>
	/// <returns> the Property contents as a Prop instance. </returns>
	/// <exception cref="Exception"> if the property is not recognized. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Prop getProp(String prop) throws Exception;
	Prop getProp(string prop);

	/// <summary>
	/// Return a property's contents given the property name.  Return null if not found. </summary>
	/// <param name="prop"> Name of property being requested. </param>
	/// <returns> the Property contents as the data object. </returns>
	/// <exception cref="Exception"> if the property is not recognized. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getPropContents(String prop) throws Exception;
	object getPropContents(string prop);

	/// <summary>
	/// Process (run) a command. </summary>
	/// <param name="command"> Command to process. </param>
	/// <exception cref="CommandWarningException"> If there are warnings running the command. </exception>
	/// <exception cref="CommandException"> If there is an error running the command. </exception>
	// TODO SAM 2005-05-05 need to implement in a generic way
	//public void processCommands () throws CommandException;

	// TODO SAM 2007-02-16 How to pass back useful information when an exception
	/// <summary>
	/// Process a request.  This provides a generalized way for commands
	/// to call specialized methods in the command processor.
	/// This version allows properties to be passed as name/value pairs
	/// in a PropList.  It also allows multiple results to be returned.
	/// The parameter and result names/contents should be well documented by specific processors. </summary>
	/// <returns> a CommandProcessorRequestResultsBean containing the results of a request and other useful information. </returns>
	/// <param name="request"> A request keyword. </param>
	/// <param name="parameters"> Input to the request, as a PropList. </param>
	/// <exception cref="Exception"> if there is an error processing the request. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CommandProcessorRequestResultsBean processRequest(String request, PropList parameters) throws Exception;
	CommandProcessorRequestResultsBean processRequest(string request, PropList parameters);

	/// <summary>
	/// Set a property given the property. </summary>
	/// <param name="prop"> Property to set. </param>
	/// <exception cref="Exception"> if the property is not recognized or cannot be set. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setProp(Prop prop) throws Exception;
	void setProp(Prop prop);

	/// <summary>
	/// Set a property's contents given the property name. </summary>
	/// <param name="prop"> Property name. </param>
	/// <param name="contents"> Property contents. </param>
	/// <exception cref="Exception"> if the property is not recognized or cannot be set. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setPropContents(String prop, Object contents) throws Exception;
	void setPropContents(string prop, object contents);

	}

}