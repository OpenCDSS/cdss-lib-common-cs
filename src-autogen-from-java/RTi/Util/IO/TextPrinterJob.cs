using System;
using System.Collections.Generic;

// TextPrinterJob - this class provides a way to print a list of strings, with minimal formatting.

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

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides a way to print a list of strings, with minimal formatting.
	/// A 10-point Courier font is used by default for printing to preserve spacing and to
	/// allow a fairly wide output to be printed.
	/// </summary>
	public class TextPrinterJob : AbstractPrinterJob, Pageable, Printable
	{

	/// <summary>
	/// List containing the text strings to print.
	/// </summary>
	private IList<string> __textList;

	/// <summary>
	/// Font used for printing, initialized on first page.
	/// </summary>
	private Font __font = null;

	/// <summary>
	/// Requested lines per page.
	/// </summary>
	private int __requestedLinesPerPage = 100;

	/// <summary>
	/// Requested header.
	/// </summary>
	private string __requestedHeader = null;

	/// <summary>
	/// Requested footer.
	/// </summary>
	private string __requestedFooter = null;

	/// <summary>
	/// Requested whether to show line count.
	/// </summary>
	private bool __requestedShowLineCount = false;

	/// <summary>
	/// Requested whether to show page count.
	/// </summary>
	private bool __requestedShowPageCount = false;

	/// <summary>
	/// Printing a list of strings by constructing the printer job.  Default properties are provided but
	/// can be changed in the printer dialog (if not in batch mode). </summary>
	/// <param name="textList"> list of strings to print </param>
	/// <param name="reqPrintJobName"> the name of the print job (default is to use the system default job name) </param>
	/// <param name="reqPrinterName"> the name of the requested printer (e.g., \\MyComputer\MyPrinter) </param>
	/// <param name="reqPaperSize"> the requested paper size (Media.toString(), MediaSizeName.toString(), e.g., "na-letter") </param>
	/// <param name="reqPaperSource"> the requested paper source - not currently supported </param>
	/// <param name="reqOrientation"> the requested orientation (e.g., "Portrait", "Landscape"), default is printer default </param>
	/// <param name="reqMarginLeft"> the requested left margin in inches, for the orientation </param>
	/// <param name="reqMarginRight"> the requested right margin in inches, for the orientation </param>
	/// <param name="reqMarginTop"> the requested top margin in inches, for the orientation </param>
	/// <param name="reqMarginBottom"> the requested bottom margin in inches, for the orientation </param>
	/// <param name="reqLinesPerPage"> the requested lines per page - default is determined from imageable page height </param>
	/// <param name="reqHeader"> header to add at top of every page </param>
	/// <param name="reqFooter"> footer to add at bottom of every page </param>
	/// <param name="reqShowLineCount"> whether to show the line count to the left of lines in output </param>
	/// <param name="reqShowPageCount"> whether to show the page count in the footer </param>
	/// <param name="reqPages"> requested page ranges, where each integer pair is a start-stop page (pages 0+) </param>
	/// <param name="reqDoubleSided"> whether double-sided printing should be used - currently not supported </param>
	/// <param name="reqPrintFile"> name of a file to print to (for PDF, etc.), or null if not used.  If specified, a full
	/// path should be given. </param>
	/// <param name="showDialog"> if true, then the printer dialog will be shown to change default printer properties </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TextPrinterJob(java.util.List<String> textList, String reqPrintJobName, String reqPrinterName, String reqPaperSize, String reqPaperSource, String reqOrientation, double reqMarginLeft, double reqMarginRight, double reqMarginTop, double reqMarginBottom, int reqLinesPerPage, String reqHeader, String reqFooter, boolean reqShowLineCount, boolean reqShowPageCount, int [][] reqPages, boolean reqDoubleSided, String reqPrintFile, boolean showDialog) throws java.awt.print.PrinterException, javax.print.PrintException, java.net.URISyntaxException
	public TextPrinterJob(IList<string> textList, string reqPrintJobName, string reqPrinterName, string reqPaperSize, string reqPaperSource, string reqOrientation, double reqMarginLeft, double reqMarginRight, double reqMarginTop, double reqMarginBottom, int reqLinesPerPage, string reqHeader, string reqFooter, bool reqShowLineCount, bool reqShowPageCount, int[][] reqPages, bool reqDoubleSided, string reqPrintFile, bool showDialog) : base(reqPrintJobName, reqPrinterName, reqPaperSize, reqPaperSource, reqOrientation, reqMarginLeft, reqMarginRight, reqMarginTop, reqMarginBottom, reqPages, reqDoubleSided, reqPrintFile, showDialog)
	{ // Most data managed in parent class...
		// Locally relevant data...
		setTextList(textList);
		setRequestedHeader(reqHeader);
		setRequestedFooter(reqFooter);
		setRequestedLinesPerPage(reqLinesPerPage);
		setRequestedShowLineCount(reqShowLineCount);
		setRequestedShowPageCount(reqShowPageCount);
		// Start printing
		base.startPrinting(this, this); // pageable
	}

	/// <summary>
	/// Determine the number of lines per page to be printed, based on the printable area and user preference.
	/// Want the font size to be >= 6 (too difficult to read) and <= 12 (starts to look comical)
	/// For some standard page sizes always pick a certain size because the imageable page height
	/// should not vary much from normal defaults.
	/// </summary>
	private int determineLinesPerPage(PageFormat pageLayout)
	{
		int linesPerPage = getRequestedLinesPerPage();
		if (linesPerPage > 0)
		{
			return linesPerPage;
		}

		// TODO SAM 2011-06-23 Need to include European sizes
		// Standard page sizes...
		double pageHeight = pageLayout.getHeight();
		if ((pageHeight > 611.0) && (pageHeight < 613.0))
		{
			// 8.5 in
			return 75;
		}
		else if ((pageHeight > 791.0) && (pageHeight < 793.0))
		{
			// 11 in
			return 100;
		}
		else if ((pageHeight > 1223.0) && (pageHeight < 1225.0))
		{
			// 17 in
			return 200;
		}
		// Other page sizes, generally want to use a smaller font if possible
		// Some of these are unlikely but someone may try to print on large media for a presentation
		int[] candidates = new int[] {50, 60, 70, 80, 100, 125, 150, 200, 250, 300, 400, 500};
		double imageablePageHeight = pageLayout.getImageableHeight();
		for (int i = candidates.Length - 1; i >= 0; i--)
		{
			double fontHeight = imageablePageHeight / candidates[i];
			if ((fontHeight >= 7.0) && (fontHeight <= 12.0))
			{
				return candidates[i];
			}
		}
		// Default
		return (int)(imageablePageHeight / 10.0); // 10 point font
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~TextPrinterJob()
	{
		__textList = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the font.
	/// </summary>
	public virtual Font getFont()
	{
		return __font;
	}

	/// <summary>
	/// Return the number of pages being printed.
	/// </summary>
	public virtual int getNumberOfPages()
	{
		int linesPerPage = determineLinesPerPage(getPrinterJob().getPageFormat(getPrintRequestAttributes()));
		int numberOfPages = 0;
		IList<string> textList = getTextList();
		if (textList != null)
		{
			numberOfPages = textList.Count / linesPerPage;
			if (textList.Count % linesPerPage != 0)
			{
				++numberOfPages;
			}
		}
		return numberOfPages;
	}

	/// <summary>
	/// Return the format to be used for the requested page (in this case the same for all pages).
	/// </summary>
	public virtual PageFormat getPageFormat(int pageIndex)
	{
		return getPrinterJob().getPageFormat(getPrintRequestAttributes());
	}

	/// <summary>
	/// Return the Printable implementation for paging (this class).
	/// </summary>
	public virtual Printable getPrintable(int pageIndex)
	{
		return this;
	}

	/// <summary>
	/// Return the requested footer.
	/// </summary>
	public virtual string getRequestedFooter()
	{
		return __requestedFooter;
	}

	/// <summary>
	/// Return the requested header.
	/// </summary>
	public virtual string getRequestedHeader()
	{
		return __requestedHeader;
	}

	/// <summary>
	/// Return the requested lines per page.
	/// </summary>
	private int getRequestedLinesPerPage()
	{
		return __requestedLinesPerPage;
	}

	/// <summary>
	/// Return whether to show the line count at the bottom of the page.
	/// </summary>
	public virtual bool getRequestedShowLineCount()
	{
		return __requestedShowLineCount;
	}

	/// <summary>
	/// Return whether to show the page count at the bottom of the page.
	/// </summary>
	public virtual bool getRequestedShowPageCount()
	{
		return __requestedShowPageCount;
	}

	/// <summary>
	/// Return the list of strings to print.
	/// </summary>
	private IList<string> getTextList()
	{
		return __textList;
	}

	/// <summary>
	/// Print the report.
	/// </summary>
	public virtual int print(Graphics g, PageFormat pageFormat, int pageIndex)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".print";
		int dl = 20;

		Graphics2D g2d = (Graphics2D)g;

		// Get the printable page dimensions (points)...

		double pageHeight = pageFormat.getHeight();
		double imageablePageHeight = pageFormat.getImageableHeight();
		double pageWidth = pageFormat.getWidth();
		double imageablePageWidth = pageFormat.getImageableWidth();
		double imageableX = pageFormat.getImageableX();
		double imageableY = pageFormat.getImageableY();

		if (Message.isDebugOn)
		{
			Message.printDebug(dl, routine, "Page dimensions are: width=" + pageWidth + " height=" + pageHeight);
		}
		Message.printStatus(2, routine, "Page dimensions are: width=" + pageWidth + " height=" + pageHeight);
		Message.printStatus(2, routine, "Imageable page dimensions are: width=" + imageablePageWidth + " height=" + imageablePageHeight);
		Message.printStatus(2, routine, "Imageable origin: X=" + imageableX + " Y=" + imageableY);

		// Use a fixed-width font to properly display fixed-with computer input/output/code.
		// Select a reasonable number of lines based on the printable size and scale the font accordingly.

		int linesPerPage = determineLinesPerPage(pageFormat);
		string header = getRequestedHeader();
		string footer = getRequestedFooter();
		bool showPageCount = getRequestedShowPageCount();
		int headerLines = 0; // Lines on page dedicated to header
		int footerLines = 0; // Lines on page dedicated to footer
		if ((!string.ReferenceEquals(header, null)) && (header.Length > 0))
		{
			headerLines = 2;
		}
		if (((!string.ReferenceEquals(footer, null)) && (footer.Length > 0)) || showPageCount)
		{
			footerLines = 2;
		}
		float yDelta = (float)imageablePageHeight / (linesPerPage + headerLines + footerLines);
		Font font = getFont();
		// If font was not set (this is the first page to print), save it for reuse
		if (font == null)
		{
			double fontHeight = yDelta; // Try 100% fill but reduce later if necessary
			Message.printStatus(2, routine, "Calculated font height is " + fontHeight);
			// Fonts can only have integer height
			font = new Font("Courier", Font.PLAIN, (int)fontHeight);
			setFont(font);
		}

		g2d.setFont(font);
		g2d.setColor(Color.black);
		FontMetrics fm = g2d.getFontMetrics(font);
		//int fontHeight = fm.getHeight();
		int fontDescent = fm.getDescent();
		//int curHeight = TOP_BORDER;

		// Determine the starting line to print based on the requested page

		IList<string> textList = getTextList();
		int firstLine = pageIndex * linesPerPage;
		if (firstLine > textList.Count)
		{
			return Printable.NO_SUCH_PAGE;
		}
		int lastLine = firstLine + linesPerPage - 1;
		if (lastLine >= textList.Count)
		{
			lastLine = textList.Count - 1;
		}

		// Print the header and footer - for now use the same font as the text with one line blank line between
		// header/footer and the main body
		// TODO SAM 2011-06-25 Work on other fonts, etc., to make look nicer

		double headerHeight = 0.0;
		if ((!string.ReferenceEquals(header, null)) && (header.Length > 0))
		{
			headerHeight = yDelta * 2.0;
			g2d.drawString(header, (float)imageableX, (float)imageableY + yDelta - fontDescent);
		}
		if ((!string.ReferenceEquals(footer, null)) && (footer.Length > 0))
		{
			g2d.drawString(footer, (float)imageableX, (float)imageableY + (float)imageablePageHeight - fontDescent);
		}
		if (showPageCount)
		{
			// Same line as the footer, but in the middle of the imageable width
			// TODO SAM 2011-06-25 To truly be centered should account for width of text
			g2d.drawString("- " + (pageIndex + 1) + " -", (float)imageableX + (float)imageablePageWidth / 2, (float)imageableY + (float)imageablePageHeight - fontDescent);
		}

		// Print the lines.  Remember that y=0 at the top of the page.

		float x = (float)imageableX;
		// Initialize vertical position on the page, slightly offset from full line
		// because character is drawn at point of descent
		float y = (float)imageableY + (float)headerHeight + yDelta - fontDescent;
		string textLine;
		bool showLineCount = getRequestedShowLineCount();
		string lineCountFormat = "%" + ((int)Math.Log10(textList.Count) + 1) + "d";
		try
		{
			for (int iLine = firstLine; iLine <= lastLine; iLine++)
			{
				// Don't do a trim() here because it may shift the line if there are leading spaces...
				textLine = textList[iLine];
				if (showLineCount)
				{
					// Prepend the line with the line number and a space
					textLine = StringUtil.formatString((iLine + 1),lineCountFormat) + " " + textLine;
				}
				if ((!string.ReferenceEquals(textLine, null)) && (textLine.Length > 0))
				{
					Message.printStatus(2, routine, "Printing \"" + textLine + "\" at " + x + "," + y);
					g2d.drawString(textLine, (int)x, (int)y);
				}
				y += yDelta;
			}
		}
		catch (Exception t)
		{
			Message.printWarning(3, routine, t);
		}

		return Printable.PAGE_EXISTS;
	}

	/// <summary>
	/// Set the font to use for printing, initialized on first page.
	/// </summary>
	private void setFont(Font font)
	{
		__font = font;
	}

	/// <summary>
	/// Set the requested footer.
	/// </summary>
	private void setRequestedFooter(string requestedFooter)
	{
		__requestedFooter = requestedFooter;
	}

	/// <summary>
	/// Set the requested header.
	/// </summary>
	private void setRequestedHeader(string requestedHeader)
	{
		__requestedHeader = requestedHeader;
	}

	/// <summary>
	/// Set the requested lines per page.
	/// </summary>
	private void setRequestedLinesPerPage(int requestedLinesPerPage)
	{
		__requestedLinesPerPage = requestedLinesPerPage;
	}

	/// <summary>
	/// Set whether to show the line count.
	/// </summary>
	private void setRequestedShowLineCount(bool reqShowLineCount)
	{
		__requestedShowLineCount = reqShowLineCount;
	}

	/// <summary>
	/// Set whether to show the page count.
	/// </summary>
	private void setRequestedShowPageCount(bool reqShowPageCount)
	{
		__requestedShowPageCount = reqShowPageCount;
	}

	/// <summary>
	/// Set the list of strings to print.
	/// </summary>
	private void setTextList(IList<string> textList)
	{
		__textList = textList;
	}

	}

}