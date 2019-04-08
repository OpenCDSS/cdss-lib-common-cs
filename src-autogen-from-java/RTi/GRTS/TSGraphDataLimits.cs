using System.Collections.Generic;

// TSGraphDataLimits - immutable helper class to store data limits and identifiers for time series, needed for graphing

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

	using GRLimits = RTi.GR.GRLimits;

	/// <summary>
	/// Immutable helper class to store data limits and identifiers for time series, needed for graphing.
	/// The main purpose of this class is to group data so that it can more easily be passed between methods.
	/// </summary>
	public class TSGraphDataLimits
	{

	/// <summary>
	/// Number of time series in the graph.
	/// </summary>
	private int __numTimeSeries;

	/// <summary>
	/// List of time series identifiers for time series being graphed.
	/// </summary>
	private IList<string> __tsidList;

	/// <summary>
	/// Data limits for time series being graphed.
	/// </summary>
	private GRLimits __dataLimits;

	/// <summary>
	/// Constructor.
	/// </summary>
	public TSGraphDataLimits(int numTimeSeries, IList<string> tsidList, GRLimits dataLimits)
	{
		this.__numTimeSeries = numTimeSeries;
		this.__tsidList = tsidList;
		this.__dataLimits = dataLimits;
	}

	/// <summary>
	/// Return the data limits for time series on the graph.
	/// </summary>
	public virtual GRLimits getDataLimits()
	{
		return __dataLimits;
	}

	/// <summary>
	/// Return the number of time series on the graph.
	/// </summary>
	public virtual int getNumTimeSeries()
	{
		return __numTimeSeries;
	}

	/// <summary>
	/// Return the time series identifiers for time series being graphed.
	/// </summary>
	public virtual IList<string> getTimeSeriesIds()
	{
		return __tsidList;
	}

	}

}