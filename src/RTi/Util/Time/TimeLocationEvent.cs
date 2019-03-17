// TimeLocationEvent - representation of a single time or timespan event that is associated with a single location

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
	/// Representation of a single time or timespan event that is associated with a single location.
	/// For example, this information is used to indicate when and where an event occurred so that it can
	/// be visualized on time series graphs and time-aware maps.  Event start and end are represented as follows:
	/// <pre>
	/// eventStart             eventEnd                Interpretation
	/// -------------------    --------------------    -----------------------------------------------------------------------------
	/// (1) DateTime           DateTimeIndefinite      Event has a start time but no end time (event is ongoing).
	/// (2) DateTime           null                    Event is a point event (interpret start and end time to be the same).
	/// For now case 1 above is not enabled and case 2 is used instead to represent 1.  Case to requires 2 date/times. 
	/// If this works, then case 1 will never be enabled.
	/// DateTime               DateTime                Event has a discrete start and end.
	/// DateTimeIndefinite     DateTime                Event has an indefinite start time (started long ago) but a definite end time.
	/// </pre>
	/// </summary>
	public class TimeLocationEvent
	{

	/// <summary>
	/// Event identifier.
	/// </summary>
	private string eventID = "";

	/// <summary>
	/// Event type (e.g., "Drought").
	/// </summary>
	private string eventType = "";

	/// <summary>
	/// Start date/time for the event.
	/// </summary>
	private DateTime eventStart;

	/// <summary>
	/// End date/time for the event (can be null if the event is a single-point event, or ongoing indefinitely).
	/// </summary>
	private DateTime eventEnd;

	/// <summary>
	/// Location type for event (e.g., "County").
	/// </summary>
	private string locationType = "";

	/// <summary>
	/// Location ID for event (e.g., "Adams" county).
	/// </summary>
	private string locationID = "";

	/// <summary>
	/// Short label to use for the event, suitable for graph or map rendered labels.
	/// </summary>
	internal string label = "";

	/// <summary>
	/// Full descriptive note for the event, suitable for a popup or report summarizing the events.
	/// </summary>
	internal string description = "";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="eventID"> identifier for event </param>
	/// <param name="eventType"> type of event, a string appropriate for the event data set </param>
	/// <param name="eventStart"> start of the event, or null if start was indeterminate </param>
	/// <param name="eventEnd"> end of the event, or null if ongoing (should be same as eventStart if a point in time event) </param>
	/// <param name="label"> label for the event, a short string appropriate for rendering on graphs and maps </param>
	/// <param name="description"> longer description of the event, such as a narrative for a pop-up or report </param>
	/// <param name="locations"> a dictionary that includes the event location type (e.g., "County"),
	/// and location identifier (e.g., "Adams" county). </param>
	public TimeLocationEvent(string eventID, string eventType, DateTime eventStart, DateTime eventEnd, string locationType, string locationID, string label, string description)
	{
		this.eventID = eventID;
		this.eventType = eventType;
		this.eventStart = eventStart;
		this.eventEnd = eventEnd;
		this.label = label;
		this.description = description;
		this.locationType = locationType;
		this.locationID = locationID;
	}

	/// <summary>
	/// Return the longer description for the event.
	/// </summary>
	public virtual string getDescription()
	{
		return this.description;
	}

	/// <summary>
	/// Return the event end, null if the event is ongoing.  If not null, the event has a discrete end.
	/// </summary>
	public virtual DateTime getEventEnd()
	{
		return this.eventEnd;
	}

	/// <summary>
	/// Return the event identifier.
	/// </summary>
	public virtual string getEventID()
	{
		return this.eventID;
	}

	/// <summary>
	/// Return the event start.
	/// </summary>
	public virtual DateTime getEventStart()
	{
		return this.eventStart;
	}

	/// <summary>
	/// Return the event type.
	/// </summary>
	public virtual string getEventType()
	{
		return this.eventType;
	}

	/// <summary>
	/// Return the short label to be used for rendering the data.
	/// </summary>
	public virtual string getLabel()
	{
		return this.label;
	}

	/// <summary>
	/// Return the location ID for the event.
	/// </summary>
	public virtual string getLocationID()
	{
		return this.locationID;
	}

	/// <summary>
	/// Return the location type for the event.
	/// </summary>
	public virtual string getLocationType()
	{
		return this.locationType;
	}

	}

}