using System;
using System.Collections.Generic;

// GUIUtil - GUI utility methods class, containing static methods

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

//-----------------------------------------------------------------------------
// GUIUtil - GUI utility methods class, containing static methods
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 27 Oct 1997	Darrell Gillmeister,	Created initial class description.
//		RTi
// 13 Nov 1997	Catherine Nutting-Lane,	Overloaded addComponent to include
//		RTi			inset as individual parameters.
// 25 Feb 1998	Steven A. Malers, RTi	Add the createCanvas functions and
//					started adding javadoc comments - to be
//					completed by others.
// 07 May 1998  DLG, RTi		Complete javadoc comments.
// 23 Jun 1998  CGB, RTi		Added setCursor and setReady utility
//					methods for managing the components.
// 08 Dec 1998	SAM, RTi		Add setWaitCursor to make it easier to
//					use.  Optimize some of the cursor code.
// 09 Sep 1999	SAM, RTi		Remove import java.awt.* to start
//					optimizing class packaging.
// 13 Dec 2000	SAM, RTi		Change so isChoiceItem() does not
//					require the index array.
// 19 Dec 2000	SAM, RTi		Add:	deselectAll(List)
//						selectAll(List)
//						size(List)
//						updateListSelections(...)
// 03 Jan 2001	SAM, RTi		Add isListItem() to see if a string is
//					in a list.  Make it simpler than
//					isChoiceItem().  Add select(List...).
// 06 Jan 2001	SAM, RTi		Copy GUI to GUIUtil and deprecate all
//					the GUI methods.  This is being done to
//					be consistent with other "Util" classes.
//					Clean up Javadoc.  Optimize garbage
//					collection by setting unused variables
//					to null.
// 15 Jan 2001	SAM, RTi		Add select (List, int[]).  Add
//					indexOf ( List ) and deprecate
//					isListItem().
// 17 Jan 2001	SAM, RTi		Add shiftDown() and controlDown() to
//					help with list handling.  Add
//					close(Frame).
// 23 Feb 2001	SAM, RTi		Add addStringToSelected(),
//					removeStringFromSelected().
// 16 Apr 2001	SAM, RTi		Add getLastFileDialogDirectory() and
//					setLastFileDialogDirectory() and static
//					string for directory.  This can be used
//					in a generic way to track the last
//					directory accessed.
// 2001-10-10	SAM, RTi		Add addToChoice().
// 2002-01-10	SAM, RTi		Add call to sync() in setWaitCursor()
//					to try to make cursor change without
//					needing to move the mouse.
// 2002-01-27	SAM, RTi		Add newFontNameChoice().
// 2002-02-02	SAM, RTi		Add selectIgnoreCase(Choice) to help
//					select properties.
// 2002-05-16	SAM, RTi		Add getFontStyle(String).
// 2002-06-09	SAM, RTi		Add selectRegionMatches(Choice).
// 2002-11-05	SAM, RTi		Update deselectAll() to only change
//					the status on selected items - this
//					should improve.
// 2003-03-25	SAM, RTi		Add selectTokenMatches() to more easily
//					make selections in formatted choices.
// 2003-05-08	J. Thomas Sapienza, RTi	Added versions of addComponent that 
//					take double arguments for weightx and
//					weighty.  This value in the grid bag
//					constraints is a double (0.0 to 1.0)
//					so the integer parameters that were
//					being used were mostly useless.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.GUI
{

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides useful static functions for handling GUI (graphical user
	/// interface) components.  Appropriate overloads for functions are provided where
	/// useful.  This class was previously named "GUI" but was renamed to be consistent
	/// with other utility classes.
	/// </summary>
	public abstract class GUIUtil
	{

	/// <summary>
	/// Do not check substrings when determining if a String exists in a Choice.
	/// </summary>
	public const int NONE = 0;

	/// <summary>
	/// Check substrings when determining if a String exists in a Choice.
	/// </summary>
	public const int CHECK_SUBSTRINGS = 1;

	/// <summary>
	/// A general purpose bit flag that can be used in GUI component constructors.
	/// </summary>
	public const int GUI_VISIBLE = 0x1;

	/// <summary>
	/// The following indicate whether the shift and control keys are pressed.  This is
	/// used with methods like updateListSelections.
	/// </summary>
	private static bool _control_down = false;
	private static bool _shift_down = false;

	/// <summary>
	/// The following saves the last directory saved with a FileDialog.
	/// </summary>
	private static string _last_file_dialog_directory = null;

	/// <summary>
	/// Add a component to a container that uses GridBagLayout using the specified
	/// constraints.
	/// <pre>
	/// example:
	/// Panel p;
	/// GridBagConstraints gbc;
	/// Label l;
	/// p = new Panel();
	/// p.setLayout( new GridBagLayout() );
	/// GUIUtil.addComponent( p, l = new Label("test"),
	/// 0, 0, 1, 1, 0, 0, gbc.NONE, gbc.WEST );
	/// </pre> </summary>
	/// <param name="container"> Container using the GridBagLayout (e.g., Panel). </param>
	/// <param name="component"> Component to add to the container. </param>
	/// <param name="gridx"> Relative grid location in the x direction (0+, starting at left). </param>
	/// <param name="gridy"> Relative grid location in the y direction (0+, starting at top). </param>
	/// <param name="gridwidth"> Number of columns the component occupies. </param>
	/// <param name="gridheight"> Number of rows the component occupies. </param>
	/// <param name="weightx"> Distributed spacing ratio in x-direction (0 - 1) (e.g., if
	/// 3 components are added and 2 have weights of 1, then extra space is distributed
	/// evenly among the two components). </param>
	/// <param name="weighty"> Distributed spacing ratio in y-direction (0 - 1). </param>
	/// <param name="fill"> Component's resize policy, as
	/// GridBagConstraint.NONE, GridBagConstraint.BOTH, GridBagConstraint.HORIZONTAL,
	/// or GridBagConstraint.VERTICAL. </param>
	/// <param name="anchor"> Component's drift direction, as GridBagConstraint.CENTER,
	/// GridBagConstraint.NORTH, GridBagConstraint.SOUTH, GridBagConstraint.EAST,
	/// GridBagConstraint.WEST, GridBagConstraint.NORTHEAST, GridBagConstraint.NORTHWEST,
	/// GridBagConstraint.SOUTHEAST, or GridBagConstraint.SOUTHWEST. </param>
	public static void addComponent(Container container, Component component, int gridx, int gridy, int gridwidth, int gridheight, int weightx, int weighty, int fill, int anchor)
	{
		addComponent(container, component, gridx, gridy, gridwidth, gridheight, (double)weightx, (double)weighty, fill, anchor);
	}

	/// <summary>
	/// Add a component to a container that uses GridBagLayout using the specified
	/// constraints.
	/// <pre>
	/// example:
	/// Panel p;
	/// GridBagConstraints gbc;
	/// Label l;
	/// p = new Panel();
	/// p.setLayout( new GridBagLayout() );
	/// GUIUtil.addComponent( p, l = new Label("test"),
	/// 0, 0, 1, 1, 0, 0, gbc.NONE, gbc.WEST );
	/// </pre> </summary>
	/// <param name="container"> Container using the GridBagLayout (e.g., Panel). </param>
	/// <param name="component"> Component to add to the container. </param>
	/// <param name="gridx"> Relative grid location in the x direction (0+, starting at left). </param>
	/// <param name="gridy"> Relative grid location in the y direction (0+, starting at top). </param>
	/// <param name="gridwidth"> Number of columns the component occupies. </param>
	/// <param name="gridheight"> Number of rows the component occupies. </param>
	/// <param name="weightx"> Distributed spacing ratio in x-direction (0 - 1) (e.g., if
	/// 3 components are added and 2 have weights of 1, then extra space is distributed
	/// evenly among the two components). </param>
	/// <param name="weighty"> Distributed spacing ratio in y-direction (0 - 1). </param>
	/// <param name="fill"> Component's resize policy, as
	/// GridBagConstraint.NONE, GridBagConstraint.BOTH, GridBagConstraint.HORIZONTAL,
	/// or GridBagConstraint.VERTICAL. </param>
	/// <param name="anchor"> Component's drift direction, as GridBagConstraint.CENTER,
	/// GridBagConstraint.NORTH, GridBagConstraint.SOUTH, GridBagConstraint.EAST,
	/// GridBagConstraint.WEST, GridBagConstraint.NORTHEAST, GridBagConstraint.NORTHWEST,
	/// GridBagConstraint.SOUTHEAST, or GridBagConstraint.SOUTHWEST. </param>
	public static void addComponent(Container container, Component component, int gridx, int gridy, int gridwidth, int gridheight, double weightx, double weighty, int fill, int anchor)
	{
		LayoutManager lm = container.getLayout();
			GridBagConstraints gbc = new GridBagConstraints();
			gbc.gridx = gridx;
			gbc.gridy = gridy;
			gbc.gridwidth = gridwidth;
			gbc.gridheight = gridheight;
			gbc.weightx = weightx;
			gbc.weighty = weighty;
			gbc.fill = fill;
			gbc.anchor = anchor;
			((GridBagLayout)lm).setConstraints(component, gbc);
			container.add(component);
	}

	/// <summary>
	/// Add a component to a container that uses GridBagLayout using the specified
	/// constraints.
	/// <pre>
	/// example:
	/// Panel p;
	/// GridBagConstraints gbc;
	/// Label l;
	/// p = new Panel();
	/// Insets insets = new Insets( 5,5,5,5 )
	/// p.setLayout( new GridBagLayout() );
	/// GUIUtil.addComponent( p, l = new Label("test"),
	/// 0, 0, 1, 1, 0, 0, insets, gbc.NONE, gbc.WEST );
	/// </pre> </summary>
	/// <param name="container"> Container using the GridBagLayout (e.g., Panel). </param>
	/// <param name="component"> Component to add to the container. </param>
	/// <param name="gridx"> Relative grid location in the x direction (0+, starting at left). </param>
	/// <param name="gridy"> Relative grid location in the y direction (0+, starting at top). </param>
	/// <param name="gridwidth"> Number of columns the component occupies. </param>
	/// <param name="gridheight"> Number of rows the component occupies. </param>
	/// <param name="weightx"> Distributed spacing ratio in x-direction (0 - 1) (e.g., if
	/// 3 components are added and 2 have weights of 1, then extra space is distributed
	/// evenly among the two components). </param>
	/// <param name="weighty"> Distributed spacing ratio in y-direction (0 - 1). </param>
	/// <param name="insets"> External padding in pixels around the component. </param>
	/// <param name="fill"> Component's resize policy, as
	/// GridBagConstraint.NONE, GridBagConstraint.BOTH, GridBagConstraint.HORIZONTAL,
	/// or GridBagConstraint.VERTICAL. </param>
	/// <param name="anchor"> Component's drift direction, as GridBagConstraint.CENTER,
	/// GridBagConstraint.NORTH, GridBagConstraint.SOUTH, GridBagConstraint.EAST,
	/// GridBagConstraint.WEST, GridBagConstraint.NORTHEAST, GridBagConstraint.NORTHWEST,
	/// GridBagConstraint.SOUTHEAST, or GridBagConstraint.SOUTHWEST. </param>
	public static void addComponent(Container container, Component component, int gridx, int gridy, int gridwidth, int gridheight, int weightx, int weighty, Insets insets, int fill, int anchor)
	{
		addComponent(container, component, gridx, gridy, gridwidth, gridheight, (double)weightx, (double)weighty, insets, fill, anchor);
	}


	/// <summary>
	/// Add a component to a container that uses GridBagLayout using the specified
	/// constraints.
	/// <pre>
	/// example:
	/// Panel p;
	/// GridBagConstraints gbc;
	/// Label l;
	/// p = new Panel();
	/// Insets insets = new Insets( 5,5,5,5 )
	/// p.setLayout( new GridBagLayout() );
	/// GUIUtil.addComponent( p, l = new Label("test"),
	/// 0, 0, 1, 1, 0, 0, insets, gbc.NONE, gbc.WEST );
	/// </pre> </summary>
	/// <param name="container"> Container using the GridBagLayout (e.g., Panel). </param>
	/// <param name="component"> Component to add to the container. </param>
	/// <param name="gridx"> Relative grid location in the x direction (0+, starting at left). </param>
	/// <param name="gridy"> Relative grid location in the y direction (0+, starting at top). </param>
	/// <param name="gridwidth"> Number of columns the component occupies. </param>
	/// <param name="gridheight"> Number of rows the component occupies. </param>
	/// <param name="weightx"> Distributed spacing ratio in x-direction (0 - 1) (e.g., if
	/// 3 components are added and 2 have weights of 1, then extra space is distributed
	/// evenly among the two components). </param>
	/// <param name="weighty"> Distributed spacing ratio in y-direction (0 - 1). </param>
	/// <param name="insets"> External padding in pixels around the component. </param>
	/// <param name="fill"> Component's resize policy, as
	/// GridBagConstraint.NONE, GridBagConstraint.BOTH, GridBagConstraint.HORIZONTAL,
	/// or GridBagConstraint.VERTICAL. </param>
	/// <param name="anchor"> Component's drift direction, as GridBagConstraint.CENTER,
	/// GridBagConstraint.NORTH, GridBagConstraint.SOUTH, GridBagConstraint.EAST,
	/// GridBagConstraint.WEST, GridBagConstraint.NORTHEAST, GridBagConstraint.NORTHWEST,
	/// GridBagConstraint.SOUTHEAST, or GridBagConstraint.SOUTHWEST. </param>
	public static void addComponent(Container container, Component component, int gridx, int gridy, int gridwidth, int gridheight, double weightx, double weighty, Insets insets, int fill, int anchor)
	{
		LayoutManager lm = container.getLayout();
			GridBagConstraints gbc = new GridBagConstraints();
			gbc.gridx = gridx;
			gbc.gridy = gridy;
			gbc.gridwidth = gridwidth;
			gbc.gridheight = gridheight;
			gbc.weightx = weightx;
			gbc.weighty = weighty;
			gbc.insets = insets;
			gbc.fill = fill;
			gbc.anchor = anchor;
			((GridBagLayout)lm).setConstraints(component, gbc);
			container.add(component);
	}

	/// <summary>
	/// This version is identical to the previous addComponent() except that the
	/// insets are passed as 4 separate integer values rather than one Inset value. </summary>
	/// <param name="container"> Container using the GridBagLayout (e.g., Panel). </param>
	/// <param name="component"> Component to add to the container. </param>
	/// <param name="gridx"> Relative grid location in the x direction (0+, starting at left). </param>
	/// <param name="gridy"> Relative grid location in the y direction (0+, starting at top). </param>
	/// <param name="gridwidth"> Number of columns the component occupies. </param>
	/// <param name="gridheight"> Number of rows the component occupies. </param>
	/// <param name="weightx"> Distributed spacing ratio in x-direction (0 - 1) (e.g., if
	/// 3 components are added and 2 have weights of 1, then extra space is distributed
	/// evenly among the two components). </param>
	/// <param name="weighty"> Distributed spacing ratio in y-direction (0 - 1). </param>
	/// <param name="top_inset"> External top padding in pixels around the component. </param>
	/// <param name="left_inset"> External left padding in pixels around the component. </param>
	/// <param name="bottom_inset"> External bottom padding in pixels around the component. </param>
	/// <param name="right_inset"> External right padding in pixels around the component. </param>
	/// <param name="fill"> Component's resize policy, as
	/// GridBagConstraint.NONE, GridBagConstraint.BOTH, GridBagConstraint.HORIZONTAL,
	/// or GridBagConstraint.VERTICAL. </param>
	/// <param name="anchor"> Component's drift direction, as GridBagConstraint.CENTER,
	/// GridBagConstraint.NORTH, GridBagConstraint.SOUTH, GridBagConstraint.EAST,
	/// GridBagConstraint.WEST, GridBagConstraint.NORTHEAST, GridBagConstraint.NORTHWEST,
	/// GridBagConstraint.SOUTHEAST, or GridBagConstraint.SOUTHWEST. </param>
	public static void addComponent(Container container, Component component, int gridx, int gridy, int gridwidth, int gridheight, int weightx, int weighty, int top_inset, int left_inset, int bottom_inset, int right_inset, int fill, int anchor)
	{
		addComponent(container, component, gridx, gridy, gridwidth, gridheight, (double)weightx, (double)weighty, top_inset, left_inset, bottom_inset, right_inset, fill, anchor);
	}

	/// <summary>
	/// This version is identical to the previous addComponent() except that the
	/// insets are passed as 4 separate integer values rather than one Inset value. </summary>
	/// <param name="container"> Container using the GridBagLayout (e.g., Panel). </param>
	/// <param name="component"> Component to add to the container. </param>
	/// <param name="gridx"> Relative grid location in the x direction (0+, starting at left). </param>
	/// <param name="gridy"> Relative grid location in the y direction (0+, starting at top). </param>
	/// <param name="gridwidth"> Number of columns the component occupies. </param>
	/// <param name="gridheight"> Number of rows the component occupies. </param>
	/// <param name="weightx"> Distributed spacing ratio in x-direction (0 - 1) (e.g., if
	/// 3 components are added and 2 have weights of 1, then extra space is distributed
	/// evenly among the two components). </param>
	/// <param name="weighty"> Distributed spacing ratio in y-direction (0 - 1). </param>
	/// <param name="top_inset"> External top padding in pixels around the component. </param>
	/// <param name="left_inset"> External left padding in pixels around the component. </param>
	/// <param name="bottom_inset"> External bottom padding in pixels around the component. </param>
	/// <param name="right_inset"> External right padding in pixels around the component. </param>
	/// <param name="fill"> Component's resize policy, as
	/// GridBagConstraint.NONE, GridBagConstraint.BOTH, GridBagConstraint.HORIZONTAL,
	/// or GridBagConstraint.VERTICAL. </param>
	/// <param name="anchor"> Component's drift direction, as GridBagConstraint.CENTER,
	/// GridBagConstraint.NORTH, GridBagConstraint.SOUTH, GridBagConstraint.EAST,
	/// GridBagConstraint.WEST, GridBagConstraint.NORTHEAST, GridBagConstraint.NORTHWEST,
	/// GridBagConstraint.SOUTHEAST, or GridBagConstraint.SOUTHWEST. </param>
	public static void addComponent(Container container, Component component, int gridx, int gridy, int gridwidth, int gridheight, double weightx, double weighty, int top_inset, int left_inset, int bottom_inset, int right_inset, int fill, int anchor)
	{
		LayoutManager lm = container.getLayout();
			GridBagConstraints gbc = new GridBagConstraints();
			Insets insets = new Insets(top_inset, left_inset, bottom_inset, right_inset);
			gbc.gridx = gridx;
			gbc.gridy = gridy;
			gbc.gridwidth = gridwidth;
			gbc.gridheight = gridheight;
			gbc.weightx = weightx;
			gbc.weighty = weighty;
			gbc.insets = insets;
			gbc.fill = fill;
			gbc.anchor = anchor;
			((GridBagLayout)lm).setConstraints(component, gbc);
			container.add(component);
	}

	/// <summary>
	/// Given a list with selected items, add the specified string to the front of the
	/// items if it is not already at the front of the items.  After the changes, the
	/// originally selected items are still selected.  This is useful, for example,
	/// when a popup menu toggles the contents of a list back and forth. </summary>
	/// <param name="list"> List to modify. </param>
	/// <param name="prefix"> String to add. </param>
	public static void addStringToSelected(List list, string prefix)
	{
		if ((list == null) || (string.ReferenceEquals(prefix, null)))
		{
			return;
		}
		int[] selected_indexes = list.getSelectedIndexes();
		int selected_size = selectedSize(list);
		int len = prefix.Length;
		for (int i = 0; i < selected_size; i++)
		{
			if (!list.getItem(selected_indexes[i]).Trim().regionMatches(true,0,prefix,0,len))
			{
				list.replaceItem(prefix + list.getItem(selected_indexes[i]), selected_indexes[i]);
			}
		}
		// Make sure the selected indices remain as before...
		select(list, selected_indexes);
		selected_indexes = null;
	}

	/// <summary>
	/// Add an array of strings to a list.  This is useful when a standard set of
	/// choices are available. </summary>
	/// <param name="choice"> Choice to add items to. </param>
	/// <param name="items"> Items to add. </param>
	public static void addToChoice(Choice choice, string[] items)
	{
		if ((choice == null) || (items == null))
		{
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			choice.add(items[i]);
		}
	}

	/// <summary>
	/// Add a list of strings to a list.  This is useful when a standard set of
	/// choices are available.  The toString() method of each object in the list is
	/// called, so even non-String items can be added. </summary>
	/// <param name="choice"> Choice to add items to. </param>
	/// <param name="items"> Items to add. </param>
	public static void addToChoice(Choice choice, IList<object> items)
	{
		if ((choice == null) || (items == null))
		{
			return;
		}
		int size = items.Count;
		for (int i = 0; i < size; i++)
		{
			choice.add(items[i].ToString());
		}
	}

	/// <summary>
	/// Center a Component in the screen.  For example, center on the screen where the main JFrame exists.
	/// <b>NOTE</b>: Make sure to call this <b>AFTER</b> calling pack(), or else it will produce unexpected results.
	/// This seems to properly centers the component on the screen that originated the component.
	/// However, some components (JFrames?) require that the parent component be specified (see the overloaded version).
	/// <pre>
	/// example:
	/// JDialog d;
	/// d.pack();
	/// GUIUtil.center( d, parentFrame );
	/// </pre> </summary>
	/// <param name="c"> Component object to center </param>
	public static void center(Component c)
	{
		// Call overloaded version and pass component as parent.
		// This will center on screen where component originated.
		center(c, c);
	}

	/// <summary>
	/// Center a Component in the screen using the current frame and screen dimensions.
	/// <b>NOTE</b>: Make sure to call this <b>AFTER</b> calling pack(), or else it will produce unexpected results.
	/// This always centers the component on the first screen (screen 0).
	/// <pre>
	/// example:
	/// Frame f;
	/// f.pack();
	/// GUIUtil.center( f );
	/// </pre> </summary>
	/// <param name="c"> Component object. </param>
	/// <param name="parent"> parent of c that defines screen on which to center </param>
	public static void center(Component c, Component parent)
	{
		Dimension component = c.getSize();
		int componentWidth = component.width;
		int componentHeight = component.height;
		Toolkit kit = c.getToolkit();
		if (parent == null)
		{
			// Get dimensions for screen (always primary screen = screen 0)
			Dimension screenDimension = kit.getScreenSize();

			// Determine heights and widths

			int screenWidth = screenDimension.width;
			int screenHeight = screenDimension.height;

			// Determine centered coordinates and set the location
			int x = (screenWidth - componentWidth) / 2;
			int y = (screenHeight - componentHeight) / 2;

			// Adjust if x or y are off the screen to make sure window controls are visible
			if (x < 0)
			{
				x = 0;
			}
			if ((y + componentHeight) > screenHeight)
			{
				y = 0;
			}
			c.setLocation(x, y);
		}
		else
		{
			// Try newer approach for multiple monitors
			GraphicsConfiguration gc = parent.getGraphicsConfiguration();
			Rectangle parentScreenBounds = gc.getBounds();
			// x and y of the bounds will be zero for the primary screen.
			// x will be the width of the primary if two devices (monitors) and the second is used by the parent (y will be zero)
			// Adjust if x or y are off the screen to make sure window controls are visible
			int x = parentScreenBounds.x + (parentScreenBounds.width - componentWidth) / 2;
			int y = parentScreenBounds.y + (parentScreenBounds.height - componentHeight) / 2;
			if (x < 0)
			{
				x = parentScreenBounds.x;
			}
			if ((y + componentHeight) > parentScreenBounds.height)
			{
				y = parentScreenBounds.y;
			}
			c.setLocation(x,y);
		}
	}

	/// <summary>
	/// Hides and dispose of a Frame Object. </summary>
	/// <param name="frame"> Frame object to hide. </param>
	public static void close(Frame frame)
	{
		if (frame != null)
		{
			frame.setVisible(false);
			frame.dispose();
		}
	}

	/// <summary>
	/// Indicate whether the control key is pressed.  This is used with the
	/// setControlDown() method. </summary>
	/// <returns> if Control is pressed. </returns>
	public static bool controlDown()
	{
		return _control_down;
	}

	/// <summary>
	/// Create a canvas and set the color.  This is useful when a canvas is needed as a
	/// place-holder because a full canvas object cannot be created for some reason (no
	/// data, error, etc.). </summary>
	/// <returns> A new instance of a canvas of specified size and background color. </returns>
	/// <param name="width"> Width of the canvas. </param>
	/// <param name="height"> Height of the canvas. </param>
	/// <param name="color"> Background color for the canvas.
	/// If the color is null, white is used. </param>
	public static Canvas createCanvas(int width, int height, Color color)
	{
		Canvas canvas = new Canvas();
		if (color == null)
		{
			canvas.setBackground(Color.white);
		}
		else
		{
			canvas.setBackground(color);
		}
		canvas.setSize(width, height);
		return canvas;
	}

	/// <summary>
	/// Create a canvas with a white background. </summary>
	/// <returns> A new instance of a canvas of specified size and white background. </returns>
	/// <param name="width"> Width of the canvas. </param>
	/// <param name="height"> Height of the canvas. </param>
	public static Canvas createCanvas(int width, int height)
	{
		return createCanvas(width, height, Color.white);
	}

	/// <summary>
	/// Deselect all items in a List. </summary>
	/// <param name="list"> List to deselect all items. </param>
	public static void deselectAll(List list)
	{
		if (list == null)
		{
			return;
		}
		int[] selected = list.getSelectedIndexes();
		int size = 0;
		if (selected != null)
		{
			size = list.getItemCount();
		}
		for (int i = 0; i < size; i++)
		{
			list.deselect(i);
		}
	}

	/// <summary>
	/// Return the font style (mask of Font.BOLD, Font.ITALIC, Font.PLAIN) given
	/// a string.  The string is searched for case-independent occurances of "Bold",
	/// "Italic", "Plain" and the integer equivalent is returned. </summary>
	/// <param name="style"> Font style as string.  Return Font.PLAIN if the style is not a
	/// standard value. </param>
	/// <returns> a font style integer. </returns>
	public static int getFontStyle(string style)
	{
		int istyle = 0;
		if (StringUtil.indexOfIgnoreCase(style,"Plain",0) >= 0)
		{
			istyle |= Font.PLAIN;
		}
		else if (StringUtil.indexOfIgnoreCase(style,"Italic",0) >= 0)
		{
			istyle |= Font.ITALIC;
		}
		else if (StringUtil.indexOfIgnoreCase(style,"Bold",0) >= 0)
		{
			istyle |= Font.BOLD;
		}
		if (istyle == 0)
		{
			return Font.PLAIN;
		}
		else
		{
			return istyle;
		}
	}

	/// <summary>
	/// Return the last directory set with setLastFileDialogDirectory().  This will
	/// have the directory separator at the end of the string. </summary>
	/// <returns> the last directory set with setLastFileDialogDirectory(). </returns>
	public static string getLastFileDialogDirectory()
	{
		return _last_file_dialog_directory;
	}

	/// <summary>
	/// Determine position of a string in a List. </summary>
	/// <param name="list"> List to search. </param>
	/// <param name="item"> String item to search for. </param>
	/// <param name="selected_only"> Indicates if only selected items should be searched. </param>
	/// <param name="ignore_case"> Indicates whether to ignore case (true) or not (false). </param>
	/// <returns> The index of the first match, or -1 if no match. </returns>
	public static int indexOf(List list, string item, bool selected_only, bool ignore_case)
	{
		if ((list == null) || (string.ReferenceEquals(item, null)) || (item.Length == 0))
		{
			return -1;
		}
		int size = 0;
		string list_item = null;
		if (selected_only)
		{
			size = selectedSize(list);
			int[] selected_indexes = list.getSelectedIndexes();
			for (int i = 0; i < size; i++)
			{
				list_item = list.getItem(selected_indexes[i]);
				if (ignore_case)
				{
					if (list_item.Equals(item, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
				else if (list_item.Equals(item))
				{
					return i;
				}
			}
		}
		else
		{
			size = list.getItemCount();
			for (int i = 0; i < size; i++)
			{
				list_item = list.getItem(i);
				if (ignore_case)
				{
					if (list_item.Equals(item, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
				else if (list_item.Equals(item))
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Determine if the specified compare String exists within a Choice.
	/// <ul>
	/// <li>	Can compare the compare String against substrings for each item
	/// in the choice object if FLAG is set to CHECK_SUBSTRINGS.</li>
	/// <li>	To not compare against substrings, set FLAG to NONE.</li>
	/// </ul> </summary>
	/// <param name="choice"> Choice object. </param>
	/// <param name="compare"> String to compare choice items against. </param>
	/// <param name="FLAG"> compare criteria (i.e, CHECK_SUBSTRINGS, NONE). </param>
	/// <param name="delimiter"> String containing delimiter to parse for CHECK_SUBSTRINGS,
	/// may be null if using FLAG == NONE. </param>
	/// <param name="index"> Index location where the compare String was located at index[0] </param>
	/// <returns> returns true if compare exist in the choice items list,
	/// false otherwise.  This is filled in unless it is passed as null. </returns>
	public static bool isChoiceItem(Choice choice, string compare, int FLAG, string delimiter, int[] index)
	{
		int size; // number of items in the
											// Choice object
			int curIndex, length; // length of curItem
			string curItem; // current Choice item
			string curChar; // current character

			// initialize variables
			compare = compare.Trim();
			size = choice.getItemCount();

			for (int i = 0; i < size; i++)
			{
					curItem = choice.getItem(i).Trim();
					string sub = curItem;

					// check substring where substrings are delineated by spaces
					if (FLAG == CHECK_SUBSTRINGS)
					{
							// Jump over all characters until the delimiter is
				// reached.  Break the remaining String into a SubString
				// and compare to the compare String.
							length = sub.Length;
							for (curIndex = 0; curIndex < length; curIndex++)
							{
									curChar = curItem[curIndex].ToString().Trim();
									if (curChar.Equals(delimiter))
									{
											sub = sub.Substring(curIndex + 1).Trim();
									}
							}
							// Compare the remaining String, sub, to the compare
				// String.  If a match occurs, return true and the index
				// in the list in which the match was found.
							if (compare.Equals(sub))
							{
									index[0] = i;
									return true;
							}
					}
			else if (FLAG == NONE)
			{
				// Compare to the curItem String directly
							if (curItem.Equals(compare))
							{
					if (index != null)
					{
										index[0] = i;
					}
									return true;
							}
			}
			}
			return false;
	}

	/// <summary>
	/// Determine whether a string is in a List. </summary>
	/// <param name="list"> List to search. </param>
	/// <param name="item"> String item to search for. </param>
	/// <param name="selected_only"> Indicates if only selected items should be searched. </param>
	/// <param name="ignore_case"> Indicates whether to ignore case (true) or not (false). </param>
	/// <returns> true if the item is in the List. </returns>
	/// @deprecated us indexOf(). 
	public static bool isListItem(List list, string item, bool selected_only, bool ignore_case)
	{
		if (indexOf(list, item, selected_only, ignore_case) >= 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Return a new Choice that contains a list of standard fonts.
	/// </summary>
	public static Choice newFontNameChoice()
	{
		Choice fonts = new Choice();
		fonts.addItem("Arial");
		fonts.addItem("Courier");
		fonts.addItem("Helvetica");
		return fonts;
	}

	/// <summary>
	/// Return a new Choice that contains a list of standard font styles. </summary>
	/// <returns> a new Choice that contains a list of standard font styles. </returns>
	public static Choice newFontStyleChoice()
	{
		Choice styles = new Choice();
		styles.addItem("Plain");
		styles.addItem("PlainItalic");
		styles.addItem("Bold");
		styles.addItem("BoldItalic");
		return styles;
	}

	/// <summary>
	/// Given a list with selected items, add the specified string to the front of the
	/// items if it is not already at the front of the items.  After the changes, the
	/// originally selected items are still selected.  This is useful, for example,
	/// when a popup menu toggles the contents of a list back and forth. </summary>
	/// <param name="list"> List to modify. </param>
	/// <param name="prefix"> String to add. </param>
	public static void removeStringFromSelected(List list, string prefix)
	{
		if ((list == null) || (string.ReferenceEquals(prefix, null)))
		{
			return;
		}
		int[] selected_indexes = list.getSelectedIndexes();
		int selected_size = selectedSize(list);
		int len = prefix.Length;
		for (int i = 0; i < selected_size; i++)
		{
			if (list.getItem(selected_indexes[i]).Trim().regionMatches(true,0,prefix,0,len) && StringUtil.tokenCount(list.getItem(selected_indexes[i])," \t", StringUtil.DELIM_SKIP_BLANKS) > 1)
			{
				list.replaceItem(list.getItem(selected_indexes[i]).substring(len).Trim(), selected_indexes[i]);
			}
		}
		// Make sure the selected indices remain as before...
		select(list, selected_indexes);
		selected_indexes = null;
	}

	/// <summary>
	/// Selected the indicated items. </summary>
	/// <param name="list"> List to select from. </param>
	/// <param name="selected_indices"> Indices in the list to select. </param>
	public static void select(List list, int[] selected_indices)
	{
		if (list == null)
		{
			return;
		}
		if (selected_indices == null)
		{
			return;
		}
		for (int i = 0; i < selected_indices.Length; i++)
		{
			list.select(selected_indices[i]);
		}
	}

	/// <summary>
	/// Select a single item in a List.  Only the first match is selected. </summary>
	/// <param name="list"> List to select from. </param>
	/// <param name="item"> Item to select. </param>
	/// <param name="ignore_case"> Indicates whether case should be ignored when searching the
	/// list for a match. </param>
	public static void select(List list, string item, bool ignore_case)
	{
		if ((list == null) || (string.ReferenceEquals(item, null)))
		{
			return;
		}
		int size = list.getItemCount();
		string list_item = null;
		for (int i = 0; i < size; i++)
		{
			list_item = list.getItem(i);
			if (ignore_case)
			{
				if (list_item.Equals(item, StringComparison.OrdinalIgnoreCase))
				{
					list.select(i);
					return;
				}
			}
			else if (list_item.Equals(item))
			{
				list.select(i);
				return;
			}
		}
	}

	/// <summary>
	/// Select all items in a List. </summary>
	/// <param name="list"> List to select all items. </param>
	public static void selectAll(List list)
	{
		if (list == null)
		{
			return;
		}
		int size = list.getItemCount();
		for (int i = 0; i < size; i++)
		{
			list.select(i);
		}
	}

	/// <summary>
	/// Return the index of the requested selected item.  For example, if there are 5
	/// items selected out of 20 total, requesting index 0 will return index of the
	/// first of the 5 selected items.  This is particularly useful when determining
	/// the first or last selected item in a list. </summary>
	/// <param name="list"> List to check. </param>
	/// <param name="selected_index"> Position in the selected rows list. </param>
	/// <returns> the position in the original data for the requested selected index or
	/// -1 if unable to determine. </returns>
	public static int selectedIndex(List list, int selected_index)
	{
		if (list == null)
		{
			return -1;
		}
		int[] selected = list.getSelectedIndexes();
		if (selected != null)
		{
			int length = selected.Length;
			if (selected_index > (length - 1))
			{
				selected = null;
				return -1;
			}
			int pos = selected[selected_index];
			selected = null;
			return pos;
		}
		return 0;
	}

	/// <summary>
	/// Select an item in a choice, ignoring the case.  This is useful when the choice
	/// shows a valid property by the property may not always exactly match in case when
	/// read from a file or hand-edited. </summary>
	/// <param name="c"> Choice to select from. </param>
	/// <param name="item"> Choice item to select as a string. </param>
	/// <exception cref="Exception"> if the string is not found in the Choice. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void selectIgnoreCase(java.awt.Choice c, String item) throws Exception
	public static void selectIgnoreCase(Choice c, string item)
	{ // Does not look like Choice.select(String) throws an exception if the
		// item is not found so go through the list every time...
		// Get the list size...
		int size = c.getItemCount();
		for (int i = 0; i < size; i++)
		{
			if (c.getItem(i).equalsIgnoreCase(item))
			{
				c.select(i);
				return;
			}
		}
		throw new Exception("String \"" + item + "\" not found in Choice");
	}

	/// <summary>
	/// Return the number of items selected in a list. </summary>
	/// <param name="list"> List to check. </param>
	/// <returns> the number items selected in a list, or 0 if a null List. </returns>
	public static int selectedSize(List list)
	{
		if (list == null)
		{
			return 0;
		}
		int[] selected = list.getSelectedIndexes();
		if (selected != null)
		{
			int length = selected.Length;
			selected = null;
			return length;
		}
		return 0;
	}

	/// <summary>
	/// Select an item in a choice, using regionMatches() for string comparisons.  This
	/// is useful when the choice shows a valid property but the property may not always
	/// exactly match in case when read from a file or hand-edited. </summary>
	/// <param name="c"> Choice to select from. </param>
	/// <param name="ignore_case"> Indicates if case should be ignored when comparing strings. </param>
	/// <param name="start_in_choice"> Index position in Choice item strings to start the 
	/// comparison. </param>
	/// <param name="item"> String item to compare to Choice items. </param>
	/// <param name="start_in_item"> Index in item string to starty the comparison. </param>
	/// <param name="length"> Number of characters to compare. </param>
	/// <exception cref="Exception"> if the string is not found in the Choice. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void selectRegionMatches(java.awt.Choice c, boolean ignore_case, int start_in_choice, String item, int start_in_item, int length) throws Exception
	public static void selectRegionMatches(Choice c, bool ignore_case, int start_in_choice, string item, int start_in_item, int length)
	{ // Does not look like Choice.select(String) throws an exception if the
		// item is not found so go through the list every time...
		// Get the list size...
		int size = c.getItemCount();
		for (int i = 0; i < size; i++)
		{
			if (c.getItem(i).regionMatches(ignore_case,start_in_choice, item,start_in_item,length))
			{
				c.select(i);
				return;
			}
		}
		throw new Exception("String \"" + item + "\" not found in Choice");
	}

	/// <summary>
	/// Select an item in a choice, comparing a specific token in the choices.  This
	/// is useful when the choice shows an extended value (e.g., "Value - Description"). </summary>
	/// <param name="c"> Choice to select from. </param>
	/// <param name="ignore_case"> Indicates if case should be ignored when comparing strings. </param>
	/// <param name="delimiter"> String delimiter used by StringUtil.breakStringList(). </param>
	/// <param name="flags"> Flags used by StringUtil.breakStringList(). </param>
	/// <param name="token"> Token position in the Choice item, to be compared. </param>
	/// <param name="item"> String item to compare to Choice item tokens. </param>
	/// <exception cref="Exception"> if the string is not found in the Choice. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void selectTokenMatches(java.awt.Choice c, boolean ignore_case, String delimiter, int flags, int token, String item) throws Exception
	public static void selectTokenMatches(Choice c, bool ignore_case, string delimiter, int flags, int token, string item)
	{ // Does not look like Choice.select(String) throws an exception if the
		// item is not found so go through the list every time...
		// Get the list size...
		int size = c.getItemCount();
		IList<string> tokens = null;
		int ntokens = 0;
		string choice_token;
		for (int i = 0; i < size; i++)
		{
			tokens = StringUtil.breakStringList(c.getItem(i), delimiter, flags);
			ntokens = 0;
			if (tokens != null)
			{
				ntokens = tokens.Count;
			}
			if (ntokens <= token)
			{
				continue;
			}
			// Now compare.  Do not use region matches because we want
			// an exact match on the token...
			choice_token = tokens[token];
			if (ignore_case)
			{
				if (choice_token.Equals(item, StringComparison.OrdinalIgnoreCase))
				{
					c.select(i);
					return;
				}
			}
			else
			{
				if (choice_token.Equals(item))
				{
					c.select(i);
					return;
				}
			}
		}
		throw new Exception("Token " + token + " \"" + item + "\" not found in Choice");
	}

	/// <summary>
	/// This function sets the cursor for the container and all the components
	/// within the container. </summary>
	/// <param name="container"> Container to set the cursor. </param>
	/// <param name="i"> Cursor mask. </param>
	public static void setCursor(Container container, int i)
	{ // Call the version that takes a cursor object...
		setCursor(container, i, null);
	}

	/// <summary>
	/// This function sets the cursor for all the container and all the components
	/// within the container.  Either a cursor type (int) or Cursor object can be
	/// specified.  The Cursor object takes precedence if specified. </summary>
	/// <param name="container"> Container in which to set the cursor. </param>
	/// <param name="i"> Cursor mask (e.g., Cursur.DEFAULT_CURSOR). </param>
	/// <param name="cursor"> Cursor object. </param>
	public static void setCursor(Container container, int i, Cursor cursor)
	{ // Make sure we have a container instance.
		if (container == null)
		{
			return;
		}

			Cursor c = null;
		if (cursor != null)
		{
			c = cursor;
		}
		else
		{ // Create a cursor from the type...
				c = new Cursor(i);
		}
			container.setCursor(c);
		// set the cursor on all the components
		Component comp = null;
		int count = container.getComponentCount();
		for (int j = 0; j < count; j++)
		{
			comp = container.getComponent(j);
			comp.setCursor(c);
		}
		c = null;
		comp = null;
	}

	/// <summary>
	/// Reset the cursor to default value for the container only (not all components
	/// managed by the container). </summary>
	/// <param name="container"> to set the cursor mask </param>
	/// <param name="textField"> TextField that will receive a message "Ready" when ready. </param>
	public static void setReady(Container container, TextField textField)
	{ // Make sure we have a Container instance...
		if (container == null)
		{
			return;
		}
			setCursor(container, Cursor.DEFAULT_CURSOR, Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
		if (textField != null)
		{
			textField.setText("Ready");
		}
	}

	/// <summary>
	/// Set whether the control key is down.  This is a utility method to simplify
	/// tracking of a GUI state. </summary>
	/// <param name="down"> Indicates whether control is pressed. </param>
	public static void setControlDown(bool down)
	{
		_control_down = down;
	}

	/// <summary>
	/// Set the last directory accessed with a FileDialog.  This is used with
	/// getLastFileDialogDirectory() and is useful for saving the state of a GUI
	/// session so that users don't have to navigate quite as much.  If multiple
	/// directories are needed, use PropList and define properties for the application
	/// or use multiple variables within the application. </summary>
	/// <param name="last_file_dialog_directory"> The value from FileDialog.getDirectory(),
	/// which includes a path separator at the end. </param>
	public static void setLastFileDialogDirectory(string last_file_dialog_directory)
	{
		_last_file_dialog_directory = last_file_dialog_directory;
	}

	/// <summary>
	/// Set whether the shift key is down.  This is a utility method to simplify
	/// tracking of a GUI state. </summary>
	/// <param name="down"> Indicates whether shift is pressed. </param>
	public static void setShiftDown(bool down)
	{
		_shift_down = down;
	}

	/// <summary>
	/// Set the cursor to the wait cursor (hourglass) or back to the default.  The
	/// cursor is not recursively changed for every component. </summary>
	/// <param name="container"> Container to change the cursor for (e.g., a Frame). </param>
	/// <param name="waiting"> true if the cursor should be changed to the wait cursor, and
	/// false if the cursor should be set to the default. </param>
	public static void setWaitCursor(Container container, bool waiting)
	{
		if (container == null)
		{
			return;
		}
		if (waiting)
		{
			// Set the cursor to the hourglass...
			setCursor(container, Cursor.WAIT_CURSOR, Cursor.getPredefinedCursor(Cursor.WAIT_CURSOR));
		}
		else
		{ // Set to the default...
			setCursor(container, Cursor.DEFAULT_CURSOR, Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
		}
		// Try to force mouse to change even without mouse movement...
		Toolkit.getDefaultToolkit().sync();
	}

	/// <summary>
	/// Indicate whether the shift key is pressed.  This is used with the setShiftDown()
	/// method. </summary>
	/// <returns> if Shift is pressed. </returns>
	public static bool shiftDown()
	{
		return _shift_down;
	}

	/// <summary>
	/// Return the text from a TextArea as a list of strings, each of which has had
	/// the newline removed.  This is useful for exporting the text to a file or for
	/// printing.  At some point Sun may change the delimiter returned but we can
	/// isolate to this routine. </summary>
	/// <param name="a"> TextArea of interest. </param>
	/// <returns> A list of strings containing the text from the text area or a list
	/// with no elements if a null TextArea. </returns>
	public static IList<string> toList(TextArea a)
	{
		if (a == null)
		{
			return new List<string>();
		}
		IList<string> v = StringUtil.breakStringList(a.getText(), "\n", 0);
		// Just to be sure, remove any trailing carriage-return characters from
		// the end...
		string @string;
		for (int i = 0; i < v.Count; i++)
		{
			@string = v[i];
			v[i] = StringUtil.removeNewline(@string);
		}
		@string = null;
		return v;
	}

	/// <summary>
	/// Update the selections in a list based on the event that is being processed.
	/// This enforces Microsoft Windows Explorer conventions:
	/// <ol>
	/// <li>	Click selects on item.</li>
	/// <li>	Shift-click selects forward or backward to nearest selection.</li>
	/// <li>	Control-click toggles one item.</li>
	/// </ol>
	/// Use the values of controlDown() and shiftDown() to determine which buttons have
	/// been pressed (this is necessary because ItemEvent and its parent classes do
	/// not track whether shift/control/meta have been pressed. </summary>
	/// <param name="list"> List to process. </param>
	/// <param name="evt"> ItemEvent to process. </param>
	public static void updateListSelections(List list, ItemEvent evt)
	{
		updateListSelections(list, evt, _shift_down, _control_down);
	}

	/// <summary>
	/// Update the selections in a list based on the event that is being processed.
	/// This enforces Microsoft Windows Explorer conventions:
	/// <ol>
	/// <li>	Click selects on item.</li>
	/// <li>	Shift-click selects forward or backward to nearest selection.</li>
	/// <li>	Control-click toggles one item.</li>
	/// </ol> </summary>
	/// <param name="list"> List to process. </param>
	/// <param name="evt"> ItemEvent to process. </param>
	/// <param name="shift_pressed"> Indicates whether shift key is pressed. </param>
	/// <param name="control_pressed"> Indicates whether control key is pressed. </param>
	public static void updateListSelections(List list, ItemEvent evt, bool shift_pressed, bool control_pressed)
	{ //  Get the position of the selected item...
		int pos = ((int?)evt.getItem()).Value; // Item triggering event

		if (shift_pressed)
		{
			// Select the items up to the previous selected.  If nothing
			// is previously selected, select down the the next selected.
			// If in a selected block, don't do anything (but have to
			// reverse the default action of the list, which will have been
			// to deselect the item!).
			bool is_selected = list.isIndexSelected(pos);
			if (!is_selected)
			{
				// Re-select to leave unchanged...
				list.select(pos);
			}
			else
			{ // Selected a new item.  Check to see if there is
				// anything above that is selected.  If so, get the
				// last item selected above...
				int[] selected_indexes = list.getSelectedIndexes();
				int selected_size = 0;
				if (selected_indexes != null)
				{
					selected_size = selected_indexes.Length;
				}
				// Going through this loop will find the maximum
				// position before the event position.
				int starting_pos = -1;
				int i = 0;
				for (i = 0; i < selected_size; i++)
				{
					if (selected_indexes[i] < pos)
					{
						starting_pos = selected_indexes[i];
					}
				}
				if (starting_pos >= 0)
				{
					deselectAll(list);
					for (i = starting_pos; i <= pos; i++)
					{
						list.select(i);
					}
					selected_indexes = null;
					return;
				}
				// If here, check to see whether need to select after
				// the current event position...
				int ending_pos = -1;
				for (i = 0; i < selected_size; i++)
				{
					if (selected_indexes[i] > pos)
					{
						ending_pos = selected_indexes[i];
						break;
					}
				}
				if (ending_pos >= 0)
				{
					deselectAll(list);
					for (i = pos; i <= ending_pos; i++)
					{
						list.select(i);
					}
				}
			}
		}
		else if (control_pressed)
		{
			// Toggle the item of interest...
			// Because the list has already toggled the item, don't need
			// to do anything here...
			;
		}
		else
		{ // Simple select.  Highlight only the selected item.
			deselectAll(list);
			list.select(pos);
		}
	}

	} // End of GUIUtil

}