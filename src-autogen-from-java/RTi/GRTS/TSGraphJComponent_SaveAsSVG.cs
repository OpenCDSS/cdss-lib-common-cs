﻿using System.Text;
using System.IO;

// TSGraphJComponent_SaveAsSVG - Basik SVG file writer

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

namespace RTi.GRTS
{
	using IOUtil = RTi.Util.IO.IOUtil;
	using GenericDOMImplementation = org.apache.batik.dom.GenericDOMImplementation;
	using SVGGeneratorContext = org.apache.batik.svggen.SVGGeneratorContext;
	using SVGGraphics2D = org.apache.batik.svggen.SVGGraphics2D;
	using DOMImplementation = org.w3c.dom.DOMImplementation;
	using Document = org.w3c.dom.Document;

	/// <summary>
	/// By keeping this code out of TSGraphJComponent (or other classes) and keeping
	/// this classes interface API neutral with respect to batik, we can allow
	/// batik functionality to be dynamically enabled/disabled by the presence of
	/// those classes in the default classpath.
	/// @author iws
	/// </summary>
	internal class TSGraphJComponent_SaveAsSVG
	{

		internal static Graphics createGraphics()
		{
			// Get a DOMImplementation.
			DOMImplementation domImpl = GenericDOMImplementation.getDOMImplementation();

			// Create an instance of org.w3c.dom.Document.
			string svgNS = "http://www.w3.org/2000/svg";
			Document document = domImpl.createDocument(svgNS, "svg", null);

			// Tell SVG to embed the fonts
			SVGGeneratorContext ctx = SVGGeneratorContext.createDefault(document);
			ctx.setComment("Generated by " + IOUtil.getProgramName() + " with Batik SVG Generator");
			// Seems like the fonts are pretty ugly, at least when viewed in Internet Explorer Adobe plugin
			ctx.setEmbeddedFontsOn(true);

			// Create an instance of the SVG Generator.
			return new SVGGraphics2D(ctx, false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void saveGraphics(java.awt.Graphics graphics,String path) throws java.io.IOException
		internal static void saveGraphics(Graphics graphics, string path)
		{
			// Finally, stream out SVG to the standard output using UTF-8 encoding.
			bool useCSS = true; // we want to use CSS style attributes
			FileStream outf = new FileStream(path, FileMode.Create, FileAccess.Write);
			Writer @out = new StreamWriter(outf, Encoding.UTF8);
			((SVGGraphics2D)graphics).stream(@out, useCSS);
			outf.Flush();
			outf.Close();
		}
	}

}