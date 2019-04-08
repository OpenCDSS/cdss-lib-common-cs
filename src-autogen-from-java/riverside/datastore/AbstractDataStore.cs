// AbstractDataStore - abstract base class for datastores

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

namespace riverside.datastore
{
	using PropList = RTi.Util.IO.PropList;

	// TODO SAM 2015-03-22 Need to fix issue that name and description are data members and also can be in properties.
	/// <summary>
	/// Abstract implementation of DataStore, to handle management of common configuration data.
	/// </summary>
	public abstract class AbstractDataStore : DataStore
	{

	/// <summary>
	/// The description for the datastore (usually a short sentence).
	/// </summary>
	private string __description = "";

	/// <summary>
	/// The name for the datastore (usually a single string without spaces, suitable for unique identification).
	/// </summary>
	private string __name = "";

	/// <summary>
	/// Property list for data properties read from configuration file.
	/// </summary>
	private PropList __props = new PropList("");

	/// <summary>
	/// Status of the datastore (0=Ok, 1=Error).
	/// </summary>
	private int __status = 0;

	/// <summary>
	/// Message corresponding to the status (e.g., error message).
	/// </summary>
	private string __statusMessage = "";

	/// <summary>
	/// Return the description for the datastore. </summary>
	/// <returns> the description for the datastore. </returns>
	public virtual string getDescription()
	{
		return __description;
	}

	/// <summary>
	/// Return the name for the datastore. </summary>
	/// <returns> the name for the datastore. </returns>
	public virtual string getName()
	{
		return __name;
	}

	/// <summary>
	/// Return the string property list for the datastore configuration. </summary>
	/// <returns> the string property list for the datastore configuration, guaranteed to be non-null. </returns>
	/// <param name="propertyName"> name of the property </param>
	public virtual PropList getProperties()
	{
		return __props;
	}

	/// <summary>
	/// Return the string value for a datastore configuration property. </summary>
	/// <returns> the string value for a datastore configuration property, or null if not matched. </returns>
	/// <param name="propertyName"> name of the property </param>
	public virtual string getProperty(string propertyName)
	{
		return __props.getValue(propertyName);
	}

	/// <summary>
	/// Return the status for the datastore. </summary>
	/// <returns> the status for the datastore. </returns>
	public virtual int getStatus()
	{
		return __status;
	}

	/// <summary>
	/// Return the status message for the datastore. </summary>
	/// <returns> the status message for the datastore. </returns>
	public virtual string getStatusMessage()
	{
		return __statusMessage;
	}


	/// <summary>
	/// Set the identifier for the datastore. </summary>
	/// <param name="description"> the identifier for the datastore. </param>
	public virtual void setDescription(string description)
	{
		__description = description;
	}

	/// <summary>
	/// Set the name for the datastore. </summary>
	/// <param name="name"> the name for the datastore. </param>
	public virtual void setName(string name)
	{
		__name = name;
	}

	/// <summary>
	/// Set the list of properties for the datastore. </summary>
	/// <param name="props"> the list of properties for the datastore </param>
	public virtual void setProperties(PropList props)
	{
		__props = props;
	}

	/// <summary>
	/// Set the status for the datastore. </summary>
	/// <param name="status"> the status for the datastore. </param>
	public virtual void setStatus(int status)
	{
		__status = status;
	}

	/// <summary>
	/// Set the status message for the datastore, for example when the status indicates an error. </summary>
	/// <param name="statusMessage"> the status message for the datastore. </param>
	public virtual void setStatusMessage(string statusMessage)
	{
		__statusMessage = statusMessage;
	}

	}

}