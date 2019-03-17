using System.Collections.Generic;

// WindowManager - abstract class to allow applications to easily manage their windows

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

//------------------------------------------------------------------------------
// WindowManager - abstract class to allow applications to easily manage
//	their windows.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// History:
// 2004-02-05	J. Thomas Sapienza, RTi	Initial version based off of StateMod's
//					WindowManager.
// 2004-02-26	JTS, RTi		Changed docs and code after review 
//					by SAM.
// 2004-12-21	JTS, RTi		Added closeAllWindows().
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//------------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The WindowManager class allows applications to more easily manage their windows.  This
	/// class handles opening 2 kinds of windows:<br><ul>
	/// <li>Windows where only instance can be opened at a time.  These are referred
	/// to simply as Windows.</li>
	/// <li>Windows where multiple instances can be opened at a time.  These are
	/// referred to as Window Instances.</li>
	/// </ul><para>
	/// 
	/// This base class should be extended to create a Window Manager for each
	/// application.  The most basic extended Window Manager should at least 
	/// implement the displayWindow() method and call the super class constructor with 
	/// a parameter that is the number of windows to be managed.  The manager should
	/// also declare integer values for referring to each window, for instance:
	/// <blockquote><pre>
	/// public final int 
	///		WINDOW_MAIN =		0,
	///		WINDOW_OPTIONS = 	1,
	///		WINDOW_NEW_DATA = 	2;
	/// </para>
	/// </pre></blockquote><para>
	/// 
	/// </para>
	/// <b>General Overview</b><para>
	/// 
	/// In the first case, 
	/// a window of a particular type can only have one visible instance.
	/// If the user opens a window and tries to open it again, the
	/// WindowManager will move the already-open copy of the window to the front of
	/// the screen and will not create a new copy of the window.  Until the user 
	/// closes the
	/// window, the WindowManager will continue to do this every time an attempt is made
	/// to re-open the window.  This case will use any method named 
	/// <b>XXXWindow()</b> (where XXX is the action performed by the method, such as
	/// </para>
	/// set or get).<para>
	/// 
	/// The second case works almost identically to the first, except that there can
	/// be multiple instances of each window open.  Each instance of the window is
	/// identified by a unique identifier (see below).  
	/// If the application attempts to 
	/// open another copy of the window with the same unique 
	/// identifier, the already-open copy of the window with that identifier is 
	/// displayed.  This case will use the methods named <b>XXXWindowInstance()</b>.
	/// In addition, any variables that have the word 'instance' in their name are
	/// </para>
	/// only used with window instances.<para>
	/// 
	/// </para>
	/// <b>Unique Identifier</b><para>
	/// The unique identifier that is used to manage multiple instances of the same
	/// window type is application-dependent and can be any Object, though the Object
	/// must implement the <tt>boolean equals(Object)</tt>
	/// </para>
	/// method, which is used to compare identifiers.<para>
	/// 
	/// Note that the unique identifiers are used to identify different Window 
	/// Instances, not Window types.  Window types are identified with integer values
	/// that typically are named WINDOW_*, as defined in the derived window manager
	/// class.  These integers are used as array indices and should therefore be
	/// numbered sequentially, starting with 0 (see below).  
	/// For example, to open a certain Window in
	/// </para>
	/// an application code like this might be used:<para>
	/// <blockquote><pre>
	/// JFrame window = __windowManager.displayWindow(WINDOW_OPTIONS, false);
	/// </para>
	/// </pre></blockquote><para>
	/// To open a certain instance of a particular window, the following code might
	/// </para>
	/// be used:<para>
	/// <blockquote><pre>
	/// JFrame window = __windowManager.displayWindow(WINDOW_NEW_DATA, false,
	///		"ID: " + data.getID());
	/// </para>
	/// </blockquote></pre><para>
	/// 
	/// In the first example only a single instance of the window represented by 
	/// WINDOW_OPTIONS can be open.  In the second example, multiple instances of the
	/// window represented by WINDOW_NEW_DATA can be open, and in this case an instance
	/// with the unique identifier '"ID: " + data.getID()' is being opened or displayed.
	/// </para>
	/// <para>
	/// 
	/// Unique identifiers are not required to be Strings.  They can be
	/// any Object that implements the equals() method.  However, using simple objects
	/// </para>
	/// may improve performance and be more robust.<para>
	/// 
	/// </para>
	/// <b>Initialize</b><para>
	/// Apart from the method displayWindow() that must be extended when 
	/// derived classes are built, 
	/// derived classes must also call the initialize() method of the
	/// super class.  The parameter passed to initialize is the number of window types
	/// that will be managed by the WindowManager.  Here is some example Constructor 
	/// </para>
	/// code:<para>
	/// <blockquote><pre>
	/// protected final static int _NUM_WINDOWS = 12;
	/// public GenericWindowManager() {		
	///		super(_NUM_WINDOWS);
	/// }
	/// </para>
	/// </pre></blockquote><para>
	/// 
	/// </para>
	/// <b>Window Instances</b><para>
	/// 
	/// By default, no window allows multple instances.  This must be turned on with
	/// </para>
	/// a call to setAllowMultipleWindowInstances(). Here some sample code:<para>
	/// <blockquote><pre>
	/// __windowManager.setAllowMultipleWindowInstances(WINDOW_OPTIONS, true);
	/// </para>
	/// </blockquote></pre><para>
	/// This turns on multiple instances for the Options window.  Otherwise, only a 
	/// </para>
	/// single options window could ever be open.<para>
	/// 
	/// </para>
	/// <b>Internal Arrays</b><para>
	/// This class manages several internal arrays (such as _windowStatus and 
	/// _windows) that are sized by the initialize() method to the number of windows
	/// passed in to that method (see above).  The index of each element in these
	/// arrays corresponds to a particular window type.  These unique indices should
	/// </para>
	/// be specified in the derived class.  As an example:<para>
	/// <blockquote><pre>
	/// public final int 
	///		WINDOW_MAIN =		0,
	///		WINDOW_OPTIONS = 	1,
	///		WINDOW_NEW_DATA = 	2;
	/// </para>
	/// </pre></blockquote><para>
	/// In the above example, the window manager's constructor should call
	/// super.initialize() with a parameter of 3, so that the window manager is set up
	/// to manage 3 windows.  At this point, each of the above window types has a 
	/// space set aside for it in the internal arrays.  Information in the arrays about
	/// the main window is in array position 0, information about the options window
	/// is in array position 1, and information about the new data window is in array
	/// </para>
	/// position 2.<para>
	/// 
	/// These window types are used to represent window regardless of whether the
	/// window supports only a single instance being open or multiple instances being
	/// open.  In fact, because of the way this class implements the window management
	/// system, all the windows can be represented as both windows that allow a single
	/// </para>
	/// instance and windows that allow multiple instances, at the same time.<para>
	/// 
	/// An example of how this could be useful uses an imaginary application in 
	/// which a window allows both entry of new data and editing of existing data.  It
	/// is decided that only a single new entry window should be open at a time.  More
	/// than one edit window for existing data can be open at a time.  When the window
	/// for creating new data is created, the setWindow() methods are used.  When
	/// the windows for editing existing data are created, the setWindowInstance() 
	/// methods are used.  In this way, the same window identifier is used to refer
	/// to both types of windows.  Since only a single Window can be open at a time,
	/// using the window methods for new data reults in only a single new data window
	/// ever able to be open.  Using the window instance methods allows multiple data
	/// edit windows to be open.
	/// </para>
	/// <para>
	/// 
	/// The WINDOW_* are used in methods to refer to the window type upon which an 
	/// operation should occur instead of referring to each window type by an integer
	/// </para>
	/// value.<para>
	/// 
	/// </para>
	/// <b>displayWindow()</b><para>
	/// The displayWindow() method must be overridden in order for derived
	/// WindowManager classes to properly open windows.  Because this method will
	/// vary greatly depending on the application in which the WindowManager is 
	/// being used, the following is a step-by-step description of what should 
	/// </para>
	/// happen in this method:<para>
	/// 
	/// <b>1.</b> If the window does not allow multiple instances to be open, then 
	/// </para>
	/// do the following:<para>
	/// <blockquote>
	/// <b>1.1.</b>If no copy of the window is already open
	/// </para>
	/// create a new copy of the window and set it to be open.<para>
	/// <pre>
	///		if (getWindowStatus(winIndex) != STATUS_OPEN) {
	///			JFrame window = new WindowToBeCreated(...);
	///			setWindowOpen(windowIndex, window);
	/// </para>
	/// </pre><para>
	/// <b>1.2.</b> Otherwise the window must already be open.
	/// If so, make sure it is not minimized and then pop the window to
	/// </para>
	/// the front of all the windows.  Code similar to this can be used:<para>
	/// <pre>
	///		if (getWindowStatus(windowType) == STATUS_OPEN) {	
	///			win = getWindow(windowType);
	///			win.setState(win.NORMAL);
	///			win.toFront();
	///			return win;
	///		}
	/// </para>
	/// </pre><para>
	/// </blockquote>
	/// <b>2.</b> Check to see if the window to be displayed allows multiple instances
	/// </para>
	/// to be displayed.<para>
	/// <blockquote>
	/// <b>2.1.</b> If so, then get the unique identifier for the window 
	/// instance to display.  For example, 
	/// have the application generate it and then pass it 
	/// </para>
	/// in to an overloaded version of the displayWindow method.<para>
	/// <b>2.2.</b> If no instance of the window with the identifier is
	/// already open then create a new one and call setInstanceOpen to
	/// set that instance of the window to be marked as open.  The following
	/// </para>
	/// pseudo-code demonstrates this:<para>
	/// <pre>
	///		// id == unique instance identifier passed in to the
	///		// displayWindow call
	///		if (getWindowInstanceStatus(winIndex, id) != STATUS_OPEN) {
	///			JFrame window = new WindowToBeCreated(...);
	///			setWindowInstanceOpen(windowIndex, window, id);
	/// </para>
	/// <pre><para>	
	/// <b>2.3.</b> Call getWindowInstanceStatus for the unique identifier
	/// and see if there is already a window instance with that identifier
	/// that is open.  If the window
	/// exists, make sure the window is not minimized and
	/// </para>
	/// also pop it to the front.  Code similar to this can be used:<para>
	/// <pre>
	///		if (getWindowInstanceStatus(windowType, id) == STATUS_OPEN) {
	///			win = getWindowInstanceWindow(windowType, id);
	///			win.setState(win.NORMAL);
	///			win.toFront();
	///			return win;
	///		}
	/// </para>
	/// </pre><para>
	/// </para>
	/// </blockquote><para>
	/// Working with Windows and Window Instances should not be confusing, as the code
	/// is practically the same.  The major differences are that method names differ
	/// slightly (doSomethingToWindow() versus doSomethingToWindowInstance()) and
	/// that Window Instance methods will require passing in the unique identifier in
	/// </para>
	/// order to locate the instance on which to operate.  <para>
	/// 
	/// </para>
	/// <b>Closing Windows</b><para>
	/// All the code for closing a window inside application JFrames must go
	/// through the WindowManager, if the WindowManager is to manage those windows.
	/// Do not dispose() of JFrames that are managed by the WindowManager -- let the
	/// WindowManager close them instead.
	/// This code will be fairly-JFrame specific, though it must finally call either
	/// closeWindow(...) or closeWindowInstance(...), depending on the type of 
	/// </para>
	/// window it was opened as.  These methods will close the window.<para>
	/// 
	/// As an example, in a JFrame, the calls for closing the frame 
	/// (from a Close button, an error in the frame, or pressing the X button) 
	/// should result in a call being made to __windowManager.closeWindow(WINDOW_X); or
	/// __windowManager.closeWindowInstance(WINDOW_X, id);.
	/// Of course, the name of the Window Manager instance and the window type variable
	/// will be different in actual code, but this is the general pattern.  
	/// </para>
	/// </summary>
	public abstract class WindowManager
	{

	/// <summary>
	/// Class name.  
	/// </summary>
	private readonly string __CLASS = "WindowManager";

	/// <summary>
	/// Window status settings.<ul>
	/// <li>STATUS_UNMANAGED - refers to window instances that have not yet been opened
	/// and managed by the window manager.  There is no reference to them in any of
	/// the Window Manager's internal data arrays.  In other words, the window instance
	/// currently is not open, and it isn't an instance that was open before but has
	/// now been set to closed.  It is just not in the Window Manager's memory -- it's
	/// a brand new instance, and currently unmanaged.  Only used in reference to 
	/// window instances.</li>
	/// <li>STATUS_CLOSED - refers to windows that are closed and to window instances
	/// that are managed but not open yet.</li>
	/// <li>STATUS_OPEN - refers to windows and window instances that are currently open
	/// and being managed.</li>
	/// </summary>
	protected internal const int STATUS_UNMANAGED = -1, STATUS_CLOSED = 0, STATUS_OPEN = 1;
		// INVISIBLE - might be needed if we decide to not fully destroy
		// windows - that is why an integer is tracked (not a boolean)

	/// <summary>
	/// Array that marks which window types are allowed to have multiple instance 
	/// windows open.  This will be sized to the number of windows passed in the
	/// WindowManager constructor, and each array position will correspond to one
	/// window and one of the WINDOW_* identifiers.
	/// </summary>
	protected internal bool[] _allowMultipleWindowInstances;

	/// <summary>
	/// Array to keep track of window status (STATUS_OPEN or STATUS_CLOSED).
	/// This will be sized to the number of windows passed in the
	/// WindowManager constructor, and each array position will correspond to one
	/// window and one of the WINDOW_* identifiers.
	/// </summary>
	protected internal int[] _windowStatus;

	/// <summary>
	/// Array of all the windows that are open.
	/// This will be sized to the number of windows passed in the
	/// WindowManager constructor, and each array position will correspond to one
	/// window and one of the WINDOW_* identifiers.
	/// </summary>
	protected internal JFrame[] _windows;

	/// <summary>
	/// Array of Vectors, each of which contains WindowManagerData Objects representing
	/// the statuses of the windows at given array index positions.  Since each window
	/// type could possibly be made to handle window instances, and since it isn't 
	/// possible to determine how many window instances could open at one time, Vectors
	/// are used.  
	/// TODO - maybe something to set in future to limit the number of instance 
	/// windows that can be open at one time?
	/// </summary>
	protected internal System.Collections.IList[] _windowInstanceInformation;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="numWindows"> the number of windows to set up the arrays to manage. </param>
	public WindowManager(int numWindows)
	{
		initialize(numWindows);
	}

	/// <summary>
	/// Indicates whether the window at the specified index allows multiple instance
	/// windows. </summary>
	/// <param name="windowType"> the index of the window to check. </param>
	/// <returns> whether the window allows multiple instances. </returns>
	public virtual bool allowsMultipleWindowInstances(int windowType)
	{
		return _allowMultipleWindowInstances[windowType];
	}

	/// <summary>
	/// Checks to see if any windows are currently open. </summary>
	/// <returns> true if any windows or window instances are open, false if not. </returns>
	public virtual bool areWindowsOpen()
	{
		for (int i = 0; i < _windowStatus.Length; i++)
		{
			if (_windowStatus[i] == STATUS_OPEN)
			{
				return true;
			}
		}

		System.Collections.IList v;
		WindowManagerData data;
		int size = 0;
		for (int i = 0; i < _windowInstanceInformation.Length; i++)
		{
			if (_windowInstanceInformation[i] != null)
			{
				v = _windowInstanceInformation[i];
				size = v.Count;
				for (int j = 0; j < size; j++)
				{
					data = (WindowManagerData)v[j];
					if (data.getStatus() == STATUS_OPEN)
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Closes all windows that are currently open.
	/// </summary>
	public virtual void closeAllWindows()
	{
		for (int i = 0; i < _windowStatus.Length; i++)
		{
			if (_windowStatus[i] == STATUS_OPEN)
			{
				closeWindow(i);
			}
		}

		System.Collections.IList v;
		WindowManagerData data;
		int size = 0;
		for (int i = 0; i < _windowInstanceInformation.Length; i++)
		{
			if (_windowInstanceInformation[i] != null)
			{
				v = _windowInstanceInformation[i];
				size = v.Count;
				for (int j = 0; j < size; j++)
				{
					data = (WindowManagerData)v[j];
					if (data.getStatus() == STATUS_OPEN)
					{
						closeWindowInstance(i, data.getID());
					}
				}
			}
		}
	}

	/// <summary>
	/// Closes a certain instance of a window. </summary>
	/// <param name="windowType"> the index of the window type. </param>
	/// <param name="id"> the unique identifier of the windows instance to close. </param>
	public virtual void closeWindowInstance(int windowType, object id)
	{
		string routine = __CLASS + ".closeWindowInstance";
		int status = getWindowInstanceStatus(windowType, id);
		Message.printStatus(2, routine, "Closing window : " + windowType + " with instance identifier " + "'" + id + "' and current status " + status);
		if (status == STATUS_CLOSED || status == STATUS_UNMANAGED)
		{
			return;
		}

		JFrame window = getWindowInstanceWindow(windowType, id);
		// Now close the window...
		window.setVisible(false);
		window.dispose();

		removeWindowInstance(windowType, id);
	}

	/// <summary>
	/// Close the window and set its reference to null.  If the window was never opened,
	/// then no action is taken.  Use setWindowOpen() when opening a window to allow
	/// the management of windows to occur. </summary>
	/// <param name="windowType"> the number of the window. </param>
	public virtual void closeWindow(int windowType)
	{
		if (getWindowStatus(windowType) == STATUS_CLOSED)
		{
			Message.printStatus(2, "closeWindow", "Window already closed, " + "not closing again.");
			// No need to do anything...
			return;
		}

		// Get the window...
		JFrame window = getWindow(windowType);
		// Now close the window...
		window.setVisible(false);
		window.dispose();
		// Set the "soft" data...
		setWindowStatus(windowType, STATUS_CLOSED);
		setWindow(windowType, null);
		Message.printStatus(2, "closeWindow", "Window closed: " + windowType);
	}

	/// <summary>
	/// Display the window of the indicated window type.  
	/// If that window type is already displayed, bring it to the front.  
	/// Otherwise create the window.  For more information, see the class description
	/// docs above. </summary>
	/// <param name="window_type"> a window of the specified type, as defined in the 
	/// derived class. </param>
	/// <param name="editable"> Indicates if the data in the window should be editable. </param>
	/// <returns> the window that is displayed. </returns>
	public abstract JFrame displayWindow(int window_type, bool editable);

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~WindowManager()
	{
		_allowMultipleWindowInstances = null;
		_windowStatus = null;
		IOUtil.nullArray(_windows);
		IOUtil.nullArray(_windowInstanceInformation);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Locates the position of an index within the internal data Vectors. </summary>
	/// <param name="windowType"> the type of window the instance of the window is. </param>
	/// <param name="id"> the unique window instance identifier. </param>
	/// <returns> the int location of the window instance within the window instance 
	/// Vector, or -1 if the window instance could not be found. </returns>
	private int findWindowInstancePosition(int windowType, object id)
	{
		WindowManagerData data;
		int size = _windowInstanceInformation[windowType].Count;
		for (int i = 0; i < size; i++)
		{
			data = (WindowManagerData)_windowInstanceInformation[windowType][i];
			if (data.getID().Equals(id))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the status (STATUS_OPEN, STATUS_CLOSED, STATUS_UNMANAGED) of the 
	/// specified window instance. </summary>
	/// <param name="windowType"> the type of window the window instance is. </param>
	/// <param name="id"> the unique identifier of the window instance. </param>
	/// <returns> the status of the window instance (STATUS_OPEN or STATUS_CLOSED) or 
	/// STATUS_UNMANAGED if no such window instance could be found. </returns>
	public virtual int getWindowInstanceStatus(int windowType, object id)
	{
		if (!allowsMultipleWindowInstances(windowType))
		{
			return getWindowStatus(windowType);
		}

		WindowManagerData data;
		int size = _windowInstanceInformation[windowType].Count;
		for (int i = 0; i < size; i++)
		{
			data = (WindowManagerData)_windowInstanceInformation[windowType][i];
			if (data.getID().Equals(id))
			{
				return data.getStatus();
			}
		}

		return STATUS_UNMANAGED;
	}

	/// <summary>
	/// Returns the JFrame for a window instance. </summary>
	/// <param name="windowType"> the type of window. </param>
	/// <param name="id"> the unique window instance identifier. </param>
	/// <returns> the JFrame for the window instance, or null if no matching 
	/// window instance could be found. </returns>
	public virtual JFrame getWindowInstanceWindow(int windowType, object id)
	{
		WindowManagerData data;
		int size = _windowInstanceInformation[windowType].Count;
		for (int i = 0; i < size; i++)
		{
			data = (WindowManagerData)_windowInstanceInformation[windowType][i];
			if (data.getID().Equals(id))
			{
				return data.getWindow();
			}
		}
		return null;
	}

	/// <summary>
	/// Returns the window at the specified position. </summary>
	/// <param name="windowType"> the position of the window (should be one of the public fields
	/// above). </param>
	/// <returns> the window at the specified position. </returns>
	public virtual JFrame getWindow(int windowType)
	{
		return _windows[windowType];
	}

	/// <summary>
	/// Returns the status of the window at the specified position. </summary>
	/// <param name="windowType"> the position of the window (should be one of the public fields
	/// above). </param>
	/// <returns> the status of the window at the specified position. </returns>
	public virtual int getWindowStatus(int windowType)
	{
		return _windowStatus[windowType];
	}

	/// <summary>
	/// Initializes arrays. </summary>
	/// <param name="numWindows"> the number of windows the window manager will manage. </param>
	private void initialize(int numWindows)
	{
		_windowStatus = new int[numWindows];
		_windows = new JFrame[numWindows];
		_allowMultipleWindowInstances = new bool[numWindows];
		_windowInstanceInformation = new List<object>[numWindows];

		for (int i = 0; i < numWindows; i++)
		{
			_windowStatus[i] = STATUS_CLOSED;
			_windows[i] = null;
			_allowMultipleWindowInstances[i] = false;
			_windowInstanceInformation[i] = null;
		}
	}

	/// <summary>
	/// Returns whether a window is currently open. </summary>
	/// <returns> true if the window is open, false if not. </returns>
	public virtual bool isWindowOpen(int windowType)
	{
		if (_windowStatus[windowType] == STATUS_OPEN)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes a window instance from the internal data collection. </summary>
	/// <param name="windowType"> the index of the window from which to remove the window 
	/// instance. </param>
	/// <param name="id"> the identifier of the window instance to remove. </param>
	private void removeWindowInstance(int windowType, object id)
	{
		string routine = __CLASS + ".removeWindowInstance";
		Message.printStatus(2, routine, "Remove instance of window " + windowType + " with identifier '" + id + "'");
		WindowManagerData data;
		for (int i = 0; i < _windowInstanceInformation[windowType].Count; i++)
		{
			data = (WindowManagerData) _windowInstanceInformation[windowType][i];
			if (data.getID().Equals(id))
			{
				_windowInstanceInformation[windowType].RemoveAt(i);
				// because the Vector is being resized within a loop,
				// decrement the counter here when a removeElemenAt
				// is done, otherwise the element that now takes
				// the place of the removed element will be skipped 
				// over.  This is also why the .size() is done within
				// the for() -- because the Vector size changes within
				// the loop.
				i--;
			}
		}

	}

	/// <summary>
	/// Sets whether the specified window should allow multiple window instances 
	/// to be open. The default is for windows to allow only a single window instance. </summary>
	/// <param name="windowType"> the index of the window to allow multiple window instances 
	/// for. </param>
	/// <param name="allow"> whether multiple window instances should be allowed (true) or not 
	/// (false). </param>
	public virtual void setAllowMultipleWindowInstances(int windowType, bool allow)
	{
		_allowMultipleWindowInstances[windowType] = allow;
		if (allow = true)
		{
			_windowInstanceInformation[windowType] = new List<object>();
		}
		else
		{
			_windowInstanceInformation[windowType] = null;
		}
	}

	/// <summary>
	/// Sets the window at the specified position. </summary>
	/// <param name="windowType"> the position of the window (should be one of the public fields
	/// above). </param>
	/// <param name="window"> the window to set. </param>
	public virtual void setWindow(int windowType, JFrame window)
	{
		_windows[windowType] = window;
	}

	/// <summary>
	/// Sets up a window instance that has been opened.  This is just a convenience call
	/// that calls setWindowInstanceWindow() and setWindowInstanceStatus(STATUS_OPEN). </summary>
	/// <param name="windowType"> the index of the window. </param>
	/// <param name="window"> the JFrame of the window instance. </param>
	/// <param name="id"> the unique window instance identifier. </param>
	public virtual void setWindowInstanceOpen(int windowType, JFrame window, object id)
	{
		setWindowInstanceWindow(windowType, window, id);
		setWindowInstanceStatus(windowType, id, STATUS_OPEN);
	}

	/// <summary>
	/// Sets the status of one of a window's instances. </summary>
	/// <param name="windowType"> the index of the window for which to set a window instance 
	/// status. </param>
	/// <param name="id"> the id of the window instance to set a status for. </param>
	/// <param name="status"> the status to set the window instance to. </param>
	public virtual void setWindowInstanceStatus(int windowType, object id, int status)
	{
		string routine = __CLASS + ".setWindowInstanceStatus";
		Message.printStatus(2, routine, "Set instance status for window: " + windowType + ", instance: '" + id + "' to " + status);
		WindowManagerData data = new WindowManagerData(null, id, status);
		if (getWindowInstanceStatus(windowType, id) == STATUS_UNMANAGED)
		{
			Message.printStatus(2, routine, "  No instance with that ID, adding a new one.");
			_windowInstanceInformation[windowType].Add(data);
		}
		else
		{
			int i = findWindowInstancePosition(windowType, id);
			if (i == -1)
			{
				Message.printStatus(2, routine, "  Instance not found!  Adding as a new one.");
				_windowInstanceInformation[windowType].Add(data);
			}
			else
			{
				WindowManagerData updateData = (WindowManagerData)_windowInstanceInformation[windowType][i];
				Message.printStatus(2, routine, "  Instance found, updating status.");
				updateData.setStatus(status);
				_windowInstanceInformation[windowType][i] = updateData;
			}
		}
	}

	/// <summary>
	/// Sets the JFrame represented by a window instance. </summary>
	/// <param name="windowType"> the index of the window to set the window instance JFrame for. </param>
	/// <param name="window"> the JFrame to set the window instance to. </param>
	/// <param name="id"> the unique identifier of the window instance. </param>
	public virtual void setWindowInstanceWindow(int windowType, JFrame window, object id)
	{
		string routine = __CLASS + ".setWindowInstanceWindow";
		Message.printStatus(2, routine, "Set instance window for: " + windowType + " with ID of '" + id + "'");
		WindowManagerData data = new WindowManagerData(window, id, STATUS_CLOSED);
		if (getWindowInstanceStatus(windowType, id) == STATUS_UNMANAGED)
		{
			Message.printStatus(2, routine, "  No instance with that ID, adding a new one.");
			_windowInstanceInformation[windowType].Add(data);
		}
		else
		{
			int i = findWindowInstancePosition(windowType, id);
			if (i == -1)
			{
				Message.printStatus(2, routine, "  Instance not found!  Adding as a new one.");
				_windowInstanceInformation[windowType].Add(data);
			}
			else
			{
				WindowManagerData updateData = (WindowManagerData)_windowInstanceInformation[windowType][i];
				Message.printStatus(2, routine, "  Instance found, updating window.");
				updateData.setWindow(window);
				_windowInstanceInformation[windowType][i] = updateData;
			}
		}
	}

	/// <summary>
	/// Indicate that a window is opened, and provide the JFrame corresponding to the
	/// window.  This method should be called to allow the StateMod GUI to track
	/// windows (so that only one copy of a data set group window is open at a time). </summary>
	/// <param name="windowType"> Window type (see WINDOW_*). </param>
	/// <param name="window"> The JFrame associated with the window. </param>
	public virtual void setWindowOpen(int windowType, JFrame window)
	{
		setWindow(windowType, window);
		setWindowStatus(windowType, STATUS_OPEN);
		Message.printStatus(2, "setWindowOpen", "Window set open: " + windowType);
	}

	/// <summary>
	/// Sets the window at the specified position to be either STATUS_OPEN or 
	/// STATUS_CLOSED. </summary>
	/// <param name="windowType"> the position of the window (should be one of the public fields
	/// above). </param>
	/// <param name="status"> the status of the window (STATUS_OPEN or STATUS_CLOSED) </param>
	private void setWindowStatus(int windowType, int status)
	{
		_windowStatus[windowType] = status;
	}

	}

}