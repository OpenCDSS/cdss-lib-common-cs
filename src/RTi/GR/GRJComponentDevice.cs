using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// GRJComponentDevice - GR device corresponding to a JComponent

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
// GRJComponentDevice - GR device corresponding to a JComponent
// ----------------------------------------------------------------------------
// History:
//
// 2002-01-07	Steven A. Malers, RTi	Copy and combine GRDevice and
//					GRJavaDevice.  Add _rubber_banding.
//					Add isPrinting().
// 2003-05-01	J. Thomas Sapienza, RTi	Made changes to accomodate the massive
//					restructuring of GR.java.
// 2003-05-02	JTS, RTi		Incorporated changes to allow double-
//					buffering
// 2003-05-06	JTS, RTi		Split the setupDoubleBuffer() method
//					into two methods.
// 2004-03-19	JTS, RTi		Started using Graphics2D instead of
//					Graphics.
// 2004-08-06	JTS, RTi		Added support for writing PNG files.
// 2005-04-26	JTS, RTi		Added all data members to finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{


	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// TODO (JTS - 2006-05-23) Document example of use (e.g., from ERDiagram_Device)
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GRJComponentDevice extends javax.swing.JComponent implements GRDevice
	public class GRJComponentDevice : JComponent, GRDevice
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_double_buffering = _doubleBuffered;
		}


	//TODO sam 2017-03-03 need to make all of these private and add appropriate methods to encapsulate
	/// <summary>
	/// BufferedImage used in double-buffering.
	/// </summary>
	protected internal BufferedImage _buffer = null;

	/// <summary>
	/// If true, drawing will be performed to the double-buffer.
	/// </summary>
	protected internal bool _doubleBuffered = false;

	/// <summary>
	/// For backwards compatibility with older code.  Probably should be removed soon.
	/// </summary>
	protected internal bool _double_buffering;

	/// <summary>
	/// Whether the device is drawing anti-aliased or not.
	/// Anti-aliased with smooth curves by filling in transition color pixels.
	/// </summary>
	private bool __isAntiAliased = false;

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
	/// The graphics will be used in drawing throughout the paint() call.
	/// </summary>
	protected internal Graphics2D _graphics = null;

	/// <summary>
	/// Image used in double-buffering in paint() method in derived classes.
	/// </summary>
	protected internal Image _image = null;

	/// <summary>
	/// Display mode (allows recording).
	/// </summary>
	protected internal int _mode;
	/// <summary>
	/// Page orientation -- shouldn't be necessary for any class other than GRPSDevice.
	/// </summary>
	protected internal int _orientation;
	/// <summary>
	/// Page count -- shouldn't be necessary for any class other than GRPSDevice.
	/// </summary>
	protected internal int _page;
	/// <summary>
	/// Size that is used by calling drawing routines.  Used in Postscript/page 
	/// systems where drawing can be to one page size with a single "scale" command.  
	/// </summary>
	protected internal int _sizedrawn;
	/// <summary>
	/// Size of output after scaling.  Used in Postscript/page 
	/// systems where drawing can be to one page size with a single "scale" command.  
	/// </summary>
	protected internal int _sizeout;
	/// <summary>
	/// Indicates the status of the drawing area.  See GRUtil.STATUS_*.  Might be an equivalent of a C++ option.
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
	/// Name of this device (assigned by creating code).  It will be used as a window name if necessary.
	/// </summary>
	protected internal string _name;
	/// <summary>
	/// Note for this device.  Used for simple on-line help for the GUI.
	/// </summary>
	protected internal string _note;

	/// <summary>
	/// List of GRDrawingArea objects for this device, guaranteed to be non-null.
	/// </summary>
	private IList<GRDrawingArea> drawingAreaList = null;

	/// <summary>
	/// Construct using name. </summary>
	/// <param name="name"> the name of the device. </param>
	public GRJComponentDevice(string name)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		PropList props = new PropList("GRJComponentDevice.default");
		props.set("Name", name);
		initialize(props);
	}

	/// <summary>
	/// Construct using name and size.  Currently, the size is ignored (controlled by layout managers).
	/// </summary>
	public GRJComponentDevice(string name, GRLimits size)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		PropList props = new PropList("GRJComponentDevice.default");
		props.set("Name", name);
		// For now do not support size...
		initialize(props);
	}

	/// <summary>
	/// Construct using a property list. </summary>
	/// <param name="props"> PropList containing settings for this device. </param>
	public GRJComponentDevice(PropList props)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		if (Message.isDebugOn)
		{
			string routine = "GRDevice(PropList)";
			Message.printDebug(1, routine, "Contructing using PropList");
		}
		initialize(props);
	}

	/// <summary>
	/// Add a drawing area to the device.  The device will then manage the drawing areas as much as possible.
	/// Drawing areas with names the same as previously added drawing areas will still be added. </summary>
	/// <param name="grda"> GRJComponentDrawingArea to add. </param>
	public virtual void addDrawingArea(GRDrawingArea grda)
	{
		addDrawingArea(grda, false);
	}

	/// <summary>
	/// Add a drawing area to the device.  The device will then manage the drawing areas as much as possible. </summary>
	/// <param name="grda"> GRJComponentDrawingArea to add. </param>
	/// <param name="replaceMatching"> if true, then a drawing area that matches an existing drawing area (same name)
	/// will replaced the previous drawing area. </param>
	public virtual void addDrawingArea(GRDrawingArea grda, bool replaceMatching)
	{
		string routine = "GRJComponentDevice.addDrawingArea";

		if (grda == null)
		{
			Message.printWarning(2, routine, "NULL drawing area");
			return;
		}

		if (replaceMatching)
		{
			// This currently will cause the drawing area to be added at the end.
			// This could be an issue if order is a problem.
			// Could do a replace at the same location but that also has implications and a new "replace" method might be better.
			for (int ida = this.drawingAreaList.Count - 1; ida >= 0; --ida)
			{
				GRDrawingArea da = this.drawingAreaList[ida];
				if (da.getName().Equals(grda.getName()))
				{
					// Remove the match
					this.drawingAreaList.RemoveAt(ida);
				}
			}
		}

		// Add the drawing area at the end.

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Adding drawing area \"" + grda.getName() + "\" to device \"" + _name + "\"");
		}
		this.drawingAreaList.Add(grda);
	}

	/// <summary>
	/// Clear the device and fill with white.  Should be defined in derived class.
	/// </summary>
	public virtual void clear()
	{
	}

	/// <summary>
	/// Clears the double-buffer.
	/// </summary>
	public virtual void clearDoubleBuffer()
	{
		int width = _buffer.getWidth();
		int height = _buffer.getHeight();
		_buffer = new BufferedImage(width, height, BufferedImage.TYPE_4BYTE_ABGR);
		_graphics = (Graphics2D)_buffer.getGraphics();
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
	~GRJComponentDevice()
	{
		_graphics = null;
		_buffer = null;
		_limits = null;
		_image = null;
		_name = null;
		_note = null;
		this.drawingAreaList = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flush the device (used by PS devices and X-Windows).  Should be defined in derived class.
	/// </summary>
	public virtual void flush()
	{
	}

	/// <summary>
	/// Unsure of just what this is meant to do. </summary>
	/// @deprecated deprecated (2006-05-22) to see if any other classes are using this
	/// method. If so, evaluate what they're doing, javadoc, and undeprecate.  
	/// Alternately, REVISIT (JTS - 2006-05-23) in a few months and if it's still here, remove. 
	public virtual void forceGraphics(Graphics g)
	{
		_graphics = (Graphics2D)g;
	}

	/// <summary>
	/// Return a drawing area requested by name. </summary>
	/// <returns> the first matching drawing area (exact string match) or null if not matched. </returns>
	public virtual GRDrawingArea getDrawingArea(string daName)
	{
		foreach (GRDrawingArea da in this.drawingAreaList)
		{
			if (da.getName().Equals(daName))
			{
				return da;
			}
		}
		return null;
	}

	/// <summary>
	/// Return the internal Image used for double-buffering or null if image is null or no double-buffering. </summary>
	/// <returns> Image for the device. </returns>
	//public Image getImage () {
	public virtual BufferedImage getImage()
	{
	//	return _image;
		return _buffer;
	}

	/// <summary>
	/// Return the current limits of the device. </summary>
	/// <param name="recompute"> If true, the limits are retrieved from the JComponent.
	/// Otherwise the previous limits are returned (use the former when creating 
	/// new drawing areas, the latter when operating from within a drawing area, 
	/// assuming that resizing is being handled somewhere). </param>
	public virtual GRLimits getLimits(bool recompute)
	{
		if (recompute)
		{
			GRLimits limits = new GRLimits();

			// Get the size of the JComponent...

			Rectangle bounds = getBounds();
			// Now transfer into GR, where the origin is at the lower left...

			limits.setLeftX(0.0);
			limits.setBottomY(0.0);
			limits.setRightX(bounds.width);
			limits.setTopY(bounds.height);
			// This will set _limits...
			setLimits(limits);
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "GRJComponentDevice.getLimits", "Device limits are: 0.0,0.0 " + bounds.width + "," + bounds.height);
			}
		}
		return _limits;
	}

	public virtual GRLimits getLimits(bool recompute, double scale)
	{
		if (recompute)
		{
			GRLimits limits = new GRLimits();

			// Get the size of the JComponent...

			Rectangle bounds = getBounds();
			// Now transfer into GR, where the origin is at the lower left...

			limits.setLeftX(0.0);
			limits.setBottomY(0.0);
			limits.setRightX(bounds.width / scale);
			limits.setTopY(bounds.height / scale);
			// This will set _limits...
			setLimits(limits);
			if (Message.isDebugOn)
			{
				Message.printDebug(30, "GRJComponentDevice.getLimits", "Device limits are: 0.0,0.0 " + bounds.width + "," + bounds.height);
			}
		}
		return _limits;
	}

	/// <summary>
	/// Returns the current limits of the device.  This checks the size of the JComponent. </summary>
	/// <returns> the current limits of the device. </returns>
	public virtual GRLimits getLimits()
	{
		return _limits;
	}

	/// <summary>
	/// Returns the Graphics instance that is being shared for drawing. </summary>
	/// <returns> the Graphics instance that is being shared for drawing. </returns>
	public virtual Graphics getPaintGraphics()
	{
		return _graphics;
	}

	/// <summary>
	/// Indicate whether the device Y axis starts at the upper left. </summary>
	/// <returns> true if the device Y axis starts at the upper left. </returns>
	public virtual bool getReverseY()
	{
		return _reverse_y;
	}

	/// <summary>
	/// Returns the device units (GRUnits.*). </summary>
	/// <returns> The device units (GRUnits.*). </returns>
	public virtual int getUnits()
	{
		return _units;
	}

	/// <summary>
	/// Initializes member variables. </summary>
	/// <param name="props"> a PropList with settings for the device. </param>
	private void initialize(PropList props)
	{ // For now we always manage the double-buffer internally for JComponents...

		// This turns off the double buffering.  Double-buffering is always
		// on by default in Swing, but RTi's preferred mode of operation
		// is for it to always be OFF by default, and enabled only when selected.

	// TODO (JTS - 2003-05-2)
	// Instead of doing the double-buffering as we are
	// doing it now (with an off-screen buffer that we manage), maybe
	// we could just make the calls to stopDoubleBuffering and 
	// startDoubleBuffering be wrappers around calls to this?
	// i.e.:
	// 
	// public void stopDoubleBuffering() {
	//	RepaintManager.currentManager(this).setDoubleBufferingEnabled(false);
	// }
	//
	// public void startDoubleBuffer() {
	//	RepaintManager.currentManager(this).setDoubleBufferingEnabled(true);
	// }
	//
	// and then ignore the rest of it?  Tests should be run as to the performance
	// effects of each.  One advantage of NOT doing it that way is the enhanced control it gives us.  
	//
	// Food for thought.

		//RepaintManager.currentManager(this).setDoubleBufferingEnabled(false);

		setDoubleBuffered(false);

		// Set the general information...

		this.drawingAreaList = new List<GRDrawingArea>(5);
		_mode = GRUtil.MODE_DRAW;
		_name = "";
		_note = "";
		_orientation = GRDeviceUtil.ORIENTATION_PORTRAIT;
		_page = 0;
		_printing = false;
		//_reverse_y = false;
		_sizedrawn = -1;
		_sizeout = -1;
		_status = GRUtil.STATUS_OPEN;
		_type = 0;
		//_units = GRUnits.MM;	// Default but needs to be reset.
		_units = GRUnits.PIXEL; // GRJComponentDevice

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
		{
			// Set the limits...
			setLimits(limits);
		}

		// The size of the device will have been set in the constructor, but
		// the base class only knows how to store the data.  Force a resize here.
		//resize ( _devx1, _devy1, _devx2, _devy2 );

		// Set to null.  Wait for the derived class to set the graphics for use throughout the drawing event...

		// Set information used by the base class and other code...

		_graphics = null;
		_reverse_y = true; // Java uses y going down.  This is handled properly in
					// GRJComponentDrawingArea.scaleYData().
		// Set the limits to the JComponent size...
		setLimits(getLimits(true));
	}

	/// <summary>
	/// Returns whether the device is drawing in anti-aliased mode or not. </summary>
	/// <returns> whether the device is drawing in anti-aliased mode or not. </returns>
	public virtual bool isAntiAliased()
	{
		return __isAntiAliased;
	}

	/// <summary>
	/// Indicate whether the device is in the process of printing. </summary>
	/// <returns> true if the device is in the process of printing, false if not. </returns>
	public virtual bool isPrinting()
	{
		return _printing;
	}

	// TODO sam 2017-03-03 remove this method as it interferes with newer paint() behavior
	// that calls paintComponent() and derived classes should call setPaintGraphics() to save the graphics.
	/// <summary>
	/// This method is called when the JComponent is to be drawn.  It is expected that
	/// classes extended from this base class will implement a paint() method that
	/// either itself sets the graphics or calls super.paint() to call this method to
	/// set the graphics.  Using a GRJComponentDevice directly will result in this method
	/// being called for resize, etc., and the graphics in effect at the time is set as
	/// the current graphics.   The Graphics can then be used by subsequent calls for
	/// drawing.  The base class paint() is not called from this method.
	/// </summary>
	//public void paint ( Graphics graphics )
	//{	_graphics = (Graphics2D)graphics;
	//}

	/// <summary>
	/// Indicates the end of a page of output.  Used in PS and should be defined in derived classes.
	/// </summary>
	public virtual void pageEnd()
	{
	}

	/// <summary>
	/// End of a plot of output, used for PS.  This should be implemented in derived classes.
	/// </summary>
	public virtual void plotEnd(int flag)
	{
	}

	/// <summary>
	/// Resize the device to the given size. </summary>
	/// <param name="limits"> Size of device (JComponent) as GRLimits. </param>
	/// <seealso cref= GRLimits </seealso>
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
	/// <param name="x1"> New lower-left X-coordinate of device (usually zero). </param>
	/// <param name="y1"> New lower-left Y-coordinate of device (usually zero). </param>
	/// <param name="x2"> New top-right X-coordinate of device. </param>
	/// <param name="y2"> New top-right Y-coordinate of device. </param>
	public virtual void resize(double x1, double y1, double x2, double y2)
	{
		resize2((int)(x2 - x1), (int)(y2 - y1));
	}

	/// <summary>
	/// Resize the device to the given size.  Rename the routine resize2 because there
	/// is a deprecated resize method in JComponent. </summary>
	/// <param name="width"> New width of the device (JComponent). </param>
	/// <param name="height"> New height of the device (JComponent). </param>
	public virtual void resize2(int width, int height)
	{
		setSize(width, height);
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
	/// reset at each paint in code that extends a GRJComponentDevice because Graphics
	/// resources are typically created and destroyed dynamically by the application. </summary>
	/// <param name="graphics"> instance to use for drawing to the device. </param>
	public virtual void setPaintGraphics(Graphics graphics)
	{
		_graphics = (Graphics2D)graphics;
	}

	/// <summary>
	/// Save as an image file.  The file name will be examined for the extension
	/// to determine what kind of file to save as (currently JPEG, JPG, and PNG 
	/// are supported).  If the file extension is not supported, a .jpg extension
	/// is added and it is saved as a JPEG. </summary>
	/// <exception cref="IOException"> if the image used for double-buffering is null. </exception>
	/// <param name="filename"> File name to write.  An appropriate extension will be added. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void saveAsFile(String filename) throws java.io.IOException
	public virtual void saveAsFile(string filename)
	{
		saveAsFile(filename, (PropList)null);
	}

	/// <summary>
	/// Save as an image file.  The file name will be examined for the extension
	/// to determine what kind of file to save as (currently JPEG, JPG, and PNG 
	/// are supported).  If the file extension is not supported, a .jpg extension
	/// is added and it is saved as a JPEG. </summary>
	/// <exception cref="IOException"> if the image used for double-buffering is null. </exception>
	/// <param name="filename"> File name to write.  An appropriate extension will be added. </param>
	/// <param name="props"> Properties for the image.  Currently the only accepted property
	/// is Quality, which can be 0 (low quality, high compression) to 100 (high
	/// quality, no compression).  It might be useful at some point to enable an
	/// Interactive=true option to allow a pop-up dialog to specify JPEG information.
	/// TODO (JTS - 2003-05-05) Evaluate pop-up dialog to query for quality - not as important now that PNG is supported. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void saveAsFile(String filename, RTi.Util.IO.PropList props) throws java.io.IOException
	public virtual void saveAsFile(string filename, PropList props)
	{
		string routine = "GRJComponentDevice.saveAsFile";

		if (_buffer == null)
		{
			throw new IOException("No internal image to save");
		}
		// Else, create an image from the JComponent???

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
		bool jpeg = true;
		string upFilename = filename.ToUpper().Trim();
		if (upFilename.EndsWith(".JPG", StringComparison.Ordinal) || upFilename.EndsWith(".JPEG", StringComparison.Ordinal))
		{
			newfilename = filename;
		}
		else if (upFilename.EndsWith(".PNG", StringComparison.Ordinal))
		{
			newfilename = filename;
			jpeg = false;
		}
		else
		{
			// Add a standard extension...
			newfilename = filename + ".jpg";
		}

		if (jpeg)
		{
			try
			{
				FileStream os = new FileStream(newfilename, FileMode.Create, FileAccess.Write);
				JpegEncoder jpg = new JpegEncoder(_buffer, image_quality, os);
				jpg.Compress();
				os.Flush();
				os.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Message.printWarning(2, routine, "Error saving image file \"" + newfilename + "\"");
				Message.printWarning(2, routine, e);
				throw new IOException("Writing JPEG file \"" + newfilename + "\" failed.");
			}
		}
		else
		{
			// TODO SAM 2011-07-09 why not use ImageIO for all image types so we can do away with
			// the custom JPEG encoder?
			try
			{
				File file = new File(newfilename);
				ImageIO.write(_buffer, "png", file);
			}
			catch (Exception)
			{
				throw new IOException("Error writing png file: " + newfilename);
			}
		}
	}

	/// <summary>
	/// Sets whether the device is drawing in anti-aliased mode or not, for general drawing.
	/// Anti-aliasing for text is always set as per the following
	/// (see: https://docs.oracle.com/javase/tutorial/2d/text/renderinghints.html)
	/// <pre>
	/// graphics2D.setRenderingHint(
	///        RenderingHints.KEY_TEXT_ANTIALIASING,
	///        RenderingHints.VALUE_TEXT_ANTIALIAS_GASP);
	/// </pre> </summary>
	/// <param name="antiAlias"> whether the device is drawing in anti-aliased mode or not. </param>
	public virtual void setAntiAlias(bool antiAlias)
	{
		if (antiAlias)
		{
			_graphics.setRenderingHint(RenderingHints.KEY_ANTIALIASING,RenderingHints.VALUE_ANTIALIAS_ON);
		}
		else
		{
			_graphics.setRenderingHint(RenderingHints.KEY_ANTIALIASING,RenderingHints.VALUE_ANTIALIAS_OFF);
		}
		__isAntiAliased = antiAlias;
		// TODO sam 2017-02-05 need to decide if this is the right place to put the text default
		// Text is always optimized
		_graphics.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING,RenderingHints.VALUE_TEXT_ANTIALIAS_GASP);
		// TODO sam 2017-02-05 LCD could use the following if could guarantee LCD on all of multiple screens
		//_graphics.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING,RenderingHints.VALUE_TEXT_ANTIALIAS_GASP);
	}

	/// <summary>
	/// Sets up clipping on the current device and clips all drawing calls to the
	/// rectangle specified in the GRLimits.  If GRLimits is null, clipping is turned off. </summary>
	/// <param name="clipLimits"> the limits to which to clip drawing.  Any values that lie
	/// outside of the rectangle specified by the GRLimits will not be drawn.  If
	/// clipLimits is null, clipping will be turned off. </param>
	public virtual void setClip(GRLimits clipLimits)
	{
		if (clipLimits == null)
		{
			_graphics.setClip(null);
			return;
		}

		int lx = (int)clipLimits.getLeftX();
		int ty = (int)clipLimits.getTopY();
		int w = (int)clipLimits.getWidth();
		int h = (int)clipLimits.getHeight();

		_graphics.setClip(lx, ty, w, h);
	}

	/// <summary>
	/// Sets the graphics to use in drawing.  Turns off anti aliasing when called. </summary>
	/// <param name="g"> the Graphics to use in drawing. </param>
	public virtual void setGraphics(Graphics g)
	{
		__isAntiAliased = false;
		if (!_doubleBuffered)
		{
			_graphics = (Graphics2D)g;
		}
		else
		{
			_graphics = (Graphics2D)_buffer.getGraphics();
		}
	}

	/// <summary>
	/// Sets the graphics to use in drawing.  Turns off anti aliasing when called. </summary>
	/// <param name="g"> the Graphics2D to use in drawing. </param>
	public virtual void setGraphics(Graphics2D g)
	{
		__isAntiAliased = false;
		if (!_doubleBuffered)
		{
			_graphics = g;
		}
		else
		{
			_graphics = (Graphics2D)_buffer.getGraphics();
		}
		// Make fonts look better
		// See:  https://docs.oracle.com/javase/tutorial/2d/text/renderinghints.html
		_graphics.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING,RenderingHints.VALUE_TEXT_ANTIALIAS_GASP);
	}

	/// <summary>
	/// Set the printing flag.  Set to true when the device is being used for printing,
	/// false when drawing to the screen. </summary>
	/// <param name="printing"> printing flag. </param>
	public virtual void setPrinting(bool printing)
	{
		_printing = printing;
	}

	/// <summary>
	/// Sets up a double buffer for the device, with a buffer size equal to the
	/// size with which the device was initialized.  This method calls 
	/// the other setupDoubleBuffer((int)_devx1, (int)_devy1, (int)_devx2, (int)_devy2);).<para>
	/// This method sets up the buffer region using a BufferedImage that has been
	/// initialized to be of type TYPE_4BYTE_ABGR.  Any pixels that are not drawn in
	/// the buffer will not be drawn to the screen when the buffer is transferred to
	/// the screen; they have a 0 alpha level.
	/// </para>
	/// </summary>
	public virtual void setupDoubleBuffer()
	{
		setupDoubleBuffer((int)_devx1, (int)_devy1, (int)_devx2, (int)_devy2);
	}

	/// <summary>
	/// Sets up a double buffer for the device, with a buffer size equal to the
	/// size with which the device was initialized.  This method calls startDoubleBuffer().<para>
	/// This method sets up the buffer region using a BufferedImage that has been
	/// initialized to be of type TYPE_4BYTE_ABGR.  Any pixels that are not drawn in
	/// the buffer will not be drawn to the screen when the buffer is transferred to
	/// the screen; they have a 0 alpha level.
	/// </para>
	/// </summary>
	/// <param name="x1"> the lower left X of the double buffer. </param>
	/// <param name="y1"> the lower left Y of the double buffer. </param>
	/// <param name="x2"> the upper right X of the double buffer. </param>
	/// <param name="y2"> the upper right Y of the double buffer. </param>
	public virtual void setupDoubleBuffer(int x1, int y1, int x2, int y2)
	{
		int width = x2 - x1;
		int height = y2 - y1;

		if (_buffer != null)
		{
			_buffer = null;
			/* FIXME SAM 2008-01-01 Evaluate why is this here - probably a performance hit
			for (int i = 0; i < 10; i++) {
				System.gc();
			}
			 */
		}

		// Message.printStatus(2, "", "Setting up double buffer size: " + width + "x" + height);
		_buffer = new BufferedImage(width, height,BufferedImage.TYPE_4BYTE_ABGR);
		startDoubleBuffer();
	}

	/// <summary>
	/// Shows what has been drawn to the double buffer by drawing it to the screen.
	/// </summary>
	public virtual void showDoubleBuffer()
	{
		if (_doubleBuffered)
		{
			_graphics.drawImage(_buffer, 0, 0, null);
		}
	}

	/// <summary>
	/// Shows what has been drawn to the double buffer by drawing it to the provided Graphics object. </summary>
	/// <param name="g"> the Graphics object to which to draw the double buffer. </param>
	public virtual void showDoubleBuffer(Graphics g)
	{
		if (_doubleBuffered)
		{
			g.drawImage(_buffer, 0, 0, null);
		}
	}

	/// <summary>
	/// Starts double buffering the drawing done with this device.  Note: This method is called by setupDoubleBuffer.
	/// </summary>
	public virtual void startDoubleBuffer()
	{
		_graphics = (Graphics2D)_buffer.getGraphics();
		_doubleBuffered = true;
		_double_buffering = _doubleBuffered;
	}

	/// <summary>
	/// Stops double buffering drawing calls.
	/// </summary>
	public virtual void stopDoubleBuffer()
	{
		_doubleBuffered = false;
		_double_buffering = _doubleBuffered;
	}

	/// <summary>
	/// Create a string representation of the device, useful for troubleshooting,
	/// will include embedded newlines (\n). </summary>
	/// <param name="outputDrawingAreas"> if true, properties for drawing areas will also be output. </param>
	/// <returns> a simple property=value list of device (and optionally drawing area) properties. </returns>
	public virtual string ToString(bool outputDrawingAreas)
	{
		StringBuilder s = new StringBuilder();
		string nl = "\n";
		s.Append("isAntiAliased=" + __isAntiAliased + nl);
		s.Append("name=" + _name + nl);
		s.Append("reverseY=" + _reverse_y + nl);
		if (outputDrawingAreas)
		{
			// Loop through the drawing areas
			// Make a copy and then sort by name
			IList<GRDrawingArea> das = new List<GRDrawingArea>();
			foreach (GRDrawingArea da in drawingAreaList)
			{
				das.Add(da);
			}
			// Sort by name
			//java.util.Collections.sort(das);
			// TODO sam 2017-02-05 decide whether should implement comparable or not
			for (int ida = 0; ida < drawingAreaList.Count; ida++)
			{
				GRDrawingArea da = drawingAreaList[ida];
				s.Append(nl + "drawingAreaIndex = " + ida + nl);
				s.Append(da.ToString() + nl);
			}
		}
		return s.ToString();
	}

	/// <summary>
	/// Translates the image a specified number of X and Y values.  Calls _graphics.translate(); </summary>
	/// <param name="x"> the x value to translate (can be negative). </param>
	/// <param name="y"> the y value to translate (can be negative).  Note that increasing Y
	/// values of translation will move the image Down, as this is a java call and Y
	/// gets larger the farther down the screen it goes. </param>
	public virtual void translate(int x, int y)
	{
		_graphics.translate(x, y);
	}

	}

}