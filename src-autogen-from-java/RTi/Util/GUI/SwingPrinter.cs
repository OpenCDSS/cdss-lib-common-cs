﻿using System;

// SwingPrinter - class for easy printing of Swing components

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
// SwingPrinter.java - Class for easy printing of Swing components.
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2002-11-14	J. Thomas Sapienza, RTi	Initial version.
// 2002-11-15	JTS, RTi		Javadoc'd.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Class to manage the printing of entire Swing components, ie, a graph,
	/// a TextArea, etc.  
	/// Code was derived from the Swing Tutorial's PrintUtilities.java (freely
	/// available for download and modification), found at:
	/// http://www.apl.jhu.edu/~hall/java/Swing-Tutorial/
	/// REVISIT (JTS - 2006-05-23)
	/// Should this be used or should some other class be used instead?  There are a 
	/// lot of printing classes now, which one should a developer use?  And if this
	/// one, add exaqmples.
	/// </summary>
	public class SwingPrinter : Printable
	{

	/// <summary>
	/// The Swing component that will be printed by the SwingPrinter.
	/// </summary>
	private Component __componentToBePrinted;
	/// <summary>
	/// The PageFormat to use for printing the page (handles margins, etc.).
	/// </summary>
	private PageFormat __pageFormat;

	/// <summary>
	/// Constructor.  Sets up the component to be printed. </summary>
	/// <param name="c"> the component that the SwingPrinter will print. </param>
	public SwingPrinter(Component c)
	{
		__componentToBePrinted = c;
	}

	/// <summary>
	/// Turns off Double Buffering for the given Component and all its subcomponents.
	/// Double-buffering screws up printing. </summary>
	/// <param name="c"> the Component to turn off the double buffering for </param>
	public static void disableDoubleBuffering(Component c)
	{
		RepaintManager currentManager = RepaintManager.currentManager(c);
		currentManager.setDoubleBufferingEnabled(false);
	}

	/// <summary>
	/// Turns on Double Buffering for the given Component and all its subcomponents.
	/// Double-buffering screws up printing. </summary>
	/// <param name="c"> the Component to turn on the double buffering for </param>
	public static void enableDoubleBuffering(Component c)
	{
		RepaintManager currentManager = RepaintManager.currentManager(c);
		currentManager.setDoubleBufferingEnabled(true);
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SwingPrinter()
	{
		__componentToBePrinted = null;
		__pageFormat = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Sets up the dialog to control printing. 
	/// </summary>
	public virtual void print()
	{
		string routine = "SwingPrinter.print";
		PrinterJob printJob = PrinterJob.getPrinterJob();
		// get the desired formatting for the page (margins, etc)
		__pageFormat = printJob.pageDialog(printJob.defaultPage());
		printJob.setPrintable(this);
		if (printJob.printDialog())
		{
			try
			{
				printJob.print();
			}
			catch (PrinterException e)
			{
				Message.printWarning(2, routine, "Error printing.");
				Message.printWarning(2, routine, e);
			}
		}
	}

	/// <summary>
	/// Prints with the given PageFormat. </summary>
	/// <param name="pageFormat"> the pageFormat with which to print. </param>
	public virtual void print(PageFormat pageFormat)
	{
		string routine = "SwingPrinter.print";
		__pageFormat = pageFormat;
		PrinterJob printJob = PrinterJob.getPrinterJob();
		printJob.setPrintable(this);
		try
		{
			printJob.print();
		}
		catch (PrinterException e)
		{
			Message.printWarning(2, routine, "Error printing.");
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Handles the actual printing of the component in the given pageFormat
	/// and on the given page.  This is called automatically when the Component is
	/// printed. </summary>
	/// <param name="g"> the Graphics object of the component to be printed. </param>
	/// <param name="pageFormat"> the format in which the page will be printed. </param>
	/// <param name="pageIndex"> the index of the page being printed -- currently only
	/// handles one page being printed. </param>
	/// <returns> NO_SUCH_PAGE if pageIndex > 0, or PAGE_EXISTS </returns>
	public virtual int print(Graphics g, PageFormat pageFormat, int pageIndex)
	{
		if (pageIndex > 0)
		{
			return (NO_SUCH_PAGE);
		}
		else
		{
			pageFormat = __pageFormat;
			Graphics2D g2d = (Graphics2D)g;
			g2d.translate(pageFormat.getImageableX(), pageFormat.getImageableY());
			// applies the previously chosen margins
			Rectangle r = new Rectangle(0, 0, (int)pageFormat.getImageableWidth(), (int)pageFormat.getImageableHeight());
			g2d.clip(r);
			Console.WriteLine("before the print --------------------");
			disableDoubleBuffering(__componentToBePrinted);
			__componentToBePrinted.paint(g2d);
			enableDoubleBuffering(__componentToBePrinted);
			Console.WriteLine("after the print -------------------");
			return (PAGE_EXISTS);
		}
	}

	public virtual PageFormat getPageFormat()
	{
		return __pageFormat;
	}

	/// <summary>
	/// Prints the given component. </summary>
	/// <param name="c"> the component to be printed. </param>
	public static PageFormat printComponent(Component c)
	{
		SwingPrinter sp = new SwingPrinter(c);
		sp.print();
		return sp.getPageFormat();
	}

	public static PageFormat printComponent(Component c, PageFormat pageFormat)
	{
		SwingPrinter sp = new SwingPrinter(c);
		sp.print(pageFormat);
		return pageFormat;
	}

	}

}