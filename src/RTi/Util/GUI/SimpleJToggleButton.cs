// SimpleJToggleButton - a simple button using a listener

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
// SimpleJToggleButton - a simple button using a listener
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-05-15	J. Thomas Sapienza, RTi	Initial version from SimpleJButton.
// 2003-10-27	JTS, RTi		Overrode paintBorder and removed call
//					to setPaintBorder() in response to 
//					changes made to the class between 
//					1.4.0 and 1.4.2.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	/// <summary>
	/// This class is used to simplify construction of a button and linkage to
	/// an action listener.  An example of it is as follows:
	/// <para>
	/// 
	/// <pre>
	/// // "this" is a GUI component, like a JFrame.
	/// button = new SimpleJToggleButton("My Button", "MyButton" + this);
	/// </pre>
	/// </para>
	/// </summary>
	public class SimpleJToggleButton : JToggleButton
	{

	/// <summary>
	/// Whether to paint the border when the button is not selected.
	/// </summary>
	private bool __paintDeselectedBorder = true;

	/// <summary>
	/// Construct a button by specifying the label and action listener.
	/// The command use for events is the same as the label. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(string label, ActionListener al, bool selected) : base(label, selected)
	{
		initialize(null, null, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(string label, string command, ActionListener al, bool selected) : base(label, selected)
	{
		initialize(command, null, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, insets, margin,
	/// and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of 
	/// the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed even when the 
	/// button is disable.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(string label, string command, Insets insets, bool margin, ActionListener al, bool selected) : base(label, selected)
	{
		initialize(command, null, insets, margin, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(string label, string command, string toolTipText, ActionListener al, bool selected) : base(label, selected)
	{
		initialize(command, toolTipText, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the label, command string, and action listener. </summary>
	/// <param name="label"> String label for button. </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of 
	/// the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed even when the
	/// button is disabled.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(string label, string command, string toolTipText, Insets insets, bool margin, ActionListener al, bool selected) : base(label, selected)
	{
		initialize(command, toolTipText, insets, margin, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(ImageIcon icon, string command, ActionListener al, bool selected) : base(icon, selected)
	{
		initialize(command, null, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, insets, margin,
	/// and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of 
	/// the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed even when the 
	/// button is disabled.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(ImageIcon icon, string command, Insets insets, bool margin, ActionListener al, bool selected) : base(icon, selected)
	{
		initialize(command, null, insets, margin, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(ImageIcon icon, string command, string toolTipText, ActionListener al, bool selected) : base(icon, selected)
	{
		initialize(command, toolTipText, null, true, al);
	}

	/// <summary>
	/// Construct a button by specifying the icon, command string, and action listener. </summary>
	/// <param name="icon"> the icon to display in the button </param>
	/// <param name="command"> Command string for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of 
	/// the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed even when the
	/// button is disabled.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	/// <param name="selected"> whether the button should initially be selected or not </param>
	public SimpleJToggleButton(ImageIcon icon, string command, string toolTipText, Insets insets, bool margin, ActionListener al, bool selected) : base(icon, selected)
	{
		initialize(command, toolTipText, insets, margin, al);
	}

	/// <summary>
	/// Initialize the button data. </summary>
	/// <param name="command"> Command string (button label) for events. </param>
	/// <param name="toolTipText"> the text to display as the button tool tip </param>
	/// <param name="insets"> the insets inside of the JButton that separate the edge of 
	/// the button from its contents </param>
	/// <param name="margin"> if true, the button's margin will be displayed even when the 
	/// button is disabled.  If false, it will not. </param>
	/// <param name="al"> Action listener. </param>
	private void initialize(string command, string toolTipText, Insets insets, bool margin, ActionListener al)
	{
		addActionListener(al);
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

		__paintDeselectedBorder = margin;
	}

	/// <summary>
	/// Overrides the default paintBorder() method from JToggleButton in order that
	/// the behavior is what we desire.  Namely, we want to be able to set the button
	/// so that the border only paints when the button is selected. </summary>
	/// <param name="g"> the Graphics context on which to draw the button. </param>
	protected internal virtual void paintBorder(Graphics g)
	{
		if (isBorderPainted())
		{
			if (!isSelected() && !__paintDeselectedBorder)
			{
				return;
			}
			else
			{
					Border border = getBorder();
					if (border != null)
					{
					border.paintBorder(this, g, 0, 0, getWidth(), getHeight());
					}
			}
		}
	}

	}

}