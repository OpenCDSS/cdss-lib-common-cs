using System.Threading;

/*
 * @(#)SplashWindow.java 1.3 2003-06-01
 *
 * Copyright (c) 1999-2003 Werner Randelshofer
 * Staldenmattweg 2, Immensee, CH-6405, Switzerland
 * All rights reserved.
 *
 * This material is provided "as is", with absolutely no warranty expressed
 * or implied. Any use is at your own risk.
 *
 * Permission to use or copy this software is hereby granted without fee,
 * provided this copyright notice is retained on all copies.
 */

namespace RTi.Util.GUI
{

	/// <summary>
	/// Splash Window to show an image during startup of an application.<para>
	/// 
	/// Usage:
	/// <pre>
	/// // open the splash window
	/// Frame splashOwner = SplashWindow.splash(anImage);
	/// 
	/// // start the application
	/// // ...
	/// 
	/// // dispose the splash window by disposing the frame that owns the window.
	/// splashOwner.dispose();
	/// </pre>
	/// 
	/// </para>
	/// <para>To use the splash window as an about dialog write this:
	/// <pre>
	///  new SplashWindow(
	///      this,
	///      getToolkit().createImage(getClass().getResource("splash.png"))
	/// ).setVisible(true);
	/// </pre>
	/// 
	/// The splash window disposes itself when the user clicks on it.
	/// 
	/// @author Werner Randelshofer, Staldenmattweg 2, Immensee, CH-6405, Switzerland.
	/// @version 1.3 2003-06-01 Revised.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class SplashWindow extends Window
	public class SplashWindow : Window
	{
		private Image splashImage;


		/// <summary>
		/// This attribute indicates whether the method
		/// paint(Graphics) has been called at least once since the
		/// construction of this window.<br>
		/// This attribute is used to notify method splash(Image)
		/// that the window has been drawn at least once
		/// by the AWT event dispatcher thread.<br>
		/// This attribute acts like a latch. Once set to true,
		/// it will never be changed back to false again.
		/// </summary>
		/// <seealso cref= #paint </seealso>
		/// <seealso cref= #splash </seealso>
		private bool paintCalled = false;

		/// <summary>
		/// Constructs a splash window and centers it on the
		/// screen. The user can click on the window to dispose it.
		/// </summary>
		/// <param name="owner">       The frame owning the splash window. </param>
		/// <param name="splashImage"> The splashImage to be displayed. </param>
		public SplashWindow(Frame owner, Image splashImage) : base(owner)
		{
			this.splashImage = splashImage;


			// Load the image
			MediaTracker mt = new MediaTracker(this);
			mt.addImage(splashImage,0);
			try
			{
				mt.waitForID(0);
			}
			catch (InterruptedException)
			{
			}

			// Center the window on the screen.
			int imgWidth = splashImage.getWidth(this);
			int imgHeight = splashImage.getHeight(this);
			setSize(imgWidth, imgHeight);
			Dimension screenDim = Toolkit.getDefaultToolkit().getScreenSize();
			setLocation((screenDim.width - imgWidth) / 2, (screenDim.height - imgHeight) / 2);

			// Users shall be able to close the splash window by
			// clicking on its display area. This mouse listener
			// listens for mouse clicks and disposes the splash window.
			MouseAdapter disposeOnClick = new MouseAdapterAnonymousInnerClass(this);
			addMouseListener(disposeOnClick);
		}

		private class MouseAdapterAnonymousInnerClass : MouseAdapter
		{
			private readonly SplashWindow outerInstance;

			public MouseAdapterAnonymousInnerClass(SplashWindow outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(MouseEvent evt)
			{
				// Note: To avoid that method splash hangs, we
				// must set paintCalled to true and call notifyAll.
				// This is necessary because the mouse click may
				// occur before the contents of the window
				// has been painted.
				lock (outerInstance)
				{
					outerInstance.paintCalled = true;
					Monitor.PulseAll(outerInstance);
				}
				dispose();
			}
		}

		/// <summary>
		/// Updates the display area of the window.
		/// </summary>
		public virtual void update(Graphics g)
		{
			// Note: Since the paint method is going to draw an
			// image that covers the complete area of the component we
			// do not fill the component with its background color
			// here. This avoids flickering.
			g.setColor(getForeground());
			paint(g);
		}
		/// <summary>
		/// Paints the image on the window.
		/// </summary>
		public virtual void paint(Graphics g)
		{
			g.drawImage(splashImage, 0, 0, this);

			// Notify method splash that the window
			// has been painted.
			// Note: To improve performance we do not enter
			// the synchronized block unless we have to.
			if (!paintCalled)
			{
				paintCalled = true;
				lock (this)
				{
					Monitor.PulseAll(this);
				}
			}
		}

		/// <summary>
		/// Constructs and displays a SplashWindow.<para>
		/// This method is useful for startup splashs.
		/// </para>
		/// Dispose the return frame to get rid of the splash window.<para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="splashImage"> The image to be displayed. </param>
		/// <returns>  Returns the frame that owns the SplashWindow. </returns>
		public static Frame splash(Image splashImage)
		{
			Frame f = new Frame();
			SplashWindow w = new SplashWindow(f, splashImage);

			// Show the window.
			w.toFront();
			w.setVisible(true);


			// Note: To make sure the user gets a chance to see the
			// splash window we wait until its paint method has been
			// called at least by the AWT event dispatcher thread.
			if (!EventQueue.isDispatchThread())
			{
				lock (w)
				{
					while (!w.paintCalled)
					{
						try
						{
							Monitor.Wait(w);
						}
						catch (InterruptedException)
						{
						}
					}
				}
			}

			return f;
		}
	}

}