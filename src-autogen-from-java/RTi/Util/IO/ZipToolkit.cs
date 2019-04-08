using System.Collections.Generic;
using System.IO;

// ZipToolkit - toolkit to work with zip files.

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

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Toolkit to work with zip files.
	/// </summary>
	public class ZipToolkit
	{

		/// <summary>
		/// Constructor.
		/// </summary>
		public ZipToolkit()
		{

		}

		// See:  http://examples.javacodegeeks.com/core-java/util/zip/zipinputstream/java-unzip-file-example/
		/// <summary>
		/// Unzip a zip file to a folder. </summary>
		/// <param name="zipFile"> path to file to unzip </param>
		/// <param name="destinationFolder">  </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> unzipFileToFolder(String zipFile, String destinationFolder, boolean returnList) throws java.io.IOException, java.io.FileNotFoundException
		public virtual IList<string> unzipFileToFolder(string zipFile, string destinationFolder, bool returnList)
		{
			IList<string> outputFileList = new List<string>();
			string routine = this.GetType().Name + ".unzipFileToFolder";
			File directory = new File(destinationFolder);

			// if the output directory doesn't exist, create it
			if (!directory.exists())
			{
				directory.mkdirs();
			}

			// buffer for read and write data to file
			sbyte[] buffer = new sbyte[2048];

			FileStream fInput = new FileStream(zipFile, FileMode.Open, FileAccess.Read);
			ZipInputStream zipInput = new ZipInputStream(fInput);

			ZipEntry entry = zipInput.getNextEntry();

			try
			{
				while (entry != null)
				{
					string entryName = entry.getName();
					string unzippedFile = destinationFolder + File.separator + entryName;
					File file = new File(unzippedFile);

					if (Message.isDebugOn)
					{
						Message.printDebug(1,routine,"Unzip file " + entryName + " to " + file.getAbsolutePath());
					}

					// create the directories of the zip directory
					if (entry.isDirectory())
					{
						File newDir = new File(file.getAbsolutePath());
						if (!newDir.exists())
						{
							bool success = newDir.mkdirs();
							if (success == false)
							{
								Message.printWarning(3,routine,"Problem creating folder \"" + file.getAbsolutePath() + "\"");
							}
						}
					}
					else
					{
						FileStream fOutput = new FileStream(file, FileMode.Create, FileAccess.Write);
						if (returnList)
						{
							outputFileList.Add(unzippedFile);
						}
						int count = 0;
						while ((count = zipInput.read(buffer)) > 0)
						{
							// write 'count' bytes to the file output stream
							fOutput.Write(buffer, 0, count);
						}
						fOutput.Close();
					}
					// close ZipEntry and take the next one
					zipInput.closeEntry();
					entry = zipInput.getNextEntry();
				}
			}
			finally
			{
				// close the last ZipEntry
				zipInput.closeEntry();
				zipInput.close();
				if (fInput != null)
				{
					fInput.Close();
				}
			}
			return outputFileList;
		}

		/// <summary>
		/// Open a BufferedReader for a zip file that contains a single file that is zipped.
		/// This is useful when a large data input file has been zipped.
		/// See:  http://www.oracle.com/technetwork/articles/java/compress-1565076.html </summary>
		/// <param name="zipFile"> zip file to read </param>
		/// <param name="useTempFile"> if 1, save the zipped file to a temporary file; if -1, keep in memory,
		/// if 0 default based on size of file (parameter is currently not enabled). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.BufferedReader openBufferedReaderForSingleFile(String zipFile, int useTempFile) throws java.io.FileNotFoundException, java.io.IOException
		public virtual StreamReader openBufferedReaderForSingleFile(string zipFile, int useTempFile)
		{
			int BUFFER = 1024;
			sbyte[] data = new sbyte[BUFFER];
			StreamReader bufferedReader = null;
			ZipEntry entry;
			MemoryStream bais = null;
			MemoryStream baos = new MemoryStream();
			FileStream fis = new FileStream(zipFile, FileMode.Open, FileAccess.Read);
			ZipInputStream zis = new ZipInputStream(new BufferedInputStream(fis));
			while ((entry = zis.getNextEntry()) != null)
			{
				if (entry.isDirectory())
				{
					zis.close();
					throw new IOException("Zip file \"" + zipFile + "\" contains directory - expecting single file.");
				}
				int count;
				while ((count = zis.read(data, 0, BUFFER)) != -1)
				{
					baos.Write(data, 0, count);
					bais = new MemoryStream(baos.toByteArray());
				}
				// Currently only process the first entry and then return
				StreamReader isr = new StreamReader(bais);
				bufferedReader = new StreamReader(isr);
				zis.close();
				Message.printStatus(2,"","Opened zip file \"" + zipFile + "\".");
				return bufferedReader;
			}
			// If here something is probably wrong but clean up
			zis.close();
			throw new IOException("No file found in zip file \"" + zipFile + "\".");
		}
	}

}