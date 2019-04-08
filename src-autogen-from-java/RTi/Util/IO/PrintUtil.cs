using System;
using System.Collections.Generic;

// PrintUtil - this is a utility class for setting up paper for printing

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

namespace RTi.Util.IO
{


	using Message = RTi.Util.Message.Message;

	// TODO (JTS - 2004-03-30) make it so that paper sizes can be specified for plotter with .X precision

	/// <summary>
	/// This is a utility class for setting up paper for printing.  An example of use:
	/// <br><pre>
	/// try {
	///		PageFormat pageFormat = PrintUtil.getPageFormat("11x17");
	///		PrintUtil.setPageFormatOrientation(pageFormat, PrintUtil.LANDSCAPE);
	///		PrintUtil.setPageFormatMargins(pageFormat, .5, 1, 2, 4);
	///		PrintUtil.print(printable, pageFormat);
	/// }
	/// catch (Exception e) {
	///		e.printStackTrace();
	/// }
	/// </pre>	
	/// </summary>
	public class PrintUtil
	{

	/// <summary>
	/// Debugging routine.  Prints information about the PageFormat object at status level 2.
	/// </summary>
	public static void dumpPageFormat(PageFormat pageFormat)
	{
		string routine = "PrintUtil.dumpPageFormat";

		Message.printStatus(2, routine, "" + pageFormatToString(pageFormat));
		Message.printStatus(2, routine, "  Height         : " + pageFormat.getHeight());
		Message.printStatus(2, routine, "  Width          : " + pageFormat.getWidth());
		Message.printStatus(2, routine, "  ImageableHeight: " + pageFormat.getImageableHeight());
		Message.printStatus(2, routine, "  ImageableWidth : " + pageFormat.getImageableWidth());
		Message.printStatus(2, routine, "  ImageableX     : " + pageFormat.getImageableX());
		Message.printStatus(2, routine, "  ImageableY     : " + pageFormat.getImageableY());
		Message.printStatus(1, routine, "  Orient         : " + pageFormat.getOrientation());
	}

	/// <summary>
	/// Returns the orientation in the PageFormat as a string (e.g., "Portrait"). </summary>
	/// <returns> the orientation in the PageFormat as a string (e.g., "Portrait"). </returns>
	public static string getOrientationAsString(PageFormat pageFormat)
	{
		int orientation = pageFormat.getOrientation();
		return getOrientationAsString(orientation);
	}

	/// <summary>
	/// Returns the orientation in the PageFormat as a string (e.g., "Portrait"). </summary>
	/// <returns> the orientation in the PageFormat as a string (e.g., "Portrait"). </returns>
	public static string getOrientationAsString(int orientation)
	{
		if (orientation == PageFormat.LANDSCAPE)
		{
			return "Landscape";
		}
		else if (orientation == PageFormat.PORTRAIT)
		{
			return "Portrait";
		}
		else if (orientation == PageFormat.REVERSE_LANDSCAPE)
		{
			return "ReverseLandscape";
		}
		else
		{
			// Mac-only orientation -- unsupported
			return "Unsupported";
		}
	}

	// TODO SAM 2011-06-25 When dealing with the StateMod network, need to ensure that the Media.toString() values
	// are also checked, to have flexibility
	/// <summary>
	/// Returns a PageFormat object corresponding to the specified format type.
	/// The margins of the PageFormat object are 0 on all sides.  This is used for getting the sizes of page types.
	/// Possible values are:<br>
	/// <ul>
	/// <li><b>11x17</b> - 11 x 17 inches</li>
	/// <li><b>A</b> - Letter size - 8.5 x 11 inches</li>
	/// <li><b>A3</b> - 11.69 x 16.54 inches</li>
	/// <li><b>A4</b> - 8.27 x 11.69 inches</li>
	/// <li><b>A5</b> - 5.83 x 8.27 inches</li>
	/// <li><b>B</b> - Ledger size - 11 x 17 inches</li>
	/// <li><b>C</b> - 17 x 22 inches</li>
	/// <li><b>D</b> - 22 x 34 inches</li>
	/// <li><b>E</b> - 34 x 44 inches</li>
	/// <li><b>Executive</b> - 7.5 x 10 inches</li>
	/// <li><b>Letter</b> - 8.5 x 11 inches</li>
	/// <li><b>Legal</b> - 8.5 x 14 inches</li>
	/// <li><b>Plotter HxW</b> - Specifies that a plotter page will be used for printing
	/// and that the height of the page will be H inches and the width will be W.</li>
	/// </ul>
	/// <b>Keep in mind that the values specified above are the widths and heights
	/// (respectively) of the pages -- and that these values are for PORTRAIT mode.
	/// If the page is put into LANDSCAPE mode, the widths will become the heights and vice versa.</b> </summary>
	/// <param name="format"> the name of the format for which to return the page format. </param>
	/// <returns> the PageFormat that corresponds to the specified format. </returns>
	public static PageFormat getPageFormat(string format)
	{
		if (string.ReferenceEquals(format, null))
		{
			return null;
		}

		format = format.ToLower();

		PageFormat pageFormat = new PageFormat();
		pageFormat.setOrientation(PageFormat.PORTRAIT);
		Paper paper = new Paper();

		if (format.Equals("11x17") || format.Equals("b"))
		{
			paper.setSize(792, 1224);
			paper.setImageableArea(0, 0, 792, 1224);
		}
		else if (format.Equals("a3"))
		{
			paper.setSize(841, 1190);
			paper.setImageableArea(0, 0, 841, 1190);
		}
		else if (format.Equals("a4"))
		{
			paper.setSize(595, 841);
			paper.setImageableArea(0, 0, 595, 841);
		}
		else if (format.Equals("a5"))
		{
			paper.setSize(419, 595);
			paper.setImageableArea(0, 0, 419, 595);
		}
		else if (format.Equals("c"))
		{
			paper.setSize(1224, 1584);
			paper.setImageableArea(0, 0, 1224, 1584);
		}
		else if (format.Equals("d"))
		{
			paper.setSize(1584, 2448);
			paper.setImageableArea(0, 0, 1584, 2448);
		}
		else if (format.Equals("e"))
		{
			paper.setSize(2448, 3168);
			paper.setImageableArea(0, 0, 1584, 2448);
		}
		else if (format.Equals("executive"))
		{
			paper.setSize(540, 720);
			paper.setImageableArea(0, 0, 540, 720);
		}
		else if (format.Equals("letter") || format.Equals("a"))
		{
			paper.setSize(612, 792);
			paper.setImageableArea(0, 0, 612, 792);
		}
		else if (format.Equals("legal"))
		{
			paper.setSize(612, 1008);
			paper.setImageableArea(0, 0, 612, 1008);
		}
		else if (format.StartsWith("plotter", StringComparison.Ordinal))
		{
			string size = format.Substring(8).Trim();
			int loc = size.IndexOf("x", StringComparison.Ordinal);
			if (loc < 0 || loc == size.Length)
			{
				return null;
			}
			string H = size.Substring(0, loc);
			string W = size.Substring(loc + 1);
			int height = (Convert.ToInt32(H)) * 72;
			int width = (Convert.ToInt32(W)) * 72;
			paper.setSize(width, height);
			paper.setImageableArea(0, 0, width, height);
		}
		else
		{
			return null;
		}

		pageFormat.setPaper(paper);

		return pageFormat;
	}

	/// <summary>
	/// Return the list of supported media for the print service.  If includeNote=false, strings are Media.toString()
	/// (e.g., "na-letter").  If includeNote=True, additional readable equivalents are added
	/// (e.g., "na-letter - North American Letter"). </summary>
	/// <param name="printService"> the print service (printer) for which media sizes are being requested </param>
	/// <param name="includeSize"> if false, only the media size name is returned, if true the size is appended after " - "
	/// (e.g., "na-letter - North American Letter - 8.5 x 11 in").  The size information is for information.
	/// THIS IS NOT YET IMPLEMENTED. </param>
	public static IList<string> getSupportedMediaSizeNames(PrintService printService, bool includeNote, bool includeSize)
	{
		Media[] supportedMediaArray = (Media [])printService.getSupportedAttributeValues(typeof(Media), null, null);
		IList<string> mediaList = new List<string>();
		// The list has page sizes (e.g., "na-letter"), trays, and named sizes (e.g., "letterhead")
		// To find only sizes, look up the string in PaperSizeLookup and return matches
		PaperSizeLookup psl = new PaperSizeLookup();
		if (supportedMediaArray != null)
		{
			for (int i = 0; i < supportedMediaArray.Length; i++)
			{
				Media media = supportedMediaArray[i];
				string displayName = psl.lookupDisplayName(media.ToString());
				if (!string.ReferenceEquals(displayName, null))
				{
					// Media name matched a paper size (otherwise was tray name, etc.)
					if (!includeNote || (string.ReferenceEquals(displayName, null)))
					{
						mediaList.Add(media.ToString());
					}
					else
					{
						// TODO SAM 2011-06-25 if displayName is false, remove the trailing " - xxxx" size information
						mediaList.Add(media.ToString() + " - " + displayName);
					}
					if (includeSize)
					{
						// Additionally append the size information
						// TODO SAM 2011-06-25 Need to enable this
					}
				}
			}
			mediaList.Sort();
		}
		return mediaList;
	}

	/// <summary>
	/// Lookup the standard media size (e.g., "na-letter" for letter) from the legacy PrintUtil sizes. </summary>
	/// <param name="paperSize"> the paper size in common notation traditionally used by some graphics code (e.g., "letter"). </param>
	/// <returns> the internal MediaSizeName name for the media (e.g., "na-letter"), which is used by the newer
	/// GraphicsPrintJob code. </returns>
	public static string lookupStandardMediaSize(string paperSize)
	{
		if (paperSize.Equals("letter", StringComparison.OrdinalIgnoreCase))
		{
			return "na-letter";
		}
		else if (paperSize.Equals("legal", StringComparison.OrdinalIgnoreCase))
		{
			return "na-legal";
		}
		else if (paperSize.Equals("11x17", StringComparison.OrdinalIgnoreCase) || paperSize.Equals("B", StringComparison.OrdinalIgnoreCase))
		{
			return "B";
		}
		else if (paperSize.Equals("A3", StringComparison.OrdinalIgnoreCase))
		{
			return "iso-a3";
		}
		else if (paperSize.Equals("A4", StringComparison.OrdinalIgnoreCase))
		{
			return "iso-a4";
		}
		else if (paperSize.Equals("A5", StringComparison.OrdinalIgnoreCase))
		{
			return "iso-a5";
		}
		else if (paperSize.Equals("C", StringComparison.OrdinalIgnoreCase))
		{
			return "C";
		}
		else if (paperSize.Equals("D", StringComparison.OrdinalIgnoreCase))
		{
			return "D";
		}
		else if (paperSize.Equals("E", StringComparison.OrdinalIgnoreCase))
		{
			return "E";
		}
		else if (paperSize.Equals("executive", StringComparison.OrdinalIgnoreCase))
		{
			return "executive";
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Returns a string representation of the specified pageFormat (e.g., "A3"). </summary>
	/// <param name="pageFormat"> the pageFormat to return a String representation of. </param>
	/// <returns> a String representation of the specified page format (e.g., "A3"). </returns>
	public static string pageFormatToString(PageFormat pageFormat)
	{
		int longSide = 0;
		int shortSide = 0;

		if (pageFormat.getHeight() > pageFormat.getWidth())
		{
			longSide = (int)pageFormat.getHeight();
			shortSide = (int)pageFormat.getWidth();
		}
		else
		{
			longSide = (int)pageFormat.getWidth();
			shortSide = (int)pageFormat.getHeight();
		}

		if (longSide == 1224 && shortSide == 792)
		{
			return "11x17";
		}
		else if (longSide == 1190 && shortSide == 841)
		{
			return "A3";
		}
		else if (longSide == 841 && shortSide == 595)
		{
			return "A4";
		}
		else if (longSide == 595 && shortSide == 419)
		{
			return "A5";
		}
		else if (longSide == 1584 && shortSide == 1224)
		{
			return "C";
		}
		else if (longSide == 2448 && shortSide == 1584)
		{
			return "D";
		}
		else if (longSide == 3168 && shortSide == 2448)
		{
			return "E";
		}
		else if (longSide == 720 && shortSide == 540)
		{
			return "Executive";
		}
		else if (longSide == 792 && shortSide == 612)
		{
			return "Letter";
		}
		else if (longSide == 1008 && shortSide == 612)
		{
			return "Legal";
		}
		else
		{
			return "Plotter " + (int)(pageFormat.getWidth() / 72) + "x" + (int)(pageFormat.getHeight() / 72);
		}
	}

	/// <summary>
	/// Prints the print job set up on a Printable class using the specified pageFormat. </summary>
	/// <param name="printable"> the Printable that will be printing. </param>
	/// <param name="pageFormat"> the page format to use for printing the page. </param>
	/// <exception cref="Exception"> if printable is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void print(java.awt.print.Printable printable, java.awt.print.PageFormat pageFormat) throws Exception
	public static void print(Printable printable, PageFormat pageFormat)
	{
		if (printable == null)
		{
			throw new Exception("Printable parameter is null");
		}
		PrinterJob pj = PrinterJob.getPrinterJob();
		if (setupPrinterJobPageDialog(pj, printable, pageFormat))
		{
			showPrintDialogAndPrint(pj);
		}
	}

	/// <summary>
	/// Sets up margins on a page format object; 
	/// <b>this method must be called <u>AFTER</u> setPageFormatOrientation() is called</b>! </summary>
	/// <param name="pageFormat"> the page format to set up margins for. </param>
	/// <param name="topInches"> the number of inches of the margin on the top of the page. </param>
	/// <param name="bottomInches"> the number of inches of the margin on the bottom of the page. </param>
	/// <param name="leftInches"> the number of inches of the margin on the left of the page. </param>
	/// <param name="rightInches"> the number of inches of the margin on the right of the page. </param>
	/// <exception cref="Exception"> if the margins are negative or are too large for the specified page format. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setPageFormatMargins(java.awt.print.PageFormat pageFormat, double topInches, double bottomInches, double leftInches, double rightInches) throws Exception
	public static void setPageFormatMargins(PageFormat pageFormat, double topInches, double bottomInches, double leftInches, double rightInches)
	{
		Paper paper = pageFormat.getPaper();

		double height = pageFormat.getHeight();
		double width = pageFormat.getWidth();

		if (topInches < 0)
		{
			throw new Exception("Top margin is negative.");
		}
		if (bottomInches < 0)
		{
			throw new Exception("Bottom margin is negative.");
		}
		if (leftInches < 0)
		{
			throw new Exception("Left margin is negative.");
		}
		if (rightInches < 0)
		{
			throw new Exception("Right margin is negative.");
		}

		if (topInches >= height)
		{
			throw new Exception("Top margin size in inches (" + topInches + ") is too large compared to the " + "overall height of the page (" + height + ")");
		}

		if (bottomInches >= height)
		{
			throw new Exception("Bottom margin size in inches (" + bottomInches + ") is too large compared to " + "the overall height of the page (" + height + ")");
		}

		if ((topInches + bottomInches) >= height)
		{
			throw new Exception("Top margin size (" + topInches + ") and bottom margin size (" + bottomInches + ") combined are greater than the overall height of the page (" + height + ")");
		}

		if (leftInches >= width)
		{
			throw new Exception("Left margin size in inches (" + leftInches + ") is too large compared to the " + "overall width of the page (" + width + ")");
		}

		if (rightInches >= width)
		{
			throw new Exception("Right margin size in inches (" + rightInches + ") is too large compared to " + "the overall width of the page (" + width + ")");
		}

		if ((leftInches + rightInches) >= width)
		{
			throw new Exception("Left margin size (" + leftInches + ") and right margin size (" + rightInches + ") combined are greater than the overall width of the page (" + width + ")");
		}

		if (pageFormat.getOrientation() == PageFormat.PORTRAIT)
		{
			paper.setImageableArea((leftInches * 72), (topInches * 72), (width - (leftInches * 72) - (rightInches * 72)), (height - (topInches * 72) - (bottomInches * 72)));
		}
		else
		{
			paper.setImageableArea((topInches * 72), (rightInches * 72), (height - (topInches * 72) - (bottomInches * 72)), (width - (leftInches * 72) - (rightInches * 72)));
		}

		pageFormat.setPaper(paper);
	}

	/// <summary>
	/// Sets up the page dialog for entering margins and formats for the page format
	/// and also sets the Printable object for the PrinterJob to use. </summary>
	/// <param name="pj"> the PrinterJob that will be used to print. </param>
	/// <param name="printable"> the Printable interface that will actually print to the page. </param>
	/// <param name="pf"> the pageFormat to use (and possibly modify in the PrinterJob's page dialog). </param>
	/// <returns> true if the printer job page dialog was OK'd and printing can continue,
	/// false if the dialog was CANCEL'd. </returns>
	public static bool setupPrinterJobPageDialog(PrinterJob pj, Printable printable, PageFormat pf)
	{
		PageFormat pageFormat = pj.pageDialog(pf);
		if (pageFormat == pf)
		{
			return false;
		}
		pj.setPrintable(printable, pageFormat);
		return true;
	}

	/// <summary>
	/// Shows the print dialog (from which a printer can be chosen) and if the print was not cancelled,
	/// prints the page. </summary>
	/// <param name="pj"> the PrinterJob being used. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void showPrintDialogAndPrint(java.awt.print.PrinterJob pj) throws Exception
	public static void showPrintDialogAndPrint(PrinterJob pj)
	{
		if (pj.printDialog())
		{
			pj.print();
		}
	}

	/// <summary>
	/// Sets the page orientation for the PageFormat object.  This must be done before
	/// any call is made to setPageFormatMargins(). </summary>
	/// <param name="pageFormat"> the pageformat for which to set the orientation. </param>
	/// <param name="orientation"> the orientation of the page (either PORTRAIT or LANDSCAPE). </param>
	public static void setPageFormatOrientation(PageFormat pageFormat, int orientation)
	{
		pageFormat.setOrientation(orientation);
	}

	/// <summary>
	/// Sets the page orientation for the PageFormat object.  This must be done before
	/// any call is made to setPageFormatMargins(). </summary>
	/// <param name="pageFormat"> the page format for which to set the orientation. </param>
	/// <param name="orientation"> the orientation of the page (either "Portrait" or "Landscape"). </param>
	/// <exception cref="Exception"> if an invalid orientation is passed in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setPageFormatOrientation(java.awt.print.PageFormat pageFormat, String orientation) throws Exception
	public static void setPageFormatOrientation(PageFormat pageFormat, string orientation)
	{
		if (orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase))
		{
			pageFormat.setOrientation(PageFormat.LANDSCAPE);
		}
		else if (orientation.Equals("Portrait", StringComparison.OrdinalIgnoreCase))
		{
			pageFormat.setOrientation(PageFormat.PORTRAIT);
		}
		else if (orientation.Equals("ReverseLandscape", StringComparison.OrdinalIgnoreCase))
		{
			pageFormat.setOrientation(PageFormat.REVERSE_LANDSCAPE);
		}
		else
		{
			throw new Exception("Invalid orientation: '" + orientation + "'");
		}
	}

	}

}