using System;

// TSGraphEditor - provides editing functionality for graphs.

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

	using GRPoint = RTi.GR.GRPoint;
	using TS = RTi.TS.TS;
	using TSData = RTi.TS.TSData;
	using TSIterator = RTi.TS.TSIterator;
	using MathUtil = RTi.Util.Math.MathUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;

	/*
	 * Provides editing functionality for graphs.
	 */
	public class TSGraphEditor
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_propertyChangeSupport = new SwingPropertyChangeSupport(this);
		}

	  /// <summary>
	  /// Time Series being edited </summary>
	  private TS _ts;
	  /// <summary>
	  /// Retains last point edited </summary>
	  private GRPoint _prevPoint = null;
	  private DateTime _prevDate = null;
	  private DateTime _currentDate;
	  private GRPoint _currentPoint;
	  /// <summary>
	  /// Tracks whether prev & current points have been swapped </summary>
	  private bool _swapFlag = false;
	  /// <summary>
	  /// Controls whether auto-connect will be applied </summary>
	  private bool _autoConnect = true;

	  // property support
	  private SwingPropertyChangeSupport _propertyChangeSupport;

	  /// <summary>
	  /// Creates an instance of TSGraphEditor.
	  /// <para>
	  /// The TSGraphEditor receives new GRPoints & applies them to the
	  /// TS. It is responsible for auto-connecting points.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="ts"> </param>
	  public TSGraphEditor(TS ts)
	  {
		  if (!InstanceFieldsInitialized)
		  {
			  InitializeInstanceFields();
			  InstanceFieldsInitialized = true;
		  }
		_ts = ts;

	  }

	  /// <summary>
	  /// Edit data point by clicking above or below it.
	  /// <para>
	  /// The date is determined by rounding to the nearest date.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="datapt"> Point to be edited (data coordinates) </param>
	  public virtual void editPoint(GRPoint datapt)
	  {
		//System.out.println("editPoint" + datapt.toString());
		DateTime date = new DateTime(datapt.x, true);

		int intervalBase = _ts.getDataIntervalBase();
		int intervalMult = _ts.getDataIntervalMult();
	//    System.out.println(">>> base: " + intervalBase + " X: " + intervalMult);
		if (intervalBase == TimeInterval.HOUR)
		{
			//TODO: handle other time intervals
			if (intervalMult > 0)
			{
				date.addHour(intervalMult / 2);
			}
		}
	//    System.out.println(">>> " + date + ": " + datapt.y);
		date.round(-1, intervalBase, intervalMult);
		_ts.setDataValue(date, datapt.y);

		// Notify listeners
		_propertyChangeSupport.firePropertyChange("TS_DATA_VALUE_CHANGE", null, _ts);
	//    System.out.println(">r  " + date + ": " + datapt.y);

		// Save for potential operation
		_prevDate = _currentDate;
		_prevPoint = _currentPoint;
		_currentDate = date;
		_currentPoint = new GRPoint(date.toDouble(), datapt.y);

		if (_prevPoint != null && _autoConnect)
		{
			autoConnect();
		}
	  }

	  private void autoConnect()
	  {
		doFillWithInterpolation();
	  }

	  /// <summary>
	  /// Interpolates the values of y  for points between the last two points
	  /// edited.
	  /// <para>
	  /// Two points must have been edited prior to calling this functionality.
	  /// </para>
	  /// <para>
	  /// Auto-connect is only to be applied for currentDate occurring to the
	  /// right of prevDate.
	  /// </para>
	  /// </summary>
	  public virtual void doFillWithInterpolation()
	  {

	//    System.out.println("doFillWithInterpolation");
		if (_prevPoint == null || _currentDate.Equals(_prevDate))
		{
			return;
		}
		if (_currentDate.lessThan(_prevDate))
		{
			return;
	//        if (!_autoConnect)
	//          {
	//            swapPoints();
	//          }
		}
		try
		{
		  TSIterator tsi = _ts.iterator(_prevDate, _currentDate);
		  TSData data; // This is volatile and the iterator reuses its reference

		  // Skip the first as it has been edited
		  tsi.next();
		  while (true)
		  {
			  data = tsi.next();

			  if (data.getDate().Equals(_currentDate))
			  {
				  // Notify listeners only once after all changes
				  _propertyChangeSupport.firePropertyChange("TS_DATA_VALUE_CHANGE", _ts, _ts);

				  if (_swapFlag)
				  {
					  swapPoints();
				  }
				  // Skip the last as it has been edited
				  return;
			  }
			  else
			  {
				  DateTime date = data.getDate();

				  double val = MathUtil.interpolate(date.toDouble(), _prevDate.toDouble(), _currentDate.toDouble(), _prevPoint.getY(), _currentPoint.getY());

				  _ts.setDataValue(date, val);
				  //  System.out.println(" ------> newY");
			  }
		  }
		}
		catch (Exception e)
		{
		  //  Message.printWarning(1, "doFillWithInterpolation", e.toString());
		  Console.WriteLine("prevDate:" + _prevDate + " curDate: " + _currentDate);
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
	  } // eof doFillWithInterpolation

	  /// <summary>
	  /// Swaps the data for previous & current points.
	  /// </summary>
	  private void swapPoints()
	  {
		DateTime tmpDateTime = _prevDate;
		_prevDate = _currentDate;
		_currentDate = tmpDateTime;

		GRPoint tmpGRPoint = _prevPoint;
		_prevPoint = _currentPoint;
		_currentPoint = tmpGRPoint;
		_swapFlag = !_swapFlag;
	  }
	  /// <summary>
	  /// Sets whether point editing uses auto-connect.
	  /// <para>
	  /// Auto-connect mode will use interpolation to determine
	  /// values between the previously edited point & the currently
	  /// edited point. The currently edited point must be to the right
	  /// of the previously edited point.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="selected"> </param>
	  public virtual void setAutoConnect(bool autoConnect)
	  {
		_autoConnect = autoConnect;
	  }

	  /* - - - Property Change Support - - - */
	  public virtual void addPropertyChangeListener(PropertyChangeListener l)
	  {
		 _propertyChangeSupport.addPropertyChangeListener(l);
	  }

	  public virtual void removePropertyChangeListener(PropertyChangeListener l)
	  {
		 _propertyChangeSupport.removePropertyChangeListener(l);
	  }
	}

}