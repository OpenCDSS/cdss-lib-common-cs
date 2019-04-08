using System;

// MessageEventQueue - event queue that will intercept AWEvents and log any exceptions that occur when the events are dispatched

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

namespace RTi.Util.Message
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Event queue that will intercept AWEvents and log any exceptions that occur when the events are dispatched.
	/// Exceptions are logged at Message level 3.
	/// Declare an instance of this class before any GUI components are initialized.
	/// </summary>
	public class MessageEventQueue : EventQueue
	{

	public MessageEventQueue()
	{
		// Replace the standard event queue with this instance
		Toolkit.getDefaultToolkit().getSystemEventQueue().push(this);
	}

	/// <summary>
	/// Handle all AWTEvents but also log to the Message class when an event throws an exception. </summary>
	/// <param name="event"> AWTEvent to handle. </param>
	protected internal virtual void dispatchEvent(AWTEvent @event)
	{
		string routine = "EventQueueMessage.dispatchEvent";
		try
		{
			// If everything is OK then this will simply pass the event on
			base.dispatchEvent(@event);
		}
		catch (ThreadDeath td)
		{
			td.printStackTrace();
			Message.printWarning(3, routine, "Thread death on AWT");
			Message.printWarning(3, routine, td);
		}
		catch (Exception t)
		{
			Console.WriteLine(t.ToString());
			Console.Write(t.StackTrace);
			Message.printWarning(3, routine, "Unexpected Internal Error");
			Message.printWarning(3, routine, t);
		}
	}

	}

}