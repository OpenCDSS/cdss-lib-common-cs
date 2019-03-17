using System;
using System.IO;

// SecurityCheck - check various security settings

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
// SecurityCheck - check various security settings
// ----------------------------------------------------------------------------
// Notes:	(1)	Use SecurityCheckGUI if you want a GUI.
//		(2)	The debug messages in this code HAVE been wrapped with
//			isDebugOn.
// ----------------------------------------------------------------------------
// History:
//
// 05 Nov 1997	Steven A. Malers, RTi	Initial version based on tests with
//					IE4.0 problems.
// 14 Mar 1998	SAM, RTi		Add Javadoc.
// 07 Jan 2001	SAM, RTi		Change IO to IOUtil.  Change import *
//					to specific imports.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class provides some general capabilities to check security settings in a
	/// way that is simpler than the Java SecurityManager calls.  These static functions
	/// can be called to determine whether a file can be written, printed, etc. </summary>
	/// <seealso cref= java.lang.SecurityManager </seealso>
	public abstract class SecurityCheck
	{

	// Check the event queue...
	/// <returns> true if the AWT event queue can be checked, false if not. </returns>
	public static bool canCheckAWTEventQueue()
	{
		string routine = "SecurityCheck.canCheckAWTEventQueue";

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so do anything...
			return true;
		}

		try
		{
			sm.checkAwtEventQueueAccess();
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot access AWT event queue");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	/// <returns> true if the indicated file can be deleted, false if not. </returns>
	/// <param name="file"> Path to file to check. </param>
	public static bool canDeleteFile(string file)
	{
		string routine = "SecurityCheck.canDeleteFile";

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		// Need to put in default?

		try
		{
			sm.checkDelete(file);
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot write file to disk");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		catch (Exception e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Problem calling checkWrite()");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	// Check the command...
	/// <returns> true if the indicated command can be executed, false if not. </returns>
	/// <param name="command0"> Command to check. </param>
	public static bool canExec(string command0)
	{
		string routine = "SecurityCheck.canExec";
		string command = null, pc_default_command = "dir C:", unix_default_command = "ls";

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so allow anything...
			return true;
		}

		if (string.ReferenceEquals(command0, null))
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				command = unix_default_command;
			}
			else
			{
				command = pc_default_command;
			}
		}
		else if (command0.Length == 0)
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				command = unix_default_command;
			}
			else
			{
				command = pc_default_command;
			}
		}
		else
		{
			command = command0;
		}

		try
		{
			sm.checkExec(command);
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot run \"" + command + "\"");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	// Check to see if we can load a DLL...
	/// <returns> true if the indicated DLL can be linked, false if not. </returns>
	/// <param name="dll0"> DLL to check. </param>
	public static bool canLinkDLL(string dll0)
	{
		string routine = "SecurityCheck.canLinkDLL";
		string dll = null, pc_default_dll = "C:\\windows\\system\\netos.dll", unix_default_dll = "";

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		if (string.ReferenceEquals(dll0, null))
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				dll = unix_default_dll;
			}
			else
			{
				dll = pc_default_dll;
			}
		}
		else if (dll0.Length == 0)
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				dll = unix_default_dll;
			}
			else
			{
				dll = pc_default_dll;
			}
		}
		else
		{
			dll = dll0;
		}

		try
		{
			sm.checkLink(dll);
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot link DLL " + dll);
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	// Check SQL package...
	/// <returns> true if the indicated package can be loaded, false if not. </returns>
	/// <param name="some_package0"> Package to check. </param>
	public static bool canLoadPackage(string some_package0)
	{
		string routine = "SecurityCheck.canLoadPackage";
		string some_package;

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		string default_package = "java.sql.ResultSet";
		if (string.ReferenceEquals(some_package0, null))
		{
			some_package = default_package;
		}
		else if (some_package0.Length == 0)
		{
			some_package = default_package;
		}
		else
		{
			some_package = some_package0;
		}

		try
		{
			sm.checkPackageAccess(some_package);
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot run dir c:\\");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	// Check printer access...

	/// <returns> true if able to print, false if not. </returns>
	public static bool canPrint()
	{
		string routine = "SecurityCheck.canPrint";

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		try
		{
			sm.checkPrintJobAccess();
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot create print jobs");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	/// <returns> true if able to read the indicated file, false if not. </returns>
	/// <param name="file0"> File name to check. </param>
	public static bool canReadFile(string file0)
	{
		string routine = "SecurityCheck.canReadFile";
		string pc_default_file = "C:\\config.sys", unix_default_file = "/etc/hosts", file = null;

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		// See if we can read a file...

		if (string.ReferenceEquals(file0, null))
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				file = unix_default_file;
			}
			else
			{
				file = pc_default_file;
			}
		}
		else if (file0.Length == 0)
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				file = unix_default_file;
			}
			else
			{
				file = pc_default_file;
			}
		}
		else
		{
			file = file0;
		}

		FileStream @is = null;
		try
		{
			@is = new FileStream(file, FileMode.Open, FileAccess.Read);
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "Got FileInputStream for \"" + file + "\"");
			}
		}
		catch (Exception e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot create FileInputStream for \"" + file + "\"");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		finally
		{
			if (@is != null)
			{
				try
				{
					@is.Close();
				}
				catch (IOException)
				{
					// Should not happen
				}
			}
		}

		// Now try to read...

		try
		{
			sm.checkRead(file);
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot read file \"" + file + "\"");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		catch (Exception e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Problem calling checkRead()");
				Message.printDebug(2, routine, e);
			}
			return false;
		}

		return true;
	}

	// Check clibboard access...
	/// <returns> true if able to use the clipboard, false if not. </returns>
	public static bool canUseClipboard()
	{
		string routine = "SecurityCheck.canUseClipboard";

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		try
		{
			sm.checkSystemClipboardAccess();
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot access clipboard");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		return true;
	}

	// canWriteFile - see if we can write to the local system...
	//
	// Notes:	(1)	We open in append mode in case the file already exists
	//			so that we do not blow it away.
	/// <returns> true if able to write the indicated file, false if not. </returns>
	/// <param name="file0"> File name to check. </param>
	public static bool canWriteFile(string file0)
	{
		string routine = "SecurityCheck.canWriteFile";
		string pc_default_file = "C:\\SecurityCheck.tmp", unix_default_file = "/tmp/SecurityCheck.tmp", file = null;
		bool using_default = false;

		SecurityManager sm = System.getSecurityManager();
		if (sm == null)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Get null SecurityManager - no SecurityManager installed!");
			}
			// No security manager so can do anything...
			return true;
		}

		if (string.ReferenceEquals(file0, null))
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				file = unix_default_file;
			}
			else
			{
				file = pc_default_file;
			}
		}
		else if (file0.Length == 0)
		{
			// Use a default file for testing.  First figure out if we
			// are on UNIX or not...
			if (IOUtil.isUNIXMachine())
			{
				file = unix_default_file;
			}
			else
			{
				file = pc_default_file;
			}
		}
		else
		{
			file = file0;
		}

		// Now try the SecurityManager check...

		try
		{
			sm.checkWrite(file);
		}
		catch (SecurityException e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Cannot write " + file + " to disk");
				Message.printDebug(2, routine, e);
			}
			return false;
		}
		catch (Exception e)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(2, routine, "Problem calling checkWrite()");
				Message.printDebug(2, routine, e);
			}
			return false;
		}

		// If we are using the default file, try to remove the file...

		if (using_default)
		{
			File f = new File(file);
			f.delete();
		}

		return true;
	}

	} // End of SecurityCheck class

}