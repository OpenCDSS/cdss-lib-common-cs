using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

// StreamConsumer - read from a stream until nothing is left

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
// StreamConsumer - read from a stream until nothing is left
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-12-03	Steven A. Malers, RTi	Initial version.  Define for use with
//					ProcessManager, mainly to consume the
//					standard error of a process so that it
//					does not hang when its buffer gets full.
// 2005-04-26	J. Thomas Sapienza, RTi	Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.IO
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The StreamConsumer class is a thread that will read all the output from a
	/// stream and then expire.  This is used, for example, to read the standard error
	/// in ProcessManager so that a process that prints extensive output to standard
	/// error will not hang.  In the future this class may be extended to to buffer
	/// output and allow requests to retrieve the output.
	/// </summary>
	public class StreamConsumer : Thread
	{

	/// <summary>
	/// Input stream to consume output from.
	/// </summary>
	private Stream __is;

	/// <summary>
	/// Whether to log the information for the stream.  Message.printStatus(2,...) will be used.
	/// TODO SAM 2009-04-03 Evaluate passing a logger when Java logging is implemented.
	/// </summary>
	private bool __logOutput = false;

	/// <summary>
	/// Whether to save stream output, for retrieval with getOutputList().  This is simpler than adding listeners, etc.,
	/// and works well with standard error.
	/// </summary>
	private bool __saveOutput = false;

	/// <summary>
	/// The list of output returned with getOutputList().
	/// </summary>
	private IList<string> __outputList = new List<string>();

	/// <summary>
	/// Label with which to prefix all logged output.
	/// </summary>
	private string __label = null;

	/// <summary>
	/// Construct with an open InputStream. </summary>
	/// <param name="is"> InputStream to consume. </param>
	public StreamConsumer(Stream @is) : this(@is, null, false, false)
	{
	}

	/// <summary>
	/// Construct with an open InputStream. </summary>
	/// <param name="is"> InputStream to consume. </param>
	/// <param name="logOutput"> if true, log the stream output using Message.printStatus(2,...). </param>
	/// <param name="saveOutput"> if true, save the output in a list in memory, to return with getOutputList(). </param>
	public StreamConsumer(Stream @is, string label, bool logOutput, bool saveOutput)
	{
		__is = @is;
		__logOutput = logOutput;
		__saveOutput = saveOutput;
		__label = label;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~StreamConsumer()
	{
		__is = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the output list. </summary>
	/// <returns> the output list. </returns>
	public virtual IList<string> getOutputList()
	{
		return __outputList;
	}

	/// <summary>
	/// Start consuming the stream.  Data will be read until a null is read, at which time the thread will expire.
	/// </summary>
	public virtual void run()
	{ //String routine = "StreamConsumer.run";
		try
		{
			StreamReader br = new StreamReader(__is);
			string line;
			while (true)
			{
				//Message.printStatus ( 2, routine, "Reading another line...");
				line = br.ReadLine();
				//Message.printStatus ( 2, routine, "...done reading another line.");
				if (string.ReferenceEquals(line, null))
				{
					break;
				}
				// TODO SAM 2007-05-09 - this is where output could be passed to listening code.
				if (__logOutput)
				{
					if (string.ReferenceEquals(__label, null))
					{
						Message.printStatus(2, "StreamConsumer", "\"" + line + "\"");
					}
					else
					{
						Message.printStatus(2, "StreamConsumer", __label + "\"" + line + "\"");
					}
				}
				if (__saveOutput)
				{
					__outputList.Add(line);
				}
			}
		}
		catch (Exception)
		{
			// Should not happen - uncomment if troubleshooting
			// Exceptions may be thrown if the system is busy and there is a delay in closing
			// the file and the next pending read
			//String routine = "StreamConsumer.run";
			//Message.printWarning ( 2, routine, "Exception processing stream." );
			//Message.printWarning ( 2, routine, e );
		}
	}

	}

}