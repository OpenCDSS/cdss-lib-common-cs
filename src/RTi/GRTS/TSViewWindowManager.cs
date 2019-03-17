using System.Collections.Generic;

// TSViewWindowManager - window manager for TSView windows

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

	// TODO SAM 2011-11-29 Need to add listener ability, for example to let the TSTool UI know when
	// windows have been closed so that the "View - Close All View Windows" state can be updated.

	/// <summary>
	/// Window manager for TSView windows.  This is used, for example, to close all open windows with one method call.
	/// TODO SAM 2011-11-29 could probably make this more agnostic but for now focus on JFrames.
	/// </summary>
	public class TSViewWindowManager
	{

	/// <summary>
	/// The list of TSViewJFrame being managed.
	/// </summary>
	internal IList<TSViewJFrame> __tsviewList = new List<TSViewJFrame>();

	/// <summary>
	/// Construct the window manager.
	/// </summary>
	public TSViewWindowManager()
	{
	}

	/// <summary>
	/// Add a window to the manager. </summary>
	/// <param name="parent"> the TSViewJFrame that is managing the set of related views </param>
	/// <param name="frame"> an individual view in a group within a TSViewJFrame </param>
	public virtual void add(TSViewJFrame parent, JFrame frame)
	{
		// Currently only work with the parent since only the closeAll method is implemented.
		// If finer-grained behavior is needed, then start dealing with the individual windows separately.
		// Make sure to avoid duplicates, which complicate logic later when closing
		int pos = __tsviewList.IndexOf(parent);
		if (pos < 0)
		{
			__tsviewList.Add(parent);
		}
	}

	/// <summary>
	/// Close all the open views.
	/// </summary>
	public virtual void closeAll()
	{
		TSViewJFrame v;
		int closed = 0;
		for (int i = 0; i < __tsviewList.Count; i++)
		{
			v = __tsviewList[i];
			// Since this method closes everything, just close each possible window.
			// TODO SAM 2011-11-29 Maybe it would just be cleaner to have a TSViewJFrame.closeAll() method
			// that does the following?
			closed = v.closeGUI(TSViewType.GRAPH);
			if (closed >= 0)
			{
				closed = v.closeGUI(TSViewType.PROPERTIES);
			}
			if (closed >= 0)
			{
				closed = v.closeGUI(TSViewType.PROPERTIES_HIDDEN);
			}
			if (closed >= 0)
			{
				closed = v.closeGUI(TSViewType.SUMMARY);
			}
			if (closed >= 0)
			{
				closed = v.closeGUI(TSViewType.TABLE);
			}
			// The above calls will result in remove() being called, which will adjust the list, but need to
			// reposition the index to continue processing
			if (closed < 0)
			{
				--i;
			}
		}
	}

	/// <summary>
	/// Return the number of windows that are being managed.
	/// </summary>
	public virtual int getWindowCount()
	{
		return __tsviewList.Count;
	}

	/// <summary>
	/// Remove a TSViewJFrame from the list because all of its windows have been closed
	/// in direct interaction with the view windows.  That way the resources will no longer be
	/// managed by this manager.
	/// </summary>
	public virtual void remove(TSViewJFrame tsview)
	{
		// Go through the list and remove all matching instances
		TSViewJFrame v;
		for (int i = 0; i < __tsviewList.Count; i++)
		{
			v = __tsviewList[i];
			if (v == tsview)
			{
				__tsviewList.RemoveAt(i);
				//Message.printStatus(2, "remove", "Removed view at " +  i + " size=" + __tsviewList.size() );
				--i;
			}
		}
	}

	}

}