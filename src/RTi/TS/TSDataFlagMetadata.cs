// TSDataFlagMetadata - metadata about flags used with a time series.

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
	/// <summary>
	/// Metadata about flags used with a time series.
	/// Instances of this class can be added to a time series via addDataFlagMetaData() method.
	/// This information is useful for output reports and displays, to explain the meaning of data flags.
	/// The class is immutable.
	/// </summary>
	public class TSDataFlagMetadata
	{

	/// <summary>
	/// Data flag.  Although this is a string, flags are generally one character.
	/// </summary>
	private string __dataFlag = "";

	/// <summary>
	/// Description for the data flag.
	/// </summary>
	private string __description = "";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataFlag"> data flag (generally one character). </param>
	/// <param name="description"> description of the data flag. </param>
	public TSDataFlagMetadata(string dataFlag, string description)
	{
		setDataFlag(dataFlag);
		setDescription(description);
	}

	/// <summary>
	/// Return the data flag. </summary>
	/// <returns> the data flag </returns>
	public virtual string getDataFlag()
	{
		return __dataFlag;
	}

	/// <summary>
	/// Return the data flag description. </summary>
	/// <returns> the data flag description </returns>
	public virtual string getDescription()
	{
		return __description;
	}

	/// <summary>
	/// Set the data flag. </summary>
	/// <param name="dataFlag"> the data flag </param>
	private void setDataFlag(string dataFlag)
	{
		__dataFlag = dataFlag;
	}

	/// <summary>
	/// Set the description for the data flag. </summary>
	/// <param name="description"> the data flag description </param>
	private void setDescription(string description)
	{
		__description = description;
	}

	}

}