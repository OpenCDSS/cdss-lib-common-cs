// WindowManagerData - class to hold instance data that the WindowManager classes will manage

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
// WindowManagerData - class to hold instance data that the WindowManager
//	classes will manage.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 2004-02-16	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-26	JTS, RTi		Added finalize().
//------------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// The WindowManagerData class holds instance data for window instances 
	/// managed by the WindowManager.  This allows multiple instances of the same
	/// window type to be opened with different data contents.
	/// </summary>
	public class WindowManagerData
	{

	/// <summary>
	/// The status of the instance.
	/// </summary>
	private int __status = WindowManager.STATUS_UNMANAGED;

	/// <summary>
	/// The window for the instance.
	/// </summary>
	private JFrame __window = null;

	/// <summary>
	/// The unique identifier for the instance.
	/// </summary>
	private object __id = null;

	/// <summary>
	/// Constructor.  
	/// Creates an instance and initializes with a null window reference,
	/// null Object identifier, and window status of WindowManager.STATUS_UNMANAGED.
	/// </summary>
	public WindowManagerData()
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="window"> the window for the instance. </param>
	/// <param name="id"> the id for the instance. </param>
	/// <param name="status"> the status for the instance. </param>
	public WindowManagerData(JFrame window, object id, int status)
	{
		__window = window;
		__id = id;
		__status = status;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~WindowManagerData()
	{
		__window = null;
		__id = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the window instance's unique identifier. </summary>
	/// <returns> the window instance's unique identifier. </returns>
	public virtual object getID()
	{
		return __id;
	}

	/// <summary>
	/// Returns the window instance's status. </summary>
	/// <returns> the window instance's status. </returns>
	public virtual int getStatus()
	{
		return __status;
	}

	/// <summary>
	/// Returns the window instance's window. </summary>
	/// <returns> the window instance's window. </returns>
	public virtual JFrame getWindow()
	{
		return __window;
	}

	/// <summary>
	/// Set the window instance's ID. </summary>
	/// <param name="id"> value to put in the ID. </param>
	public virtual void setID(object id)
	{
		__id = id;
	}

	/// <summary>
	/// Sets the window instance's status. </summary>
	/// <param name="status"> value to put in the status. </param>
	public virtual void setStatus(int status)
	{
		__status = status;
	}

	/// <summary>
	/// Sets the window instance's window. </summary>
	/// <param name="window"> window to set the instance window to. </param>
	public virtual void setWindow(JFrame window)
	{
		__window = window;
	}

	/// <summary>
	/// Returns a String representation of the Object. </summary>
	/// <returns> a String representation of the Object. </returns>
	public override string ToString()
	{
		return "ID: '" + __id + "'  Status: " + __status;
	}

	}

}