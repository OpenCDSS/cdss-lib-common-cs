using System.Collections.Generic;

// RegExFileFilter - JFileChooser file filters that match files based on Regular Expressions

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
// RegExFileFilter - A simple way to implement JFileChooser file filters
// that match files based on Regular Expressions.
//---------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//---------------------------------------------------------------------------
// History:
//
// 2002-11-13	J. Thomas Sapienza, RTi	Initial version from SimpleFileFilter.
// 2002-11-25	JTS, RTi		Revised based on comments from SAM.
//					Improved documentation, cleaned up code,
//					removed Initialize method.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//---------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	/// <summary>
	/// RegExFileFilter is a simple JFileChooser file filter that
	/// matches files based on Regular Expressions.
	/// 
	/// A RegExFileFilter contains a <b>single</b> filter for use in a dialog.  This 
	/// single RegExFileFilter can match more than one regular expression.<para>
	/// 
	/// </para>
	/// The description used in the various constructors should be concise.  <para>
	/// 
	/// The following is an example in which a JFileChooser is set up with two
	/// </para>
	/// RegExFileFilters:<para>
	/// <pre>
	/// JFileChooser fc = new JFileChooser();
	/// fc.setDialogTitle("Choose data file to open");
	/// 
	/// RegExFileFilter cff = new RegExFileFilter("^A.*", 
	/// "Filenames starting with 'A'");
	/// 
	/// Vector v = new Vector(2);
	/// v.addElement(".*in.*");
	/// v.addElement(".*out.*");
	/// RegExFileFilter dff = new RegExFileFilter(v,
	/// "Filenames containing 'in' or 'out'");
	/// 
	/// fc.addChoosableFileFilter(cff);
	/// fc.addChoosableFileFilter(dff);
	/// fc.setFileFilter(dff);
	/// </para>
	/// </pre><para>
	/// 
	/// The code above will create a JFileChooser for opening data files of two 
	/// specified kinds.  In the file dialog, the user can choose between opening:<br>
	/// "Filenames starting with 'A'" or<br>
	/// </para>
	/// "Filenames containing 'in' or 'out'"<para>
	/// The default file filter that will be selected when the JFileChooser appears
	/// </para>
	/// on-screen will be dff, or "Filenames containing 'in' or 'out'"<para>
	/// 
	/// The above are very simple examples, and are not meant to explain anything
	/// about regular expressions.  To learn more about regular expressions, search
	/// </para>
	/// the web or see the Javadocs for java.lang.String.matches().<para>
	/// 
	/// Note that the developer can choose to have the description for a filter 
	/// show the regular expression that is used to match with (ie, the chooser 
	/// could instead show:<br>
	/// "Filenames starting with 'A' (^A.*)"<br>
	/// but this could confuse users.
	/// </para>
	/// </summary>
	public class RegExFileFilter : FileFilter
	{

	/// <summary>
	/// Whether the regular expressions that files are matched with should be 
	/// shown in the description.  
	/// </summary>
	private bool __displayRegExs = false;

	/// <summary>
	/// A description of the filter.
	/// </summary>
	private string __description = null;

	/// <summary>
	/// A long description of the filter.  This is different from the short 
	/// description in that if the regular expressions with which files are 
	/// matched are to be shown in the JFileChooser, this description will 
	/// contain both the short description and the regular expressions.
	/// See showRegExListInDescription() for more information.
	/// </summary>
	//TODO SAM 2007-05-09 Evaluate if needed.
	//private String __fullDescription = null;

	/// <summary>
	/// A list of all the regular expressions in the filter.
	/// </summary>
	private IList<string> __filters = null;

	/// <summary>
	/// Creates a RegExFileFilter that will filter for the given regular expression,
	/// and which has the given description. </summary>
	/// <param name="regex"> a regular expression to filter files for </param>
	/// <param name="description"> a concise description of the filter (e.g., "Shockwave Media Files"). </param>
	public RegExFileFilter(string regex, string description)
	{
		__filters = new List<string>();

		if (string.ReferenceEquals(regex, null))
		{
			return;
		}

		__filters.Add(regex);

		__description = description;
		//__fullDescription = getDescription();
	}

	/// <summary>
	/// Creates a RegExFileFilter that will filter for each regular expression in the
	/// list of Strings, and will use the given description. </summary>
	/// <param name="filters"> a list of Strings, each of which is a regular 
	/// expression to be used as a filter. </param>
	/// <param name="description"> a concise description of the filter. (e.g., "Shockwave Media Files"). </param>
	public RegExFileFilter(IList<string> filters, string description)
	{
		__filters = new List<string>();

		for (int i = 0; i < filters.Count; i++)
		{
			__filters.Add(filters[i]);
		}
		__description = description;
		//__fullDescription = getDescription();
	}

	/// <summary>
	/// Checks a File to see if it is matched by one of the regular expressions
	/// in the filter. </summary>
	/// <param name="f"> the File to check </param>
	/// <returns> true if the File matches one of the regular expressions </returns>
	public virtual bool accept(File f)
	{
		if (f != null)
		{
			// the next line makes it so that directories always match
			// the file filter.  If directories were not set to match,
			// then a file dialog would not have the capability to browse
			// into other directories, as they would not show up in the
			// dialog.
			if (f.isDirectory())
			{
				return true;
			}
			string filename = f.getName();
			for (int i = 0; i < __filters.Count; i++)
			{
				if (filename.matches((string)__filters[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~RegExFileFilter()
	{
		__description = null;
		__filters = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the full description for the filter.  The description will consist
	/// of the short description, and that could possibly be followed by a list 
	/// of all the regular expressions, depending on how 
	/// showRegExListInDescription() was called.  If showRegExListInDescription()
	/// hadn't been called yet, the tailing list of all the regular expressions
	/// will not be shown.
	/// Example:
	/// "Filenames with 'in' or 'out followed by a 4-digit number'" or
	/// "Filenames with 'in' or 'out' (.*in.*, .*out.*\D\D\D\D.*)"
	/// Note that '.' are not literals.  This is explained in the documentation for
	/// regular expressions.  To do a literal '.', use '\.'. </summary>
	/// <returns> the whole description of the filter. </returns>
	public virtual string getDescription()
	{
		string fullDescription = "";

		if (__displayRegExs == true)
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
			for (int i = 0; i < size; i++)
			{
				if (i > 0)
				{
					fullDescription += ", ";
				}
				fullDescription += (string) __filters[i];
			}
			fullDescription += ")";
		}
		else
		{
			fullDescription = __description;
		}
		return fullDescription;
	}

	/// <summary>
	/// Sets whether to display the list of regular expressions in the filter 
	/// description shown in the dialog.  If true, the dialog will display a 
	/// phrase similar to
	/// "Example Files (.*\.ex, .*\..exf)".  Otherwise, only "Example Files" 
	/// would be shown.
	/// The default behavior is to not show any of the regular expressions. </summary>
	/// <param name="displayRegExs"> if true, the regular expressions will be shown in the 
	/// list of filters in the dialog, otherwise, they won't be. </param>
	public virtual void showRegExListInDescription(bool displayRegExs)
	{
		__displayRegExs = displayRegExs;
		//__fullDescription = getDescription();
	}

	}

}