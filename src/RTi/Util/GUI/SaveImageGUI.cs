﻿using System;
using System.Collections.Generic;
using System.IO;

// SaveImageGUI - file choosers and image quality choosers for saving files

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

//---------------------------------------------------------------------------
// SaveImageGUI - Displays file choosers and image quality choosers for 
//	saving files, and also does the actual file writing.
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
// 2003-08-28	J. Thomas Sapienza, RTi	Initial version.
// 2003-09-02	JTS, RTi		Hourglass now displays during image
//					saves.
//---------------------------------------------------------------------------

// REVISIT (JTS - 2004-03-12)
// should rename to JGUI

namespace RTi.Util.GUI
{
	// TODO SAM 2015-03-11 Remove if ImageIO package works
	//import com.sun.image.codec.jpeg.JPEGCodec;
	//import com.sun.image.codec.jpeg.JPEGEncodeParam;
	//import com.sun.image.codec.jpeg.JPEGImageEncoder;



	using Element = org.w3c.dom.Element;

	using JPEGImageWriter = com.sun.imageio.plugins.jpeg.JPEGImageWriter;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class displays file chooser and image quality choosers for saving files,
	/// and also does the actual file writing.  It queries the system on which the 
	/// program is running for all the file formats that the current Java VM natively
	/// supports writing, and then displays a file chooser with all the supported 
	/// file extensions.  The user chooses a file name and file extension of the file
	/// to save, and depending on what they select (for instance, for JPEGs) a 
	/// file quality dialog may be displayed.  At the end, the image is written to the file.<para>
	/// </para>
	/// <b>Using this class:</b><para>
	/// Using this class is simple.  Declare an instance of this class with the 
	/// image to be saved and it takes care of the rest.  At the end, retrieve the
	/// </para>
	/// return status (to see why execution did not complete or what the status was) if necessary.<para>
	/// <blockquote>
	/// <tt>
	/// BufferedImage image = ...<br>
	/// JFrame parentJFrame = ...<br>
	/// ...<br>
	/// SaveImageGUI sig = SaveImgeGUI(image, parentJFrame);<br>
	/// &nbsp;<br>
	/// String returnStatus = sig.getReturnStatus();<br>
	/// int index = returnStatus.indexOf(")");<br>
	/// setStatusBarMessageField(returnStatus.substring(index + 1));<br>
	/// setStatusBarStatusField(returnStatus.substring(1, index));<br>
	/// </blockquote>
	/// </tt>
	/// </para>
	/// </summary>
	public class SaveImageGUI
	{

	/// <summary>
	/// The buffered image to write to the file.
	/// </summary>
	private BufferedImage __image;

	/// <summary>
	/// The parent JFrame on which all this class's GUIs will display.
	/// </summary>
	private JFrame __parent;

	/// <summary>
	/// A String with information about the last status of the class before it returns.
	/// It may contain information about an error that occurred or a message saying
	/// that saving was successful.
	/// </summary>
	private string __returnStatus = "(ERROR) No return status set yet.";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="image"> the BufferedImage to save to a file.  Must not be nul. </param>
	public SaveImageGUI(BufferedImage image) : this(image, new JFrame())
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="image"> the BufferedImage to save to a file.  Must not be null. </param>
	/// <param name="parent"> the parent JFrame on which this class's GUIs will be displayed,
	/// and which will be used for setting the mouse cursor hourglass.  Must not be null. </param>
	public SaveImageGUI(BufferedImage image, JFrame parent) : this(image, parent, null)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="image"> the BufferedImage to save to a file.  Must not be null. </param>
	/// <param name="parent"> the parent JFrame on which this class's GUIs will be displayed, 
	/// and which will be used for setting the mouse cursor hourglass.  Must not be null. </param>
	/// <param name="title"> the title to use for the file chooser dialog.  If null, the
	/// title will default to "Select Image to Save". </param>
	public SaveImageGUI(BufferedImage image, JFrame parent, string title)
	{
		__parent = parent;
		__image = image;

		displayFileChooser(title);
	}

	/// <summary>
	/// Takes a String array of the image types that can be saved with the current 
	/// JVM and returns a Vector of SimpleFileFilters, suitable for use in a  JFileChooser. </summary>
	/// <param name="imageTypes"> a String array of the image types that the JVM supports
	/// saving of.  Must not be null. </param>
	/// <returns> a Vector of SimpleFileFilters.  Mostly, the file filters are made
	/// on a 1 to 1 basis for the elements in the <tt>imageTypes</tt> array.  For
	/// JPEG images, however, if there are any "jpg" or "jpeg" elements in the array,
	/// they are both places in the same SimpleFileFilter.  The returned Vector will
	/// never be null, but it may be empty. </returns>
	private IList<SimpleFileFilter> createFileFilters(string[] imageTypes)
	{
		IList<SimpleFileFilter> v = new List<SimpleFileFilter>();
		IList<string> jpeg = new List<string>();
		string s = null;
		SimpleFileFilter sff = null;

		for (int i = 0; i < imageTypes.Length; i++)
		{
			s = imageTypes[i];

			if (s.Equals("png", StringComparison.OrdinalIgnoreCase))
			{
				sff = new SimpleFileFilter("png", "Portable Network Graphics files");
				v.Add(sff);
			}
			else if (s.Equals("jpg", StringComparison.OrdinalIgnoreCase) || s.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
			{
				jpeg.Add(s);
			}
			else if (s.Equals("gif", StringComparison.OrdinalIgnoreCase))
			{
				sff = new SimpleFileFilter("gif", "Graphics Interchange Format files");
				v.Add(sff);
			}
			else
			{
				sff = new SimpleFileFilter(s, s + " files");
				v.Add(sff);
			}
		}

		if (jpeg.Count > 0)
		{
			sff = new SimpleFileFilter(jpeg, "JPEG files");
			v.Add(sff);
		}

		return v;
	}

	/// <summary>
	/// Displays the file chooser from which the user selects the type of file to
	/// save and the name of the file to save.  If the file already exist, the user
	/// is prompted for whether they want to overwrite it. </summary>
	/// <param name="title"> the title to use on the file chooser.  If null, the title defaults
	/// to "Select Image to Save". </param>
	private void displayFileChooser(string title)
	{
		JGUIUtil.setWaitCursor(__parent, true);

		// The file chooser initially set to the directory as specified
		// in JGUIUtil.setLastFileDialogDirectory.  If no directory has
		// been set, the dialog will open in the default directory. On 
		// Windows, this will probably be the user's "My Documents" directory.
		string directory = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = null;
		if (!string.ReferenceEquals(directory, null))
		{
			fc = new JFileChooser(directory);
		}
		else
		{
			fc = new JFileChooser();
		}

		// Create the list of possible filters.  It is conceivable, though
		// HIGHLY unlikely, that there will be no writable file types, in 
		// which case the user will get a mostly unusable file chooser.
		string[] imageTypes = getListOfWritableImageTypes();
		IList<SimpleFileFilter> filters = createFileFilters(imageTypes);

		foreach (SimpleFileFilter filter in filters)
		{
			fc.addChoosableFileFilter(filter);
		}

		// do not let the user choose a file type of "*.* All Files"
		fc.setAcceptAllFileFilterUsed(false);

		if (!string.ReferenceEquals(title, null))
		{
			fc.setDialogTitle(title);
		}
		else
		{
			fc.setDialogTitle("Select Image to Save");
		}

		if (filters.Count > 0)
		{
			fc.setFileFilter((SimpleFileFilter)filters[0]);
		}

		fc.setDialogType(JFileChooser.SAVE_DIALOG);

		JGUIUtil.setWaitCursor(__parent, false);
		int retVal = fc.showSaveDialog(__parent);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			setReturnStatus("(ERROR) User clicked 'cancel' in the file chooser dialog.");
			return;
		}

		SimpleFileFilter selectedFilter = (SimpleFileFilter)fc.getFileFilter();
		JGUIUtil.setWaitCursor(__parent, true);

		string currDir = (fc.getCurrentDirectory()).ToString();
		if (!currDir.Equals(directory, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}
		string filename = fc.getSelectedFile().getName();
		File file = new File(currDir + File.separator + filename);

		saveImage(file, selectedFilter);

		JGUIUtil.setWaitCursor(__parent, false);
	}

	/// <summary>
	/// Queries the JVM for the list of all the image formats that are supported for
	/// image writing and returns a list of all the extensions that can be written. </summary>
	/// <returns> a String array containing a list of all the unique file extensions
	/// that can be written with the current JVM.  Will not be null. </returns>
	private string[] getListOfWritableImageTypes()
	{
		string[] formatNames = ImageIO.getWriterFormatNames();
		formatNames = uniquify(formatNames);
		return formatNames;
	}

	/// <summary>
	/// Returns the return status, which is a String denoting what the state of the
	/// class was when control was returned to the calling program.  The return status
	/// string will always be of the form:<br>
	/// <tt><blockquote>(SUCCESS) TEXT TEXT</blockquote></tt><para>
	/// </para>
	/// or<para>
	/// </para>
	/// <tt><blockquote>(ERROR) TEXT TEXT</blockquote></tt><para>
	/// and is guaranteed to be non-null.
	/// </para>
	/// </summary>
	/// <returns> the return status. </returns>
	public virtual string getReturnStatus()
	{
		return __returnStatus;
	}

	/// <summary>
	/// Creates a dialog that asks the user if they wish to overwrite the existing
	/// file.  If they choose to overwrite the file, the method returns <tt>true</tt>,
	/// otherwise it will return <tt>false</tt>. </summary>
	/// <param name="file"> the file to check if the user wants to overwrite.  Must not be null. </param>
	/// <returns> true if the user choose to overwrite the file, false if they choose not to. </returns>
	private bool overwriteExistingFile(File file)
	{
		string label = null;
		string name = null;
		try
		{
			name = file.getCanonicalPath();
		}
		catch (Exception)
		{
		}
		// Ignore the exception above ... the file obviously has a name
		// and a path (otherwise the File object could not be made), but for
		// some reason getCanonicalPath throws an exception.

		if (!string.ReferenceEquals(name, null))
		{
			label = "The file:\n   " + name + "\nalready exists.  Overwrite?";
		}
		else
		{
			label = "The selected file already exists.  Overwrite?";
		}

		int response = (new ResponseJDialog(__parent, "Overwrite existing file?", label, ResponseJDialog.YES | ResponseJDialog.NO)).response();

		if (response == ResponseJDialog.YES)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// This routine determines the kind of file that needs to be saved and does so accordingly. </summary>
	/// <param name="file"> the File to be saved.  If the file already exists, the user
	/// will be prompted for whether they want to overwrite it or not.  If they choose
	/// 'yes', the file will first be deleted before being written.  If they choose
	/// 'no', the saving will end and control will return to the calling program.  Must not be null. </param>
	/// <param name="filter"> the filter that was selected from the JFileChooser.  Must not be null. </param>
	private void saveImage(File file, SimpleFileFilter filter)
	{
		IList<string> filters = filter.getFilters();

		if (filters.Count == 0)
		{
			setReturnStatus("(ERROR) There was no selected file extension.");
			return;
		}

		string s = (string)filters[0];

		string name = null;
		try
		{
			name = file.getCanonicalPath();
		}
		catch (Exception)
		{
		}
		// Ignore the exception above ... the file obviously has a name
		// and a path (otherwise the File object could not be made), but for
		// some reason getCanonicalPath throws an exception.	

		if (s.Equals("jpg", StringComparison.OrdinalIgnoreCase) || s.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
		{
			if (!string.ReferenceEquals(name, null))
			{
				if (!(StringUtil.endsWithIgnoreCase(name, "jpg") || StringUtil.endsWithIgnoreCase(name, "jpeg")))
				{
					name = name + ".jpg";
					file = new File(name);
				}
			}
			if (file.exists())
			{
				if (!overwriteExistingFile(file))
				{
					setReturnStatus("(SUCCESS) User chose not to overwrite existing file.");
					return;
				}
				file.delete();
			}
			writeJPEG(file);
		}
		else
		{
			if (!string.ReferenceEquals(name, null))
			{
				if (!StringUtil.endsWithIgnoreCase(name, s))
				{
					name = name + "." + s;
					file = new File(name);
				}
			}
			if (file.exists())
			{
				if (!overwriteExistingFile(file))
				{
					setReturnStatus("(SUCCESS) User chose not to overwrite existing file.");
					return;
				}
				file.delete();
			}
			writeImage(file, s);
		}
	}

	/// <summary>
	/// Sets the return status, so that a calling program can see what happened when
	/// this class finally returned control. </summary>
	/// <param name="status"> the return status to set.  The return status string should 
	/// be of the form:<br>
	/// <tt><blockquote>(SUCCESS) TEXT TEXT</blockquote></tt><para>
	/// </para>
	/// or<para>
	/// </para>
	/// <tt><blockquote>(ERROR) TEXT TEXT</blockquote></tt><para>
	/// </para>
	/// and must not be null.  If status is null, the return status will be set to<para>
	/// "(ERROR) Null return status set." </param>
	private void setReturnStatus(string status)
	{
		if (string.ReferenceEquals(status, null))
		{
			__returnStatus = "(ERROR) Null return status set.";
			 return;
		}
		__returnStatus = status;
	}

	/// <summary>
	/// "Unique-ifies" a String array by first lower-casing all the elements in the 
	/// String array and then removing all the ones that have duplicates. </summary>
	/// <param name="strings"> the String array to 'unique-ify'.  Must not be null. </param>
	/// <returns> a new String array where all elements are lower-case and there are no
	/// duplicate elements.  The returned array will never be null, but it may be empty. </returns>
	private string[] uniquify(string[] strings)
	{
		ISet<string> set = new HashSet<object>();
		for (int i = 0; i < strings.Length; i++)
		{
			string name = strings[i].ToLower();
			set.Add(name);
		}
		return ((string[])(set.toArray(new string[0])));
	}

	/// <summary>
	/// Writes the image specified in the constructor to a JPEG file.  Since JPEGs can
	/// be written with variable quality vs. compression levels, a dialog is opened to
	/// ask the user what sort of quality they want for the JPEG. </summary>
	/// <param name="file"> the jpeg file to write.  Must not be null. </param>
	private void writeJPEG(File file)
	{
		JGUIUtil.setWaitCursor(__parent, true);
		// pop up the ImageQualityJDialog to find out what kind of quality
		// the user wants for the JPEG compression.
		ImageQualityJDialog qualityDialog = new ImageQualityJDialog(__parent);
		float quality = (float)(qualityDialog.getQuality());
		quality = quality / 100;

		// Most of the time, RTi's BufferedImages will be stored with Alpha
		// information -- but this causes problems when writing a JPEG.  This
		// will copy the ARGB BufferedImage into a plain RGB BufferedImage with
		// no transparency.  If it is not done, the JPEG will have errors and be unreadable.
		BufferedImage bimg = null;
		int w = __image.getWidth(null);
		int h = __image.getHeight(null);
		int[] pixels = new int[w * h];
		PixelGrabber pg = new PixelGrabber(__image,0,0,w,h,pixels,0,w);
		try
		{
			pg.grabPixels();
		}
		catch (Exception ie)
		{
			Console.WriteLine(ie.ToString());
			Console.Write(ie.StackTrace);
			setReturnStatus("(ERROR) Error getting pixels for writing JPEG file.");
			JGUIUtil.setWaitCursor(__parent, false);
			return;
		}

		bimg = new BufferedImage(w,h,BufferedImage.TYPE_INT_RGB);
		bimg.setRGB(0,0,w,h,pixels,0,w);

		// finally, write out the jpeg
		try
		{
			/* TODO SAM 2015-03-11 Remove if ImageIO works
			FileOutputStream out = new FileOutputStream(file);
	
			JPEGImageEncoder encoder = JPEGCodec.createJPEGEncoder(out);
			JPEGEncodeParam param = encoder.getDefaultJPEGEncodeParam(bimg);
	
			param.setQuality(quality, true);
			encoder.encode(bimg, param);
	
			out.flush();
			out.close();
			*/
			// See:  See:  https://blog.idrsolutions.com/2012/05/replacing-the-deprecated-java-jpeg-classes-for-java-7/
			JPEGImageWriter imageWriter = (JPEGImageWriter)ImageIO.getImageWritersBySuffix("jpeg").next();
			FileStream @out = new FileStream(file, FileMode.Create, FileAccess.Write);
			ImageOutputStream ios = ImageIO.createImageOutputStream(@out);
			imageWriter.setOutput(ios);
			int dpi = 96;
			IIOMetadata imageMetaData = imageWriter.getDefaultImageMetadata(new ImageTypeSpecifier(bimg),null);
			Element tree = (Element)imageMetaData.getAsTree("javax_imageio_jpeg_image_1.0");
			Element jfif = (Element)tree.getElementsByTagName("app0JFIF").item(0);
			jfif.setAttribute("Xdensity", "" + dpi);
			jfif.setAttribute("Ydensity", "" + dpi);
			JPEGImageWriteParam jpegParams = (JPEGImageWriteParam)imageWriter.getDefaultWriteParam();
			jpegParams.setCompressionMode(JPEGImageWriteParam.MODE_EXPLICIT);
			jpegParams.setCompressionQuality(quality);
			imageWriter.write(imageMetaData, new IIOImage(bimg,null,null),null);
			ios.close();
			imageWriter.dispose();
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			setReturnStatus("(ERROR) Exception thrown when writing JPEG.");
			JGUIUtil.setWaitCursor(__parent, false);
			return;
		}

		setReturnStatus("(SUCCESS) JPEG successfully written.");
		JGUIUtil.setWaitCursor(__parent, false);
	}

	/// <summary>
	/// This writes images (other than jpegs) out to the file, using the natively supported writing. </summary>
	/// <param name="file"> the file to write.  Must not be null. </param>
	/// <param name="extension"> the extension of the file to write (for determining the 
	/// kind of file that will be written).  Must not be null. </param>
	private void writeImage(File file, string extension)
	{
		JGUIUtil.setWaitCursor(__parent, true);
		try
		{
			ImageIO.write(__image, extension, file);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			setReturnStatus("(ERROR) Exception thrown when writing " + extension + " file.");
			JGUIUtil.setWaitCursor(__parent, false);
			return;
		}
		setReturnStatus("(SUCCESS) Image successfully written.");
		JGUIUtil.setWaitCursor(__parent, false);
	}

	}

}