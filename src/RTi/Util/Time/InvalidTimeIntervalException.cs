using System;

// InvalidTimeIntervalException - should be thrown when a TimeInterval string can't be parsed

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

namespace RTi.Util.Time
{
	/// <summary>
	/// An InvalidTimeIntervalException should be thrown when a TimeInterval string
	/// is invalid (e.g., when it cannot be parsed or is not supported by a method).
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class InvalidTimeIntervalException extends RuntimeException
	public class InvalidTimeIntervalException : Exception
	{

	public InvalidTimeIntervalException(string message) : base(message)
	{
	}

	}

}