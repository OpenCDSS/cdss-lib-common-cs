using System;
using System.Collections.Generic;

// PaperSizeLookup - facilitate looking up paper sizes.

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


	using StringUtil = RTi.Util.String.StringUtil;

	// TODO SAM 2011-06-24 Evaluate using a singleton, etc., but don't want a bunch of static code if it can be avoided.

	/// <summary>
	/// Facilitate looking up paper sizes.
	/// Borrowed from:  http://www.jpedal.org/gplSrc/org/jpedal/examples/simpleviewer/paper/PaperSizes.java.html
	/// </summary>
	public class PaperSizeLookup
	{

	/// <summary>
	/// Map to convert between MediaSizeName and more readable names.
	/// </summary>
	private IDictionary<string, string> paperNames = new Dictionary<string, string>();

	/// <summary>
	/// Constructor.
	/// </summary>
	protected internal PaperSizeLookup()
	{
		populateDisplayNameMap();
	}

	/// <summary>
	/// Get the common page size name for the page size. </summary>
	/// <param name="the"> mediaName (media.toString() to look up (e.g., "na-letter") </param>
	/// <returns> the common name that can be used for displays (e.g., "North American Letter") </returns>
	public virtual string lookupDisplayName(string mediaName)
	{
		return paperNames[mediaName];
	}

	/// <summary>
	/// Lookup the Media integer from the media name (Media.toString()). </summary>
	/// <param name="mediaName"> name of the media (e.g., "na-letter"). </param>
	/// <returns> the Media that corresponds to the name, or null if not matched. </returns>
	public virtual Media lookupMediaFromName(PrintService printService, string mediaName)
	{
		Media[] supportedMediaArray = (Media [])printService.getSupportedAttributeValues(typeof(Media), null, null);
		if (supportedMediaArray != null)
		{
			for (int i = 0; i < supportedMediaArray.Length; i++)
			{
				if (supportedMediaArray[i].ToString().Equals(mediaName, StringComparison.OrdinalIgnoreCase))
				{
					return supportedMediaArray[i];
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Populate the list of paper sizes corresponding to the paperNames list.
	/// These are protected (?why?) in the MediaSize so need to have this list.
	/// </summary>
	public virtual MediaSizeName lookupMediaSizeNameFromString(string mediaSizeName)
	{
		MediaSizeName[] mediaSizeNameArray = new MediaSizeName[] {MediaSizeName.ISO_A4, MediaSizeName.NA_LETTER, MediaSizeName.ISO_A0, MediaSizeName.ISO_A1, MediaSizeName.ISO_A2, MediaSizeName.ISO_A3, MediaSizeName.ISO_A5, MediaSizeName.ISO_A6, MediaSizeName.ISO_A7, MediaSizeName.ISO_A8, MediaSizeName.ISO_A9, MediaSizeName.ISO_A10, MediaSizeName.ISO_B0, MediaSizeName.ISO_B1, MediaSizeName.ISO_B2, MediaSizeName.ISO_B3, MediaSizeName.ISO_B4, MediaSizeName.ISO_B5, MediaSizeName.ISO_B6, MediaSizeName.ISO_B7, MediaSizeName.ISO_B8, MediaSizeName.ISO_B9, MediaSizeName.ISO_B10, MediaSizeName.JIS_B0, MediaSizeName.JIS_B1, MediaSizeName.JIS_B2, MediaSizeName.JIS_B3, MediaSizeName.JIS_B4, MediaSizeName.JIS_B5, MediaSizeName.JIS_B6, MediaSizeName.JIS_B7, MediaSizeName.JIS_B8, MediaSizeName.JIS_B9, MediaSizeName.JIS_B10, MediaSizeName.ISO_C0, MediaSizeName.ISO_C1, MediaSizeName.ISO_C2, MediaSizeName.ISO_C3, MediaSizeName.ISO_C4, MediaSizeName.ISO_C5, MediaSizeName.ISO_C6, MediaSizeName.NA_LEGAL, MediaSizeName.EXECUTIVE, MediaSizeName.LEDGER, MediaSizeName.TABLOID, MediaSizeName.INVOICE, MediaSizeName.FOLIO, MediaSizeName.QUARTO, MediaSizeName.JAPANESE_POSTCARD, MediaSizeName.JAPANESE_DOUBLE_POSTCARD, MediaSizeName.A, MediaSizeName.B, MediaSizeName.C, MediaSizeName.D, MediaSizeName.E, MediaSizeName.ISO_DESIGNATED_LONG, MediaSizeName.ITALY_ENVELOPE, MediaSizeName.MONARCH_ENVELOPE, MediaSizeName.PERSONAL_ENVELOPE, MediaSizeName.NA_NUMBER_9_ENVELOPE, MediaSizeName.NA_NUMBER_10_ENVELOPE, MediaSizeName.NA_NUMBER_11_ENVELOPE, MediaSizeName.NA_NUMBER_12_ENVELOPE, MediaSizeName.NA_NUMBER_14_ENVELOPE, MediaSizeName.NA_6X9_ENVELOPE, MediaSizeName.NA_7X9_ENVELOPE, MediaSizeName.NA_9X11_ENVELOPE, MediaSizeName.NA_9X12_ENVELOPE, MediaSizeName.NA_10X13_ENVELOPE, MediaSizeName.NA_10X14_ENVELOPE, MediaSizeName.NA_10X15_ENVELOPE, MediaSizeName.NA_5X7, MediaSizeName.NA_8X10};

		for (int i = 0; i < mediaSizeNameArray.Length; i++)
		{
			if (mediaSizeNameArray[i].ToString().Equals(mediaSizeName, StringComparison.OrdinalIgnoreCase))
			{
				return mediaSizeNameArray[i];
			}
		}
		return null;
	}

	// FIXME SAM 2011-06-26 Need to enable this somehow
	/// <summary>
	/// Lookup a MediaSizeName instance from the media size name (Media.toString(), when Media is a MediaSizeName). </summary>
	/// <param name="mediaSizeName"> name of the media size (e.g., "na-letter"). </param>
	/// <returns> the MediaSizeName that corresponds to the name, or null if not matched.
	/// public MediaSizeName lookupMediaSizeNameFromName ( String mediaSizeName )
	/// {   // Why are the string table and EnumSyntax array protected?  How can a lookup be done?
	///    //String [] stringTable = MediaSizeName.getStringTable();
	///    return null;
	/// } </returns>

	/// <summary>
	/// Fill the name map from standardized (Media.toString() to more usable names.
	/// </summary>
	private void populateDisplayNameMap()
	{
		paperNames["iso-a0"] = "A0";
		paperNames["iso-a1"] = "A1";
		paperNames["iso-a2"] = "A2";
		paperNames["iso-a3"] = "A3";
		paperNames["iso-a4"] = "A4";
		paperNames["iso-a5"] = "A5";
		paperNames["iso-a6"] = "A6";
		paperNames["iso-a7"] = "A7";
		paperNames["iso-a8"] = "A8";
		paperNames["iso-a9"] = "A9";
		paperNames["iso-a10"] = "A10";
		paperNames["iso-b0"] = "B0";
		paperNames["iso-b1"] = "B1";
		paperNames["iso-b2"] = "B2";
		paperNames["iso-b3"] = "B3";
		paperNames["iso-b4"] = "B4";
		paperNames["iso-b5"] = "B5";
		paperNames["iso-b6"] = "B6";
		paperNames["iso-b7"] = "B7";
		paperNames["iso-b8"] = "B8";
		paperNames["iso-b9"] = "B9";
		paperNames["iso-b10"] = "B10";
		paperNames["na-letter"] = "North American Letter";
		paperNames["na-legal"] = "North American Legal";
		paperNames["na-8x10"] = "North American 8x10 inch";
		paperNames["na-5x7"] = "North American 5x7 inch";
		paperNames["executive"] = "Executive";
		paperNames["folio"] = "Folio";
		paperNames["invoice"] = "Invoice";
		paperNames["tabloid"] = "Tabloid";
		paperNames["ledger"] = "Ledger";
		paperNames["quarto"] = "Quarto";
		paperNames["iso-c0"] = "C0";
		paperNames["iso-c1"] = "C1";
		paperNames["iso-c2"] = "C2";
		paperNames["iso-c3"] = "C3";
		paperNames["iso-c4"] = "C4";
		paperNames["iso-c5"] = "C5";
		paperNames["iso-c6"] = "C6";
		paperNames["iso-designated-long"] = "ISO Designated Long size";
		paperNames["na-10x13-envelope"] = "North American 10x13 inch";
		paperNames["na-9x12-envelope"] = "North American 9x12 inch";
		paperNames["na-number-10-envelope"] = "North American number 10 business envelope";
		paperNames["na-7x9-envelope"] = "North American 7x9 inch envelope";
		paperNames["na-9x11-envelope"] = "North American 9x11 inch envelope";
		paperNames["na-10x14-envelope"] = "North American 10x14 inch envelope";
		paperNames["na-number-9-envelope"] = "North American number 9 business envelope";
		paperNames["na-6x9-envelope"] = "North American 6x9 inch envelope";
		paperNames["na-10x15-envelope"] = "North American 10x15 inch envelope";
		paperNames["monarch-envelope"] = "Monarch envelope";
		paperNames["jis-b0"] = "Japanese B0";
		paperNames["jis-b1"] = "Japanese B1";
		paperNames["jis-b2"] = "Japanese B2";
		paperNames["jis-b3"] = "Japanese B3";
		paperNames["jis-b4"] = "Japanese B4";
		paperNames["jis-b5"] = "Japanese B5";
		paperNames["jis-b6"] = "Japanese B6";
		paperNames["jis-b7"] = "Japanese B7";
		paperNames["jis-b8"] = "Japanese B8";
		paperNames["jis-b9"] = "Japanese B9";
		paperNames["jis-b10"] = "Japanese B10";
		paperNames["a"] = "Engineering ANSI A";
		paperNames["b"] = "Engineering ANSI B";
		paperNames["c"] = "Engineering ANSI C";
		paperNames["d"] = "Engineering ANSI D";
		paperNames["e"] = "Engineering ANSI E";
		paperNames["arch-a"] = "Architectural A";
		paperNames["arch-b"] = "Architectural B";
		paperNames["arch-c"] = "Architectural C";
		paperNames["arch-d"] = "Architectural D";
		paperNames["arch-e"] = "Architectural E";
		paperNames["japanese-postcard"] = "Japanese Postcard";
		paperNames["oufuko-postcard"] = "Oufuko Postcard";
		paperNames["italian-envelope"] = "Italian Envelope";
		paperNames["personal-envelope"] = "Personal Envelope";
		paperNames["na-number-11-envelope"] = "North American Number 11 Envelope";
		paperNames["na-number-12-envelope"] = "North American Number 12 Envelope";
		paperNames["na-number-14-envelope"] = "North American Number 14 Envelope";

		// Loop through the map and additionally populate the names with the page size.
		// For example this would result in a display name "North American Letter - 8.5x11 in"
		// TODO SAM 2011-06-25 Need to finish this - don't have time to figure out all the lookups!

		MediaSizeName mediaSizeName;
		float pageWidth;
		float pageHeight;
		foreach (KeyValuePair<string, string> entry in paperNames.SetOfKeyValuePairs())
		{
			string key = entry.Key;
			string value = entry.Value;
			// Determine the MediaSizeName instance for the string name
			mediaSizeName = lookupMediaSizeNameFromString(key);
			MediaSize mediaSize = MediaSize.getMediaSizeForName(mediaSizeName);
			//Message.printStatus(2, routine, "paper size for \"" + paperSize + "\" is " + mediaSize );
			if (mediaSize != null)
			{
				pageWidth = mediaSize.getX(MediaSize.INCH);
				pageHeight = mediaSize.getY(MediaSize.INCH);
				// Update the value with the dimension (even if already included in the name)
				value = value + " (" + StringUtil.formatString(pageWidth,"%.2f") + "x" + StringUtil.formatString(pageHeight,"%.2f") + " in)";
				paperNames[key] = value;
			}
		}
	}

	}

}