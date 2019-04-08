using System;

// AbstractPrinterJob - abstract class that provides basic functionality to set up and execute a PrinterJob in batch or interactive mode

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

	/// <summary>
	/// This abstract provides basic functionality to set up and execute a PrinterJob in batch or interactive mode.
	/// This class should be extended by a class that implements Printable and optionally Pageable.
	/// The constructor should be called and then startPrinting().
	/// </summary>
	public class AbstractPrinterJob
	{

	/// <summary>
	/// The instance of Printable to print.
	/// </summary>
	//private Printable __printable = null;

	/// <summary>
	/// The instance of Pageable to print.
	/// </summary>
	//private Pageable __pageable = null;

	/// <summary>
	/// Whether printing should show the print dialog to adjust printer job properties.
	/// </summary>
	private bool __showDialog = false;

	/// <summary>
	/// The PrinterJob used for printing, initialized in startPrinting().
	/// </summary>
	private PrinterJob __printerJob = null;

	/// <summary>
	/// The print request attributes, initialized in startPrinting().
	/// </summary>
	private PrintRequestAttributeSet __printRequestAttributes = null;

	// Requested printer attributes, specified by calling code...

	/// <summary>
	/// Print job name.
	/// </summary>
	private string __requestedPrinterName = null;

	/// <summary>
	/// Print job name.
	/// </summary>
	private string __requestedPrintJobName = null;

	/// <summary>
	/// Page size.
	/// </summary>
	private string __requestedPaperSize = null;

	/// <summary>
	/// Page orientation.
	/// </summary>
	private string __requestedOrientation = null;

	/// <summary>
	/// Margin, left.
	/// </summary>
	private double __requestedMarginLeft = .75;

	/// <summary>
	/// Margin, right.
	/// </summary>
	private double __requestedMarginRight = .75;

	/// <summary>
	/// Margin, top.
	/// </summary>
	private double __requestedMarginTop = .75;

	/// <summary>
	/// Margin, bottom.
	/// </summary>
	private double __requestedMarginBottom = .75;

	// TODO SAM 2011-06-30 possible allow user to specify units
	/// <summary>
	/// Margin, units.
	/// </summary>
	//private int __requestedMarginUnits = MediaPrintableArea.INCH;

	/// <summary>
	/// Requested pages to print.
	/// </summary>
	private int[][] __requestedPages = null;

	/// <summary>
	/// Requested print file to write.
	/// </summary>
	private string __requestedPrintFile = null;

	/// <summary>
	/// Requested whether to print double-sided.
	/// </summary>
	private bool __requestedDoubleSided = false;

	/// <summary>
	/// Private constructor to protect abstract status.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private AbstractPrinterJob()
	private AbstractPrinterJob()
	{
	}

	/// <summary>
	/// Construct a printer job.  Default properties are provided but
	/// can be changed in the printer dialog (if not in batch mode). </summary>
	/// <param name="reqPrintJobName"> the name of the print job (default is to use the system default job name) </param>
	/// <param name="reqPrinterName"> the name of the requested printer (e.g., \\MyComputer\MyPrinter) </param>
	/// <param name="reqPaperSize"> the requested paper size (Media.toString(), MediaSizeName.toString(), e.g., "na-letter") </param>
	/// <param name="reqPaperSource"> the requested paper source - not currently supported </param>
	/// <param name="reqOrientation"> the requested orientation (e.g., "Portrait", "Landscape"), default is printer default </param>
	/// <param name="reqMarginLeft"> the requested left margin in inches, for the orientation </param>
	/// <param name="reqMarginRight"> the requested right margin in inches, for the orientation </param>
	/// <param name="reqMarginTop"> the requested top margin in inches, for the orientation </param>
	/// <param name="reqMarginBottom"> the requested bottom margin in inches, for the orientation </param>
	/// <param name="reqPages"> requested page ranges, where each integer pair is a start-stop page (pages 0+) </param>
	/// <param name="reqDoubleSided"> whether double-sided printing should be used - currently not supported </param>
	/// <param name="reqPrintFile"> name of a file to print to (for PDF, etc.), or null if not used.  If specified, a full
	/// path should be given. </param>
	/// <param name="showDialog"> if true, then the printer dialog will be shown to change default printer properties </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AbstractPrinterJob(String reqPrintJobName, String reqPrinterName, String reqPaperSize, String reqPaperSource, String reqOrientation, double reqMarginLeft, double reqMarginRight, double reqMarginTop, double reqMarginBottom, int [][] reqPages, boolean reqDoubleSided, String reqPrintFile, boolean showDialog) throws java.awt.print.PrinterException, javax.print.PrintException, java.net.URISyntaxException
	public AbstractPrinterJob(string reqPrintJobName, string reqPrinterName, string reqPaperSize, string reqPaperSource, string reqOrientation, double reqMarginLeft, double reqMarginRight, double reqMarginTop, double reqMarginBottom, int[][] reqPages, bool reqDoubleSided, string reqPrintFile, bool showDialog)
	{
		//setPrintable ( printable );
		//setPageable ( pageable );
		setRequestedPrintJobName(reqPrintJobName);
		setRequestedPrinterName(reqPrinterName);
		setRequestedPaperSize(reqPaperSize);
		// paper source
		setRequestedOrientation(reqOrientation);
		setRequestedMarginLeft(reqMarginLeft);
		setRequestedMarginRight(reqMarginRight);
		setRequestedMarginTop(reqMarginTop);
		setRequestedMarginBottom(reqMarginBottom);
		setRequestedPages(reqPages);
		setRequestedDoubleSided(reqDoubleSided);
		setRequestedPrintFile(reqPrintFile);
		setShowDialog(showDialog);
	}

	/// <summary>
	/// Return the format to be used for the requested page (in this case the same for all pages).
	/// </summary>
	/*
	public PageFormat getPageFormat ( int pageIndex )
	{
	    return getPrinterJob().getPageFormat(getPrintRequestAttributes());
	}
	*/

	/// <summary>
	/// Return the Pageable implementation.
	/// </summary>
	/*
	public Pageable getPageable ()
	{
	    return __pageable;
	}
	*/

	/// <summary>
	/// Return the Printable implementation.
	/// </summary>
	/*
	public Printable getPrintable ()
	{
	    return __printable;
	}
	*/

	/// <summary>
	/// Return the print job.
	/// </summary>
	/*
	public PrinterJob getPrinterJob ()
	{
	    return __printerJob;
	}*/

	/// <summary>
	/// Return the print job.
	/// </summary>
	public virtual PrinterJob getPrinterJob()
	{
		return __printerJob;
	}

	/// <summary>
	/// Return the printer request attributes.
	/// </summary>
	public virtual PrintRequestAttributeSet getPrintRequestAttributes()
	{
		return __printRequestAttributes;
	}

	/// <summary>
	/// Return whether double-sided was requested.
	/// </summary>
	public virtual bool getRequestedDoubleSided()
	{
		return __requestedDoubleSided;
	}



	/// <summary>
	/// Return the requested margin, bottom.
	/// </summary>
	public virtual double getRequestedMarginBottom()
	{
		return __requestedMarginBottom;
	}

	/// <summary>
	/// Return the requested margin, left.
	/// </summary>
	public virtual double getRequestedMarginLeft()
	{
		return __requestedMarginLeft;
	}

	/// <summary>
	/// Return the requested margin, right.
	/// </summary>
	public virtual double getRequestedMarginRight()
	{
		return __requestedMarginRight;
	}

	/// <summary>
	/// Return the requested margin, top.
	/// </summary>
	public virtual double getRequestedMarginTop()
	{
		return __requestedMarginTop;
	}

	/// <summary>
	/// Return the requested orientation.
	/// </summary>
	public virtual string getRequestedOrientation()
	{
		return __requestedOrientation;
	}

	/// <summary>
	/// Return the requested paper size.
	/// </summary>
	public virtual string getRequestedPaperSize()
	{
		return __requestedPaperSize;
	}

	/// <summary>
	/// Return the requested pages.
	/// </summary>
	public virtual int [][] getRequestedPages()
	{
		return __requestedPages;
	}

	/// <summary>
	/// Return the requested printer name.
	/// </summary>
	public virtual string getRequestedPrinterName()
	{
		return __requestedPrinterName;
	}

	/// <summary>
	/// Return the requested print file.
	/// </summary>
	public virtual string getRequestedPrintFile()
	{
		return __requestedPrintFile;
	}

	/// <summary>
	/// Return the requested print job name.
	/// </summary>
	public virtual string getRequestedPrintJobName()
	{
		return __requestedPrintJobName;
	}

	/// <summary>
	/// Return the whether printing is in batch mode.
	/// </summary>
	public virtual bool getShowDialog()
	{
		return __showDialog;
	}

	/// <summary>
	/// Set the Pageable instance.
	/// </summary>
	/*
	private void setPageable ( Pageable pageable )
	{
	    __pageable = pageable;
	}
	*/

	/// <summary>
	/// Set the Printable instance.
	/// </summary>
	/*
	private void setPrintable ( Printable printable )
	{
	    __printable = printable;
	}*/

	/// <summary>
	/// Set the printer job.
	/// </summary>
	private void setPrinterJob(PrinterJob printerJob)
	{
		__printerJob = printerJob;
	}

	/// <summary>
	/// Set the printer request attributes.
	/// </summary>
	private void setPrintRequestAttributes(PrintRequestAttributeSet printRequestAttributes)
	{
		__printRequestAttributes = printRequestAttributes;
	}

	/// <summary>
	/// Set the whether double sided was requested.
	/// </summary>
	private void setRequestedDoubleSided(bool requestedDoubleSided)
	{
		__requestedDoubleSided = requestedDoubleSided;
	}

	/// <summary>
	/// Set the requested margin, bottom.
	/// </summary>
	private void setRequestedMarginBottom(double requestedMarginBottom)
	{
		__requestedMarginBottom = requestedMarginBottom;
	}

	/// <summary>
	/// Set the requested margin, left.
	/// </summary>
	private void setRequestedMarginLeft(double requestedMarginLeft)
	{
		__requestedMarginLeft = requestedMarginLeft;
	}

	/// <summary>
	/// Set the requested margin, bottom.
	/// </summary>
	private void setRequestedMarginRight(double requestedMarginRight)
	{
		__requestedMarginRight = requestedMarginRight;
	}

	/// <summary>
	/// Set the requested margin, top.
	/// </summary>
	private void setRequestedMarginTop(double requestedMarginTop)
	{
		__requestedMarginTop = requestedMarginTop;
	}

	/// <summary>
	/// Set the requested orientation.
	/// </summary>
	private void setRequestedOrientation(string requestedOrientation)
	{
		__requestedOrientation = requestedOrientation;
	}

	/// <summary>
	/// Set the requested page size.
	/// </summary>
	private void setRequestedPaperSize(string requestedPaperSize)
	{
		__requestedPaperSize = requestedPaperSize;
	}

	/// <summary>
	/// Set the requested pages.
	/// </summary>
	private void setRequestedPages(int[][] requestedPages)
	{
		__requestedPages = requestedPages;
	}

	/// <summary>
	/// Set the requested printer name.
	/// </summary>
	private void setRequestedPrinterName(string requestedPrinterName)
	{
		__requestedPrinterName = requestedPrinterName;
	}

	/// <summary>
	/// Set the requested print file to write.
	/// </summary>
	private void setRequestedPrintFile(string requestedPrintFile)
	{
		__requestedPrintFile = requestedPrintFile;
	}

	/// <summary>
	/// Set the name of the print job.
	/// </summary>
	private void setRequestedPrintJobName(string requestedPrintJobName)
	{
		__requestedPrintJobName = requestedPrintJobName;
	}

	/// <summary>
	/// Set whether the print dialog should be shown.
	/// </summary>
	private void setShowDialog(bool showDialog)
	{
		__showDialog = showDialog;
	}

	/// <summary>
	/// Start the printing by setting up the printer job and calling its print() method
	/// (which calls the print() method in the instances that are passed in. </summary>
	/// <param name="printable"> an instance of Printable, to format content for printing </param>
	/// <param name="pageable"> an instance of Pageable, to handle multiple-page content </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startPrinting(java.awt.print.Printable printable, java.awt.print.Pageable pageable) throws javax.print.PrintException, java.awt.print.PrinterException, java.net.URISyntaxException
	public virtual void startPrinting(Printable printable, Pageable pageable)
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		string routine = this.GetType().FullName + ".startPrinting";

		/*
		StopWatch sw = new StopWatch();
		sw.start();
		PrintService[] services = PrintServiceLookup.lookupPrintServices(null, null);
		sw.stop();
		Message.printStatus(2,routine,"Took " + sw.getSeconds() + " seconds to get print services." );
		*/
		PrintService defaultPrintService = PrintServiceLookup.lookupDefaultPrintService();
		/*
		DocFlavor[] supportedFlavors = defaultPrintService.getSupportedDocFlavors();
		for ( int i = 0; i < supportedFlavors.length; i++ ) {
		    Message.printStatus(2, routine, "Supported flavor for " + defaultPrintService.getName() +
		        ":  " + supportedFlavors[i] );
		}
		*/

		// Get available printers...

		// TODO SAM 2011-06-25 The following commented code generally returned NO printers
		// And, because formatting below uses Graphics, it is necessary to use something other than
		// a text printer.  Leave in the code for illustration for now, but take out once print code
		// has been used for awhile.
		/*
		PrintService printService = ServiceUI.printDialog(
		   null, // To select screen (parent frame?) - null is use default screen
		   100, // x location of dialog on screen
		   100, // y location of dialog on screen
		   services, // Printer services to browse
		   defaultPrintService, // default (initial) service to display
		   null, // the document flavor to print (null for all)
		   printRequestAttributes ); // Number of copies, orientation, etc. - will be updated
		*/
	   /* This should work but does not automatically format text and printer does not support text flavor
	    DocPrintJob job = printService.createPrintJob();
	    // Create the document to print
	    Doc myDoc = new SimpleDoc(
	        getText(), // Text to print
	        DocFlavor.STRING.TEXT_PLAIN, // Document (data object) flavor
	        null ); // Document attributes
	    if ( printService != null ) {
	        // Null indicates cancel
	        job.print(myDoc, printRequestAttributes);
	    }
	    */

		// Try going the PrinterJob route which should be compatible with PrintService
		PrinterJob printerJob = PrinterJob.getPrinterJob();
		setPrinterJob(printerJob);
		// Tell the printer job what is printing
		printerJob.setPrintable(printable);
		if (pageable != null)
		{
			printerJob.setPageable(pageable);
		}
		// Get the list of all print services available for the print job...
		PrintService[] printServices = PrinterJob.lookupPrintServices();
		if (printServices.Length > 0)
		{
			// Select a specific printer to use
			PrintService printService = null;
			// Make sure that the requested printer name is matched
			string reqPrinterName = getRequestedPrinterName();
			Message.printStatus(2,routine,"Requested printer name is \"" + reqPrinterName + "\"");
			if ((string.ReferenceEquals(reqPrinterName, null)) || (reqPrinterName.Length == 0))
			{
				// Specific printer was not requested so use default
				printService = defaultPrintService;
			}
			else
			{
				// Find the matching printer
				for (int i = 0; i < printServices.Length; i++)
				{
					Message.printStatus(2,routine,"Comparing requested printer name to \"" + printServices[i].getName() + "\"");
					if (printServices[i].getName().equalsIgnoreCase(reqPrinterName))
					{
						printService = printServices[i];
						break;
					}
				}
				if (printService == null)
				{
					// Requested printer was not matched
					throw new PrintException("Unable to locate printer \"" + reqPrinterName + "\"");
				}
			}
			// Pre-populate some attributes, in the order of the constructor parameters
			PrintRequestAttributeSet printRequestAttributes = new HashPrintRequestAttributeSet();
			// Print job name
			string reqPrintJobName = getRequestedPrintJobName();
			if (!string.ReferenceEquals(reqPrintJobName, null))
			{
				printRequestAttributes.add(new JobName(reqPrintJobName,null));
			}
			// Page size - Media.toString()
			string paperSize = getRequestedPaperSize();
			PaperSizeLookup psl = null; // Can also be reused below for margins
			if ((!string.ReferenceEquals(paperSize, null)) && (paperSize.Length > 0))
			{
				psl = new PaperSizeLookup();
				// Look up the media value from the string
				Media media = psl.lookupMediaFromName(printService, paperSize);
				if (media == null)
				{
					throw new PrintException("Printer does not have media available:  \"" + paperSize + "\"");
				}
				printRequestAttributes.add(media);
			}
			// Paper source - TODO SAM 2011-06-25 need to enable paper source (tray)
			// Orientation
			string orientation = getRequestedOrientation();
			if ((!string.ReferenceEquals(orientation, null)) && (orientation.Length > 0))
			{
				if (orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase))
				{
					printRequestAttributes.add(OrientationRequested.LANDSCAPE);
				}
				else if (orientation.Equals("Portrait", StringComparison.OrdinalIgnoreCase))
				{
					printRequestAttributes.add(OrientationRequested.PORTRAIT);
				}
			}
			// Margins - can't set margins because API wants context based on page size, so specify as imageable area
			// compared to paper size
			// From MediaPrintableArea doc:  "The rectangular printable area is defined thus: The (x,y) origin is 
			// positioned at the top-left of the paper in portrait mode regardless of the orientation specified in
			// the requesting context. For example a printable area for A4 paper in portrait or landscape
			// orientation will have height > width. 
			float marginLeft = (float)getRequestedMarginLeft();
			float marginRight = (float)getRequestedMarginRight();
			float marginTop = (float)getRequestedMarginTop();
			float marginBottom = (float)getRequestedMarginBottom();
			//Message.printStatus ( 2, routine, "Requested margins left=" + marginLeft + " right=" + marginRight +
			//    " top="+ marginTop + " bottom=" + marginBottom );
			if ((marginLeft >= 0.0) && (marginRight >= 0.0) && (marginTop >= 0.0) && (marginBottom >= 0.0) && (psl != null))
			{ // psl being instantiated means the paper size was specified - required to set margins
				// Check the orientation.  If landscape, the paper is rotated 90 degrees clockwise,
				// and the user-specified margins need to be converted to the portrait representations
				// for the following code
				if ((!string.ReferenceEquals(orientation, null)) && (orientation.Length > 0) && orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase))
				{
					// Convert the requested margins (landscape) to portrait
					float marginLeftOrig = marginLeft;
					float marginRightOrig = marginRight;
					float marginTopOrig = marginTop;
					float marginBottomOrig = marginBottom;
					marginLeft = marginTopOrig;
					marginRight = marginBottomOrig;
					marginBottom = marginLeftOrig;
					marginTop = marginRightOrig;
				}
				// The media size always comes back in portrait mode
				// TODO SAM 2011-06-26 Need to figure out portrait and landscape for the large paper sizes
				MediaSizeName mediaSizeName = psl.lookupMediaSizeNameFromString(paperSize);
				MediaSize mediaSize = MediaSize.getMediaSizeForName(mediaSizeName);
				//Message.printStatus(2, routine, "paper size for \"" + paperSize + "\" is " + mediaSize );
				float pageWidth = mediaSize.getX(MediaSize.INCH);
				float pageHeight = mediaSize.getY(MediaSize.INCH);
				//Message.printStatus ( 2, routine, "Media size width = " + pageWidth + " height=" + pageHeight );
				// Imageable area is the total page minus the margins, with the origin at the top left
				MediaPrintableArea area = new MediaPrintableArea(marginLeft, marginTop, (pageWidth - marginLeft - marginRight),(pageHeight - marginTop - marginBottom),MediaSize.INCH);
				printRequestAttributes.add(area);
				//Message.printStatus(2,routine,"MediaPrintableArea x=" + area.getX(MediaSize.INCH) +
				//    " y=" + area.getY(MediaSize.INCH) + " width=" + area.getWidth(MediaSize.INCH) +
				//    " height=" + area.getHeight(MediaSize.INCH) );
			}
			// Pages
			int[][] reqPages = getRequestedPages();
			if (reqPages != null)
			{
				printRequestAttributes.add(new PageRanges(reqPages));
			}
			// Print file (e.g., used with PDF)
			// TODO SAM 2011-07-01 Going through this sequence seems to generate a warning about
			// fonts needing to be included, but there is no way to edit native printer properties to do this - ARG!!!
			// Microsoft XPS and image files seem to work OK.
			string reqPrintFile = getRequestedPrintFile();
			if (!string.ReferenceEquals(reqPrintFile, null))
			{
				File f = new File(reqPrintFile);
				printRequestAttributes.add(new Destination(f.toURI()));
				Message.printStatus(2, routine, "Printing to file \"" + f.toURI() + "\"");
			}
			// Double-sided TODO SAM 2011-06-25 This is actually more complicated with short-edge, long-edge, etc.
			bool reqDoubleSided = getRequestedDoubleSided();
			if (reqDoubleSided)
			{
				printRequestAttributes.add(Sides.DUPLEX);
			}
			// FIXME SAM 2011-07-01 The dialog may be slow (up to 30 seconds) as per:
			// http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6539061
			// Hopefully this will be fixed in an upcoming release
			if (getShowDialog())
			{
				// User may want to modify the print job properties
				// Now let the user interactively review and edit...
				Message.printStatus(2,routine,"Opening print dialog for printer \"" + printService.getName() + "\"");
				printService = ServiceUI.printDialog(null, 100, 100, printServices, printService, null, printRequestAttributes); // Number of copies, orientation, etc. - will be updated
			}
			if (printService != null)
			{ // If null the print job was canceled
				// Save the PrinterJob and attributes for use elsewhere in this class
				setPrinterJob(printerJob);
				setPrintRequestAttributes(printRequestAttributes);
				// User has selected a printer, so set for the job
				printerJob.setPrintService(printService);
				// Now print with the print job assets that may have been defined in the print dialog...
				printerJob.print(printRequestAttributes);
			}
		}
	}

	}

}