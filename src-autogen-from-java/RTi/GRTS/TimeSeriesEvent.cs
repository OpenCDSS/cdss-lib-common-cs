// TimeSeriesEvent - manage and provide time series annotation event data, for annotating time series graphs with events

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
	using TimeLocationEvent = RTi.Util.Time.TimeLocationEvent;

	/// <summary>
	/// Manage and provide time series annotation event data, for annotating time series graphs with events.
	/// </summary>
	public class TimeSeriesEvent
	{

	/// <summary>
	/// Time series that is associated with the events.
	/// </summary>
	private TS ts = null;

	/// <summary>
	/// List of time series events.
	/// </summary>
	private TimeLocationEvent @event = null;

	/// <summary>
	/// Constructor.
	/// </summary>
	public TimeSeriesEvent(TS ts, TimeLocationEvent @event)
	{
		this.ts = ts;
		this.@event = @event;
	}

	/// <summary>
	/// Return the time series for the event.
	/// </summary>
	public virtual TS getTimeSeries()
	{
		return this.ts;
	}

	/// <summary>
	/// Return the event for the time series event.
	/// </summary>
	public virtual TimeLocationEvent getEvent()
	{
		return this.@event;
	}

	}

}