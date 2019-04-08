// SimpleJButton - a simple button using a listener

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
// SimpleJButton - a simple button using a listener
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 10 Nov 1997	Steven A. Malers, RTi	Initial version based on several
//					examples.
// 2001-11-27	SAM, RTi		Remove import *.
// 2001-12-03	SAM, RTi		Update to use Swing.
// 2003-05-15	J. Thomas Sapienza, RTi	Added a couple new constructors:
//					* one to take an image icon
//					* image icon & insets & margin
//					* image icon & tooltip
//					* image icon & insets & margin & tooltip
//					* text & insets & margin
//					* text & tooltip
//					* text & insets & margin & tooltip
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	/// <summary>
	/// This class is used to simplify construction of a button and linkage to
	/// an action listener.  An example of its is as follows:
	/// <para>
	/// <pre>
	/// // "this" is a GUI component, like a JFrame.
	/// button = new SimpleJButton("My Button", "MyButton" + this );
	/// </pre>
	/// </para>
	/// </summary>
	public class SimpleJButton : JButton
	{

	/// <summary>
	/// Construct a button by specifying the label and action listener.
	/// The command use for events is the same as the label. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(string label, ActionListener al) : base(label)
	{
		initialize(null, null, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(string label, string command, ActionListener al) : base(label)
	{
		initialize(command, null, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, insets, margin, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of 
	/// the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(string label, string command, Insets insets, bool margin, ActionListener al) : base(label)
	{
		initialize(command, null, insets, margin, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(string label, string command, string toolTipText, ActionListener al) : base(label)
	{
		initialize(command, toolTipText, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(string label, string command, string toolTipText, Insets insets, bool margin, ActionListener al) : base(label)
	{
		initialize(command, toolTipText, insets, margin, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(ImageIcon icon, string command, ActionListener al) : base(icon)
	{
		initialize(command, null, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, insets, margin, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(ImageIcon icon, string command, Insets insets, bool margin, ActionListener al) : base(icon)
	{
		initialize(command, null, insets, margin, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(ImageIcon icon, string command, string toolTipText, ActionListener al) : base(icon)
	{
		initialize(command, toolTipText, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	public SimpleJButton(ImageIcon icon, string command, string toolTipText, Insets insets, bool margin, ActionListener al) : base(icon)
	{
		initialize(command, toolTipText, insets, margin, al);
	}

	/// <summary>
	/// Initialize the button data. </summary>
	/// <param name="command"> Command string (button label)for events. </param>
	/// <param name="al"> Action listener. </param>
	private void initialize(string command, string toolTipText, Insets insets, bool margin, ActionListener al)
	{
		if (al != null)
		{
			addActionListener(al);
		}
		if (!string.ReferenceEquals(command, null))
		{
			if (command.Length > 0)
			{
				setActionCommand(command);
			}
		}

		if (!string.ReferenceEquals(toolTipText, null))
		{
			setToolTipText(toolTipText);
		}

		if (insets != null)
		{
			setMargin(insets);
		}

		setBorderPainted(margin);
	}

	}

}