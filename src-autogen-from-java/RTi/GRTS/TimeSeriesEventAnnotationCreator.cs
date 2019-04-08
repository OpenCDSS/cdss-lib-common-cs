using System;
using System.Collections.Generic;

// TimeSeriesEventAnnotationCreator - Create annotation event data using input table and time series

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

namespace RTi.GRTS
{

	using TS = RTi.TS.TS;
	using DataTable = RTi.Util.Table.DataTable;
	using TableRecord = RTi.Util.Table.TableRecord;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeLocationEvent = RTi.Util.Time.TimeLocationEvent;

	/// <summary>
	/// Create annotation event data using input table and time series.
	/// </summary>
	public class TimeSeriesEventAnnotationCreator
	{

	/// <summary>
	/// Table that contains event data. 
	/// </summary>
	internal DataTable eventTable = null;

	/// <summary>
	/// Time series to be matched with events.
	/// </summary>
	internal TS ts = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="eventTable"> the data table containing events </param>
	/// <param name="ts"> time series to associate events </param>
	public TimeSeriesEventAnnotationCreator(DataTable eventTable, TS ts)
	{
		this.eventTable = eventTable;
		this.ts = ts;
	}

	/// <summary>
	/// Create a list of annotation events from the table and time series that were used to initialize the instance. </summary>
	/// <param name="eventTypes"> event types (e.g., "Drought") to include </param>
	/// <param name="eventIDColumn"> event table column name for event ID </param>
	/// <param name="eventStartColumn"> event table column name for event start date/time </param>
	/// <param name="eventEndColumn"> event table column name for event end date/time </param>
	/// <param name="eventLabelColumn"> event table column name for event label </param>
	/// <param name="eventDescriptionColumn"> event table column name for event description </param>
	/// <param name="eventLocationColumnMap"> dictionary of event table column names for location type and location value.
	/// If the key and value are the same, then the key indicates the location type as a column name, and the value is in that column.
	/// If the key and value are different, then the key indicates the column name for location types
	/// (e.g., "LocationType" column with values "County", "State", etc.),
	/// and the value is the column name for location type values
	/// (e.g., "Adams" for location type "County" and "CO" for location type "State". </param>
	/// <param name="tsLocationMap"> dictionary of time series location type and time series properties (e.g., "County", "${TS:County}"). </param>
	/// <param name="start"> the starting DateTime for events, used to request a time window of events (null to process all events) </param>
	/// <param name="end"> the ending DateTime for events, used to request a time window of events (null to process all events) </param>
	public virtual IList<TimeSeriesEvent> createTimeSeriesEvents(IList<string> eventTypes, string eventIDColumn, string eventTypeColumn, string eventStartColumn, string eventEndColumn, string eventLabelColumn, string eventDescriptionColumn, Dictionary<string, string> eventLocationColumnMap, Dictionary<string, string> tsLocationMap, DateTime start, DateTime end)
	{ // Get the primary data
		DataTable table = getEventTable();
		TS ts = getTimeSeries();
		// If event types are not specified, will return all
		if (eventTypes == null)
		{
			eventTypes = new List<string>();
		}
		// Determine the column numbers for the table data
		int eventIDColumnNum = -1;
		try
		{
			eventIDColumnNum = table.getFieldIndex(eventIDColumn);
		}
		catch (Exception)
		{
			throw new Exception("Event table \"" + table.getTableID() + " event ID column \"" + eventIDColumn + "\" not found in table.");
		}
		int eventTypeColumnNum = -1;
		try
		{
			eventTypeColumnNum = table.getFieldIndex(eventTypeColumn);
		}
		catch (Exception)
		{
			throw new Exception("Event table \"" + table.getTableID() + " event type column \"" + eventTypeColumn + "\" not found in table.");
		}
		int eventStartColumnNum = -1;
		try
		{
			eventStartColumnNum = table.getFieldIndex(eventStartColumn);
		}
		catch (Exception)
		{
			throw new Exception("Event table \"" + table.getTableID() + " event start column \"" + eventStartColumn + "\" not found in table.");
		}
		int eventEndColumnNum = -1;
		try
		{
			eventEndColumnNum = table.getFieldIndex(eventEndColumn);
		}
		catch (Exception)
		{
			throw new Exception("Event table \"" + table.getTableID() + " event end column \"" + eventEndColumn + "\" not found in table.");
		}
		int eventLabelColumnNum = -1;
		try
		{
			eventLabelColumnNum = table.getFieldIndex(eventLabelColumn);
		}
		catch (Exception)
		{
			throw new Exception("Event table \"" + table.getTableID() + " label column \"" + eventLabelColumn + "\" not found in table.");
		}
		int eventDescriptionColumnNum = -1;
		try
		{
			eventDescriptionColumnNum = table.getFieldIndex(eventDescriptionColumn);
		}
		catch (Exception)
		{
			throw new Exception("Event table \"" + table.getTableID() + " description column \"" + eventDescriptionColumn + "\" not found in table.");
		}
		string[] eventLocationTypes = new string[eventLocationColumnMap.Count];
		int[] eventLocationColumnNum = new int[eventLocationColumnMap.Count];
		string[] eventLocationColumns = new string[eventLocationColumnNum.Length];
		for (int i = 0; i < eventLocationTypes.Length; i++)
		{
			eventLocationColumnNum[i] = -1;
		}
		int ikey = -1;
		foreach (KeyValuePair<string, string> pairs in eventLocationColumnMap.SetOfKeyValuePairs())
		{
			eventLocationTypes[++ikey] = pairs.Key;
			try
			{
				eventLocationColumns[ikey] = pairs.Value;
				eventLocationColumnNum[ikey] = table.getFieldIndex(eventLocationColumns[ikey]);
			}
			catch (Exception)
			{
				throw new Exception("Event table \"" + table.getTableID() + "\" location column \"" + eventLocationColumns[ikey] + "\" not found in table.");
			}
		}
		// Determine the location types and ID values from the time series
		ikey = -1;
		string[] tsLocationTypes = new string[tsLocationMap.Count];
		string[] tsLocationIDs = new string[tsLocationTypes.Length];
		foreach (KeyValuePair<string, string> pairs in tsLocationMap.SetOfKeyValuePairs())
		{
			tsLocationTypes[++ikey] = pairs.Key;
			// Expand the ID based on the time series properties.
			tsLocationIDs[ikey] = pairs.Value;
		}
		// Loop through the table records and try to match the location types in the event records with the location types
		// in the time series.
		string eventID, eventType, eventLocationType = null, eventLocationID = null, label = null, description = null;
		object eventStartO = null, eventEndO = null;
		DateTime eventStart = null, eventEnd = null;
		bool includeEvent;
		IList<TimeSeriesEvent> tsEventList = new List<TimeSeriesEvent>();
		foreach (TableRecord rec in table.getTableRecords())
		{
			// Skip records that are not the correct event type
			try
			{
				eventType = rec.getFieldValueString(eventTypeColumnNum);
			}
			catch (Exception)
			{
				// Should not happen since valid index checked above
				continue;
			}
			includeEvent = false;
			if (eventTypes.Count == 0)
			{
				includeEvent = true;
			}
			else
			{
				foreach (string eventTypeReq in eventTypes)
				{
					if (eventType.Equals(eventTypeReq, StringComparison.OrdinalIgnoreCase))
					{
						includeEvent = true;
						break;
					}
				}
			}
			if (!includeEvent)
			{
				continue;
			}
			// Reset because need to check for location type match
			includeEvent = false;
			// Loop through the location data for the record
			for (int iloc = 0; iloc < eventLocationColumnNum.Length; iloc++)
			{
				try
				{
					eventLocationType = eventLocationTypes[iloc];
				}
				catch (Exception)
				{
					// Should not happen since column verified above
				}
				try
				{
					eventLocationID = rec.getFieldValueString(eventLocationColumnNum[iloc]);
				}
				catch (Exception)
				{
					// Should not happen since column verified above
				}
				// Loop through the location information for the time series and see if the table record matches...
				for (int itsloc = 0; itsloc < tsLocationTypes.Length; itsloc++)
				{
					//Message.printStatus(2,"","Comparing event record location type \"" + eventLocationType +
					//    "\" with time series location type \"" + tsLocationTypes[itsloc] +
					//    "\" and event location ID \"" + eventLocationID + "\" with time series location ID \"" + tsLocationIDs[itsloc] +
					//    "\"" );
					if ((!string.ReferenceEquals(eventLocationType, null)) && eventLocationType.Equals(tsLocationTypes[itsloc], StringComparison.OrdinalIgnoreCase) && (!string.ReferenceEquals(eventLocationID, null)) && eventLocationID.Equals(tsLocationIDs[itsloc], StringComparison.OrdinalIgnoreCase))
					{
						// Event location type and location ID match the time series
						includeEvent = true;
						//Message.printStatus(2,"","Found matching event record location type \"" + eventLocationType +
						//"\" with time series location type \"" + tsLocationTypes[itsloc] +
						//"\" and event location ID \"" + eventLocationID + "\" with time series location ID \"" + tsLocationIDs[itsloc] +
						//"\"" );
						break;
					}
				}
				if (includeEvent)
				{
					break;
				}
			}
			if (!includeEvent)
			{
				// Location did not match
				continue;
			}
			// If here the event type was matched and the event location match the time series location.
			// Get the remaining data from the table and create a corresponding event.
			try
			{
				eventStartO = rec.getFieldValue(eventStartColumnNum);
				if (eventStartO == null)
				{
					eventStart = null;
				}
				else
				{
					if (eventStartO is DateTime)
					{
						// Just set
						eventStart = (DateTime)eventStartO;
					}
					else if (eventStartO is System.DateTime)
					{
						eventStart = new DateTime((System.DateTime)eventStartO);
					}
					else if (eventStartO is string)
					{
						eventStart = DateTime.parse((string)eventStartO);
					}
				}
			}
			catch (Exception)
			{
				// Should not happen since valid index checked above
				// TODO Handle date/time parsing exceptions
				continue;
			}
			try
			{
				eventEndO = rec.getFieldValue(eventEndColumnNum);
				if (eventEndO == null)
				{
					eventEnd = null;
				}
				else
				{
					if (eventEndO is DateTime)
					{
						// Just set
						eventEnd = (DateTime)eventEndO;
					}
					else if (eventEndO is System.DateTime)
					{
						eventEnd = new DateTime((System.DateTime)eventEndO);
					}
					else if (eventEndO is string)
					{
						eventEnd = DateTime.parse((string)eventEndO);
					}
				}
			}
			catch (Exception)
			{
				// Should not happen since valid index checked above
				// TODO Handle date/time parsing exceptions
				continue;
			}
			try
			{
				eventID = rec.getFieldValueString(eventIDColumnNum);
			}
			catch (Exception)
			{
				// Should not happen since valid index checked above
				continue;
			}
			try
			{
				label = rec.getFieldValueString(eventLabelColumnNum);
			}
			catch (Exception)
			{
				// Should not happen since valid index checked above
				continue;
			}
			try
			{
				description = rec.getFieldValueString(eventDescriptionColumnNum);
			}
			catch (Exception)
			{
				// Should not happen since valid index checked above
				continue;
			}
			tsEventList.Add(new TimeSeriesEvent(ts, new TimeLocationEvent(eventID, eventType, eventStart, eventEnd, eventLocationType, eventLocationID, label, description)));
		}
		return tsEventList;
	}

	/// <summary>
	/// Return the event table.
	/// </summary>
	public virtual DataTable getEventTable()
	{
		return this.eventTable;
	}

	/// <summary>
	/// Return the time series being associated with events.
	/// </summary>
	public virtual TS getTimeSeries()
	{
		return this.ts;
	}

	}

}