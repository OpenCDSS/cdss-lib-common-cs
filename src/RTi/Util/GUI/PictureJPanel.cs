using System;

// PictureJPanel - JPanel that displays an Image

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
// PictureJPanel - Component to display images.
//------------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//------------------------------------------------------------------------------
// Notes:
//	(1)Supports the following images: gif, jpg
//------------------------------------------------------------------------------
// History: 
//
// 2002-09-12	J. Thomas Sapienza, RTi	Created initial version from 
//					PictureJPanel.java
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// PictureJPanel is a JPanel that displays an Image.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class PictureJPanel extends javax.swing.JPanel
	public class PictureJPanel : JPanel
	{

	/// <summary>
	/// The image to be displayed.
	/// </summary>
	private Image __image;

	/// <summary>
	/// PictureJPanel Constructor.
	/// </summary>
	public PictureJPanel() : base()
	{
	}

	/// <summary>
	/// PictureJPanel Constructor. </summary>
	/// <param name="image"> Image object. </param>
	public PictureJPanel(Image image) : base()
	{
		__image = image;
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~PictureJPanel()
	{
		__image = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Manages redraw events for an instance of this class. </summary>
	/// <param name="g"> Graphics object </param>
	public virtual void paintComponent(Graphics g)
	{
		base.paintComponent(g);
		if (__image != null)
		{
				g.drawImage(__image, 0, 0, this);
		}
	}

	/// <summary>
	/// Set a new image for an instance of this class. </summary>
	/// <param name="image"> Image object </param>
	public virtual void setImage(Image image)
	{
		__image = image;
		this.repaint();
	}

	/// <summary>
	/// Set a new image for an instance of this class, given a path to the image file.
	/// The image file is assumed to be in the class path and/or JAR. </summary>
	/// <param name="image_file"> Image file to read (GIF or JPG). </param>
	public virtual void setImage(string image_file)
	{
		/*
		Image image = Toolkit.getDefaultToolkit().getImage(
			getClass().getResource(image_file));
		setImage ( image );
		image = null;
		*/
		Image image = this.getToolkit().getImage(image_file);
		string function = "setImage";
		// use the MediaTracker object to hault processing
		// until the _map_Image is completely loaded.
		MediaTracker mt = new MediaTracker(this);
		mt.addImage(image, 0);
		try
		{
			setImage(image);
			mt.waitForID(0);
			if (mt.isErrorID(0))
			{
				Message.printWarning(2, function, "mt.isErrorID(0)");
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, function, e);
		}
		mt = null;
	}

	}

}