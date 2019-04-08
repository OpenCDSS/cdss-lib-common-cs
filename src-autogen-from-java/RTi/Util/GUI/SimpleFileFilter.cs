using System;
using System.Collections.Generic;

// SimpleFileFilter - filter file selection based on file extension

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
// SimpleFileFilter 
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
//
// 03 Oct 2002	J. Thomas Sapienza, RTi	Initial version.
// 10 Oct 2002	JTS, RTi		Javadoc'd
// 24 Oct 2002	JTS, RTi		Corrected error that left extensions
//					null when adding them as individual
//					Strings.
// 12 Nov 2002	JTS, RTi		Moved into RTi.Util.GUI;  Revised code
//					according to SAM's comments.  Mostly
//					reformatting and comment changes, but 
//					also:
//					* descriptions are now required when
//					  a filter is made.
//					* filter-matching is done with case
//					  sensitivity now, for UNIX systems.
//					* removed the setDescription method and
//					  inlined it.
//					* removed and inlined the addExtension
//					  method
// 2003-08-25	JTS, RTi		Class now implements the 
//					java.io.FileFilter interface, so that 
//					it can be used for multiple file 
//					filtering purposes.
// 2003-08-27	JTS, RTi		Added getFilters().
// 2003-09-23	JTS, RTi		Added getShortDescription().
// 2004-05-04	JTS, RTi		Added the NA member variable.
// 2005-04-26	JTS, RTi		Added finalize().
// 2006-02-01	JTS, RTi		Added __allFiles boolean flag support 
//					for single-extension file filters.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//---------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using IOUtil = RTi.Util.IO.IOUtil;

	// These classes are also used, but are not imported because they both must
	// be referred to with the full path name in order to avoid conflicts.
	//import java.io.FileFilter;
	//import javax.swing.filechooser.FileFilter;

	/// <summary>
	/// <para>
	/// SimpleFileFilter is an easy way to implement File Filters in JFileChoosers.
	/// A SimpleFileFilter contains a _single_ filter for use in a dialog, and
	/// file extensions that should be matched against should NOT be added to 
	/// </para>
	/// the filter with a period before the characters (ie, add "gif", not ".gif").<para>
	/// 
	/// For example, if building a graphics application, the designer would still need 
	/// to create a separate filter object for .gifs, .jpegs, .bmps, etc.  When 
	/// extensions are passed in to the Filter constructors, they can be in the form
	/// (using JPegs as the example): .jpeg or jpeg -- the period automatically will be added.
	/// </para>
	/// <para>
	/// The description used in the various constructors should be fairly concise.  
	/// For .jpegs, the description "jpeg image files" would suffice.
	/// </para>
	/// <para>
	/// </para>
	/// The following is an example in which a JFileChooser is set up with two SimpleFileFilters:<para>
	/// <blockquote><pre>
	/// JFileChooser fc = new JFileChooser();
	/// fc.setDialogTitle("Choose data file to open");
	/// 
	/// SimpleFileFilter cff = new SimpleFileFilter("txt", "Comma-delimited Files");
	/// 
	/// List<String> v = new Vector(2);
	/// v.addElement("dat");
	/// v.addElement("data");
	/// SimpleFileFilter dff = new SimpleFileFilter(v, "Application Data Files");
	/// 
	/// fc.addChoosableFileFilter(cff);
	/// fc.addChoosableFileFilter(dff);
	/// fc.setFileFilter(dff);
	/// </pre></blockquote>
	/// </para>
	/// <para>
	/// The code above will create a JFileChooser for opening data files of two 
	/// specified kinds.  In the file dialog, the user can choose between opening:
	/// "Comma-delimited Files (.txt)" or
	/// "Application Data Files (.dat, .data)"
	/// The default file filter that will be selected when the JFileChooser appears
	/// on-screen will be dff, or "Application Data Files (.dat, .data)"
	/// </para>
	/// </summary>
	public class SimpleFileFilter : javax.swing.filechooser.FileFilter, java.io.FileFilter
	{

	/// <summary>
	/// "N/A" -- a String that can be used when specifying a filter extension that
	/// forces no extension to appear.
	/// </summary>
	public const string NA = "N/A";

	private bool __allFiles = false;

	/// <summary>
	/// Whether the description of the extensions should be used.
	/// </summary>
	private bool __describeExtensions = true;

	/// <summary>
	/// A description of the filter.
	/// </summary>
	private string __description = null;

	/// <summary>
	/// A long description of the filter, containing the list of extensions to be
	/// filtered and the description of the list.
	/// </summary>
	// TODO SAM 2007-05-09 Evaluate whether needed
	//private String __fullDescription = null;

	/// <summary>
	/// A list of all the filtered extensions in the filter.
	/// </summary>
	private IList<string> __filters = null;

	/// <summary>
	/// Creates a SimpleFileFilter that will filter for the given extension with 
	/// the given description.  If the extension is specified as "N/A" -- use the
	/// public NA member variable -- no extension will be displayed. </summary>
	/// <param name="extension"> a String extension (without the preceding '.' ) to
	/// be filtered.  If the extension is specified as "N/A" -- use the
	/// public NA member variable -- no extension will be displayed. </param>
	/// <param name="description"> a couple words describing the filter. </param>
	public SimpleFileFilter(string extension, string description)
	{
		initialize();

		if (string.ReferenceEquals(extension, null))
		{
			return;
		}

		__filters.Add(extension);
		if (extension.Equals(NA))
		{
			__describeExtensions = false;
		}

		if (extension.Equals("*"))
		{
			__allFiles = true;
		}

		__description = description;
		//__fullDescription = getDescription();
	}

	/// <summary>
	/// Creates a SimpleFileFilter that will filter for each extension in the 
	/// list of Strings, using the given description. </summary>
	/// <param name="filters"> a list of Strings, each of which will be an extension to
	/// be filtered for.  For example, "jpg, jpeg" or "htm, html". </param>
	/// <param name="description"> a couple words describing the filter. </param>
	public SimpleFileFilter(IList<string> filters, string description)
	{
		initialize();
		foreach (string filter in filters)
		{
			__filters.Add(filter);
		}
		__description = description;
		//__fullDescription = getDescription();
	}

	/// <summary>
	/// Checks a File to see if it its extension matches one of the extensions in the filter. </summary>
	/// <param name="f"> the File to check the extension of. </param>
	/// <returns> true if the File has a matching extension, false if not. </returns>
	public virtual bool accept(File f)
	{
		if (f != null)
		{
			// the next line makes it so that directories always match
			// the file filter.  If directories were not set to match,
			// then a file dialog would not have the capability to browse
			// into other directories, as they would not show up in the dialog.
			if (f.isDirectory())
			{
				return true;
			}

			if (__allFiles)
			{
				return true;
			}

			string extension = getExtension(f);
			if (!string.ReferenceEquals(extension, null))
			{
				int size = __filters.Count;
				for (int i = 0; i < size; i++)
				{
					if (IOUtil.isUNIXMachine())
					{
						if (extension.Equals(__filters[i]))
						{
							  return true;
						}
					}
					else
					{
						if (extension.Equals(__filters[i], StringComparison.OrdinalIgnoreCase))
						{
							  return true;
						}
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Returns the full description for the filter.  The description would consist
	/// of the short description followed by a list of all the extensions.  
	/// Example:
	/// "Jpeg Image Files (.jpg, .jpeg)" </summary>
	/// <returns> the whole description of the filter. </returns>
	public virtual string getDescription()
	{
		string fullDescription = "";

		if (__describeExtensions == true)
		{
			if (string.ReferenceEquals(__description, null))
			{
				 fullDescription = "(";
			}
			else
			{
				fullDescription = __description + " (";
			}

			int size = __filters.Count;

			if (size == 1 && __allFiles)
			{
				fullDescription += "All Files)";
			}
			else
			{
				for (int i = 0; i < size; i++)
				{
					if (i > 0)
					{
						fullDescription += ", ";
					}
					fullDescription += "." + (string) __filters[i];
				}
				fullDescription += ")";
			}
		}
		else
		{
			fullDescription = __description;
		}
		return fullDescription;
	}

	/// <summary>
	/// Pulls off the extension from the given file and returns it. </summary>
	/// <param name="f"> the File off of which to get the extension. </param>
	/// <returns> the String extension if the file exists, or null if the file doesn't exist. </returns>
	public virtual string getExtension(File f)
	{
		if (f != null)
		{
			string filename = f.getName();
			return IOUtil.getFileExtension(filename);
		}
		return null;
	}

	/// <summary>
	/// Returns the list of extensions for which this file filter filters. </summary>
	/// <returns> the list of extensions for which this file filter filters. </returns>
	public virtual IList<string> getFilters()
	{
		return __filters;
	}

	/// <summary>
	/// Returns the short description, always without any data about the filtered extensions. </summary>
	/// <returns> the short description, always without any data about the filtered extensions. </returns>
	public virtual string getShortDescription()
	{
		return __description;
	}

	/// <summary>
	/// Initialize values.
	/// </summary>
	private void initialize()
	{
		__filters = new List<string>();
	}

	/// <summary>
	/// Sets whether to display the list of extensions in the filter description
	/// shown in the dialog.  If true, the dialog will display a phrase similar to
	/// "Example Files (.ex, .exf)".  Otherwise, only "Example Files" would be shown.
	/// The default behavior is to show all the extensions. </summary>
	/// <param name="describeExtensions"> if true, the extensions will be shown in the 
	/// list of filters in the dialog, otherwise, they won't be. </param>
	public virtual void showExtensionListInDescription(bool describeExtensions)
	{
		__describeExtensions = describeExtensions;
		//__fullDescription = getDescription();
	}

	/// <summary>
	/// Returns a string description of this file filter. </summary>
	/// <returns> a string description of this file filter. </returns>
	public override string ToString()
	{
		return getDescription();
	}

	}

}