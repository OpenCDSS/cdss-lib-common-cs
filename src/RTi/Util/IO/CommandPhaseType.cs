﻿// CommandPhaseType - this class provides an enumeration of possible command phase values

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
	/// This class provides an enumeration of possible command phase values.
	/// An enum could be used when Java 1.5 is utilized.    
	/// </summary>
	public class CommandPhaseType
	{

		/// <summary>
		/// When used with Command processing,
		/// INITIALIZATION indicates that the command is being initialized, including
		/// construction and validation of parameters.
		/// </summary>
		public static CommandPhaseType INITIALIZATION = new CommandPhaseType(0, "INITIALIZATION");

		/// <summary>
		/// DISCOVERY is the phase where a command is partially run, in order to provide data to dependent commands.
		/// </summary>
		public static CommandPhaseType DISCOVERY = new CommandPhaseType(1, "DISCOVERY");

		/// <summary>
		/// RUN indicates the phase where the command is being run.
		/// </summary>
		public static CommandPhaseType RUN = new CommandPhaseType(2, "RUN");

		//private int __type;
		/// <summary>
		/// @uml.property  name="__typename"
		/// </summary>
		private string __typename;

		private CommandPhaseType(int type, string typename)
		{
			//__type = type;
			__typename = typename;
		}

		/// <summary>
		/// Return a String representation of the command processing phase.
		/// </summary>
		public override string ToString()
		{
			return __typename;
		}
	}

}