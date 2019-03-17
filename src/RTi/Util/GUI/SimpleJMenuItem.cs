// SimpleJMenuItem - a simple menu item to add a listener

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
// SimpleJMenuItem - a simple menu item to add a listener
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2001-11-03	Steven A. Malers, RTi	Initial version based on SimpleMenuItem.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// This class simplifies the addition of menu items by combining the construction
	/// and event handler setup.  An example of use is:
	/// <para>
	/// 
	/// <pre>
	///   JMenu mymenu = new JMenu ( "MyMenu" );
	///   mymenu.add ( new SimpleJMenuItem("Exit","Myapp.Exit", this );
	/// </pre>
	/// </para>
	/// <para>
	/// 
	/// Then implement an ActionListener interface and check for events with a command
	/// name of "Myapp.Exit".
	/// </para>
	/// </summary>
	public class SimpleJMenuItem : JMenuItem
	{

	/// <summary>
	/// Construct a menu item given a menu label and action listener.  The event
	/// command will be the same as the label. </summary>
	/// <param name="label"> Menu label. </param>
	/// <param name="al"> ActionListener. </param>
	public SimpleJMenuItem(string label, ActionListener al) : this(label, null, al)
	{
	}

	/// <summary>
	/// Construct a menu item given a menu label, event command, and action listener. </summary>
	/// <param name="label"> Menu label. </param>
	/// <param name="command"> Event command string. </param>
	/// <param name="al"> ActionListener. </param>
	public SimpleJMenuItem(string label, string command, ActionListener al) : base(label)
	{
		addActionListener(al);
		if (!string.ReferenceEquals(command, null))
		{
			if (command.Length > 0)
			{
				setActionCommand(command);
			}
		}
	}

	}

}