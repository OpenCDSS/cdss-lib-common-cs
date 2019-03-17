using System;
using System.IO;

// JpegEncoder - the JPEG main program that performs a jpeg compression of an image

// ----------------------------------------------------------------------------
// History:
//
// 23 May 2000	Steven A. Malers, RTi	See copyright for source.  Add to GR
//					library and test with some GR devices
//					(e.g., GeoView).  Modified the code
//					as follows:
//					* Convert import * to explicit imports.
//					* Use RTi messages instead of errors to
//					  standard error.
//					* Remove derivation from Frame - this
//					  appears to have been used only to
//					  enable use of the Media tracker.
// 2002-11-05	J. Thomas Sapienza, RTi	Reformatted.  Got rid of 4-space tabs,
//					made variable private and public 
//					depending on use.  Eliminated unused
//					variables, cleaned up code, commented
//					where possible, organized methods 
//					alphabetically, and split out into
//					separate classes.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------
// COPYRIGHT:
//
// Version 1.0a
// Copyright (C) 1998, James R. Weeks and BioElectroMech.
// Visit BioElectroMech at www.obrador.com.  Email James@obrador.com.

// See license.txt for details about the allowed used of this software.
// This software is based in part on the work of the Independent JPEG Group.
// See IJGreadme.txt for details about the Independent JPEG Group's license.

// This encoder is inspired by the Java Jpeg encoder by Florian Raemy,
// studwww.eurecom.fr/~raemy.
// It borrows a great deal of code and structure from the Independent
// Jpeg Group's Jpeg 6a library, Copyright Thomas G. Lane.
// See license.txt for details.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	//import java.awt.MediaTracker;
	//import java.awt.Frame;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// JpegEncoder - The JPEG main program which performs a jpeg compression of
	/// an image.
	/// </summary>
	public class JpegEncoder // extends Frame
	{
	/// <summary>
	/// The output stream to be used to write the jpeg.
	/// </summary>
	private BufferedOutputStream __outStream;
	/// <summary>
	/// ?
	/// </summary>
	private JpegEncoder_DCT __dct;
	/// <summary>
	/// ? 
	/// </summary>
	private JpegEncoder_Huffman __huf;
	/// <summary>
	/// An object to store information about the jpeg.
	/// </summary>
	private JpegEncoder_JpegInfo JpegObj;

	//private int code;
	/// <summary>
	/// The height of the jpeg.
	/// </summary>
	private int __imageHeight;
	/// <summary>
	/// The width of the jpeg.
	/// </summary>
	private int __imageWidth;
	/// <summary>
	/// ?
	/// </summary>
	public static int[] __jpegNaturalOrder = new int[] {0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5, 12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28, 35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51, 58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63};
	/// <summary>
	/// The quality of the jpeg.
	/// </summary>
	private int __quality;

	/// <summary>
	/// Constructor.  Sets up all the relvant jpeg generation and writing info
	/// based on the parameters passed in.
	/// </summary>
	/// <param name="image"> the image to be saved as a jpeg </param>
	/// <param name="quality"> the quality (0 - 100) to save the Jpeg as </param>
	/// <param name="out"> the outputstream to write the jpeg with </param>
	public JpegEncoder(Image image, int quality, Stream @out)
	{
		/* SAM - comment this out.  Why is it really needed?
		MediaTracker tracker = new MediaTracker(this);
		tracker.addImage(image, 0);
		try {
			tracker.waitForID(0);
		}
		catch (InterruptedException e) {
			// Got to do something?
		}
		*/

		/*
		* quality of the image.
		* 0 to 100 and from bad image quality, high compression to good
		* image quality low compression
		*/
		__quality = quality;

		/*
		* Getting picture information
		* It takes the Width, Height and RGB scans of the image. 
		*/
		JpegObj = new JpegEncoder_JpegInfo(image);

		__imageHeight = JpegObj.imageHeight;
		__imageWidth = JpegObj.imageWidth;

		if (Message.isDebugOn)
		{
			Message.printDebug(1, "JpegEncoder", "Width=" + __imageWidth + " Height=" + __imageHeight);
		}

		__outStream = new BufferedOutputStream(@out);
		__dct = new JpegEncoder_DCT(__quality);
		__huf = new JpegEncoder_Huffman(__imageWidth,__imageHeight);
	}

	/// <summary>
	/// Only here for backward compatability with older code.
	/// </summary>
	public virtual void Compress()
	{
		compress();
	}

	/// <summary>
	/// Compress the Jpeg according to the quality desired.
	/// </summary>
	public virtual void compress()
	{
		writeHeaders(__outStream);
		writeCompressedData(__outStream);
		writeEOI(__outStream);
		try
		{
			__outStream.flush();
		}
		catch (IOException e)
		{
			Message.printWarning(2, "JpegEncoder.Compress", "IO Error: " + e.Message);
		}
	}

	/// <summary>
	/// Get the quality.
	/// </summary>
	/// <returns> the quality </returns>
	public virtual int getQuality()
	{
		return __quality;
	}

	/// <summary>
	/// Set the quality.
	/// </summary>
	/// <param name="quality"> the quality to set the quality to </param>
	public virtual void setQuality(int quality)
	{
		__quality = quality;
		__dct = new JpegEncoder_DCT(quality);
	}

	/// <summary>
	/// Only here for backwards compatability with older code.
	/// </summary>
	private void WriteArray(sbyte[] data, BufferedOutputStream @out)
	{
		writeArray(data, @out);
	}

	/// <summary>
	/// Writes an array of data to the jpeg file.
	/// </summary>
	/// <param name="data"> the data to be written </param>
	/// <param name="out"> the outputstream to which to write the data </param>
	private void writeArray(sbyte[] data, BufferedOutputStream @out)
	{
		int length;
		try
		{
			length = (((int)(data[2] & 0xFF)) << 8) + (int)(data[3] & 0xFF) + 2;
			@out.write(data, 0, length);
		}
		catch (IOException e)
		{
			Message.printWarning(2, "JpegEncoder.writeArray", "IO Error: " + e.Message);
		}
	}

	/// <summary>
	/// Only here for backwards compatability with older code.
	/// </summary>
	public virtual void WriteCompressedData(BufferedOutputStream outStream)
	{
		writeCompressedData(outStream);
	}

	/// <summary>
	/// This method controls the compression of the image.
	/// Starting at the upper left of the image, it compresses 8x8 blocks
	/// of data until the entire image has been compressed.
	/// </summary>
	/// <param name="outStream"> the outstream to which to write the file. </param>
	public virtual void writeCompressedData(BufferedOutputStream outStream)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: double[][] dctArray2 = new double[8][8];
		double[][] dctArray2 = RectangularArrays.RectangularDoubleArray(8, 8);

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: float[][] dctArray1 = new float[8][8];
		float[][] dctArray1 = RectangularArrays.RectangularFloatArray(8, 8);
		float[][] inputArray;

		int a, b, c, i, j, r;
		int comp;
		int[] dctArray3 = new int[8 * 8];
		int[] lastDCValue = new int[JpegObj.numberOfComponents];
		int minBlockHeight;
		int minBlockWidth;
		int xblockoffset;
		int xpos;
		int yblockoffset;
		int ypos;
		//int zeroArray[] = new int[64];
		// commented out for reasons seen below (grep for 11-05-2002 to find
		// specific section) 

		// This initial setting of minBlockWidth and minBlockHeight is done to
		// ensure they start with values larger than will actually be the case.
		minBlockWidth = ((__imageWidth % 8 != 0) ? (int)(Math.Floor((double) __imageWidth / 8.0) + 1) * 8 : __imageWidth);
		minBlockHeight = ((__imageHeight % 8 != 0) ? (int)(Math.Floor((double) __imageHeight / 8.0) + 1) * 8 : __imageHeight);

		for (comp = 0; comp < JpegObj.numberOfComponents; comp++)
		{
			minBlockWidth = Math.Min(minBlockWidth, JpegObj.blockWidth[comp]);
			minBlockHeight = Math.Min(minBlockHeight, JpegObj.blockHeight[comp]);
		}

		xpos = 0;
		for (r = 0; r < minBlockHeight; r++)
		{
		for (c = 0; c < minBlockWidth; c++)
		{
			xpos = c * 8;
			ypos = r * 8;
			for (comp = 0; comp < JpegObj.numberOfComponents; comp++)
			{
				inputArray = (float[][]) JpegObj.components[comp];
				for (i = 0; i < JpegObj.vSampFactor[comp]; i++)
				{
				for (j = 0; j < JpegObj.hSampFactor[comp]; j++)
				{
					xblockoffset = j * 8;
					yblockoffset = i * 8;
					for (a = 0; a < 8; a++)
					{
					for (b = 0; b < 8; b++)
					{

					// 11-05-2002 Old comments by SAM?  MLT?
					// I believe this is where the dirty line at 
					// the bottom of the image is coming from.  I 
					// need to do a check here to make sure I'm 
					// not reading past image data. This seems to 
					// not be a big issue right now. (04/04/98)

						dctArray1[a][b] = inputArray[ypos + yblockoffset + a][xpos + xblockoffset + b];
					} // match b = 0, < 8
					} // match a = 0, < 8
					// 11-05-2002: Old comments by SAM?  MLT?
					// The following code commented out because on 
					// some images this technique results in poor 
					// right and bottom borders.
					//if ((!JpegObj.lastColumnIsDummy[comp] || 
					//c < Width - 1) && (!JpegObj.lastRowIsDummy[
					//comp] || r < Height - 1)) {
					dctArray2 = __dct.forwardDCT(dctArray1);
					dctArray3 = __dct.quantizeBlock(dctArray2, JpegObj.qTableNumber[comp]);
					//}
					//else {
					//	zeroArray[0] = dctArray3[0];
					//	zeroArray[0] = lastDCValue[comp];
					//	dctArray3 = zeroArray;
					//}
					__huf.HuffmanBlockEncoder(outStream, dctArray3, lastDCValue[comp], JpegObj.dcTableNumber[comp], JpegObj.acTableNumber[comp]);
					lastDCValue[comp] = dctArray3[0];
				} // match j = 0; < JpegObj.hSampFactor[comp]
				} // match i = 0; < JpegObj.vSampFactor[comp]
			} // match comp = 0; < JpegObj.numberOfComponents
		} // match c = 0; < minBlockWidth
		} // match r = 0; < minBlockHeight
		__huf.flushBuffer(outStream);
	}

	/// <summary>
	/// Only here for backwards compatability with older code.
	/// </summary>
	public virtual void WriteEOI(BufferedOutputStream @out)
	{
		writeEOI(@out);
	}

	/// <summary>
	/// Write the end of image byte to the output stream.
	/// </summary>
	/// <param name="out"> the stream to which to write the EOI </param>
	public virtual void writeEOI(BufferedOutputStream @out)
	{
		sbyte[] EOI = new sbyte[] {unchecked((sbyte) 0xFF), unchecked((sbyte) 0xD9)};
		writeMarker(EOI, @out);
	}

	/// <summary>
	/// Only here for backwards compatability with older code.
	/// </summary>
	public virtual void WriteHeaders(BufferedOutputStream @out)
	{
		writeHeaders(@out);
	}

	/// <summary>
	/// Write the headers to the output stream.
	/// </summary>
	/// <param name="out"> the output stream to which to write the jpeg headers </param>
	public virtual void writeHeaders(BufferedOutputStream @out)
	{
		int i, j;
		int index;
		int length;
		int offset;
		int[] tempArray;

		// The order of the following headers doesn't matter.

		// the SOI marker
		sbyte[] SOI = new sbyte[] {unchecked((sbyte) 0xFF), unchecked((sbyte) 0xD8)};
		writeMarker(SOI, @out);

		// the JFIF header
		sbyte[] JFIF = new sbyte[18];
		JFIF[0] = unchecked((sbyte) 0xff);
		JFIF[1] = unchecked((sbyte) 0xe0);
		JFIF[2] = (sbyte) 0x00;
		JFIF[3] = (sbyte) 0x10;
		JFIF[4] = (sbyte) 0x4a;
		JFIF[5] = (sbyte) 0x46;
		JFIF[6] = (sbyte) 0x49;
		JFIF[7] = (sbyte) 0x46;
		JFIF[8] = (sbyte) 0x00;
		JFIF[9] = (sbyte) 0x01;
		JFIF[10] = (sbyte) 0x00;
		JFIF[11] = (sbyte) 0x00;
		JFIF[12] = (sbyte) 0x00;
		JFIF[13] = (sbyte) 0x01;
		JFIF[14] = (sbyte) 0x00;
		JFIF[15] = (sbyte) 0x01;
		JFIF[16] = (sbyte) 0x00;
		JFIF[17] = (sbyte) 0x00;
		writeArray(JFIF, @out);

		// Comment Header
		string comment = "";
		comment = JpegObj.getComment();
		length = comment.Length;
		sbyte[] COM = new sbyte[length + 4];
		COM[0] = unchecked((sbyte) 0xFF);
		COM[1] = unchecked((sbyte) 0xFE);
		COM[2] = unchecked((sbyte)((length >> 8) & 0xFF));
		COM[3] = unchecked((sbyte)(length & 0xFF));
		Array.Copy(JpegObj.getComment().GetBytes(), 0, COM, 4, JpegObj.getComment().Length);
		writeArray(COM, @out);

		// The DQT header
		// 0 is the luminance index and 1 is the chrominance index
		sbyte[] DQT = new sbyte[134];
		DQT[0] = unchecked((sbyte) 0xFF);
		DQT[1] = unchecked((sbyte) 0xDB);
		DQT[2] = (sbyte) 0x00;
		DQT[3] = unchecked((sbyte) 0x84);
		offset = 4;
		for (i = 0; i < 2; i++)
		{
			DQT[offset++] = (sbyte)((0 << 4) + i);
			tempArray = (int[]) __dct.quantum[i];
			for (j = 0; j < 64; j++)
			{
				DQT[offset++] = (sbyte) tempArray[__jpegNaturalOrder[j]];
			}
		}
		writeArray(DQT, @out);

		// Start of Frame Header
		sbyte[] SOF = new sbyte[19];
		SOF[0] = unchecked((sbyte) 0xFF);
		SOF[1] = unchecked((sbyte) 0xC0);
		SOF[2] = (sbyte) 0x00;
		SOF[3] = (sbyte) 17;
		SOF[4] = (sbyte) JpegObj.precision;
		SOF[5] = unchecked((sbyte)((JpegObj.imageHeight >> 8) & 0xFF));
		SOF[6] = unchecked((sbyte)((JpegObj.imageHeight) & 0xFF));
		SOF[7] = unchecked((sbyte)((JpegObj.imageWidth >> 8) & 0xFF));
		SOF[8] = unchecked((sbyte)((JpegObj.imageWidth) & 0xFF));
		SOF[9] = (sbyte) JpegObj.numberOfComponents;
		index = 10;
		for (i = 0; i < SOF[9]; i++)
		{
			SOF[index++] = (sbyte) JpegObj.compID[i];
			SOF[index++] = (sbyte)((JpegObj.hSampFactor[i] << 4) + JpegObj.vSampFactor[i]);
			SOF[index++] = (sbyte) JpegObj.qTableNumber[i];
		}
		writeArray(SOF, @out);

		// The DHT Header
		sbyte[] DHT1;
		sbyte[] DHT2;
		sbyte[] DHT3;
		sbyte[] DHT4;

		int bytes;
		int intermediateIndex;
		int oldIndex;
		int temp;

		length = 2;
		index = 4;
		oldIndex = 4;

		DHT1 = new sbyte[17];
		DHT4 = new sbyte[4];
		DHT4[0] = unchecked((sbyte) 0xFF);
		DHT4[1] = unchecked((sbyte) 0xC4);

		for (i = 0; i < 4; i++)
		{
			bytes = 0;
			DHT1[index++ - oldIndex] = (sbyte)((int[]) __huf.bits[i])[0];
			for (j = 1; j < 17; j++)
			{
				temp = ((int[]) __huf.bits[i])[j];
				DHT1[index++ - oldIndex] = (sbyte) temp;
				bytes += temp;
			}
			intermediateIndex = index;

			DHT2 = new sbyte[bytes];
			for (j = 0; j < bytes; j++)
			{
			DHT2[index++ - intermediateIndex] = (sbyte)((int[]) __huf.val[i])[j];
			}

			DHT3 = new sbyte[index];
			Array.Copy(DHT4, 0, DHT3, 0, oldIndex);
			Array.Copy(DHT1, 0, DHT3, oldIndex, 17);
			Array.Copy(DHT2, 0, DHT3, oldIndex + 17, bytes);
			DHT4 = DHT3;
			oldIndex = index;
		}
		DHT4[2] = unchecked((sbyte)(((index - 2) >> 8) & 0xFF));
		DHT4[3] = unchecked((sbyte)((index - 2) & 0xFF));
		WriteArray(DHT4, @out);

	// Start of Scan Header
			sbyte[] SOS = new sbyte[14];
			SOS[0] = unchecked((sbyte) 0xFF);
			SOS[1] = unchecked((sbyte) 0xDA);
			SOS[2] = (sbyte) 0x00;
			SOS[3] = (sbyte) 12;
			SOS[4] = (sbyte) JpegObj.numberOfComponents;
			index = 5;
			for (i = 0; i < SOS[4]; i++)
			{
					SOS[index++] = (sbyte) JpegObj.compID[i];
					SOS[index++] = (sbyte)((JpegObj.dcTableNumber[i] << 4) + JpegObj.acTableNumber[i]);
			}
			SOS[index++] = (sbyte) JpegObj.ss;
			SOS[index++] = (sbyte) JpegObj.se;
			SOS[index++] = (sbyte)((JpegObj.ah << 4) + JpegObj.al);
			WriteArray(SOS, @out);
	}

	/// <summary>
	/// Writes a marker to the jpeg file.
	/// </summary>
	/// <param name="data"> the marker to write </param>
	/// <param name="out"> the outputstream to which to write the marker </param>
	private void writeMarker(sbyte[] data, BufferedOutputStream @out)
	{
		try
		{
			@out.write(data, 0, 2);
		}
		catch (IOException e)
		{
			Message.printWarning(2, "JpegEncoder.writeMarker", "IO Error: " + e.Message);
		}
	}
	}

}