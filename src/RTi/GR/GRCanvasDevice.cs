using System;
using System.Collections.Generic;
using System.IO;

// GRCanvasDevice - GR device corresponding to an AWT canvas

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
// GRCanvasDevice - GR device corresponding to an AWT canvas
// ----------------------------------------------------------------------------
// History:
//
// 2001-01-13	Steven A. Malers, RTi	Add getImage().
// 2001-07-31	SAM, RTi		Add getGraphics(), setGraphics().
// 2002-02-07	SAM, RTi		Major rework.  The GRDevice class is now
//					an interface and its non-interface code
//					is folded into this class.  This class
//					used to be GRCanvasDevice but has been
//					renamed to be compatible with the
//					GRJComponentDevice.  Add _rubber_banding
//					to this class.  Add isPrinting() method.
// 2005-04-26	J. Thomas Sapienza, RTi	Added all member variables to finalize()
// 2005-04-29	JTS, RTi		Added anti alias methods (they do 
//					nothing but are now required by 
//					GRDevice).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{

	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// GR Device that corresponds to an AWT canvas.  This class is the base class for
	/// on-screen-related drawing, with secondary printing off-screen or image
	/// file creation.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GRCanvasDevice extends java.awt.Canvas implements GRDevice
	public class GRCanvasDevice : Canvas, GRDevice
	{

	/// <summary>
	/// Indicates whether double buffering is used.
	/// </summary>
	protected internal bool _double_buffering = true;
	/// <summary>
	/// Indicates if the device is being used for printing.  For example, the
	/// GRCanvasDevice is used for screen and printed output but for printed output
	/// the Y-Axis does not need to be shifted.
	/// </summary>
	protected internal bool _printing;
	/// <summary>
	/// Indicates that the Y axis must be reversed for the GR zero at the bottom.
	/// </summary>
	protected internal bool _reverse_y;
	/// <summary>
	/// Indicates whether rubber-banding from a select is in effect, in which case the
	/// drawing code should implement some type of XOR logic.
	/// </summary>
	protected internal bool _rubber_banding = false;

	/// <summary>
	/// Minimum X coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0x1;
	/// <summary>
	/// Maximum X coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0x2;
	/// <summary>
	/// Minimum Y coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0y1;
	/// <summary>
	/// Maximum Y coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0y2;

	/// <summary>
	/// Minimum X coordinate, relative.
	/// </summary>
	protected internal double _devx1;
	/// <summary>
	/// Maximum X coordinate, relative.
	/// </summary>
	protected internal double _devx2;
	/// <summary>
	/// Minimum Y coordinate, relative.
	/// </summary>
	protected internal double _devy1;
	/// <summary>
	/// Maximum Y coordinate, relative.
	/// </summary>
	protected internal double _devy2;

	/// <summary>
	/// GRLimits containing the relative points of the display device.
	/// </summary>
	protected internal GRLimits _limits;

	/// <summary>
	/// The following should be set by derived classes in the paint() method.
	/// The graphics will be used in drawing throughout the paint() method in 
	/// derived classes.
	/// </summary>
	protected internal Graphics _graphics = null;

	/// <summary>
	/// Image used in double-buffering in paint() method in derived classes.
	/// </summary>
	protected internal Image _image = null;

	/// <summary>
	/// Display mode (allows recording).
	/// </summary>
	protected internal int _mode;
	/// <summary>
	/// Page orientation -- shouldn't be necessary for any class other than 
	/// GRPSDevice.
	/// </summary>
	protected internal int _orientation;
	/// <summary>
	/// Page count -- shouldn't be necessary for any class other than 
	/// GRPSDevice.
	/// </summary>
	protected internal int _page;
	/// <summary>
	/// Size that is used by calling drawing routines.  Used in Postscript/page 
	/// systems where drawing can be to one page size with a single "scale"
	/// command.  
	/// </summary>
	protected internal int _sizedrawn;
	/// <summary>
	/// Size of output after scaling.  Used in Postscript/page 
	/// systems where drawing can be to one page size with a single "scale"
	/// command.  
	/// </summary>
	protected internal int _sizeout;
	/// <summary>
	/// Indicates the status of the drawing area.  See GRUtil.STATUS_*.  Might be
	/// an equivalent of a C++ option.
	/// </summary>
	protected internal int _status;
	/// <summary>
	/// Graphics driver type.  Offered because different graphics code might make
	/// different decisions, e.g., Postscript draws thick lines, Canvas does not.
	/// </summary>
	protected internal int _type;
	/// <summary>
	/// Device units.
	/// </summary>
	protected internal int _units;

	/// <summary>
	/// Name of this device (assigned by creating code).  It will be used as a window
	/// name if necessary.
	/// </summary>
	protected internal string _name;
	/// <summary>
	/// Note for this device.  Used for simple on-line help for the GUI.
	/// </summary>
	protected internal string _note;

	/// <summary>
	/// List of GRDrawingArea objects for this device.
	/// </summary>
	protected internal IList<GRDrawingArea> _drawing_area_list;

	/// <summary>
	/// Construct using name. </summary>
	/// <param name="name"> the name of the device. </param>
	public GRCanvasDevice(string name) : base()
	{
		PropList props = new PropList("GRCanvasDevice.default");
		props.set("Name", name);
		initialize(props);
	}

	/// <summary>
	/// Construct using name and size.  Currently, the size is ignored (controlled by
	/// layout managers) but in the future may use to cause a setSize() to be done
	/// at creation. </summary>
	/// <param name="name"> the name of the device. </param>
	/// <param name="size"> the grlimits specifying the size of the device. </param>
	public GRCanvasDevice(string name, GRLimits size) : base()
	{
		PropList props = new PropList("GRCanvasDevice.default");
		props.set("Name", name);
		// For now do not support size...
		initialize(props);
	}

	/// <summary>
	/// Construct using a property list. </summary>
	/// <param name="props"> a PropList specifying settings for the device. </param>
	public GRCanvasDevice(PropList props) : base()
	{
		if (Message.isDebugOn)
		{
			string routine = "GRCanvasDevice(PropList)";
			Message.printDebug(1, routine, "Contructing using PropList");
		}
		initialize(props);
	}

	/// <summary>
	/// Add a drawing area to the device.  The device will then manage the drawing
	/// areas as much as possible. </summary>
	/// <param name="grda"> GRDrawingArea to add. </param>
	public virtual void addDrawingArea(GRDrawingArea grda)
	{
		string routine = "GRDevice.addDrawingArea";

		if (grda == null)
		{
			Message.printWarning(2, routine, "NULL drawing area");
			return;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Adding drawing area \"" + grda.getName() + "\" to device \"" + _name + "\"");
		}
		_drawing_area_list.Add((GRCanvasDrawingArea)grda);
	}

	/// <summary>
	/// Clear the device and fill with white.  Should be defined in derived class.
	/// </summary>
	public virtual void clear()
	{
	}

	/// <summary>
	/// Close the device (used with PS files).  Should be defined in derived class.
	/// </summary>
	public virtual void close()
	{
	}

	/// <summary>
	/// Fill the device with the current color.  Should be defined in derived class.
	/// </summary>
	public virtual void fill()
	{
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRCanvasDevice()
	{
		_graphics = null;
		_name = null;
		_note = null;
		_drawing_area_list = null;
		_image = null;
		_limits = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flush the device (used by PS devices and X-Windoes).  Should be defined in 
	/// derived class.
	/// </summary>
	public virtual void flush()
	{
	}

	/// <summary>
	/// Return the internal Image used for double-buffering or null if image is null
	/// or no double-buffering.  This image is used by the paint() method in derived
	/// classes. </summary>
	/// <returns> Image for the device. </returns>
	public virtual Image getImage()
	{
		return _image;
	}

	/// <summary>
	/// Return the current limits of the device. </summary>
	/// <param name="recompute"> If true, the limits are retrieved from the canvas.  Otherwise
	/// the previous limits are returned (use the former when creating new drawing
	/// areas, the latter when operating from within a drawing area, assuming that
	/// resizing is being handled somewhere). </param>
	/// <returns> the current limits of the device. </returns>
	public virtual GRLimits getLimits(bool recompute)
	{
		if (recompute)
		{
			GRLimits limits = new GRLimits();

			// Get the size of the canvas...

			Rectangle bounds = getBounds();

			// Now transfer into GR, where the origin is at the lower
			// left...

			limits.setLeftX(0.0);
			limits.setBottomY(0.0);
			limits.setRightX(bounds.width);
			limits.setTopY(bounds.height);
			// This will set _limits...
			setLimits(limits);
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "GRCanvasDevice.getLimits", "Device limits are: 0.0,0.0 " + bounds.width + "," + bounds.height);
			}
		}
		return _limits;
	}

	/// <summary>
	/// Returns the current limits of the device.    This checks the size of the canvas. </summary>
	/// <returns> the current limits of the device. </returns>
	public virtual GRLimits getLimits()
	{
		return _limits;
	}

	/// <summary>
	/// Return the Graphics instance used for drawing.  The graphics instance is 
	/// set when the paint() method for this instance is called in derived classes. </summary>
	/// <returns> the Graphics instance used for drawing. </returns>
	public virtual Graphics getPaintGraphics()
	{
		return _graphics;
	}

	/// <summary>
	/// Indicate whether the device Y axis starts at the upper left. </summary>
	/// <returns> true if the device Y axies starts at the upper left. </returns>
	public virtual bool getReverseY()
	{
		return _reverse_y;
	}

	/// <summary>
	/// Returns the device units (GRUnits.*) </summary>
	/// <returns> The device units (GRUnits.*). </returns>
	public virtual int getUnits()
	{
		return _units;
	}

	/// <summary>
	/// Initializes member variables.
	/// </summary>
	private void initialize(PropList props)
	{ // Set the general information...

		_drawing_area_list = new List<GRDrawingArea>();
		_mode = GRUtil.MODE_DRAW;
		_name = "";
		_note = "";
		_orientation = GRDeviceUtil.ORIENTATION_PORTRAIT;
		_page = 0;
		_printing = false;
		_reverse_y = false;
		_sizedrawn = -1;
		_sizeout = -1;
		_status = GRUtil.STATUS_OPEN;
		_type = 0;
		_units = GRUnits.MM; // Default but needs to be reset.

		/// Set the values that were passed in...

		if (props == null)
		{
			return;
		}

		string prop_value;
		prop_value = props.getValue("Name");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_name = prop_value;
		}
		prop_value = props.getValue("PageSizeDrawn");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value[0] == 'A')
			{
				_sizedrawn = GRDeviceUtil.SIZE_A;
			}
			else if (prop_value[0] == 'B')
			{
				_sizedrawn = GRDeviceUtil.SIZE_B;
			}
			else if (prop_value[0] == 'C')
			{
				_sizedrawn = GRDeviceUtil.SIZE_C;
			}
			else if (prop_value[0] == 'D')
			{
				_sizedrawn = GRDeviceUtil.SIZE_D;
			}
			else if (prop_value[0] == 'E')
			{
				_sizedrawn = GRDeviceUtil.SIZE_E;
			}
		}
		prop_value = props.getValue("PageSizeOutput");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value[0] == 'A')
			{
				_sizeout = GRDeviceUtil.SIZE_A;
			}
			else if (prop_value[0] == 'B')
			{
				_sizeout = GRDeviceUtil.SIZE_B;
			}
			else if (prop_value[0] == 'C')
			{
				_sizeout = GRDeviceUtil.SIZE_C;
			}
			else if (prop_value[0] == 'D')
			{
				_sizeout = GRDeviceUtil.SIZE_D;
			}
			else if (prop_value[0] == 'E')
			{
				_sizeout = GRDeviceUtil.SIZE_E;
			}
		}
		if ((_sizeout < 0) && (_sizedrawn < 0))
		{
			// Neither specified...
			_sizedrawn = GRDeviceUtil.SIZE_A;
			_sizeout = GRDeviceUtil.SIZE_A;
		}
		else if (_sizedrawn < 0)
		{
			_sizedrawn = _sizeout;
		}
		else if (_sizeout < 0)
		{
			_sizeout = _sizedrawn;
		}
		prop_value = props.getValue("Orientation");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value[0] == 'p' || prop_value[0] == 'P')
			{
				_orientation = GRDeviceUtil.ORIENTATION_PORTRAIT;
			}
			else
			{
				_orientation = GRDeviceUtil.ORIENTATION_LANDSCAPE;
			}
		}
		prop_value = props.getValue("Note");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_note = prop_value;
		}

		// Fill in later...
		GRLimits limits = null;
		if (limits == null)
		{
			// Use default sizes...
			_dev0x1 = 0.0;
			_dev0x2 = 1.0;
			_dev0y1 = 0.0;
			_dev0y2 = 1.0;

			_devx1 = 0.0;
			_devx2 = 1.0;
			_devy1 = 0.0;
			_devy2 = 1.0;
			_limits = new GRLimits(_devx1, _devy1, _devx2, _devy2);
		}
		else
		{ // Set the limits...
			setLimits(limits);
		}
		// The size of the device will have been set in the constructor, but
		// the base class only knows how to store the data.  Force a resize
		// here.
		//resize ( _devx1, _devy1, _devx2, _devy2 );

		// Set to null.  Wait for the derived class to set the graphics for
		// use throughout the drawing event...

		// Set information used by the base class and other code...

		_graphics = null;
		_reverse_y = true; // Java uses y going down.  This is handled
					// properly in GRCanvasDrawingArea.scaleYData().
		_units = GRUnits.PIXEL;
		// Set in the super because there is some redundant data there...
		setLimits(getLimits(true));
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual bool isAntiAliased()
	{
		return false;
	}

	/// <summary>
	/// Indicate whether the device is in the process of printing.  The paint()
	/// method in derived classes should call setPrinting() as appropriate to
	/// indicate when prints starts and stops. </summary>
	/// <returns> true if the device is in the process of printing, false if not. </returns>
	public virtual bool isPrinting()
	{
		return _printing;
	}

	/// <summary>
	/// Indicates the end of a page of output.  Used in PS and should be defined in
	/// derived classes.
	/// </summary>
	public virtual void pageEnd()
	{
	}

	/// <summary>
	/// This method is called when the canvas is to be drawn.  It is expected that
	/// classes extended from this base class will implement a paint() method that
	/// either itself sets the graphics or calls super.paint() to call this method to
	/// set the graphics.  Using a GRCanvasDevice directly will result in this method
	/// being called for resize, etc., and the graphics in effect at the time is set as
	/// the current graphics.   The Graphics can then be used by subsequent calls for
	/// drawing.  The base class paint() is not called from this method.
	/// 
	/// REVISIT (SAM - 2003-05-07)
	/// This is where if we wanted to give base class more functionality for simple 
	/// drawing we would implement somthing like:
	/// addPainter(painter)
	/// then 
	/// for (loop through painters)
	/// {
	/// painter.paint(graphics);
	/// }
	/// </summary>
	public virtual void paint(Graphics graphics)
	{
		_graphics = graphics;
	}

	/// <summary>
	/// End of a plot of output, used for PS.  This should be implemented 
	/// in derived classes.
	/// </summary>
	public virtual void plotEnd(int flag)
	{
	}

	/// <summary>
	/// Resize the device to the given size.  This should be implemented in 
	/// derived classes.
	/// </summary>
	public virtual void resize(GRLimits limits)
	{
		if (limits == null)
		{
			return;
		}
		resize2((int)limits.getWidth(), (int)limits.getHeight());
	}

	/// <summary>
	/// Resize the device to the given size. </summary>
	/// <param name="x1"> New lower-left X-coordinte of device (usually zero). </param>
	/// <param name="y1"> New lower-left Y-coordinte of device (usually zero). </param>
	/// <param name="x2"> New top-right X-coordinte of device. </param>
	/// <param name="y2"> New top-right Y-coordinte of device. </param>
	public virtual void resize(double x1, double y1, double x2, double y2)
	{
		resize2((int)(x2 - x1), (int)(y2 - y1));
	}

	/// <summary>
	/// Resize the device to the given size.  Rename the routine resize2 because there
	/// is a deprecated resize method in canvas. </summary>
	/// <param name="width"> New width of the device (canvas). </param>
	/// <param name="height"> New height of the device (canvas). </param>
	public virtual void resize2(int width, int height)
	{
		setSize(width, height);
	}

	/// <summary>
	/// Save as an image file.  Currently the file is always a JPEG.  In the future,
	/// the file name will be examined for the extension.
	/// REVISIT (JTS - 2003-05-05)
	/// Are there any plans to examine the file name for extension?
	/// SAM:
	/// Sure as we get into it I'd really like to try PNG for Vector-oriented graphics
	/// so time series plots are not fuzzy. </summary>
	/// <param name="filename"> File name to write.  An appropriate extension will be added. </param>
	/// <exception cref="IOException"> if the image used for double-buffering is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void saveAsFile(String filename) throws java.io.IOException
	public virtual void saveAsFile(string filename)
	{
		saveAsFile(filename, (PropList)null);
	}

	/// <summary>
	/// Save as an image file.  Currently the file is always a JPEG.  In the future,
	/// the file name will be examined for the extension.  For Java 1.1.8 or earlier,
	/// TrueColor screen resolution will save as gray-scale.  There is no work-around
	/// unless an upgrade to Java 1.2.x is made. </summary>
	/// <param name="filename"> File name to write.  An appropriate extension will be added. </param>
	/// <param name="props"> Properties for the image.  Currently the only accepted property
	/// is Quality, which can be 0 (low quality, high compression) to 100 (high
	/// quality, no compression).  It might be useful at some point to enable an
	/// Interactive=true option to allow a pop-up dialog to specify JPEG information.
	/// REVISIT (JTS - 2003-05-05)
	/// Any plans for the above?
	/// SAM:
	/// Sure as we get into it. </param>
	/// <exception cref="IOException"> if the image used for double-buffering is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void saveAsFile(String filename, RTi.Util.IO.PropList props) throws java.io.IOException
	public virtual void saveAsFile(string filename, PropList props)
	{
		string routine = "GRCanvasDevice.saveAsFile";
		if (_image == null)
		{
			throw new IOException("No internal image to save");
		}
		// Else, create an image from the canvas???

		// Defaults...

		int image_quality = 90; // Go for quality versus compression.

		// Make sure there is a property list...

		PropList proplist = props;
		if (proplist == null)
		{
			proplist = new PropList("ImageProps");
		}
		string prop_value = proplist.getValue("Quality");
		if (!string.ReferenceEquals(prop_value, null))
		{
			image_quality = StringUtil.atoi(prop_value);
		}

		string newfilename = null;
		if (filename.EndsWith(".jpg", StringComparison.Ordinal) || filename.EndsWith(".JPG", StringComparison.Ordinal) || filename.EndsWith(".jpeg", StringComparison.Ordinal) || filename.EndsWith(".JPEG", StringComparison.Ordinal))
		{
			newfilename = filename;
		}
		else
		{ // Add a standard extension...
			newfilename = filename + ".jpg";
		}
		try
		{
			FileStream os = new FileStream(newfilename, FileMode.Create, FileAccess.Write);
			JpegEncoder jpg = new JpegEncoder(_image, image_quality, os);
			jpg.Compress();
			os.Flush();
			os.Close();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, "Error saving image file \"" + newfilename + "\"");
			Message.printWarning(2, routine, e);
			throw new IOException("Writing JPEG file \"" + newfilename + "\" failed.");
		}
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void setAntiAlias(bool antiAlias)
	{
	}

	/// <summary>
	/// Set the device limits (size) using a GRLimits.  This only sets the limits.  The
	/// device must be resized in the derived class. </summary>
	/// <param name="limits"> GRLimits indicating the size of the device. </param>
	public virtual void setLimits(GRLimits limits)
	{
		_devx1 = limits.getLeftX();
		_devy1 = limits.getBottomY();
		_devx2 = limits.getRightX();
		_devy2 = limits.getTopY();
		_limits = new GRLimits(limits);
		if (Message.isDebugOn)
		{
			string routine = "GRDevice.setLimits";
			Message.printDebug(1, routine, "Setting \"" + _name + "\" device limits to " + limits);
		}
	}

	/// <summary>
	/// Set the Graphics used by the device for drawing.  This Graphics should be
	/// reset each time that paint() is called in code that implements a 
	/// GRDevice because Graphics
	/// resources are typically created and destroyed dynamically by the application. </summary>
	/// <param name="graphics"> instance to use for drawing to the device.
	/// REVISIT (SAM - 2003-05-07)
	/// Need to see how printer graphics is handled.  I don't think it hurts to
	/// temporarily save it here during printing. </param>
	public virtual void setPaintGraphics(Graphics graphics)
	{
		_graphics = graphics;
	}

	/// <summary>
	/// Set the printing flag.  Set to true when the device is being used for printing,
	/// false when drawing to the screen. </summary>
	/// <param name="printing"> printing flag. </param>
	public virtual void setPrinting(bool printing)
	{
		_printing = printing;
	}

	} // End GRCanvasDevice class

}