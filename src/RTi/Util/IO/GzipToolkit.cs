using System.IO;

// GzipToolkit - toolkit to work with gzip files.

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

	/// <summary>
	/// Toolkit to work with gzip files.
	/// </summary>
	public class GzipToolkit
	{

		/// <summary>
		/// Constructor.
		/// </summary>
		public GzipToolkit()
		{

		}

		// See:  http://examples.javacodegeeks.com/core-java/io/fileinputstream/decompress-a-gzip-file-in-java-example/
		/// <summary>
		/// Unzip a zip file to a folder. </summary>
		/// <returns> the path to the unzipped file </returns>
		/// <param name="gzipFile"> path to gzipped file </param>
		/// <param name="destinationFolder"> folder where unzipped file should be created </param>
		/// <param name="unzippedFile"> name of unzippled file (with no leading path) -
		/// if null the output filename will default to input with .gz extension </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String unzipFileToFolder(String gzipFile, String destinationFolder, String unzippedFile) throws java.io.IOException, java.io.FileNotFoundException
		public virtual string unzipFileToFolder(string gzipFile, string destinationFolder, string unzippedFile)
		{
			// Output filename is same as input but without .gzip extension
			File directory = new File(destinationFolder);
			if ((!string.ReferenceEquals(unzippedFile, null)) && unzippedFile.Length > 0)
			{
				unzippedFile = destinationFolder + File.separator + unzippedFile;
			}
			else
			{
				string extension = IOUtil.getFileExtension(gzipFile);
				File gf = new File(gzipFile);
				unzippedFile = destinationFolder + File.separator + gf.getName().substring(0,gf.getName().length() - extension.Length - 1);
			}

			// if the output directory doesn't exist, create it
			if (!directory.exists())
			{
				directory.mkdirs();
			}

			// buffer for read and write data to file
			sbyte[] buffer = new sbyte[2048];

			FileStream fInput = null;
			GZIPInputStream gzipInput = null;
			FileStream fOutput = null;
			try
			{
				fInput = new FileStream(gzipFile, FileMode.Open, FileAccess.Read);
				gzipInput = new GZIPInputStream(fInput);

				// TODO SAM 2016-01-23 What to do when gzip file contains more than single file?
				// Does not seem to often be the case because something like tar is used to create one file?

				fOutput = new FileStream(unzippedFile, FileMode.Create, FileAccess.Write);
				int count = 0;
				while ((count = gzipInput.read(buffer)) > 0)
				{
					// write 'count' bytes to the file output stream
					fOutput.Write(buffer, 0, count);
				}
			}
			finally
			{
				if (gzipInput != null)
				{
					gzipInput.close();
				}
				if (fInput != null)
				{
					fInput.Close();
				}
				if (fOutput != null)
				{
					fOutput.Close();
				}
			}
			return unzippedFile;
		}

		/// <summary>
		/// Open a BufferedReader for a gzip file that contains a single file that is gzipped.
		/// This is useful when a large data input file has been gzipped.
		/// See:  ZipToolkit </summary>
		/// <param name="gzipFile"> zip file to read </param>
		/// <param name="useTempFile"> if 1, save the zipped file to a temporary file; if -1, keep in memory,
		/// if 0 default based on size of file (parameter is currently not enabled). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.BufferedReader openBufferedReaderForSingleFile(String gzipFile, int useTempFile) throws java.io.FileNotFoundException, java.io.IOException
		public virtual StreamReader openBufferedReaderForSingleFile(string gzipFile, int useTempFile)
		{
			int BUFFER = 1024;
			sbyte[] data = new sbyte[BUFFER];
			StreamReader bufferedReader = null;
			MemoryStream bais = null;
			MemoryStream baos = new MemoryStream();
			FileStream fis = new FileStream(gzipFile, FileMode.Open, FileAccess.Read);
			GZIPInputStream zis = new GZIPInputStream(new BufferedInputStream(fis));
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
			return bufferedReader;
		}
	}

}