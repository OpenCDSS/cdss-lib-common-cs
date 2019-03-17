// DataStore - interface to define general datastore behavior

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

	/// <summary>
	/// Interface for a datastore, which is for persistent storage of data.
	/// A datastore can be a database, web service, file, etc.
	/// @author sam
	/// 
	/// </summary>
	public interface DataStore
	{

		/// <summary>
		/// Get the description for the datastore.  This is a longer name that is helpful to the user
		/// (e.g., "Historical temperature database."). </summary>
		/// <param name="return"> the description of the datastore </param>
		string getDescription();

		/// <summary>
		/// Get the name for the datastore.  This is typically a short string (e.g., "HistoricalDB").
		/// In some cases (such as time series identifiers), this is used as the input type. </summary>
		/// <returns> the name of the datastore </returns>
		string getName();

		/// <summary>
		/// Get the property list for the datastore.  These are strings stored in the datastore configuration.
		/// Properties allow for custom-configuration of datastores beyond the basic behavior enforced by
		/// the datastore design.  "Name" and "Description" are specific properties that are required. </summary>
		/// <returns> the property list for the datastore </returns>
		PropList getProperties();

		/// <summary>
		/// Get a property for the datastore.  These are strings stored in the datastore configuration.
		/// Properties allow for custom-configuration of datastores beyond the basic behavior enforced by
		/// the datastore design.  "Name" and "Description" are specific properties that are required. </summary>
		/// <returns> the string value of a datastore configuration property, or null if not found </returns>
		/// <param name="propertyName"> the name of a property to retrieve </param>
		string getProperty(string propertyName);

		/// <summary>
		/// Get the status of datastore. </summary>
		/// <returns> the status as 0=OK, 1=Error.  In the future more specific error codes may be implemented. </returns>
		int getStatus();

		/// <summary>
		/// Get the status message for datastore. </summary>
		/// <returns> the status message, for example an error message. </returns>
		string getStatusMessage();

		// TODO SAM 2012-02-29 Evaluate using a more general object than PropList
		/// <summary>
		/// Set the list of properties for a datastore (typically at creation from a configuration file).
		/// </summary>
		void setProperties(PropList properties);

		/// <summary>
		/// Set the status of datastore. </summary>
		/// <param name="status"> as 0=OK, 1=Error, 2=Closed.
		/// TODO SAM 2015-02-14 In the future additional error codes may be implemented or convert to enumeration. </param>
		void setStatus(int status);

		/// <summary>
		/// Set the status message for the datastore. </summary>
		/// <param name="statusMessage"> message corresponding to status, for example error message. </param>
		void setStatusMessage(string statusMessage);
	}

}