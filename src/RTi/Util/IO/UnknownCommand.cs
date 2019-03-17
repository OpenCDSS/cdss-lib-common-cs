// UnknownCommand - this class is essentially the same as GenericCommand

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
	using MessageUtil = RTi.Util.Message.MessageUtil;

	/// <summary>
	/// This class is essentially the same as GenericCommand.  However, whereas GenericCommand might
	/// be used for a known command, this UnknownCommand class can be used in cases where a command is
	/// not known but needs to be managed.  The GenericCommand_JDialog is used to edit the command.
	/// </summary>
	public class UnknownCommand : AbstractCommand
	{

	/// <summary>
	/// Default constructor for a command.
	/// </summary>
	public UnknownCommand()
	{
	}

	/// <summary>
	/// Check the command parameter for valid values, combination, etc. </summary>
	/// <param name="parameters"> The parameters for the command. </param>
	/// <param name="command_tag"> an indicator to be used when printing messages, to allow a
	/// cross-reference to the original commands. </param>
	/// <param name="warning_level"> The warning level to use when printing parse warnings
	/// (recommended is 2 for initialization, and 1 for interactive command editor dialogs). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void checkCommandParameters(PropList parameters, String command_tag, int warning_level) throws InvalidCommandParameterException
	public override void checkCommandParameters(PropList parameters, string command_tag, int warning_level)
	{
	}

	/// <summary>
	/// Edit a command instance.  The instance may be a newly created command or one
	/// that has been created previously and is now being re-edited. </summary>
	/// <returns> the Command instance that is created and edited, or null if the edit was cancelled. </returns>
	/// <param name="parent"> Parent JFrame on which the model command editor dialog will be shown. </param>
	public override bool editCommand(JFrame parent)
	{ // Use the generic command editor...
		return (new GenericCommand_JDialog(parent, this)).ok();
	}

	/// <summary>
	/// Initialize the command by parsing the command and indicating warnings. </summary>
	/// <param name="command"> A string command to parse. </param>
	/// <param name="full_initialization"> If true, the command string will be parsed and
	/// checked for errors.  If false, a blank command will be initialized (e.g.,
	/// suitable for creating a new command instance before editing in the command editor). </param>
	/// <exception cref="InvalidCommandSyntaxException"> if during parsing the command is determined to have invalid syntax. </exception>
	/// <exception cref="InvalidCommandParameterException"> if during parsing the command
	/// parameters are determined to be invalid. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void initializeCommand(String command, CommandProcessor processor, boolean full_initialization) throws InvalidCommandSyntaxException, InvalidCommandParameterException
	public override void initializeCommand(string command, CommandProcessor processor, bool full_initialization)
	{ // Save the processor...
		base.initializeCommand(command, processor, full_initialization);
		if (full_initialization)
		{
			// Parse the command...
			parseCommand(command);
		}
	}

	/// <summary>
	/// Parse the command string into a PropList of parameters.  Does nothing in this base class. </summary>
	/// <param name="command"> A string command to parse. </param>
	/// <exception cref="InvalidCommandSyntaxException"> if during parsing the command is determined to have invalid syntax. </exception>
	/// <exception cref="InvalidCommandParameterException"> if during parsing the command
	/// parameters are determined to be invalid. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void parseCommand(String command) throws InvalidCommandSyntaxException, InvalidCommandParameterException
	public override void parseCommand(string command)
	{ // Does nothing.
	}

	/// <summary>
	/// Run the command. </summary>
	/// <exception cref="CommandWarningException"> Thrown if non-fatal warnings occur (the command could produce some results). </exception>
	/// <exception cref="CommandException"> Thrown if fatal warnings occur (the command could not produce output). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void runCommand(int command_number) throws InvalidCommandParameterException, CommandWarningException, CommandException
	public override void runCommand(int command_number)
	{
		string routine = "UnknownCommand.runCommand";
		int warning_count = 0;
		int warning_level = 2;
		string command_tag = "" + command_number;

		if (getCommandString().Trim().Equals(""))
		{
			// Empty line so don't do anything
		}
		else
		{
			Message.printStatus(2, "UnknownCommand.runCommand", "In runCommand().");
			CommandStatus status = getCommandStatus();
			status.clearLog(CommandPhaseType.RUN);

			string message = "Do not know how to run unknown command \"" + ToString() + "\"";
			Message.printWarning(warning_level, MessageUtil.formatMessageTag(command_tag, ++warning_count), routine, message);
			status.addToLog(CommandPhaseType.RUN, new CommandLogRecord(CommandStatusType.FAILURE, message, "Verify command spelling and if necessary report the problem to software support."));

			// Throw an exception because if something tries to run with this it needs
			// to be made known that nothing is happening.

			throw new CommandException(getCommandName() + " run() method is not enabled.");
		}
	}

	// TODO SAM 2005-05-31 If the editor is ever implemented with a tabular
	// display for parameters, will need to deal with the parsing.  For now, this
	// will at least allow unrecognized commands to be edited using the string representation.
	/// <summary>
	/// Return the string representation of the command.
	/// This always returns the command string.
	/// </summary>
	public override string ToString()
	{
		return getCommandString();
	}

	}

}