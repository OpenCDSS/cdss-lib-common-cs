using System;
using System.Collections.Generic;

// FileCollector - collects a list of files from a folder and it's subfolders, given a fileMask.

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

	// TODO SAM 2017-03-13 need to decide if this should be removed since used in legacy code.
	/// <summary>
	/// Collects a list of files from a folder and it's subfolders, given a fileMask.
	/// 
	/// Usage:
	/// <pre>
	///   String root = ".";
	///   String fileMask = ".java" // find any file containing .java
	/// 
	///   FileCollector fileCollector = new FileCollector(root, fileMask);
	///   fileCollector.getFiles();
	///   // free memory
	///   fileCollector = null;
	///   </pre>
	/// 
	/// @author dre
	/// </summary>
	public class FileCollector
	{
	  private bool _debug = false;
	  private bool _recursive = false;

	  private IList<string> _files = new List<string>();

	  /// <summary>
	  /// Constructor
	  /// </summary>
	  /// <param name="folderPath">    Root-folder where to start the search </param>
	  /// <param name="fileMask">      Part that must be in file that you are looking for
	  ///                                (in your case ".ini") </param>
	  /// <param name="Whether"> to recursively descend into sub-folders </param>
	  /// <exception cref="Exception">  </exception>
	  public FileCollector(string folderPath, string fileMask, bool recursive)
	  {
		_recursive = recursive;

		File rootFile = new File(folderPath);

		if (!rootFile.exists())
		{
			throw new Exception("FolderPathDoesNotExist: " + folderPath);
		}

		findFiles(rootFile, fileMask);
	  }

	  /// <summary>
	  /// Find (recursively) all files with a specific
	  /// mask and do something with each file...
	  /// </summary>
	  /// <param name="folder">    Root-folder where to start the search </param>
	  /// <param name="fileMask">  Part that must be in file that you are looking for
	  ///                  (in your case ".ini") </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void findFiles(java.io.File folder, final String fileMask)
	  private void findFiles(File folder, string fileMask)
	  {
		// since you are running Windows you probably don't care about case sensitivity...
		const bool ignoreCase = false;

		// read all files in actual folder that meets the specified criteria
		File[] files = folder.listFiles(new FileFilterAnonymousInnerClass(this, fileMask, ignoreCase));

		// do something with the files found
		for (int i = 0; files != null && i < files.Length; i++)
		{
			if (files[i] != null)
			{
				//...
				if (_debug)
				{
					Console.WriteLine(files[i]);
				}
				_files.Add(files[i].getAbsolutePath());
				//...
			}
		}

	if (_recursive)
	{
		// for all subfolder to the actual folder...
		File[] folders = folder.listFiles(new FileFilterAnonymousInnerClass2(this));

		int currentFolderIndex = 0;

		for (int i = 0; folders != null && i < folders.Length; i++)
		{
			// move to next folder (recursively) and do the
			// above processing on the files found there
			findFiles(folders[currentFolderIndex++], fileMask);
		}
	}
	  }

	  private class FileFilterAnonymousInnerClass : FileFilter
	  {
		  private readonly FileCollector outerInstance;

		  private string fileMask;
		  private bool ignoreCase;

		  public FileFilterAnonymousInnerClass(FileCollector outerInstance, string fileMask, bool ignoreCase)
		  {
			  this.outerInstance = outerInstance;
			  this.fileMask = fileMask;
			  this.ignoreCase = ignoreCase;
		  }

		  public bool accept(File file)
		  {
			string fileName = file.getName();
			if (fileMask.Length == 0)
			{
				return file.isFile();
			}
			int index = fileName.Length - fileMask.Length;

			//TODO: dre Should support either globbing or java RE conventions
			return file.isFile() && fileMask.regionMatches(ignoreCase, 0, fileName, index, fileMask.Length);
		  }
	  }

	  private class FileFilterAnonymousInnerClass2 : FileFilter
	  {
		  private readonly FileCollector outerInstance;

		  public FileFilterAnonymousInnerClass2(FileCollector outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public bool accept(File dir)
		  {
			return dir.isDirectory();
		  }
	  }

	  /// <summary>
	  /// Returns the list of files found under the root folder matching the 
	  /// file mask.
	  /// 
	  /// @return
	  /// </summary>
	  public virtual IList<string> getFiles()
	  {
		return _files;
	  }

	  /// <summary>
	  /// Returns shorter names for display by stripping the specified root & extension.
	  /// </summary>
	  /// <param name="root"> </param>
	  /// <returns> abbreviated file names </returns>
	  public static IList<string> getAbbrNames(IList<string> fileNames, string root, string ext)
	  {
		IList<string> _filesAbbr = new List<string>(fileNames.Count);

		File f = new File(root);
		int beginIndex = f.getAbsolutePath().length() + 1; // for following sep.
		int extLength = ext.Length;
		string s;

		int len = fileNames.Count;
		for (int i = 0; i < len; i++)
		{
			s = fileNames[i].Substring(beginIndex, (fileNames[i].Length - extLength) - beginIndex);
			_filesAbbr.Add(s);
		}
		return _filesAbbr;
	  }

	  /// <summary>
	  /// Test harness
	  /// 
	  /// Should print all files .java files </summary>
	  /// <param name="args"> </param>
	  public static void Main(string[] args)
	  {
		string folderPath = ".";
		string mask = ".java";

		new FileCollector(folderPath, mask, true);
	  }
	}

}