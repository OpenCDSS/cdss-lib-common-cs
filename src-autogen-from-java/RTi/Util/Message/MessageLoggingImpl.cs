using System;

// MessageLoggingImpl - message logging implementation

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

/// 
/// <summary>
/// Created on February 19, 2007, 10:50 AM
/// 
/// </summary>

namespace RTi.Util.Message
{

	/// 
	/// <summary>
	/// @author iws
	/// </summary>
	public class MessageLoggingImpl : MessageImpl
	{

		private Logger status = Logger.getLogger("RTi.Util.Message.status");
		private Logger warning = Logger.getLogger("RTi.Util.Message.warning");
		private Logger debug = Logger.getLogger("RTi.Util.Message.debug");

		public static void install()
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			System.setProperty("RTi.Util.MessageImpl",typeof(MessageLoggingImpl).FullName);
		}

		public MessageLoggingImpl()
		{
			status.setLevel(Level.OFF);
			warning.setLevel(Level.WARNING);
			debug.setLevel(Level.OFF);
		}

		protected internal override void flushOutputFiles(int flag)
		{
			// do nothing
		}

		private Level translateWarningLevel(int level)
		{
			Level l;
			if (level <= 1)
			{
				l = Level.WARNING;
			}
			else if (level <= 10)
			{
				l = Level.FINE;
			}
			else
			{
				l = Level.FINER;
			}
			return l;
		}

		private Level translateDebugLevel(int level)
		{
			Level l;
			if (level <= 1)
			{
				l = Level.FINE;
			}
			else if (level <= 10)
			{
				l = Level.FINER;
			}
			else
			{
				l = Level.FINEST;
			}
			return l;
		}

		protected internal override void printWarning(int l, string routine, Exception e)
		{
			log(warning,translateWarningLevel(l),routine,e);
		}

		protected internal override void printDebug(int l, string routine, Exception e)
		{
			log(debug,translateDebugLevel(l),routine,e);
		}

		protected internal override void printWarning(int l, string routine, string message, JFrame top_level)
		{
			printWarning(l,routine,message);
		}

		protected internal override void printWarning(int l, string routine, string message)
		{
			log(warning,translateWarningLevel(l),routine,message);
		}

		protected internal override void printStatus(int l, string routine, string message)
		{
			log(status,translateDebugLevel(l),routine,message);
		}

		protected internal override void printWarning(int l, string tag, string routine, string message)
		{
			log(warning,translateWarningLevel(l),routine,message);
		}

		protected internal override void printDebug(int l, string routine, string message)
		{
			log(debug,translateDebugLevel(l),routine,message);
		}

		protected internal override void initStreams()
		{

		}

		private void log(Logger logger, Level level, string routine, string message)
		{
			if (logger.isLoggable(level))
			{
				int idx = routine.IndexOf('.');
				string clazz = null;
				string method = null;
				if (idx >= 0)
				{
					clazz = routine.Substring(0,idx);
					method = routine.Substring(idx + 1, routine.Length - (idx + 1));
				}
				else
				{
					clazz = routine;
				}
				logger.logp(level,clazz,method,message);
			}
		}

		private void log(Logger logger, Level level, string routine, Exception e)
		{
			if (logger.isLoggable(level))
			{
				int idx = routine.IndexOf('.');
				string clazz = null;
				string method = null;
				if (idx >= 0)
				{
					clazz = routine.Substring(0,idx);
					method = routine.Substring(idx + 1, routine.Length - (idx + 1));
				}
				else
				{
					clazz = routine;
				}
				logger.logp(level,clazz,method,e.Message,e);
			}
		}


	}

}