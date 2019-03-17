// SimpleJTree_TreeWillExpandListener - class that allows control over whether JTrees can expand or collapse

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
// SimpleJTree_TreeWillExpandListener - Class that allows control over
// whether JTrees can expand or collapse.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-05-09	J. Thomas Sapienza, RTi	Initial version
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	using WindowsTreeUI = com.sun.java.swing.plaf.windows.WindowsTreeUI;

	/// <summary>
	/// This class overrides the default Java Windows UI for the tree so that
	/// the drawing of connection lines can be stopped, and for other purposes.
	/// It can only be used in a Windows environment.
	/// </summary>
	public class SimpleJTree_WindowsUI : WindowsTreeUI
	{

	/// <summary>
	/// The kind of line style that the Tree shouuld have.  See SimpleJTree.LINE_*
	/// </summary>
	private int __lineStyle = SimpleJTree.LINE_ANGLED;

	/// <summary>
	/// Constructor.
	/// </summary>
	public SimpleJTree_WindowsUI() : base()
	{
	}

	/// <summary>
	/// Returns the line style used for painting.  See SimpleJTree.LINE_* </summary>
	/// <returns> the line style used for painting. </returns>
	public virtual int getLineStyle()
	{
		return __lineStyle;
	}

	/// <summary>
	/// Overrides the default paintVerticalLine method and will only draw lines if
	/// the line style has been set to LINE_ANGLED.  LINE_HORIZONTAL is not yet 
	/// supported. </summary>
	/// <param name="g"> the Graphics context with which to paint the tree lines </param>
	/// <param name="c"> unused. </param>
	/// <param name="x"> the x location at which to draw </param>
	/// <param name="top"> the top of the vertical line </param>
	/// <param name="bottom"> the bottom of the vertical line </param>
	protected internal virtual void paintVerticalLine(Graphics g, JComponent c, int x, int top, int bottom)
	{
		if (__lineStyle == SimpleJTree.LINE_ANGLED)
		{
			drawDashedVerticalLine(g, x, top, bottom);
		}
	}

	/// <summary>
	/// Overrides the default paintHorizontal line method and will only draw lines if
	/// the line style has been set to LINE_ANGLED.  LINE_HORIZONTAL is not yet 
	/// supported. </summary>
	/// <param name="g"> the Graphics context with which to paint the tree lines </param>
	/// <param name="c"> unused </param>
	/// <param name="y"> the y location at which to draw </param>
	/// <param name="left"> the left of the horizontal line </param>
	/// <param name="right"> the right of the horizontal line </param>
	protected internal virtual void paintHorizontalLine(Graphics g, JComponent c, int y, int left, int right)
	{
		if (__lineStyle == SimpleJTree.LINE_ANGLED)
		{
			drawDashedHorizontalLine(g, y, left, right);
		}
	}

	/// <summary>
	/// Sets the line style to use for painting.  See SimpleJTree.LINE_* </summary>
	/// <param name="style"> the line style to use for painting. </param>
	public virtual void setLineStyle(int style)
	{
		__lineStyle = style;
	}

	}

}