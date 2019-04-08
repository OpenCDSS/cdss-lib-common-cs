using System;
using System.Collections.Generic;
using System.IO;

// JarResources - class for reading resources out of a .jar file

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
// JarResources.java - class for reading resources out of a .jar file
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// Original class file taken from:
// http://www.javaworld.com/javaworld/javatips/jw-javatip49.html
// by John D. Mitchell and Arthur Choi
// Reworked and edited on 15/07/02 by J. Thomas Sapienza, RTi
// ----------------------------------------------------------------------------
// History:
// 2002-07-15	J. Thomas Sapienza, RTi	Initial RTi version
// 2002-07-22	JTS, RTi		Updated, Javadoc'd
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{


	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// The JarResources class that can read and manipulate resources
	/// in jar files.  It can do this one of two ways.  <para>
	/// <ol>
	/// <li>Read resources from the jar file upon demand</b><br>
	/// To do this, declare the JarResources class as usual and then use the 
	/// method <b>getResourceFromJar(...)</b> when a resource is needed.</li>
	/// <li>Read all resources in the jar file into a hash table 
	/// (this providesquicker access)</b><br>
	/// Working with files like this, the JarResources class is declared as usual,
	/// but the function <b>buildResourceHashtable()</b> must be called next.  
	/// This sets up all the resources in the hash table.  To retrieve resources
	/// from the hashtable, use the method <b>getResourceFromHashtable(...)</b></li>
	/// This example Is a test driver. 
	/// Given a jar file and a resource name, it trys to
	/// extract the resource and then tells us whether it could or not.
	/// 
	/// <strong>Example</strong>
	/// If there was have a JAR file which jarred up a bunch of gif image
	/// files. Now, by using JarResources, a user could extract, create, and display
	/// those images on-the-fly.
	/// <pre>
	///    ...
	///    JarResources JR = new JarResources("GifBundle.jar");
	///    Image image = Toolkit.createImage(JR.getResource("logo.gif");
	///    Image logo = Toolkit.getDefaultToolkit().createImage(
	///                  JR.getResources("logo.gif")
	///                  );
	///    ...
	/// </pre>
	/// 
	/// </para>
	/// <para>
	/// </para>
	/// <b>Here is an example main() that uses the JarResource code:<para></b>
	/// <pre>
	/// public static void main(String[] args) throws IOException {
	/// if (args.length != 2) {
	/// System.err.println(
	/// "usage: java JarResources <jar file name> <resource name>"
	/// );
	/// System.exit(1);
	/// }
	/// JarResources jr=new JarResources(args[0]);
	/// jr.buildResourceHashtable();
	/// byte[] buff=jr.getResourceFromHashtable(args[1]);
	/// if (buff == null) {
	/// System.out.println("Could not find " + args[1] + ".");
	/// } 
	/// else {
	/// System.out.println("Found "+
	///		args[1]+ " (length=" + buff.length + ").");
	/// }
	/// } 
	/// </para>
	/// </summary>
	public sealed class JarResources
	{

	/// <summary>
	/// Hashtable for holding the contents of the jar file
	/// </summary>
	private Dictionary<string, sbyte[]> __jarResources_Hashtable = new Dictionary<string, sbyte[]>();

	/// <summary>
	/// Hashtable for holding the sizes of the files in the jar file
	/// </summary>
	private Dictionary<string, int> __sizes_Hashtable = new Dictionary<string, int>();

	/// <summary>
	/// The name of the jar file being worked with
	/// </summary>
	private string __jarFileName;

	/// <summary>
	/// Creates a JarResources, extracting all resources from a Jar
	/// into an internal hashtable, keyed by resource names.<para>
	/// 
	/// The jarFileName that is passed into this method should be the valid name
	/// </para>
	/// and path of a jar file name.  The path can be relative, or absolute. <para>
	/// 
	/// For instance, consider the following directory structure:<br>
	/// <tt>
	/// c:\tmp<br>
	/// c:\tmp\test<br>
	/// c:\tmp\test\lib<br>
	/// c:\tmp\test\lib\images.jar<br>
	/// <br></tt>
	/// If a user is in the c:\tmp\test directory running a program that requires a
	/// jar file, and the JarResources class is being initialized to read from the
	/// images.jar file, the path to the jar file can be given as:<br>
	/// <tt>
	/// lib\images.jar <br>
	/// c:\tmp\test\lib\images.jar<br>
	/// </para>
	/// </tt><para>
	/// If the user was in the same directory as the jar file, the name of the jarfile
	/// would be enough.
	/// </para>
	/// </summary>
	/// <param name="jarFileName"> a jar or zip file </param>
	public JarResources(string jarFileName)
	{
		__jarFileName = jarFileName;
		Message.printStatus(1, "", "SAMX JarResources");
		init();
	}

	/// <summary>
	/// Reads all the resources in the jar file into a hashtable (for quicker 
	/// access)
	/// </summary>
	public void buildResourceHashtable()
	{
		try
		{
			// extract resources and put them into the hashtable.
			ZipInputStream zis = new ZipInputStream(new BufferedInputStream(new FileStream(__jarFileName, FileMode.Open, FileAccess.Read)));
			ZipEntry ze = null;

			while ((ze = zis.getNextEntry()) != null)
			{
				if (ze.isDirectory())
				{
					continue;
				}

				if (Message.isDebugOn)
				{
					Message.printDebug(1, "JarResources.buildResourceHashtable", "ze.getName() = " + ze.getName() + ", getSize() = " + ze.getSize());
				}

				int size = (int) ze.getSize();
				// -1 means unknown size. 
				if (size == -1)
				{
					size = ((int?) __sizes_Hashtable[ze.getName()]).Value;
				}

				sbyte[] b = new sbyte[(int) size];
				int rb = 0;
				int chunk = 0;
				while (((int) size - rb) > 0)
				{
					chunk = zis.read(b, rb, (int) size - rb);
					if (chunk == -1)
					{
						break;
					}
					rb += chunk;
				}

				// add to internal resource hashtable
				__jarResources_Hashtable[ze.getName()] = b;
				if (Message.isDebugOn)
				{
					Message.printDebug(1, "JarResources.buildResourceHashtable", ze.getName() + "  rb=" + rb + ", size = " + size + ", csize = " + ze.getCompressedSize());
				}
			}
			zis.close();
		}
		catch (Exception e)
		{
			Message.printWarning(2, "buildResourceHashtable", "An error occured while building the resource " + "hashtable.");
			Message.printWarning(2, "buildResourceHashtable", e);
		}
	}

	// TODO SAM 2007-04-09 Evaluate whether needed or should be public
	/// <summary>
	/// Dumps a zip entry into a string for debugging purposes.  The string 
	/// is of the form:<para>
	/// <b>[d|f] [stored|deflated] [name] [size (/ deflated_size)]</b><br>
	/// where:<br>
	/// d: the file is a directory<br>
	/// f: the file is a file<br>
	/// stored: the file was not compressed in the jar<br>
	/// deflated: the file was compressed in the jar<br>
	/// name: the name of the file or directory in the jar<br>
	/// size: the size of the file
	/// deflated_size: if the file is deflated, the compressed size of the file<br>
	/// </para>
	/// </summary>
	/// <param name="ze"> a ZipEntry </param>
	/// <returns> the string describing the entry in the jar file </returns>
	/*
	private String dumpZipEntry(ZipEntry ze) {
		StringBuffer sb = new StringBuffer();
		if (ze.isDirectory()) {
			sb.append("d "); 
		} 
		else {
			sb.append("f "); 
		}
		
		if (ze.getMethod() == ZipEntry.STORED) {
			sb.append("stored   "); 
		} 
		else {
			sb.append("deflated ");
		}
		
		sb.append(ze.getName());
		sb.append("\t");
		sb.append("" + ze.getSize());
		
		if (ze.getMethod() == ZipEntry.DEFLATED) {
			sb.append("/" + ze.getCompressedSize());
		}
		return sb.toString();
	}
	*/

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JarResources()
	{
		__jarResources_Hashtable = null;
		__sizes_Hashtable = null;
		__jarFileName = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns a list of all the resources in the jar file in hashtable form. </summary>
	/// <returns> a hashtable of all the resources and their file sizes.  The name
	/// of the resource is the key, and the size of the resource is the value </returns>
	public Dictionary<string, int> getResourcesHashtable()
	{
		return (__sizes_Hashtable);
	}

	/// <summary>
	/// Returns a String list of the names of all the resources in the jar file </summary>
	/// <returns> a String list of the names of all the resources in the jar file </returns>
	public IList<string> getResourcesList()
	{
		IEnumerator<string> e = __sizes_Hashtable.Keys.GetEnumerator();
		IList<string> v = new List<string>(__sizes_Hashtable.Count);
		while (e.MoveNext())
		{
			string key = e.Current;
			v.Add(key);
		}

		return v;
	}

	/// <summary>
	/// Extracts a jar resource as a byte array.  The array can be used in 
	/// methods that take byte array parameters.  For instance, if an image 
	/// is read from a jar file, it can be used in ImageIcon(byte[] imageData);<para>
	/// </para>
	/// </summary>
	/// <param name="name"> a resource name. </param>
	/// <returns> a resource as a byte array </returns>
	public sbyte[] getResourceFromHashtable(string name)
	{
		return (sbyte[])__jarResources_Hashtable[name];
	}

	/// <summary>
	/// Returns a resource from the jar file by reading the resource directly
	/// from the jar file.  This is less efficient that using the hashtable if there
	/// are many resources being retrieved. </summary>
	/// <param name="resourceName"> the name of the resource to return </param>
	/// <returns> a resource in a byte array </returns>
	public sbyte[] getResourceFromJar(string resourceName)
	{
		ZipInputStream zis = null;
		try
		{
			// extract resources and put them into the hashtable.
			zis = new ZipInputStream(new BufferedInputStream(new FileStream(__jarFileName, FileMode.Open, FileAccess.Read)));
			ZipEntry ze = null;

			while ((ze = zis.getNextEntry()) != null)
			{
				if (ze.isDirectory())
				{
					continue;
				}

				int size = (int)ze.getSize();
				// -1 means unknown size. 
				if (size == -1)
				{
					size = __sizes_Hashtable[ze.getName()];
				}

				if (ze.getName().equalsIgnoreCase(resourceName))
				{
					sbyte[] b = new sbyte[(int) size];
					int rb = 0;
					int chunk = 0;

				chunk = zis.read(b, rb, size);
					rb += chunk;
					while ((size - rb) > 0)
					{
						chunk = zis.read(b, rb, (size - rb));

						if (chunk == -1)
						{
							break;
						}

						rb += chunk;
					}
					zis.close();
					return b;
				}
			}

			zis.close();
			return null;
		}
		catch (Exception e)
		{
			Message.printWarning(2, "getResourceFromJar", "An error occured while getting the resource from the jar.");
			Message.printWarning(2, "getResourceFromJar", e);
		}
		finally
		{
			if (zis != null)
			{
				try
				{
					zis.close();
				}
				catch (IOException)
				{
					// Should not happen
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Returns the size of the specified resource in the jar file in bytes. </summary>
	/// <param name="resourceName"> the name of the resource to return the size of </param>
	/// <returns> an int of the size of the specified jar file resource </returns>
	public int getSize(string resourceName)
	{
		return ((int?) __sizes_Hashtable[resourceName]).Value;
	}

	/// <summary>
	/// Reads the jar file and creates a hashtable of all the resources in the
	/// hashtable and their filesizes.
	/// </summary>
	private void init()
	{
		ZipFile zf = null;
		try
		{
			// extracts just sizes only. 
			zf = new ZipFile(__jarFileName);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends java.util.zip.ZipEntry> e = zf.entries();
			IEnumerator<ZipEntry> e = zf.entries();
			while (e.MoveNext())
			{
				ZipEntry ze = e.Current;
				if (!ze.isDirectory())
				{
					__sizes_Hashtable[ze.getName()] = new int?((int) ze.getSize());
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, "init", "An error occured while " + "initializing the JarResources.");
			Message.printWarning(2, "init", e);
		}
		finally
		{
			if (zf != null)
			{
				try
				{
					zf.close();
				}
				catch (IOException)
				{
					// Should not happen
				}
			}
		}
	}

	}

}