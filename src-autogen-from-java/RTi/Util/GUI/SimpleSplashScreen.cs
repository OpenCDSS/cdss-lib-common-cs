using System;

// SimpleSplashScreen - simple splash screen window

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

//==============================================================================
// SimpleSplashScreen - A simple splash screen window
// -----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// -----------------------------------------------------------------------------
// History:
//
// 2005-01-04 Luiz Teixeira, RTi	Origional version based on samples on
//					on the web.
// 2005-01-07 Luiz Teixeira, RTi	Added dispose on mouse clicked. 
// 2005-04-26	J. Thomas Sapienza, RTi	* Removed all ".*"s in imports.
//					* Added finalize().
// -----------------------------------------------------------------------------
namespace RTi.Util.GUI
{


	/// <summary>
	/// Class to display a simple splash screen window during the initialization of the
	/// main application.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class SimpleSplashScreen extends java.awt.Window
	public class SimpleSplashScreen : Window
	{

	/// <summary>
	/// The string containing the absolute path to the file containing the splash image.
	/// </summary>
	private string __splashFilename;

	/// <summary>
	/// The Image object for the splash image.
	/// </summary>
	private Image __splashImage;

	/// <summary>
	/// The dimension of the splash image.
	/// </summary>
	private int __splashWidth, __splashHeight;

	/// <summary>
	/// Border of the splash screen.
	/// </summary>
	private const int BORDERSIZE = 1;

	/// <summary>
	/// Color of the splash screen border.
	/// </summary>
	private static readonly Color BORDERCOLOR = Color.black;

	/// <summary>
	/// Toolkit object.
	/// </summary>
	internal Toolkit __toolK;

	/// <summary>
	/// Default constructor </summary>
	/// <param name="frm"> the main application Frame </param>
	/// <param name="splashFilename"> the main application Frame </param>
	public SimpleSplashScreen(Frame applicastionFrame, string splashFilename) : base(applicastionFrame)
	{
	// REVISIT (JTS - 2005-04-26)
	// Frame should probably be changed to JFrame
		__splashFilename = splashFilename;
		__toolK = Toolkit.getDefaultToolkit();
		__splashImage = loadSplashImage();
		showSplashScreen();
		applicastionFrame.addWindowListener(new WindowListener(this));

		// Users shall be able to close the splash window by clicking on its
		// display area. This mouse listener listens for mouse clicks and
		// disposes the splash window.
			MouseAdapter disposeOnClick = new MouseAdapterAnonymousInnerClass(this);
			addMouseListener(disposeOnClick);
	}

	private class MouseAdapterAnonymousInnerClass : MouseAdapter
	{
		private readonly SimpleSplashScreen outerInstance;

		public MouseAdapterAnonymousInnerClass(SimpleSplashScreen outerInstance)
		{
			this.outerInstance = outerInstance;
		}

		public void mouseClicked(MouseEvent evt)
		{
			dispose();
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~SimpleSplashScreen()
	{
		__splashFilename = null;
		__splashImage = null;
		__toolK = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Load the splash image.
	/// </summary>
	public virtual Image loadSplashImage()
	{
		MediaTracker tracker = new MediaTracker(this);
		Image result;
		result = __toolK.getImage(__splashFilename);
		tracker.addImage(result, 0);
		try
		{
			tracker.waitForAll();
		}
		catch (Exception e)
		{
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
		}
		__splashWidth = result.getWidth(this);
		__splashHeight = result.getHeight(this);
		return result;
	}

	/// <summary>
	/// Show the splash screen. 
	/// </summary>
	public virtual void showSplashScreen()
	{
		Dimension screenDimension = __toolK.getScreenSize();
		int w = __splashWidth + (2 * BORDERSIZE);
		int h = __splashHeight + (2 * BORDERSIZE);
		int x = (screenDimension.width - w) / 2;
		int y = (screenDimension.height - h) / 2;
		setBounds(x, y, w, h);
		setBackground(BORDERCOLOR);
		setVisible(true);
	}

	/// <summary>
	/// Paint
	/// </summary>
	public virtual void paint(Graphics g)
	{
		g.drawImage(__splashImage, BORDERSIZE, BORDERSIZE, __splashWidth, __splashHeight, this);
	}

	//==============================================================================

	internal class WindowListener : WindowAdapter
	{
			private readonly SimpleSplashScreen outerInstance;

			public WindowListener(SimpleSplashScreen outerInstance)
			{
				this.outerInstance = outerInstance;
			}


	public virtual void windowOpened(WindowEvent Event)
	{
		setVisible(false);
		dispose();
	}

	}

	}
	//==============================================================================

}