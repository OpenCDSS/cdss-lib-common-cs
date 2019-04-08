using System;
using System.Collections.Generic;

// GeoViewZoomHistory - history of zooms to allow tracing zoom history

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

// ----------------------------------------------------------------------------
// History:
// ----------------------------------------------------------------------------
// 08 Jul 1999	Catherine E.		Implemented code.
//		Nutting-Lane, RTi
// 07 Sep 1999	CEN, RTi		Added clear function.
// 2001-10-17	Steven A. Malers, RTi	Review javadoc.  Add finalize().  Set
//					unused data to null to help garbage
//					collection.  Fix bug where
//					getDataLimits() was not checking the
//					index properly to throw the exception.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{
	using GRLimits = RTi.GR.GRLimits;

	/// <summary>
	/// Ghe GeoViewZoomHistory class saves data and device limits from zoom events in a
	/// GeoView.  Any application using this class must implement GeoViewListener and
	/// add the geoViewListener to the desired GeoView.
	/// The application must instantiate a GeoViewZoomHistory and add
	/// to the zoom history in the following manner:
	/// <para>
	/// 
	/// <pre>
	/// GeoView			_canvas = new GeoView();
	/// GeoViewZoomHistory 	_zoomHistory = new GeoViewZoomHistory();
	/// 
	/// _canvas.addGeoViewListener ( this );
	/// 
	/// public geoViewZoom ( GRLimits devlim, GRLimits datalim )
	/// {	_zoomHistory.addZoom ( devlim, datalim );
	/// }
	/// </pre>
	/// <b>This class will be phased out as the new GeoViewPanel is used exclusively.
	/// Its reference window does not have a zoom history.</b>
	/// </para>
	/// </summary>
	public class GeoViewZoomHistory
	{

	private IList<GRLimits> _dataLimitsHistory;
	private IList<GRLimits> _devLimitsHistory;
	private int _currentIndex;

	/// <summary>
	/// Construct a zoom history.
	/// </summary>
	public GeoViewZoomHistory()
	{
		_currentIndex = -1;
		_dataLimitsHistory = new List<GRLimits>();
		_devLimitsHistory = new List<GRLimits>();
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoViewZoomHistory()
	{
		_dataLimitsHistory = null;
		_devLimitsHistory = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Save the zoom limits at the current index.  Therefore, if the current index is
	/// not at the end of the zoom history, this set of limits will be inserted in the
	/// history at the appropriate location. </summary>
	/// <param name="devLimits"> current device limits to store </param>
	/// <param name="dataLimits"> current data limits to store </param>
	public virtual void addHistory(GRLimits devLimits, GRLimits dataLimits)
	{
		_currentIndex++;

		if (_currentIndex == _dataLimitsHistory.Count)
		{
			_dataLimitsHistory.Add(dataLimits);
		}
		else
		{
			_dataLimitsHistory.Insert(_currentIndex, dataLimits);
		}

		if (_currentIndex == _devLimitsHistory.Count)
		{
			_devLimitsHistory.Add(devLimits);
		}
		else
		{
			_devLimitsHistory.Insert(_currentIndex, devLimits);
		}
	}

	/// <summary>
	/// Reset the members, which is useful if a new map is being read.
	/// </summary>
	public virtual void clear()
	{
		_currentIndex = -1;
		_dataLimitsHistory.Clear();
		_devLimitsHistory.Clear();
	}

	/// <summary>
	/// Return the current index in the zoom history. </summary>
	/// <returns> the current index in the zoom history. </returns>
	public virtual int getCurrentIndex()
	{
		return _currentIndex;
	}

	/// <summary>
	/// Return the current data limits. </summary>
	/// <returns> Data limits associated with the current setting. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RTi.GR.GRLimits getCurrentDataLimits() throws Exception
	public virtual GRLimits getCurrentDataLimits()
	{
		return getDataLimits(_currentIndex);
	}

	/// <summary>
	/// Return the data limits for the given zoom index position. </summary>
	/// <returns> Data limits associated with the given index. </returns>
	/// <param name="index"> index of desired data limits history. </param>
	/// <exception cref="Exception"> if index is invalid. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RTi.GR.GRLimits getDataLimits(int index) throws Exception
	public virtual GRLimits getDataLimits(int index)
	{
		if ((index < 0) || (index >= _dataLimitsHistory.Count))
		{
			throw new Exception("Index " + index + " out of bounds (" + _dataLimitsHistory.Count + ")");
		}
		return ((GRLimits)_dataLimitsHistory[index]);
	}

	/// <summary>
	/// Return the last index in the zoom history. </summary>
	/// <returns> the last index in the zoom history. </returns>
	public virtual int getLastIndex()
	{
		return (_dataLimitsHistory.Count - 1);
	}

	/// <summary>
	/// Retrieve the next data limits if available and increment the current index if
	/// changeCurrentIndex is true. </summary>
	/// <returns> Data limits associated with the next index. </returns>
	/// <param name="changeCurrentIndex"> indicates whether the currentIndex should be changed. </param>
	/// <exception cref="Exception"> if there are no next limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RTi.GR.GRLimits getNextDataLimits(boolean changeCurrentIndex) throws Exception
	public virtual GRLimits getNextDataLimits(bool changeCurrentIndex)
	{
		int tmpIndex = _currentIndex + 1;
		if (tmpIndex == _dataLimitsHistory.Count)
		{
			throw new Exception("No next data limits exist.");
		}

		if (changeCurrentIndex)
		{
			_currentIndex++;
		}

		return getDataLimits(tmpIndex);
	}

	/// <summary>
	/// Retrieve the previous data limits if available and decrements the current index
	/// if changeCurrentIndex is true. </summary>
	/// <returns> Data limits associated with the previous index. </returns>
	/// <param name="changeCurrentIndex"> indicates whether the currentIndex should be changed. </param>
	/// <exception cref="Exception"> if there is no previous limits. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RTi.GR.GRLimits getPreviousDataLimits(boolean changeCurrentIndex) throws Exception
	public virtual GRLimits getPreviousDataLimits(bool changeCurrentIndex)
	{
		int tmpIndex = _currentIndex - 1;
		if (tmpIndex < 0)
		{
			throw new Exception("No previous data limits exist.");
		}

		if (changeCurrentIndex)
		{
			_currentIndex--;
		}

		return getDataLimits(tmpIndex);
	}

	} // End GeoViewZoomHistory

}