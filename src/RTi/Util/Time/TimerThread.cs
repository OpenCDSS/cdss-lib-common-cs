using System;
using System.Collections.Generic;
using System.Threading;

// TimerThread - handles timing a thread

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

// TODO SAM 2007-05-09 is this class used?

namespace RTi.Util.Time
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TimerThread implements Runnable, java.io.Serializable
	[Serializable]
	public class TimerThread : ThreadStart
	{
	  protected internal int increment = 1000;
	  protected internal int elapsed = 0;
	  protected internal int alarm;
	  protected internal IList<TimerListener> timerListeners = new List<TimerListener> ();
	  [NonSerialized]
	  protected internal Thread runner;

	  public TimerThread()
	  {
	  }

	  public TimerThread(int t)
	  {
		alarm = t;
	  }

	  public virtual void setIncrementTime(int t)
	  {
		increment = t;
	  }

	  public virtual int getIncrementTime()
	  {
		return increment;
	  }

	  public virtual void setAlarmTime(int t)
	  {
		alarm = t;
	  }

	  public virtual int getAlarmTime()
	  {
		return alarm;
	  }

	  public virtual void setElapsedTime(int t)
	  {
		elapsed = t;
	  }

	  public virtual int getElapsedTime()
	  {
		return elapsed;
	  }

	  public virtual void start()
	  {
		  lock (this)
		  {
			if (runner == null)
			{
			  runner = new Thread(this);
			  runner.Start();
			}
		  }
	  }

	  public virtual void stop()
	  {
		  lock (this)
		  {
			//Thread t = runner;
			if (runner != null)
			{
			  //runner.stop ();
			  runner = null;
			}
		  }
	  }

	  public virtual void run()
	  {
		while (true)
		{
		  if (elapsed >= alarm)
		  {
		break;
		  }
		  try
		  {
		Thread.Sleep(increment);
		elapsed += increment;
		incrementUpdate();
		  }
		  catch (Exception)
		  {
		  }
		}
		alarmUpdate();
	  }

	  public virtual void addTimerListener(TimerListener listener)
	  {
		timerListeners.Add(listener);
	  }

	  public virtual void removeTimerListener(TimerListener listener)
	  {
		timerListeners.Remove(listener);
	  }

	  protected internal virtual void incrementUpdate()
	  {
		IncrementEvent @event = new IncrementEvent(this);
		//Vector timerListeners = (Vector) this.timerListeners.clone ();

		for (int i = 0; i < timerListeners.Count; ++i)
		{
		 timerListeners[i].incrementUpdate(@event);
		}
	  }

	  protected internal virtual void alarmUpdate()
	  {
		AlarmEvent @event = new AlarmEvent(this);
		//Vector timerListeners = (Vector) this.timerListeners.clone ();

		for (int i = 0; i < timerListeners.Count; ++i)
		{
		  timerListeners[i].alarmUpdate(@event);
		}
	  }
	}

}