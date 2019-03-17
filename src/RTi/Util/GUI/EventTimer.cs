using System;
using System.Threading;

// EventTimer - class for dispatching event on a specific timescale

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

//------------------------------------------------------------------------------
// EventTimer - class for dispatching event on a specific timescale.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 
// 26 Jan 1997	Matthew J. Rutherford,	Created initial version.
//		Riverside Technolgy,
//		inc.
// 26 Aug 2001	Steven A. Malers, RTi	Clean up javadoc and internal comments.
//					Add finalize.  Call stop() when the
//					thread is done running.  This should
//					improve performance.
//					I suspect Matt got this code from a
//					book or Javasoft web site but I don't
//					totally understand its implementation.
//					Hopefully my changes make it behave a
//					little better.
//------------------------------------------------------------------------------

namespace RTi.Util.GUI
{
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class dispatches a specific event on a defined time interval, as a separate
	/// thread.  This is useful for checking on the intermediate status of a task, like
	/// a query or external program execution.  One sleep cycle occurs before any event
	/// is generated.
	/// The EventTimer should be used as follows:
	/// <pre>
	/// EventTimer et = new EventTimer ( 1000, SomeActionListener,
	/// "AnActionPerformedString" );
	/// // Implement actionPerformed() and check getActionCommand() for
	/// // "AnActionPerformdedString"
	/// 
	/// // When done:
	/// et.finish();
	/// </pre>
	/// </summary>
	public class EventTimer : Thread
	{
	private long _ms = 0; // Time to wait between events.
	private SimpleJButton _comp = null; // Button to use for generating events.
	private bool? _isDone = new bool?(false);
						// Indicates whether event generation
						// is done.  Need a Boolean to allow
						// synchronization.
	private string _event = null; // Event string that is passed to
						// listener.

	/// <summary>
	/// Create an event timer, which will generate an ActionEvent with a command
	/// string of "s", generating an event every "ms" milliseconds. </summary>
	/// <param name="ms"> Interval between generated events, milliseconds. </param>
	/// <param name="al"> ActionListener that receives the events. </param>
	/// <param name="s"> String command name.  Check the ActionEvent's getActionCommand() value
	/// against this. </param>
	public EventTimer(long ms, ActionListener al, string s)
	{
		_ms = ms;

		if (al == null)
		{
			throw (new System.NullReferenceException("EventTimer: ActionListener argument is NULL."));
		}
		_comp = new SimpleJButton(s, s, al);

		_event = s;

		start();
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~EventTimer()
	{
		_comp = null;
		_isDone = null;
		_event = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Starts the thread that actually begins running the event timer.
	/// </summary>
	public virtual void run()
	{
		while (true)
		{
			lock (_isDone)
			{
				if (_isDone.Value)
				{
					try
					{
						stop();
					}
					catch (Exception)
					{
					}
					catch (Exception)
					{
						// Should be a ThreadDeath...
					}
					return;
				}
			}
			try
			{
				sleep(_ms);
			}
			catch (InterruptedException)
			{
				return;
			}

			if (Message.isDebugOn)
			{
				Message.printDebug(50, "EventTimer.run", "Dispatching event: " + _event);
			}

			// Use the button to dispatch the event...

			_comp.dispatchEvent(new ActionEvent(_comp, ActionEvent.ACTION_PERFORMED, _event));
		}
	}

	/// <summary>
	/// Stop generating events.  This synchronized method can be called by a different
	/// thread (presumably the one that created the EventTimer.  The thread is
	/// stopped the next time the thread is checked (at an even interval).
	/// </summary>
	public virtual void finish()
	{
		lock (this)
		{
			_isDone = new bool?(true);
		}
	}

	}

}