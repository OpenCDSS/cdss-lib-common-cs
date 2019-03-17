using System;
using System.Collections.Generic;
using System.Threading;

// GeoViewAnimationProcessor - threaded animation processor for animated layers

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

//-----------------------------------------------------------------------------
// GeoViewAnimationProcessor - Threaded animation processor for animated
//	layers.
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 2004-08-04	J. Thomas Sapienza, RTi	Initial version.
// 2004-08-09	JTS, RTi		* Revised GUI.
//					* Added process listener code.
// 2005-04-27	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using ProcessListener = RTi.Util.IO.ProcessListener;

	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class is a threaded processor that runs an animation.
	/// </summary>
	public class GeoViewAnimationProcessor : ThreadStart
	{

	/// <summary>
	/// Whether an animation is going on.
	/// </summary>
	private bool __animating = false;

	/// <summary>
	/// Whether the animation is cancelled.
	/// </summary>
	private bool __cancelled = false;

	/// <summary>
	/// Whether the animation is paused.
	/// </summary>
	private bool __paused = false;

	/// <summary>
	/// The date currently being animated.
	/// </summary>
	private DateTime __currentDate;

	/// <summary>
	/// The last date to be animated.
	/// </summary>
	private DateTime __endDate;

	/// <summary>
	/// The gui that controls a layer animation.
	/// </summary>
	private GeoViewAnimationJFrame __animationJFrame;

	/// <summary>
	/// The component drawing the map.
	/// </summary>
	private GeoViewJComponent __viewComponent;

	/// <summary>
	/// The amount of time to pause between animation steps.
	/// </summary>
	private int __pause;

	/// <summary>
	/// List of listeners to be notified during a process.
	/// </summary>
	private IList<ProcessListener> __processListeners;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent gui controlling the animation. </param>
	/// <param name="viewComponent"> the component on which the map is drawn. </param>
	/// <param name="startDate"> the date from which to being animating. </param>
	/// <param name="steps"> the number of steps of data to animate. </param>
	/// <param name="interval"> the interval between dates. </param>
	/// <param name="pause"> the amount of pause (in milliseconds) between animation updates. </param>
	public GeoViewAnimationProcessor(GeoViewAnimationJFrame parent, GeoViewJComponent viewComponent, DateTime startDate, DateTime endDate, int pause)
	{
		__animationJFrame = parent;
		__viewComponent = viewComponent;
		__currentDate = new DateTime(startDate);
		__endDate = endDate;
		__pause = pause;
	}

	/// <summary>
	/// Adds a process listener to be notified during processing. </summary>
	/// <param name="listener"> the listener to be added. </param>
	public virtual void addProcessListener(ProcessListener p)
	{
		if (__processListeners == null)
		{
			__processListeners = new List<ProcessListener>();
		}
		__processListeners.Add(p);
	}

	/// <summary>
	/// Cancels the animation.
	/// </summary>
	public virtual void cancel()
	{
		__cancelled = true;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewAnimationProcessor()
	{
		__currentDate = null;
		__endDate = null;
		__animationJFrame = null;
		__viewComponent = null;
		__processListeners = null;
	}

	/// <summary>
	/// Returns the current date of animation. </summary>
	/// <returns> the current date of animation. </returns>
	public virtual DateTime getCurrentDate()
	{
		return __currentDate;
	}

	/// <summary>
	/// Returns whether an animation is going on or not. </summary>
	/// <returns> whether an animation is going on or not. </returns>
	public virtual bool isAnimating()
	{
		return __animating;
	}

	/// <summary>
	/// Notifies listeners of status messages. </summary>
	/// <param name="code"> the code to send with status messages. </param>
	/// <param name="message"> the text of the status message. </param>
	public virtual void notifyListenersStatus(int code, string message)
	{
		if (__processListeners == null)
		{
			return;
		}

		int size = __processListeners.Count;
		ProcessListener p = null;
		for (int i = 0; i < size; i++)
		{
			p = (ProcessListener)__processListeners[i];
			p.processStatus(code, message);
		}
	}

	/// <summary>
	/// Pauses or unpauses the animation. </summary>
	/// <param name="pause"> if true the animation is paused, if false it continues. </param>
	public virtual void pause(bool paused)
	{
		__paused = paused;
	}

	/// <summary>
	/// Runs the animation.
	/// </summary>
	public virtual void run()
	{
		__paused = false;
		__cancelled = false;

		string currDateString = null;

		while (true)
		{
			__animating = true;
			currDateString = __currentDate.ToString(DateTime.FORMAT_YYYY_MM);
			if (__paused)
			{
				notifyListenersStatus(1, "(Paused at " + currDateString + ")");
				try
				{
					Thread.Sleep(200);
				}
				catch (Exception)
				{
				}
				continue;
			}
			else if (__cancelled)
			{
				__animationJFrame.animationDone();
				__animating = false;
				return;
			}

			notifyListenersStatus(1, "Retrieving data for " + currDateString);
			__animationJFrame.fillData(__currentDate);
			notifyListenersStatus(0, __currentDate.ToString(DateTime.FORMAT_YYYY_MM));
			notifyListenersStatus(1, "Drawing map display for " + currDateString);
			try
			{
				Thread.Sleep(100);
			}
			catch (Exception)
			{
			}
			__viewComponent.redraw();
			__currentDate.addMonth(1);
			try
			{
				Thread.Sleep(200);
			}
			catch (Exception)
			{
			}

			if (((double)__pause / 1000.0) == 1)
			{
				notifyListenersStatus(1, "Waiting 1 second at " + currDateString);
			}
			else
			{
				notifyListenersStatus(1, "Waiting " + ((double)__pause / 1000.0) + " seconds at " + currDateString);
			}

			try
			{
				Thread.Sleep(__pause);
			}
			catch (Exception)
			{
			}

			if (__currentDate.greaterThanOrEqualTo(__endDate))
			{
				__animationJFrame.animationDone();
				__animating = false;
				return;
			}
		}
	}

	/// <summary>
	/// Sets the end date of the run. </summary>
	/// <param name="date"> the last date of animation. </param>
	public virtual void setEndDate(DateTime endDate)
	{
		__endDate = endDate;
	}

	/// <summary>
	/// Sets the amount of pause (in milliseconds) between animation updates. </summary>
	/// <param name="pause"> the amount of pause between updates. </param>
	public virtual void setPause(int pause)
	{
		__pause = pause;
	}

	/// <summary>
	/// Sets the start date (from which the animation begins). </summary>
	/// <param name="date"> the starting date of animation. </param>
	public virtual void setStartDate(DateTime date)
	{
		__currentDate = new DateTime(date);
	}

	public virtual void sleep(int sleep)
	{
		try
		{
			Thread.Sleep(sleep);
		}
		catch (Exception)
		{
		}
	}

	}

}