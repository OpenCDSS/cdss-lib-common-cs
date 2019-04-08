using System;
using System.Text;
using System.IO;

// HTMLWriter - creates files or Strings of HTML-formatted text.

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
// HTMLWriter - Creates files or Strings of HTML-formatted text.
// See href="http://www.willcam.com/cmat/html/crossref.html for a good 
// HTML tag reference.
// ----------------------------------------------------------------------------
// History:
//
// 2003-04-17	J. Thomas Sapienza, RTi	Initial version.
// 2003-04-21	JTS, RTi		* Revised, adding a lot of commenting.
//					* Removed the CENTER tag (use <P align=
//					  CENTER> instead).
//					* HTML now is written either directly
//					  to a file or to memory.
// 2003-09-26	JTS, RTi		* Renamed endHTML to closeFile().
//					* Made the closeFile() javadocs clearer.
// 2003-09-26	Anne Morgan Love, RTi	* Updated the image() method to
//					  use the correct tag and to include
//					  border, width, height, float, and
//					  alt tag options.
//					* Updated HEAD tag to take up to
//					  2 META tags.
// 2003-12-03	JTS, RTi		* Added finalize().  
//
// 2004-02-25	AML, RTi		* Updated tags to be in LowerCase
//					according to World Wide Web Consortium
//					(W3C).
//		
//					* added <q> Quote tag.
//
// 2004-03-02	AML, RTi		* Added htmlEncode(String s) to
//					* call checkText() and replace 
//					* special characters with
//					* encoded versions.
// 2004-11-18	JTS, RTi		* Added addLinkText(), which works like
//					  addText(), but doesn't append a 
//					  newline to the end of the text (which 
//					  can screw up links in IE).
//					* anchor() now uses addLinkText().
// 2007-03-15	Kurt Tometich, RTi	*Added new functions for CheckFiles
//					  that use html.  Fixed several functions that were not
//					  up to date.  Added some keyword replacements in the
//					  checkText() method.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	/// <summary>
	/// Class to format HTML content.  This class is meant to be used
	/// at the most basic level for HTML formatting.  Therefore, HTML tags are
	/// written in pieces.  It is expected that a higher-level class will be developed
	/// to provide a more friendly interface to HTML.<para>
	/// 
	/// All of the HTML tag methods are named in a specific pattern (and the list of
	/// supported tags along with their appropriate functions is 
	/// <a href="#tagMethodTable">here</a>).  Each method is a verbose name (e.g.,
	/// instead of a method called <tt>&quot;b()&quot;</tt>, there is one called 
	/// <tt>&quot;bold()&quot;</tt> of the kind of tag it is used for and may have
	/// <tt>Start</tt> or <tt>End</tt> appended to it according to the following table:
	/// 
	/// </para>
	/// <para>
	/// <b>Note:</b> in the following table, assume the HTML tag being referred to is
	/// </para>
	/// &lt;TAG&gt; and the verbose English name is tagName.<para>
	/// 
	/// <table border>
	/// <tr valign=top>
	/// <th>Method name pattern</th>
	/// <th>Description of method's actions</th>
	/// <tr>
	/// 
	/// <tr valign=top>
	/// <td>tagName(String)</td>
	/// <td>inserts &lt;TAG&gt;, followed by the String passed in to the method, followed by &lt;/TAG&gt;</td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>tagNameEnd()</td>
	/// <td>inserts &lt;/TAG&gt;</td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>tagNameStart()</td>
	/// <td>inserts &lt;TAG&gt;</td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>tagNameStart(PropList)<br>tagNameStart(String)</td>
	/// <td>inserts &lt;TAG followed by either: 
	/// <ul>
	/// <li>the list of parameters contained in the PropList (or)</li>
	/// <li>the String passed into the method</li>
	/// </ul>
	/// and finally, the closing &gt;.
	/// </td>
	/// </tr>
	/// </table>g
	/// 
	/// </para>
	/// Here is an example of how it can be used:<para>
	/// <BLOCKQUOTE><PRE>
	/// HTMLWriter w = new HTMLWriter("Title of HTML Page");
	/// 
	/// w.tableStart();
	/// 
	/// w.tableRowStart();
	/// w.tableHeader("Column 1");
	/// w.tableHeader("Column 2");
	/// w.tableRowEnd();
	/// 
	/// w.tableRowStart();
	/// w.tableCell("Row 1 Column 1");
	/// w.tableCell("Row 2 Column 2");
	/// w.tableRowEnd();
	/// 
	/// w.tableEnd();
	/// 
	/// w.numberedListStart();
	/// w.listItem("First item");
	/// w.listItem("Second item");
	/// w.numberedListEnd();
	/// 
	/// w.closeFile();
	/// </BLOCKQUOTE>
	/// </PRE>
	/// 
	/// Because in some cases the name of the method to use for inserting a specific
	/// tag may not be immediately obvious, here is a list of the class methods for
	/// inserting HTML tags, sorted by the HTML tag.
	/// <a name="tagMethodTable">
	/// <table border>
	/// <tr>
	/// <TH>HTML Tag</TH>    <TH>Tag description</TH>   <TH>Method name</TH>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;!-- ... --&gt;</td>
	/// <td>Comments out everything between the tags</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#comment(java.lang.String)">
	/// comment(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#commentEnd()">commentEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#commentStart()">commentStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;a href&gt;</td>
	/// <td>Adds a hyperlink to another document</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#link(java.lang.String, java.lang.String)">
	/// link(String, String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#linkEnd()">linkEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#linkStart(java.lang.String)">
	/// linkStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;A NAME&gt;</td>
	/// <td>Named anchor</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#anchor(java.lang.String)">
	/// anchor(String)</a></td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;B&gt;</td>
	/// <td>Sets text to bold</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bold(java.lang.String)">
	/// bold(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#boldEnd()">boldEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#boldStart()">boldStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;BLOCKQUOTE&gt;</td>
	/// <td>Quotes a block of text and sets it in one tabstop</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#blockquote(java.lang.String)">
	/// blockquote(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#blockquoteEnd()">blockquoteEnd()
	/// </a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#blockquoteStart()">
	/// blockquoteStart()</a><br>
	/// <A 
	/// HREF=
	/// "../../../RTi/Util/IO/HTMLWriter.html#blockquoteStart(RTi.Util.IO.PropList)">
	/// blockquoteStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#blockquoteStart(java.lang.String)"
	/// >blockquoteStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;BODY&gt;</td>
	/// <td>Marks the body of the HTML document</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bodyEnd()">bodyEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bodyStart()">bodyStart()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bodyStart(RTi.Util.IO.PropList)">
	/// bodyStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bodyStart(java.lang.String)">
	/// bodyStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;BR&gt;</td>
	/// <td>Breaks a line</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#breakLine()">breakLine()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;CAPTION&gt;</td>
	/// <td>Captions text as a table caption</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#caption(java.lang.String)">
	/// caption(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#captionEnd()">captionEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#captionStart()">captionStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;DD&gt;</td>
	/// <td>Sets a definition in a definition list</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definition(java.lang.String)">
	/// definition(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definition(java.lang.String, java.lang.String)">
	/// definition(String, String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionEnd()">
	/// definitionEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionStart()">
	/// definitionStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;DL&gt;</td>
	/// <td>Denotes a definition list</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionListEnd()">
	/// definitionListEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionListStart()">
	/// definitionListStart()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionListStart(RTi.Util.IO.PropList)">
	/// definitionListStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionListStart(java.lang.String)">
	/// definitionListStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;DT&gt;</td>
	/// <td>Sets a definition term in a definition list</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionTerm(java.lang.String)">
	/// definitionTerm(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionTermEnd()">
	/// definitionTermEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#definitionTermStart()">
	/// definitionTermStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;FONT&gt;</td>
	/// <td>Sets text to a certain font</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#fontEnd()">fontEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#fontStart(RTi.Util.IO.PropList)">
	/// fontStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#fontStart(java.lang.String)">
	/// fontStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;H&gt;</td>
	/// <td>Sets a heading</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#heading(int, java.lang.String)">
	/// heading(int, String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#headingEnd(int)">
	/// headingEnd(int)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#headingStart(int)">
	/// headingStart(int)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#headingStart(int, RTi.Util.IO.PropList)">
	/// headingStart(int, PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#headingStart(int, java.lang.String)">
	/// headingStart(int, String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;HR&gt;</td>
	/// <td>Creates a horizontal line across the document</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#horizontalRule()">
	/// horizontalRule()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;HTML&gt;</td>
	/// <td>Starts the HTML document</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#htmlEnd()">htmlEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#htmlStart()">htmlStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;I&gt;</td>
	/// <td>Italicizes text</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#italic(java.lang.String)">
	/// italic(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#italicEnd()">
	/// italicEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#italicStart()">
	/// italicStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;IMG&gt;</td>
	/// <td>Inserts an image</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#image(RTi.Util.IO.PropList)">
	/// image(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#image(java.lang.String)">
	/// image(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;LI&gt;</td>
	/// <td>Inserts an item into a list</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#listItem(java.lang.String)">
	/// listItem(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#listItemEnd()">
	/// listItemEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#listItemStart()">
	/// listItemStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;NOBR&gt;</td>
	/// <td>Marks text that is not to have line breaks</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#nobr(java.lang.String)">
	/// nobr(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#nobrEnd()">nobrEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#nobrStart()">nobrStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;OL&gt;</td>
	/// <td>Defines a numbered list</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#numberedListEnd()">
	/// numberedListEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#numberedListStart()">
	/// numberedListStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;P&gt;</td>
	/// <td>Defines a paragraph</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#paragraph()">paragraph()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#paragraph(java.lang.String)">
	/// paragraph(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#paragraphEnd()">
	/// paragraphEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#paragraphStart()">
	/// paragraphStart()</a><br>
	/// <A 
	/// HREF="../../../RTi/Util/IO/HTMLWriter.html#paragraphStart(RTi.Util.IO.PropList)"
	/// >
	/// paragraphStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#paragraphStart(java.lang.String)">
	/// paragraphStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;PRE&gt;</td>
	/// <td>Defines a block of pre-formatted text</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#pre(java.lang.String)">
	/// pre(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#preEnd()">preEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#preStart()">preStart()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#preStart(RTi.Util.IO.PropList)">
	/// preStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#preStart(java.lang.String)">
	/// preStart(java.lang.String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;SUB&gt;</td>
	/// <td>Sets text to be subscripted</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#subscript(java.lang.String)">
	/// subscript(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#subscriptEnd()">
	/// subscriptEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#subscriptStart()">
	/// subscriptStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;SUP&gt;</td>
	/// <td>Sets text to be superscripted</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#superscript(java.lang.String)">
	/// superscript(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#superscriptEnd()">
	/// superscriptEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#superscriptStart()">
	/// superscriptStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;TABLE&gt;</td>
	/// <td>Creates a table</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableEnd()">tableEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableStart()">tableStart()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableStart(RTi.Util.IO.PropList)">
	/// tableStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableStart(java.lang.String)">
	/// tableStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;td&gt;</td>
	/// <td>Creates a table cell</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableCell(java.lang.String)">
	/// tableCell(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableCellEnd()">
	/// tableCellEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableCellStart()">
	/// tableCellStart()</a><br>
	/// <A 
	/// HREF="../../../RTi/Util/IO/HTMLWriter.html#tableCellStart(RTi.Util.IO.PropList)"
	/// >
	/// tableCellStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableCellStart(java.lang.String)">
	/// tableCellStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;TH&gt;</td>
	/// <td>Creates a table header cell</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableHeader(java.lang.String)">
	/// tableHeader(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableHeaderEnd()">
	/// tableHeaderEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableHeaderStart()">
	/// tableHeaderStart()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableHeaderStart(RTi.Util.IO.PropList)">
	/// tableHeaderStart(PropList)</a><br>
	/// <A 
	/// HREF="../../../RTi/Util/IO/HTMLWriter.html#tableHeaderStart(java.lang.String)">
	/// tableHeaderStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;tr&gt;</td>
	/// <td>Creates a table row</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableRowEnd()">
	/// tableRowEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableRowStart()">tableRowStart()
	/// </a><br>
	/// <A 
	/// HREF="../../../RTi/Util/IO/HTMLWriter.html#tableRowStart(RTi.Util.IO.PropList)">
	/// tableRowStart(PropList)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#tableRowStart(java.lang.String)">
	/// tableRowStart(String)</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;TT&gt;</td>
	/// <td>Sets text to teletype monospaced</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#teletype(java.lang.String)">
	/// teletype(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#teletypeEnd()">teletypeEnd()</a>
	/// <br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#teletypeStart()">teletypeStart()
	/// </a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;U&gt;</td>
	/// <td>Sets text to be underlined</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#underline(java.lang.String)">
	/// underline(String)</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#underlineEnd()">
	/// underlineEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#underlineStart()">
	/// underlineStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;UL&gt;</td>
	/// <td>Creates a bulleted list</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bulletedListEnd()">
	/// bulletedListEnd()</a><br>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#bulletedListStart()">
	/// bulletedListStart()</a>
	/// </td>
	/// </tr>
	/// 
	/// <tr valign=top>
	/// <td>&lt;WBR&gt;</td>
	/// <td>Inserts a position at which the HTML rendered should try to break a word
	/// if it needs to</td>
	/// <td>
	/// <a href="../../../RTi/Util/IO/HTMLWriter.html#wordBreak()">wordBreak()</a>
	/// </td>
	/// </tr>
	/// </table>g
	/// 
	/// </para>
	/// <para>
	/// To specify tag properties, use one of the two tagNameStart() methods which take
	/// a parameter (either a PropList or a String).  The method that takes the 
	/// PropList will gather every property from the list and create a long string
	/// of key=value statements and pass it to the other tagNameStart method that
	/// takes a String parameter.
	/// </para>
	/// <para>
	/// For instance, a PropList with two properties (Prop1: "align" = "left", Prop2:
	/// "nowrap" = "") will be converted to this string:<br>
	/// "align=left nowrap".
	/// 
	/// </para>
	/// </summary>
	public class HTMLWriter
	{

	/// <summary>
	/// Specifies whether to check text for metacharacters before writing it.
	/// If set to true, the text that is visible on the HTML page is checked for 
	/// metacharacters (e.g., "&gt;" or "&lt;") that may not appear properly and
	/// the valid escape sequence is entered instead.  This can be expensive, because
	/// every character is checked prior to being placed in the html text.  Text 
	/// inside of a PRE or TT tag will not be checked for such things as multiple  spaces.
	/// </summary>
	private bool __checkText = true;

	/// <summary>
	/// Specifies whether the HTML has been closed and written yet or not.
	/// </summary>
	private bool __closed = false;

	/// <summary>
	/// Specifies whether the HTML has HTML and BODY tags.
	/// If true, then this html has a header (an HTML and a BODY tag) that will need
	/// to be closed before the HTML is written out to a file.  This is used to 
	/// differentiate between HTML that is being written to a file (i.e., it has a
	/// full set of HTML, TITLE, and BODY tags) and HTML that is being created for
	/// use in pasting to the clipboard (i.e., it is incomplete HTML).
	/// </summary>
	private bool __hasHeader = false;

	/// <summary>
	/// Whether this is an HTMLWriter using another writer's settings.
	/// </summary>
	private bool __subWriter = false;

	/// <summary>
	/// Specifies whether to write the HTML to memory or a file.
	/// If true, then the HTML will be written to the __html StringBuffer in memory.
	/// If false the HTML will be written out to a a file.
	/// </summary>
	private bool __writeToFile = false;

	/// <summary>
	/// The BufferedWriter that will be used if the HTML is written out to a file.
	/// </summary>
	private StreamWriter __htmlFile = null;

	/// <summary>
	/// The number of &lt;H&gt; tags that are currently unclosed.
	/// </summary>
	private int[] __hL;

	/// <summary>
	/// The number of &lt;A&gt; (anchor) tags that are currently unclosed.
	/// </summary>
	private int __aL = 0;

	/// <summary>
	/// The number of &lt;B&gt; (bold) tags that are currently unclosed.
	/// </summary>
	private int __bL = 0;

	/// <summary>
	/// The number of &lt;BLOCKQUOTE&gt; tags that are currently unclosed.
	/// </summary>
	private int __blockquoteL = 0;

	/// <summary>
	/// The number of &lt;BODY&gt; tags that are currently unclosed.
	/// </summary>
	private int __bodyL = 0;

	/// <summary>
	/// The number of &lt;CAPTION&gt; tags that are currently unclosed.
	/// </summary>
	private int __captionL = 0;

	/// <summary>
	/// The number of &lt;COMMENT&gt; tags that are currently unclosed.
	/// </summary>
	private int __commentL = 0;

	/// <summary>
	/// The number of &lt;DD&gt; tags that are currently unclosed.
	/// </summary>
	private int __ddL = 0;

	/// <summary>
	/// The number of &lt;DL&gt; tags that are currently unclosed.
	/// </summary>
	private int __dlL = 0;

	/// <summary>
	/// The number of &lt;DT&gt; tags that are currently unclosed.
	/// </summary>
	private int __dtL = 0;

	/// <summary>
	/// The number of &lt;FONT&gt; tags that are currently unclosed.
	/// </summary>
	private int __fontL = 0;

	/// <summary>
	/// The number of &lt;HTML&gt; tags that are currently unclosed.
	/// </summary>
	private int __htmlL = 0;

	/// <summary>
	/// The number of &lt;I&gt; tags that are currently unclosed.
	/// </summary>
	private int __iL = 0;

	/// <summary>
	/// The number of &lt;LI&gt; tags that are currently unclosed.
	/// </summary>
	private int __liL = 0;

	/// <summary>
	/// The number of &lt;NOBR&gt; tags that are currently unclosed.
	/// </summary>
	private int __nobrL = 0;

	/// <summary>
	/// The number of &lt;OL&gt; tags that are currently unclosed.
	/// </summary>
	private int __olL = 0;

	/// <summary>
	/// The number of &lt;P&gt; tags that are currently unclosed.
	/// </summary>
	private int __pL = 0;

	/// <summary>
	/// The number of &lt;PRE&gt; tags that are currently unclosed.
	/// </summary>
	private int __preL = 0;

	/// <summary>
	/// The number of &lt;SUB&gt; tags that are currently unclosed.
	/// </summary>
	private int __subL = 0;

	/// <summary>
	/// The number of &lt;SUP&gt; tags that are currently unclosed.
	/// </summary>
	private int __supL = 0;

	/// <summary>
	/// The number of &lt;TABLE&gt; tags that are currently unclosed.
	/// </summary>
	private int __tableL = 0;

	/// <summary>
	/// The number of &lt;TH&gt; tags that are currently unclosed.
	/// </summary>
	private int __thL = 0;

	/// <summary>
	/// The number of &lt;tr&gt; tags that are currently unclosed.
	/// </summary>
	private int __trL = 0;

	/// <summary>
	/// The number of &lt;td&gt; tags that are currently unclosed.
	/// </summary>
	private int __tdL = 0;

	/// <summary>
	/// The number of &lt;TT&gt; tags that are currently unclosed.
	/// </summary>
	private int __ttL = 0;

	/// <summary>
	/// The number of &lt;U&gt; tags that are currently unclosed.
	/// </summary>
	private int __uL = 0;

	/// <summary>
	/// The number of &lt;UL&gt; tags that are currently unclosed.
	/// </summary>
	private int __ulL = 0;

	/// <summary>
	/// The StringBuffer of HTML that is being generated.
	/// </summary>
	private StringBuilder __htmlBuffer = null;

	/// <summary>
	/// Constructor.  Calls HTMLWriter(null, null, false) and sets up HTML that will
	/// not be written to a file but will instead be written to a StringBuffer in memory.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLWriter() throws Exception
	public HTMLWriter() : this(null, null, false)
	{
	}

	/// <summary>
	/// Constructor.  Calls HTMLWriter(null, title, true) and sets up HTML that will
	/// not be written out to a file but which WILL be formatted as a complete HTML
	/// file (i.e., it has HTML, BODY and TITLE tags and is not just a snippet). </summary>
	/// <param name="title"> the title of the HTML page. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLWriter(String title) throws Exception
	public HTMLWriter(string title) : this(null, title, true)
	{
	}

	/// <summary>
	/// Constructor.  Calls HTMLWriter(filename, title, true) and sets up a file to have HTML written to it. </summary>
	/// <param name="filename"> the name of the file to write (or if it exists, overwrite) </param>
	/// <param name="title"> the title of the html page </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLWriter(String filename, String title) throws Exception
	public HTMLWriter(string filename, string title) : this(filename, title, true)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="filename"> the filename to which to write HTML.  Can be null, in which 
	/// case the HTML will not be written to a file but will be stored in memory. </param>
	/// <param name="title"> the title to assign to the HTML.  Can be null. </param>
	/// <param name="createHead"> whether to create a head (an HTML and BODY tag) for this
	/// HTML.  This should be true if creating an HTML page and false if only 
	/// generating a snippet of HTML (e.g., for placing in the Clipboard). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLWriter(String filename, String title, boolean createHead) throws Exception
	public HTMLWriter(string filename, string title, bool createHead)
	{
		if (string.ReferenceEquals(filename, null))
		{
			__writeToFile = false;
			__htmlBuffer = new StringBuilder();
			__htmlFile = null;
		}
		else
		{
			__writeToFile = true;
			__htmlBuffer = null;
			__htmlFile = new StreamWriter(new StreamWriter(filename, false));
		}

		__hasHeader = createHead;
		if (__hasHeader)
		{
			htmlStart();
			head(title);
			bodyStart();
		}

		// The following are easier to initialize here, rather than above
		// where they are declared
		__hL = new int[7];
		__hL[1] = 0;
		__hL[2] = 0;
		__hL[3] = 0;
		__hL[4] = 0;
		__hL[5] = 0;
		__hL[6] = 0;
	}

	/// <summary>
	/// Constructor for an HTMLWriter that will write into HTML already produced by another HTMLWriter. </summary>
	/// <param name="htmlWriter"> an existing HTMLWriter into which this HTML will be written. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HTMLWriter(HTMLWriter htmlWriter) throws Exception
	public HTMLWriter(HTMLWriter htmlWriter)
	{
		__writeToFile = htmlWriter.getWriteToFile();
		__htmlBuffer = htmlWriter.getStringBuffer();
		__htmlFile = htmlWriter.getBufferedWriter();
		__hasHeader = htmlWriter.getHasHeader();
		__checkText = htmlWriter.checkTextForMetaCharacters();

		// The following are easier to initialize here, rather than above where they are declared
		__hL = new int[7];
		__hL[1] = 0;
		__hL[2] = 0;
		__hL[3] = 0;
		__hL[4] = 0;
		__hL[5] = 0;
		__hL[6] = 0;

		__subWriter = true;
	}

	/// <summary>
	/// Cleans up member variables.  If the HTML has not been closed yet (i.e., not
	/// written to disk), this will attempt to do so.  This may be unreliable, so do not rely on it.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~HTMLWriter()
	{
		if (!__subWriter)
		{
			if (!__closed)
			{
				closeFile();
				__closed = true;
			}

			__htmlFile = null;
			__hL = null;
			__htmlBuffer = null;
		}

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Writes the given text to the HTML without appending a newline.  addText()
	/// appends a newline after the text is added -- this can mess up links in IE. </summary>
	/// <param name="s"> the text to write to the HTML. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addLinkText(String s) throws Exception
	public virtual void addLinkText(string s)
	{
		if (__checkText)
		{
			write(textToHtml(s));
		}
		else
		{
			write(s);
		}
	}

	/// <summary>
	/// Writes the given text to the HTML and adds a newline. </summary>
	/// <param name="s"> the text to write to the HTML. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addText(String s) throws Exception
	public virtual void addText(string s)
	{
		if (__checkText)
		{
			write(textToHtml(s) + "\n");
		}
		else
		{
			write(s + "\n");
		}
	}

	/// <summary>
	/// Creates an anchor tag with the given anchor name. </summary>
	/// <param name="s"> the anchor name for the tag. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Anchor">&lt;A&gt; tag</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void anchor(String s) throws Exception
	public virtual void anchor(string s)
	{
		write("<a name=\"");
		addLinkText(s);
		write("\">");
	}

	/// <summary>
	/// Inserts the end of the anchor. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void anchorEnd() throws Exception
	public virtual void anchorEnd()
	{
		__aL--;
		write("</a>");
	}

	/// <summary>
	/// Inserts the start of the anchor with the given name. </summary>
	/// <param name="s"> the anchor name. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void anchorStart(String s) throws Exception
	public virtual void anchorStart(string s)
	{
		__aL++;
		write("<a name=\"");
		addLinkText(s);
		write("\">");
	}

	/// <summary>
	/// Creates a BLOCKQUOTE tag around the given String. </summary>
	/// <param name="s"> the String to enclose in a BLOCKQUOTE tag. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Block%20Quote">
	/// &lt;BLOCKQUOTE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void blockquote(String s) throws Exception
	public virtual void blockquote(string s)
	{
		write("<blockquote>");
		addText(s);
		write("</blockquote>\n");
	}

	/// <summary>
	/// Closes a BLOCKQUOTE tag. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Block%20Quote">
	/// &lt;BLOCKQUOTE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void blockquoteEnd() throws Exception
	public virtual void blockquoteEnd()
	{
		if (__blockquoteL == 0)
		{
			return;
		}
		__blockquoteL--;
		write("</blockquote>\n");
	}

	/// <summary>
	/// Opens a BLOCKQUOTE tag. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Block%20Quote">
	/// &lt;BLOCKQUOTE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void blockquoteStart() throws Exception
	public virtual void blockquoteStart()
	{
		blockquoteStart("");
	}

	/// <summary>
	/// Opens a BLOCKQUOTE tag with the given proplist values supplying the 
	/// BLOCKQUOTE parameters. </summary>
	/// <param name="p"> PropList of the BLOCKQUOTE parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Block%20Quote">
	/// &lt;BLOCKQUOTE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void blockquoteStart(PropList p) throws Exception
	public virtual void blockquoteStart(PropList p)
	{
		blockquoteStart(propListToString(p));
	}

	/// <summary>
	/// Opens a BLOCKQUOTE tag with the string supplying the BLOCKQUOTE parameters. </summary>
	/// <param name="s"> the BLOCKQUOTE parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Block%20Quote">
	/// &lt;BLOCKQUOTE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void blockquoteStart(String s) throws Exception
	public virtual void blockquoteStart(string s)
	{
		if (s.Trim().Equals(""))
		{
			write("<blockquote>");
		}
		else
		{
			write("<blockquote " + s + ">");
		}
		__blockquoteL++;
	}

	/// <summary>
	/// Ends a BODY declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#Body">
	/// &lt;BODY&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bodyEnd() throws Exception
	public virtual void bodyEnd()
	{
		if (__bodyL == 0)
		{
			return;
		}
		__bodyL--;
		write("\n</body>\n");
	}

	/// <summary>
	/// Starts a body declaration and assigns the values in the proplist to the BODY. </summary>
	/// <param name="p"> the values to use as the BODY parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#Body">
	/// &lt;BODY&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bodyStart(PropList p) throws Exception
	public virtual void bodyStart(PropList p)
	{
		bodyStart(propListToString(p));
	}

	/// <summary>
	/// Starts a BODY declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#Body">
	/// &lt;BODY&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bodyStart() throws Exception
	public virtual void bodyStart()
	{
		bodyStart("");
	}

	/// <summary>
	/// Starts a BODY declaration and appends the String to the BODY tag. </summary>
	/// <param name="s"> the BODY parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#Body">
	/// &lt;BODY&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bodyStart(String s) throws Exception
	public virtual void bodyStart(string s)
	{
		__bodyL++;
		write("<body");
		if (s.Trim().Equals(""))
		{
			write(">\n");
		}
		else
		{
			write(s + ">\n");
		}
	}

	/// <summary>
	/// Puts the given text in inside a set of B bold tags. </summary>
	/// <param name="s"> the text to bold. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Bold">
	/// &lt;B&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bold(String s) throws Exception
	public virtual void bold(string s)
	{
		write("<b>");
		addText(s);
		write("</b>");
	}

	/// <summary>
	/// Ends a bold text section </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Bold">
	/// &lt;B&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void boldEnd() throws Exception
	public virtual void boldEnd()
	{
		if (__bL == 0)
		{
			return;
		}
		write("</b>");
		__bL--;
	}

	/// <summary>
	/// Starts a bold text section. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Bold">
	/// &lt;B&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void boldStart() throws Exception
	public virtual void boldStart()
	{
		write("<b>");
		__bL++;
	}

	/// <summary>
	/// Inserts a line break. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Line%20Break">
	/// &lt;BR&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void breakLine() throws Exception
	public virtual void breakLine()
	{
		write("<br>\n");
	}

	/// <summary>
	/// Ends an bulleted list declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Unordered%20List">
	/// &lt;UL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bulletedListEnd() throws Exception
	public virtual void bulletedListEnd()
	{
		if (__ulL == 0)
		{
			return;
		}
		write("\n</ul>\n");
		__ulL--;
	}

	/// <summary>
	/// Starts an bulleted list declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Unordered%20List">
	/// &lt;UL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void bulletedListStart() throws Exception
	public virtual void bulletedListStart()
	{
		__ulL++;
		write("\n<ul>\n");
	}

	/// <summary>
	/// Adds the given string as a table caption. </summary>
	/// <param name="s"> the String to caption. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Caption">
	/// &lt;CAPTION&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void caption(String s) throws Exception
	public virtual void caption(string s)
	{
		write("<caption>");
		addText(s);
		write("</caption>\n");
	}

	/// <summary>
	/// Ends a table caption. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Caption">
	/// &lt;CAPTION&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void captionEnd() throws Exception
	public virtual void captionEnd()
	{
		if (__captionL == 0)
		{
			return;
		}
		write("</caption>\n");
	}

	/// <summary>
	/// Starts a table caption. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Caption">
	/// &lt;CAPTION&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void captionStart() throws Exception
	public virtual void captionStart()
	{
		__captionL++;
		write("<caption>");
	}

	/// <summary>
	/// Returns whether to check text for metacharacters. </summary>
	/// <returns> whether to check text for metacharacters. </returns>
	public virtual bool checkTextForMetaCharacters()
	{
		return __checkText;
	}

	/// <summary>
	/// Converts normal text to HTML, where special characters such as &lt;, &gt;, &amp;, and &quot; are
	/// replaced with html escape codes for these characters. </summary>
	/// <param name="s"> a String of text to search for special characters. </param>
	/// <returns> the String that was passed in to the method with HTML escape codes for the special characters. </returns>
	private string textToHtml(string s)
	{
		int length = s.Length;
		char ch;
		string rep = "";
		string front = "";
		string back = "";
		bool replace = false;
		bool spaceAfterSpace = false;

		for (int i = 0; i < length; i++)
		{
			ch = s[i];

			if (ch == '>')
			{
				rep = "&gt;";
				replace = true;
			}
			else if (ch == '<')
			{
				 rep = "&lt;";
				replace = true;
			}
			else if (ch == ' ')
			{
				if (spaceAfterSpace)
				{
					rep = "&nbsp;";
					replace = true;
				}
				else
				{
					spaceAfterSpace = true;
					replace = false;
				}
			}
			else if (ch == '&')
			{
				rep = "&amp;";
				replace = true;
			}
			else if (ch == '\'')
			{
				rep = "&rsquo;";
				replace = true;
			}
			//else if (ch == '-') {
			//	rep = "&ndash;";
			//	replace = true;
			//}
			else if (ch == '"')
			{
				rep = "&quot;";
				replace = true;
			}
			else
			{
				spaceAfterSpace = false;
				replace = false;
			}

			if (replace)
			{
				front = s.Substring(0, i);
				if (i < (length - 1))
				{
					back = s.Substring((i + 1), length - (i + 1));
				}
				else
				{
					back = "";
				}

				s = front + rep + back;

				length = s.Length;
				i += rep.Length - 1;
			}
		}

		// FIXME SAM 2009-04-21 Evaluate the following - should not do anything special here for check file HTML
		// Do other checks related to check files
		// shouldn't affect anything unless they have a special
		// sequence of characters
		string tmp = s.replaceAll("%font_red", "<b><font color=red>");
		s = tmp.replaceAll("%font_end", "</font></b>");

		// Replace tooltips with HTML title strings
		if (s.IndexOf("%tooltip", StringComparison.Ordinal) >= 0)
		{
			tmp = s.replaceAll("%tooltip_start", "<p title=\"");
			s = tmp.replaceAll("%tooltip_end", "\">");
			s = s + "</p>";
		}

		return s;
	}

	/// <summary>
	/// Sets whether text should be checked for metacharacters prior to being written to the HTML. </summary>
	/// <param name="check"> if true, text such as &gt; will be translated to "\&amp;gt;". </param>
	public virtual void checkTextForMetacharacters(bool check)
	{
		__checkText = check;
	}

	/// <summary>
	/// Closes all tags that are currently open.  It does this in a non-intelligent
	/// fashion -- tags are tracked as they are opened and closed and then in a rough
	/// ordering scheme closing tags are added to the HTML.  Don't rely on this; it 
	/// will probably be changed to an error-checking routine in the future. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closeAllOpenTags() throws Exception
	public virtual void closeAllOpenTags()
	{
		for (; __commentL > 0; __commentL--)
		{
			write("-->");
		}

		for (; __fontL > 0; __fontL--)
		{
			write("</font>");
		}
		for (; __tdL > 0; __tdL--)
		{
			write("</td>");
		}
		for (; __thL > 0; __thL--)
		{
			write("</th>");
		}
		for (; __trL > 0; __trL--)
		{
			write("</tr>");
		}
		for (; __tableL > 0; __tableL--)
		{
			write("</table>");
		}
		for (; __captionL > 0; __captionL--)
		{
			write("</caption>");
		}
		for (int i = 1; i < 7; i++)
		{
			for (; __hL[i] > 0; __hL[i]--)
			{
				write("</h" + i + ">");
			}
		}
		for (; __liL > 0; __liL--)
		{
			write("</li>");
		}
		for (; __olL > 0; __olL--)
		{
			write("</ol>");
		}
		for (; __ulL > 0; __ulL--)
		{
			write("</ul>");
		}
		for (; __dtL > 0; __dtL--)
		{
			write("</dt>");
		}
		for (; __ddL > 0; __ddL--)
		{
			write("</dd>");
		}
		for (; __dlL > 0; __dlL--)
		{
			write("</dl>");
		}
		for (; __aL > 0; __aL--)
		{
			write("</a>");
		}
		for (; __bL > 0; __bL--)
		{
			write("</b>");
		}
		for (; __iL > 0; __iL--)
		{
			write("</i>");
		}
		for (; __uL > 0; __uL--)
		{
			write("</u>");
		}
		for (; __blockquoteL > 0; __blockquoteL--)
		{
			write("</blockquote>");
		}
		for (; __pL > 0; __pL--)
		{
			write("</p>");
		}
		for (; __preL > 0; __preL--)
		{
			write("</pre>");
		}
		for (; __ttL > 0; __ttL--)
		{
			write("</tt>");
		}
		for (; __subL > 0; __subL--)
		{
			write("</sub>");
		}
		for (; __supL > 0; __supL--)
		{
			write("</sup>");
		}
		for (; __nobrL > 0; __nobrL--)
		{
			write("</nobr>");
		}
		write("\n");
	}

	/// <summary>
	/// Closes an HTML file and writes it to disk.  If the HTMLWriter was constructed 
	/// with a header (i.e., BODY and HTML tags), this method will add the matching
	/// &lt;/BODY&gt; and &lt;/HTML&gt; tags at the bottom of the HTML document.  
	/// Otherwise, no &lt;/BODY&gt; and &lt;/HTML&gt; tags will be written to the
	/// end of the document.  The document will then be flushed to disk and written. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closeFile() throws Exception
	public virtual void closeFile()
	{
		if (__subWriter)
		{
			return;
		}
		if (__hasHeader)
		{
			bodyEnd();
			htmlEnd();
		}
		if (__writeToFile)
		{
			__htmlFile.Close();
		}
		__closed = true;
	}

	/// <summary>
	/// Adds the given string in "code" to the HTML. </summary>
	/// <param name="s"> the String to output in code format. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void code(String s) throws Exception
	public virtual void code(string s)
	{
		write("<code>");
		addText(s);
		write("</code>");
	}

	/// <summary>
	/// Puts the given text inside a comment. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Comment">
	/// &lt;COMMENT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void comment(String s) throws Exception
	public virtual void comment(string s)
	{
		write("<!-- " + s + " -->\n");
	}

	/// <summary>
	/// Stops commenting. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Comment">
	/// &lt;COMMENT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void commentEnd() throws Exception
	public virtual void commentEnd()
	{
		if (__commentL == 0)
		{
			return;
		}
		__commentL--;
		write("-->\n");
	}

	/// <summary>
	/// Starts a comment. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Comment">
	/// &lt;COMMENT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void commentStart() throws Exception
	public virtual void commentStart()
	{
		__commentL++;
		write("<!--\n");
	}

	/// <summary>
	/// Adds the given definition to a definition list. </summary>
	/// <param name="s"> the string to use as the definition. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DD&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definition(String s) throws Exception
	public virtual void definition(string s)
	{
		if (__dlL == 0)
		{
			return;
		}
		write("<dd>");
		addText(s);
		write("</dd>\n");
	}

	/// <summary>
	/// Adds the given definition term and definition to a definition list. </summary>
	/// <param name="term"> the term to define. </param>
	/// <param name="def"> the definition of the term. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definition(String term, String def) throws Exception
	public virtual void definition(string term, string def)
	{
		if (__dlL == 0)
		{
			return;
		}
		write("<dt>" + term + "</dt>\n");
		write("<dd>" + def + "</dd>\n");
	}

	/// <summary>
	/// Ends a definition. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DD&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionEnd() throws Exception
	public virtual void definitionEnd()
	{
		if (__ddL == 0)
		{
			return;
		}
		__ddL--;
		write("</dd>\n");
	}

	/// <summary>
	/// Starts a definition. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DD&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionStart() throws Exception
	public virtual void definitionStart()
	{
		if (__dlL == 0)
		{
			return;
		}
		__ddL++;
		write("<dd>");
	}

	/// <summary>
	/// Ends a definition list. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionListEnd() throws Exception
	public virtual void definitionListEnd()
	{
		if (__dlL == 0)
		{
			return;
		}
		__dlL--;
		write("</dl>\n");
	}

	/// <summary>
	/// Starts a definition list. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionListStart() throws Exception
	public virtual void definitionListStart()
	{
		definitionListStart("");
	}

	/// <summary>
	/// Starts a definition list and uses the values in the proplist as the parameters. </summary>
	/// <param name="p"> PropList containing the definition list parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionListStart(PropList p) throws Exception
	public virtual void definitionListStart(PropList p)
	{
		definitionListStart(propListToString(p));
	}

	/// <summary>
	/// Starts a definition list and uses the string as the string of parameters. </summary>
	/// <param name="s"> the parameter list. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionListStart(String s) throws Exception
	public virtual void definitionListStart(string s)
	{
		__dlL++;
		if (s.Trim().Equals(""))
		{
			write("<dl>\n");
			return;
		}
		write("<dl " + s + ">\n");
	}

	/// <summary>
	/// Puts the given string in a definition list as a term. </summary>
	/// <param name="s"> the definition term </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionTerm(String s) throws Exception
	public virtual void definitionTerm(string s)
	{
		if (__dlL == 0)
		{
			return;
		}
		write("<dt>" + s + "</dt>\n");
	}

	/// <summary>
	/// Ends a definition term. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionTermEnd() throws Exception
	public virtual void definitionTermEnd()
	{
		if (__dtL == 0)
		{
			return;
		}
		__dtL--;
		write("</dt>\n");
	}

	/// <summary>
	/// Starts a definition term. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Definition%20List">
	/// &lt;DT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void definitionTermStart() throws Exception
	public virtual void definitionTermStart()
	{
		if (__dlL == 0)
		{
			return;
		}
		__dtL++;
		write("<dt>");
	}

	/// @deprecated use closeFile() instead 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void endHTML() throws Exception
	public virtual void endHTML()
	{
		closeFile();
	}

	/// <summary>
	/// Starts a new font with the parameters in the proplist. </summary>
	/// <param name="p"> PropList containing the Font's parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Font">
	/// &lt;FONT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void fontStart(PropList p) throws Exception
	public virtual void fontStart(PropList p)
	{
		fontStart(propListToString(p));
	}

	/// <summary>
	/// Ends a font declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Font">
	/// &lt;FONT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void fontEnd() throws Exception
	public virtual void fontEnd()
	{
		if (__fontL == 0)
		{
			return;
		}
		__fontL--;
		write("</font>");
	}

	/// <summary>
	/// Starts a font with the parameters in the given String. </summary>
	/// <param name="s"> String containing the font parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Font">
	/// &lt;FONT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void fontStart(String s) throws Exception
	public virtual void fontStart(string s)
	{
		__fontL++;
		write("<font " + s + ">");
	}

	/// <summary>
	/// Returns the BufferedWriter used to write HTML. </summary>
	/// <returns> the BufferedWriter used to write HTML. </returns>
	protected internal virtual StreamWriter getBufferedWriter()
	{
		return __htmlFile;
	}

	/// <summary>
	/// Returns whether there is a header. </summary>
	/// <returns> whether there is a header. </returns>
	protected internal virtual bool getHasHeader()
	{
		return __hasHeader;
	}

	/// <summary>
	/// Returns the HTML that was written to memory, or null if no HTML was/has been written to memory. </summary>
	/// <returns> a String of HTML. </returns>
	public virtual string getHTML()
	{
		if (__htmlBuffer == null)
		{
			return null;
		}
		else
		{
			return __htmlBuffer.ToString();
		}
	}

	/// <summary>
	/// Returns the StringBuffer used to write HTML. </summary>
	/// <returns> the StringBuffer used to write HTML. </returns>
	protected internal virtual StringBuilder getStringBuffer()
	{
		return __htmlBuffer;
	}

	/// <summary>
	/// Returns whether HTML is being written to a file or not. </summary>
	/// <returns> whether HTML is being written to a file or not. </returns>
	protected internal virtual bool getWriteToFile()
	{
		return __writeToFile;
	}

	/// <summary>
	/// Creates a HEAD tag and uses the given String as the title of the page. </summary>
	/// <param name="s"> the title to set the page to.  Can be null. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#Head">
	/// &lt;HEAD&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void head(String s) throws Exception
	public virtual void head(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			s = "";
		}
		write("<head>\n");
		title(s);
		headEnd();
	}

	/// <summary>
	/// Creates a HEAD tag and uses the given title String as the title of the page
	/// and the arrays (of 2 Strings) passed in for adding 1 META data tags. 
	/// The first item in each array is used to assign the data for NAME part of 
	/// the META tag and the second item in each array is assign the data to content part of the meta tag. 
	/// (see description of below method for more details). </summary>
	/// <param name="s"> the title to set the page to.  Can be null. </param>
	/// <param name="arrMeta1"> the array of 2 String to use for the first META tag.  
	/// arrMeta1[0] is used to assign the NAME part of the META tag and 
	/// arrMeta1[1] is used to assign the data that goes after the content </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void head(String s, String[] arrMeta1) throws Exception
	public virtual void head(string s, string[] arrMeta1)
	{
		head(s, arrMeta1, new string[]{"", ""});
	}


	/// <summary>
	/// Creates a HEAD tag and uses the given title String as the title of the page
	/// and the 2 arrays (of 2 Strings) passed in for adding 2 META data tags. 
	/// The first item in each array is used to assign the data for NAME part of 
	/// the META tag and the second item in each array is assign the data to 
	/// content part of the META tag. For example, for typical META tags such as the ones below:
	/// <PRE>
	/// <P><I><META NAME="keywords" content="hydrology, reservoir"></I>
	/// <BR><I><META NAME="description" content="Project analyzed the ..."></I>
	/// <table>
	/// <tr>
	/// <TH>Part of Meta Tag</TH><TH>Value in Array</TH><TH>Value in Meta Tag</TH></tr>
	/// <tr><td COLSPAN="3">First META TAG</td></tr>
	/// <tr><td>name</td><td>arrMeta1[0]</td><td>"keywords"</td></tr>
	/// <tr> <td>content</td><td>arrMeta1[1]</td><td>"hydrology, reservoir"</td></tr>
	/// <tr><td colspan="3">Second META TAG</td></tr>
	/// <tr><td>name</td><td>arrMeta2[0]</td><td>"description"</td></tr>
	/// <tr><td>content</td><td>arrMeta2[1]</td><td>"Project analyzed the hydrological..."</td></tr>
	/// </tr></table></P></PRE> </summary>
	/// <param name="s"> the title to set the page to.  Can be null. </param>
	/// <param name="arrMeta1"> the array of 2 String to use for the first META tag.  
	/// arrMeta1[0] is used to assign the NAME part of the META tag and 
	/// arrMeta1[1] is used to assign the data that goes after the content 
	/// part of the META tag. </param>
	/// <param name="arrMeta2"> the array of 2 String to use for a second META tag.  
	/// arrMeta2[0] is used to assign the NAME part of the META tag and 
	/// arrMeta2[1] is used to assign the data that goes after the content 
	/// part of the META tag. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void head(String s, String[] arrMeta1, String[] arrMeta2) throws Exception
	public virtual void head(string s, string[] arrMeta1, string[] arrMeta2)
	{
		if (string.ReferenceEquals(s, null))
		{
			s = "";
		}
		string meta_name1 = null;
		string meta_content1 = null;
		string meta_name2 = null;
		string meta_content2 = null;

		meta_name1 = arrMeta1[0];
		meta_content1 = arrMeta1[1];

		meta_name2 = arrMeta2[0];
		meta_content2 = arrMeta2[1];

		if ((string.ReferenceEquals(meta_name1, null)) || (meta_name1.Length <= 0))
		{
			//don't write any meta data...	
			write("<head>\n  <title>" + s + "</title>\n</head>\n");
		}
		else if ((string.ReferenceEquals(meta_name2, null)) || (meta_name2.Length <= 0))
		{
			//just write first meta tag
			write("<head>\n  <title>" + s + "</title>\n <meta name=\"" + meta_name1 + "\" content =\"" + meta_content1 + "\">\n</head>\n");
		}
		else
		{
			//just write both meta tags
			write("<head>\n  <title>" + s + "</title>\n <meta name=\"" + meta_name1 + "\" content =\"" + meta_content1 + "\">\n" + " <meta name=\"" + meta_name2 + "\" content =\"" + meta_content2 + "\">\n </head>\n");
		}
	}

	/// <summary>
	/// Head end tag. </summary>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headEnd() throws Exception
	public virtual void headEnd()
	{
		write("</head>\n");
	}

	// TODO SAM 2010-06-06 This is confusing - need to convert to heading methods when there is time.
	/// <summary>
	/// Writes a header end tag based on the size input needed.
	/// Size of 1 refers to creating an H1 tag.
	/// Size of 2 creates a H2 tag, and so on.
	/// This is based on the valid sizes for HTML header tags. </summary>
	/// <param name="size"> The size of the header tag.  1 is largest, 6 is smallest. </param>
	/// <param name="id"> The id to use for this tag if one is to be assigned. </param>
	/// <exception cref="Exception"> if file can't be written to. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headerEnd(int size) throws Exception
	public virtual void headerEnd(int size)
	{
		// check for valid tag
		if (size > 0 && size < 7)
		{
			write("</h" + size + ">\n");
		}
	}

	/// <summary>
	/// Writes a header start tag based on the size input needed.  
	/// Size of 1 refers to creatingan H1 tag.
	/// Size of 2 creates a H2 tag, and so on.
	/// This is based on the valid sizes for HTML header tags. </summary>
	/// <param name="size"> The size of the header tag.  1 is largest, 6 is smallest. </param>
	/// <param name="id"> The id to use for this tag if one is to be assigned. </param>
	/// <exception cref="Exception"> if file can't be written to. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headerStart(int size) throws Exception
	public virtual void headerStart(int size)
	{
		// check for valid tag
		if (size > 0 && size < 7)
		{
			write("<h" + size + ">\n");
		}
	}

	/// <summary>
	/// Writes a header tag based on the size input needed.
	/// Size of 1 refers to creating an H1 tag.  
	/// Size of 2 creates a H2 tag, and so on.
	/// This is based on the valid sizes for HTML header tags. </summary>
	/// <param name="int"> size - The size of the header tag.  1 is largest, 6 is smallest. </param>
	/// <param name="PropList"> p - Properties to assign to this tag. </param>
	/// <exception cref="Exception"> if file can't be written to.  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headerStart(int size, PropList p) throws Exception
	public virtual void headerStart(int size, PropList p)
	{
		string prop = propListToString(p);
		// check for valid tag
		if (size > 0 && size < 7)
		{
			if (string.ReferenceEquals(prop, null) || prop.Length == 0)
			{
				headerStart(size);
			}
			else
			{
				write("<h" + size + " " + prop + ">\n");
			}
		}
	}

	/// <summary>
	/// Creates a heading for the given size (1 is bigger, 6 is smallest) and the given text. </summary>
	/// <param name="number"> the kind of heading (1-6) to make. </param>
	/// <param name="s"> the string to store in the heading. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Heading%201">
	/// &lt;HX&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void heading(int number, String s) throws Exception
	public virtual void heading(int number, string s)
	{
		heading(number, s, null);
	}

	/// <summary>
	/// Creates a heading for the given size (1 is bigger, 6 is smallest) and the given text, and include a
	/// "name" property to allow linking to the heading. </summary>
	/// <param name="number"> the kind of heading (1-6) to make. </param>
	/// <param name="s"> the string to store in the heading.
	/// @paam name the target name for a link. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Heading%201">
	/// &lt;HX&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void heading(int number, String s, String name) throws Exception
	public virtual void heading(int number, string s, string name)
	{
		if (number < 1 || number > 6)
		{
			addText(s);
			return;
		}
		__hL[number]++;
		if ((!string.ReferenceEquals(name, null)) && (name.Length > 0))
		{
			write("\n<h" + number + "><a name=\"" + name + "\">" + s + "</a></h" + number + ">\n");
		}
		else
		{
			write("\n<h" + number + ">" + s + "</h" + number + ">\n");
		}
	}

	/// <summary>
	/// Ends a heading. </summary>
	/// <param name="number"> the kind of heading (1-6) to end. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Heading%201">
	/// &lt;HX&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headingEnd(int number) throws Exception
	public virtual void headingEnd(int number)
	{
		if (number < 1 || number > 6)
		{
			return;
		}
		__hL[number]--;
		write("</h" + number + ">\n");
	}

	/// <summary>
	/// Starts a heading section with the given size number. </summary>
	/// <param name="number"> the kind of heading (1-6) to make. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Heading%201">
	/// &lt;HX&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headingStart(int number) throws Exception
	public virtual void headingStart(int number)
	{
		headingStart(number, "");
	}

	/// <summary>
	/// Starts a heading section with the given size number and parameters. </summary>
	/// <param name="number"> the kind of heading (1-6) to make. </param>
	/// <param name="p"> PropList containing the parameters for the heading. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Heading%201">
	/// &lt;HX&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headingStart(int number, PropList p) throws Exception
	public virtual void headingStart(int number, PropList p)
	{
		headingStart(number, propListToString(p));
	}

	/// <summary>
	/// Starts a heading section with the given size number and parameters. </summary>
	/// <param name="number"> the kind of heading (1-6) to make. </param>
	/// <param name="s"> string of the parameters for the heading. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Heading%201">
	/// &lt;HX&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headingStart(int number, String s) throws Exception
	public virtual void headingStart(int number, string s)
	{
		if (number < 1 || number > 6)
		{
			return;
		}
		__hL[number]++;
		if (s.Trim().Equals(""))
		{
			write("\n<h" + number + ">");
			return;
		}
		write("\n<h" + number + " " + s + ">");
	}

	/// <summary>
	/// Start tag for head tag element </summary>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void headStart() throws Exception
	public virtual void headStart()
	{
		write("<head>\n");
	}

	/// <summary>
	/// Creates a horizontal rule across the page. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Horizontal%20Rule">
	/// &lt;HR&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void horizontalRule() throws Exception
	public virtual void horizontalRule()
	{
		write("\n<hr>\n");
	}

	/// <summary>
	/// Encodes the String passed in so that special characters
	/// are encoded for HTML.  For example: a ";" is changed to: "&amp;" </summary>
	/// <param name="s"> String to encode </param>
	/// <returns> encoded string </returns>
	public virtual string encodeHTML(string s)
	{
		return textToHtml(s);
	}

	/// <summary>
	/// Ends an HTML declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#HTML">
	/// &lt;HTML&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void htmlEnd() throws Exception
	public virtual void htmlEnd()
	{
		if (__htmlL == 0)
		{
			return;
		}
		if (__bodyL > 0)
		{
			bodyEnd();
		}
		__htmlL--;
		write("</html>\n");
	}

	/// <summary>
	/// Starts an HTML declaration.  Also add a DTD line for strict:
	/// <pre>
	/// <!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
	/// </pre> </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/toplevel.html#HTML">
	/// &lt;HTML&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void htmlStart() throws Exception
	public virtual void htmlStart()
	{
		__htmlL++;
		write("<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
		write("<html>\n");
	}

	/// <summary>
	/// Inserts an image tag with the given parameters. </summary>
	/// <param name="p"> PropList containing image tag parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Inline%20Image">
	/// &lt;IMG&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void image(PropList p) throws Exception
	public virtual void image(PropList p)
	{
		image(propListToString(p));
	}

	/// <summary>
	/// Inserts an image tag using the image at the path passed in. </summary>
	/// <param name="s"> Path to image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void image(String s) throws Exception
	public virtual void image(string s)
	{
		image(s, "", 0, -999, -999, "");
	}

	/// <summary>
	/// Inserts an image tag using the image at the path passed in and creates
	/// an alt tag with the second string passed in. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void image(String s, String alt_str) throws Exception
	public virtual void image(string s, string alt_str)
	{
		image(s, alt_str.Trim(), 0, -999, -999, "");
	}

	/// <summary>
	/// Inserts an image tag using the image at the path passed in and creates
	/// an alt tag with the second string passed in. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <param name="border"> Int to use to set border around image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void image(String s, String alt_str, int border) throws Exception
	public virtual void image(string s, string alt_str, int border)
	{
		image(s, alt_str.Trim(), border, -999, -999, "");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag.  If "", will write "" as alt Tag. </param>
	/// <param name="border"> Int to use to set border around image. </param>
	/// <param name="width"> Int to indicate width.  Pass in -999 to not specify. </param>
	/// <param name="height"> Int to indicate heigth.  Pass in -999 to not specify. </param>
	/// <param name="floatStr"> "left" or "right" to indicate image float with 
	/// margins of 10 pixels on all sides of the image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void image(String s, String alt_str, int border, int width, int height, String floatStr) throws Exception
	private void image(string s, string alt_str, int border, int width, int height, string floatStr)
	{
		if (s.Trim().Equals(""))
		{
			return;
		}
		if (floatStr.Equals(""))
		{
			if ((width > 0) & (height > 0))
			{
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" width=\"" + width + "\" height=\"" + height + "\">");
			}
			else if ((width > 0) & (height < 0))
			{
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" width=\"" + width + "\">");
			}
			else if ((width < 0) & (height > 0))
			{
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" height=\"" + height + "\">");
			}
			else
			{ //width and height are -999
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\">");
			}
		}
		else
		{
			if ((width > 0) & (height > 0))
			{
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" width=\"" + width + "\" height=\"" + height + "\" align=\"" + floatStr + "\" " + "STYLE=\"margin-left:10;margin-right:10;" + "margin-top:10;margin-bottom:10\">");
			}
			else if ((width > 0) & (height < 0))
			{
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" width=\"" + width + "\" align=\"" + floatStr + "\" " + "STYLE=\"margin-left:10;margin-right:10;" + "margin-top:10;margin-bottom:10\">");
			}
			else if ((width < 0) & (height > 0))
			{
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" height=\"" + height + "\" align=\"" + floatStr + "\" " + "STYLE=\"margin-left:10;margin-right:10;" + "margin-top:10;margin-bottom:10\">");
			}
			else
			{ //width and height are -999
				write("<img src= \"" + s + "\" alt=\"" + alt_str + "\" border=\"" + border + "\" align=\"" + floatStr + "\" " + "STYLE=\"margin-left:10;margin-right:10;" + "margin-top:10;margin-bottom:10\">");
			}
		}
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Left of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatLeft(String s) throws Exception
	public virtual void imageFloatLeft(string s)
	{
		image(s, "", 0, -999, -999, "left");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Left of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatLeft(String s, String alt_str) throws Exception
	public virtual void imageFloatLeft(string s, string alt_str)
	{
		image(s, alt_str, 0, -999, -999, "left");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Left of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <param name="border"> Int to use to set border around image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatLeft(String s, String alt_str, int border) throws Exception
	public virtual void imageFloatLeft(string s, string alt_str, int border)
	{
		image(s, alt_str, border, -999, -999, "left");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Left of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <param name="border"> Int to use to set border around image. </param>
	/// <param name="width"> Int to indicate width. </param>
	/// <param name="height"> Int to indicate heigth. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatLeft(String s, String alt_str, int border, int width, int height) throws Exception
	public virtual void imageFloatLeft(string s, string alt_str, int border, int width, int height)
	{
		image(s, alt_str, border, width, height, "left");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Right of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatRight(String s) throws Exception
	public virtual void imageFloatRight(string s)
	{
		image(s, "", 0, -999, -999, "right");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Right of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatRight(String s, String alt_str) throws Exception
	public virtual void imageFloatRight(string s, string alt_str)
	{
		image(s, alt_str, 0, -999, -999, "right");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Right of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <param name="border"> Int to use to set border around image. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatRight(String s, String alt_str, int border) throws Exception
	public virtual void imageFloatRight(string s, string alt_str, int border)
	{
		image(s, alt_str, border, -999, -999, "right");
	}

	/// <summary>
	/// Inserts an image tag with image path, alt tag, and border information 
	/// so that it floats to the Right of the text. </summary>
	/// <param name="s"> Path to image. </param>
	/// <param name="alt_str"> Text for alt Tag </param>
	/// <param name="border"> Int to use to set border around image. </param>
	/// <param name="width"> Int to indicate width. </param>
	/// <param name="height"> Int to indicate heigth. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void imageFloatRight(String s, String alt_str, int border, int width, int height) throws Exception
	public virtual void imageFloatRight(string s, string alt_str, int border, int width, int height)
	{
		image(s, alt_str, border, width, height, "right");
	}

	/// <summary>
	/// Adds the given string in italics to the HTML. </summary>
	/// <param name="s"> the String to italicize. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Italic">
	/// &lt;I&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void italic(String s) throws Exception
	public virtual void italic(string s)
	{
		write("<i>");
		addText(s);
		write("</i>");
	}

	/// <summary>
	/// Stops italicizing text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Italic">
	/// &lt;I&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void italicEnd() throws Exception
	public virtual void italicEnd()
	{
		if (__iL == 0)
		{
			return;
		}
		write("</i>");
		__iL--;
	}

	/// <summary>
	/// Begings italicizing text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Italic">
	/// &lt;I&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void italicStart() throws Exception
	public virtual void italicStart()
	{
		write("<i>");
		__iL++;
	}

	/// <summary>
	/// Inserts a hyperlink to the given location with the given text. </summary>
	/// <param name="aString"> the location to link to. </param>
	/// <param name="linkString"> the text to click on. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Anchor">
	/// &lt;a href&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void link(String aString, String linkString) throws Exception
	public virtual void link(string aString, string linkString)
	{
		write("<a href=\"" + aString + "\">");
		addLinkText(linkString);
		write("</a>\n");
	}

	/// <summary>
	/// Inserts a hyperlink to the given location with the given text. </summary>
	/// <param name="PropList"> p - List of properties for this tag. </param>
	/// <param name="aString"> the location to link to. </param>
	/// <param name="linkString"> the text to click on. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void link(PropList p, String aString, String linkString) throws Exception
	public virtual void link(PropList p, string aString, string linkString)
	{
		string prop = propListToString(p);
		if (!string.ReferenceEquals(aString, null) && aString.Length > 0)
		{
			write("<a " + prop + " href=" + aString + "> ");
		}
		else
		{
			write("<a " + prop + ">");
		}
		addLinkText(linkString);
		write("</a>\n");
	}

	/// <summary>
	/// Ends a Hyperlink tag (the section that is clicked on to go to a link). </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Anchor">
	/// &lt;a href&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void linkEnd() throws Exception
	public virtual void linkEnd()
	{
		if (__aL == 0)
		{
			return;
		}
		__aL--;
		write("</a>");
	}

	/// <summary>
	/// Inserts a hyperlink to the given location. </summary>
	/// <param name="s"> the location to link to. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Anchor">
	/// &lt;a href&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void linkStart(String s) throws Exception
	public virtual void linkStart(string s)
	{
		__aL++;
		write("<a href=\"" + s + "\">");
	}

	/// <summary>
	/// Inserts a list item into a list. </summary>
	/// <param name="s"> the item to insert in the list. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#List%20Item">
	/// &lt;LI&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void listItem(String s) throws Exception
	public virtual void listItem(string s)
	{
		if (__ulL == 0 && __olL == 0)
		{
			return;
		}
		__liL++;
		write("<li>");
		addText(s);
		write("</li>\n");
	}

	/// <summary>
	/// Ends a list item declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#List%20Item">
	/// &lt;LI&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void listItemEnd() throws Exception
	public virtual void listItemEnd()
	{
		if (__liL == 0)
		{
			return;
		}
		__liL--;
		write("</li>\n");
	}

	/// <summary>
	/// Starts a list item declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#List%20Item">
	/// &lt;LI&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void listItemStart() throws Exception
	public virtual void listItemStart()
	{
		if (__ulL == 0 && __olL == 0)
		{
			return;
		}
		__liL++;
		write("<li>");
	}

	/// <summary>
	/// Defines a block of text that has no line breaks. </summary>
	/// <param name="s"> the text with no line breaks. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#No%20Break">
	/// &lt;NOBR&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void nobr(String s) throws Exception
	public virtual void nobr(string s)
	{
		write("<nobr>" + s + "</nobr>");
	}

	/// <summary>
	/// Ends a section of text with no line breaks. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#No%20Break">
	/// &lt;NOBR&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void nobrEnd() throws Exception
	public virtual void nobrEnd()
	{
		if (__nobrL == 0)
		{
			return;
		}
		__nobrL--;
		write("</nobr>");
	}

	/// <summary>
	/// Starts a section of text with no line breaks. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#No%20Break">
	/// &lt;NOBR&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void nobrStart() throws Exception
	public virtual void nobrStart()
	{
		write("<nobr>");
		__nobrL++;
	}

	/// <summary>
	/// Starts a numbered list. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Ordered%20List">
	/// &lt;OL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void numberedListStart() throws Exception
	public virtual void numberedListStart()
	{
		__olL++;
		write("\n<ol>\n");
	}

	/// <summary>
	/// Ends a numbered list. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lists.html#Ordered%20List">
	/// &lt;OL&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void numberedListEnd() throws Exception
	public virtual void numberedListEnd()
	{
		if (__olL == 0)
		{
			return;
		}
		__olL--;
		write("\n</ol>\n");
	}

	/// <summary>
	/// Inserts a new paragraph. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Paragraph">
	/// &lt;P&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void paragraph() throws Exception
	public virtual void paragraph()
	{
		write("<p>\n");
	}

	/// <summary>
	/// Inserts a new paragraph with the given text. </summary>
	/// <param name="s"> the text to put in the paragraph. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Paragraph">
	/// &lt;P&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void paragraph(String s) throws Exception
	public virtual void paragraph(string s)
	{
		write("<p>\n" + textToHtml(s) + "</p>\n");
	}

	/// <summary>
	/// Ends a paragraph. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Paragraph">
	/// &lt;P&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void paragraphEnd() throws Exception
	public virtual void paragraphEnd()
	{
		if (__pL == 0)
		{
			return;
		}
		__pL--;
		write("</p>\n");
	}

	/// <summary>
	/// Starts a paragraph declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Paragraph">
	/// &lt;P&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void paragraphStart() throws Exception
	public virtual void paragraphStart()
	{
		paragraphStart("");
	}

	/// <summary>
	/// Starts a paragraph declaration with the given parameters. </summary>
	/// <param name="p"> the parameters to use in the paragraph declaration. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Paragraph">
	/// &lt;P&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void paragraphStart(PropList p) throws Exception
	public virtual void paragraphStart(PropList p)
	{
		paragraphStart(propListToString(p));
	}

	/// <summary>
	/// Starts a paragraph declaration with the given parameters. </summary>
	/// <param name="s"> String containing the paragraph parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/pformat.html#Paragraph">
	/// &lt;P&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void paragraphStart(String s) throws Exception
	public virtual void paragraphStart(string s)
	{
		__pL++;
		if (string.ReferenceEquals(s, null) || s.Trim().Equals(""))
		{
			write("<p>\n");
		}
		else
		{
			write("<p " + s + ">\n");
		}
	}

	/// <summary>
	/// Inserts a block of pre-formatted text. </summary>
	/// <param name="s"> the text to be pre-formatted. </param>
	/// <seealso cref= <a 
	/// href="http://www.willcam.com/cmat/html/pformat.html#Preformatted%20Text">
	/// &lt;PRE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pre(String s) throws Exception
	public virtual void pre(string s)
	{
		write("<pre>");
		// don't call addToText on this one since pre should
		// print exactly what is shown.  No special characters
		// need to be substituted
		write(s);
		write("</pre>");
	}

	/// <summary>
	/// Ends a block of pre-formatted text. </summary>
	/// <seealso cref= <a 
	/// href="http://www.willcam.com/cmat/html/pformat.html#Preformatted%20Text">
	/// &lt;PRE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void preEnd() throws Exception
	public virtual void preEnd()
	{
		if (__preL == 0)
		{
			return;
		}
		__preL--;
		write("</pre>\n");
	}

	/// <summary>
	/// Starts a block of pre-formatted text with the given parameters. </summary>
	/// <param name="p"> the parameters for the pre-formatted text block. </param>
	/// <seealso cref= <a 
	/// href="http://www.willcam.com/cmat/html/pformat.html#Preformatted%20Text">
	/// &lt;PRE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void preStart(PropList p) throws Exception
	public virtual void preStart(PropList p)
	{
		preStart(propListToString(p));
	}

	/// <summary>
	/// Starts a block of pre-formatted text. </summary>
	/// <seealso cref= <a 
	/// href="http://www.willcam.com/cmat/html/pformat.html#Preformatted%20Text">
	/// &lt;PRE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void preStart() throws Exception
	public virtual void preStart()
	{
		preStart("");
	}

	/// <summary>
	/// Starts a block of pre-formatted text with the given parameters. </summary>
	/// <param name="s"> String of the pre-formatted text parameters. </param>
	/// <seealso cref= <a 
	/// href="http://www.willcam.com/cmat/html/pformat.html#Preformatted%20Text">
	/// &lt;PRE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void preStart(String s) throws Exception
	public virtual void preStart(string s)
	{
		__preL++;
		if (s.Trim().Equals(""))
		{
			write("<pre>");
			return;
		}
		write("<pre " + s + ">");
	}

	/// <summary>
	/// Takes a PropList and turns every key/value pair into a single string that 
	/// contains <tt>key1=value1 key2=value2 ... keyN=valueN</tt> for each PropList
	/// prop from 1 to N. </summary>
	/// <param name="p"> PropList for which to get the properties. </param>
	/// <returns> a String with all the properties concatenated together and separated
	/// by spaces. </returns>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
	private string propListToString(PropList p)
	{
		if (p == null)
		{
			return "";
		}
		int size = p.size();
		string s = "";
		string val = "";
		for (int i = 0; i < size; i++)
		{
			Prop prop = p.elementAt(i);
			val = prop.getValue();
			if (val.Trim().Equals(""))
			{
				s += prop.getKey();
			}
			else
			{
				s += prop.getKey() + "=" + prop.getValue() + " ";
			}
		}

		return s;
	}


	/// <summary>
	/// Inserts a new quote. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quote() throws Exception
	public virtual void quote()
	{
		write("<q>\n");
	}

	/// <summary>
	/// Inserts a new quote with the given text. </summary>
	/// <param name="s"> the text to put in the quote. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quote(String s) throws Exception
	public virtual void quote(string s)
	{
		write("<q>\n" + textToHtml(s) + "</q>\n");
	}

	/// <summary>
	/// Ends a quote. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quoteEnd() throws Exception
	public virtual void quoteEnd()
	{
		write("</q>\n");
	}

	/// <summary>
	/// Starts a quote declaration. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quoteStart() throws Exception
	public virtual void quoteStart()
	{
		quoteStart("");
	}

	/// <summary>
	/// Starts a quote declaration with the given parameters. </summary>
	/// <param name="p"> the parameters to use in the quote declaration. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quoteStart(PropList p) throws Exception
	public virtual void quoteStart(PropList p)
	{
		quoteStart(propListToString(p));
	}

	/// <summary>
	/// Starts a quote declaration with the given parameters. </summary>
	/// <param name="s"> String containing the quote parameters. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quoteStart(String s) throws Exception
	public virtual void quoteStart(string s)
	{
		if (s.Trim().Equals(""))
		{
			write("<q>\n");
		}
		else
		{
			write("<q " + s + ">\n");
		}
	}

	/// <summary>
	/// Inserts a span element. </summary>
	/// <param name="spanText"> text to insert. </param>
	/// <param name="props"> property list for HTML attributes, for example style </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void span(String spanText, PropList props) throws Exception
	public virtual void span(string spanText, PropList props)
	{
		spanStart(props);
		addText(spanText);
		spanEnd();
	}

	/// <summary>
	/// Ends a span declaration. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void spanEnd() throws Exception
	public virtual void spanEnd()
	{
		write("</span>");
	}

	/// <summary>
	/// Starts a span declaration with the given properties. </summary>
	/// <param name="p"> PropList of span properties. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void spanStart(PropList p) throws Exception
	public virtual void spanStart(PropList p)
	{
		spanStart(propListToString(p));
	}

	/// <summary>
	/// Starts a span declaration with the given parameters. </summary>
	/// <param name="s"> String of span parameters. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void spanStart(String s) throws Exception
	public virtual void spanStart(string s)
	{
		if (s.Trim().Equals(""))
		{
			write("<span>");
			return;
		}
		write("<span " + s + ">");
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void styleStart() throws Exception
	public virtual void styleStart()
	{
		write("<style>\n");
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void styleEnd() throws Exception
	public virtual void styleEnd()
	{
		write("</style>\n");
	}

	/// <summary>
	/// Inserts a block of subscripted text. </summary>
	/// <param name="s"> the text to subscript </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Subscript">
	/// &lt;SUB&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void subscript(String s) throws Exception
	public virtual void subscript(string s)
	{
		write("<sub>");
		addText(s);
		write("</sub>");
	}

	/// <summary>
	/// Ends a block of subscripted text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Subscript">
	/// &lt;SUB&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void subscriptEnd() throws Exception
	public virtual void subscriptEnd()
	{
		write("</sub>");
	}

	/// <summary>
	/// Starts a block of subscripted text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Subscript">
	/// &lt;SUB&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void subscriptStart() throws Exception
	public virtual void subscriptStart()
	{
		write("<sub>");
	}

	/// <summary>
	/// Inserts a block of superscripted text. </summary>
	/// <param name="s"> the text to superscript. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Superscript">
	/// &lt;SUP&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void superscript(String s) throws Exception
	public virtual void superscript(string s)
	{
		write("<sup>");
		addText(s);
		write("</sup>");
	}

	/// <summary>
	/// Ends a block of superscripted text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Superscript">
	/// &lt;SUP&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void superscriptEnd() throws Exception
	public virtual void superscriptEnd()
	{
		write("</sup>");
	}

	/// <summary>
	/// Starts a block of superscripted text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Superscript">
	/// &lt;SUP&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void superscriptStart() throws Exception
	public virtual void superscriptStart()
	{
		write("<sup>");
	}

	/// <summary>
	/// Ends a table declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table">
	/// &lt;TABLE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableEnd() throws Exception
	public virtual void tableEnd()
	{
		if (__tableL == 0)
		{
			return;
		}
		write("</table>\n");
	}

	/// <summary>
	/// Starts a table declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table">
	/// &lt;TABLE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableStart() throws Exception
	public virtual void tableStart()
	{
		tableStart("");
	}

	/// <summary>
	/// Starts a table declaration with the given parameters. </summary>
	/// <param name="p"> PropList of the table's parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table">
	/// &lt;TABLE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableStart(PropList p) throws Exception
	public virtual void tableStart(PropList p)
	{
		tableStart(propListToString(p));
	}

	/// <summary>
	/// Starts a table declaration with the given parameters. </summary>
	/// <param name="s"> String of the table parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table">
	/// &lt;TABLE&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableStart(String s) throws Exception
	public virtual void tableStart(string s)
	{
		__tableL++;
		if (s.Trim().Equals(""))
		{
			write("<table>\n");
			return;
		}
		write("<table " + s + ">\n");
	}

	/// <summary>
	/// Starts a table declaration with the given parameters. </summary>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableStartFloatLeft() throws Exception
	public virtual void tableStartFloatLeft()
	{
		tableStartFloatLeft("");
	}

	/// <summary>
	/// Starts a table declaration with the given parameters. </summary>
	/// <param name="p"> PropList of the table's parameters </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableStartFloatLeft(PropList p) throws Exception
	public virtual void tableStartFloatLeft(PropList p)
	{
		tableStartFloatLeft(propListToString(p));
	}

	/// <summary>
	/// Starts a table declaration with the given parameters. </summary>
	/// <param name="s"> String of the table parameters that floats to the left. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableStartFloatLeft(String s) throws Exception
	public virtual void tableStartFloatLeft(string s)
	{
		__tableL++;
		if (s.Trim().Equals(""))
		{
			write("<table align=\"left\">\n");
			return;
		}
		write("<table align=\"left\" " + s + ">\n");
	}

	/// <summary>
	/// Inserts a cell into a table with the given text. </summary>
	/// <param name="s"> the text to put into the cell. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Data">
	/// &lt;td&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCell(String s) throws Exception
	public virtual void tableCell(string s)
	{
		if (__trL == 0)
		{
			return;
		}
		write("<td>");
		addText(s.replaceAll("\n", ""));
		write("</td>\n");
	}

	/// <summary>
	/// Inserts multiple cells into a table. </summary>
	/// <param name="cells"> Array of Strings to write to each cell. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCells(String cells []) throws Exception
	public virtual void tableCells(string[] cells)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			if (!string.ReferenceEquals(cells[i], null) && cells[i].Length > 0)
			{
				tableCell(cells[i]);
			}
		}
	}

	/// <summary>
	/// Inserts multiple cells into a table. </summary>
	/// <param name="cells"> Array of Strings to write to each cell.  Null strings are inserted as empty strings. </param>
	/// <param name="Property"> list for HTML attributes. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCells(String cells [], PropList props) throws Exception
	public virtual void tableCells(string[] cells, PropList props)
	{
		string cell;
		for (int i = 0; i < cells.Length; i++)
		{
			cell = cells[i];
			if (string.ReferenceEquals(cells[i], null))
			{
				cell = "";
			}
			tableCellStart(props);
			addText(cell);
			tableCellEnd();
		}
	}

	/// <summary>
	/// Ends a table cell declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Data">
	/// &lt;td&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCellEnd() throws Exception
	public virtual void tableCellEnd()
	{
		if (__tdL == 0)
		{
			return;
		}
		__tdL--;
		write("</td>\n");
	}

	/// <summary>
	/// Starts a table cell declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Data">
	/// &lt;td&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCellStart() throws Exception
	public virtual void tableCellStart()
	{
		tableCellStart("");
	}

	/// <summary>
	/// Starts a table cell declaration with the given properties. </summary>
	/// <param name="p"> PropList of table cell properties. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Data">
	/// &lt;td&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCellStart(PropList p) throws Exception
	public virtual void tableCellStart(PropList p)
	{
		tableCellStart(propListToString(p));
	}

	/// <summary>
	/// Starts a table cell declaration with the given parameters. </summary>
	/// <param name="s"> String of table cell parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Data">
	/// &lt;td&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableCellStart(String s) throws Exception
	public virtual void tableCellStart(string s)
	{
		if (__trL == 0)
		{
			return;
		}
		__tdL++;
		if (s.Trim().Equals(""))
		{
			write("<td>");
			return;
		}
		write("<td " + s + ">");
	}

	/// <summary>
	/// Inserts a table header cell with the given text. </summary>
	/// <param name="s"> text to put in the table header cell. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Header">
	/// &lt;TH&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableHeader(String s) throws Exception
	public virtual void tableHeader(string s)
	{
		if (__trL == 0)
		{
			return;
		}
		write("<th>" + textToHtml(s.replaceAll("\n", "")) + "</th>\n");
	}

	/// <summary>
	/// Inserts multiple table headers with the Strings specified. </summary>
	/// <param name="headers"> Array of Strings to use for each header. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableHeaders(String [] headers) throws Exception
	public virtual void tableHeaders(string[] headers)
	{
		for (int i = 0; i < headers.Length; i++)
		{
			if (!string.ReferenceEquals(headers[i], null) && headers[i].Length > 0)
			{
				tableHeader(headers[i].Trim());
			}
		}
	}

	/// <summary>
	/// Ends a table header cell declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Header">
	/// &lt;TH&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableHeaderEnd() throws Exception
	public virtual void tableHeaderEnd()
	{
		if (__thL == 0)
		{
			return;
		}
		__thL--;
		write("</th>\n");
	}

	/// <summary>
	/// Starts a table header cell declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Header">
	/// &lt;TH&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableHeaderStart() throws Exception
	public virtual void tableHeaderStart()
	{
		tableHeaderStart("");
	}

	/// <summary>
	/// Starts a table header cell declaration with the given parameters. </summary>
	/// <param name="p"> PropList of table header cell parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Header">
	/// &lt;TH&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableHeaderStart(PropList p) throws Exception
	public virtual void tableHeaderStart(PropList p)
	{
		tableHeaderStart(propListToString(p));
	}

	/// <summary>
	/// Starts a table header cell declaration with the given parameters. </summary>
	/// <param name="s"> String of table header cell parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Header">
	/// &lt;TH&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableHeaderStart(String s) throws Exception
	public virtual void tableHeaderStart(string s)
	{
		if (__trL == 0)
		{
			return;
		}
		__thL++;
		if (s.Trim().Equals(""))
		{
			write("<th>");
			return;
		}
		write("<th " + s + ">");
	}

	/// <summary>
	/// Inserts multiple cells into a table with the row start and end tags. </summary>
	/// <param name="cells"> Array of Strings to write to each cell. </param>
	/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableRow(String cells []) throws Exception
	public virtual void tableRow(string[] cells)
	{
		tableRowStart();
		tableCells(cells);
		tableRowEnd();
	}

	/// <summary>
	/// Ends a table row declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Row">
	/// &lt;tr&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableRowEnd() throws Exception
	public virtual void tableRowEnd()
	{
		if (__trL == 0)
		{
			return;
		}
		write("</tr>\n");
	}

	/// <summary>
	/// Starts a table row declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Row">
	/// &lt;tr&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableRowStart() throws Exception
	public virtual void tableRowStart()
	{
		tableRowStart("");
	}

	/// <summary>
	/// Starts a table row declaration with the given parameters. </summary>
	/// <param name="p"> the table row parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Row">
	/// &lt;tr&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableRowStart(PropList p) throws Exception
	public virtual void tableRowStart(PropList p)
	{
		tableRowStart(propListToString(p));
	}

	/// <summary>
	/// Starts a table row declaration with the given parameters. </summary>
	/// <param name="s"> String of the table row parameters. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/tables.html#Table%20Row">
	/// &lt;tr&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void tableRowStart(String s) throws Exception
	public virtual void tableRowStart(string s)
	{
		if (__tableL == 0)
		{
			return;
		}
		__trL++;
		if (s.Trim().Equals(""))
		{
			write("<tr>\n");
			return;
		}
		write("<tr " + s + ">\n");
	}

	/// <summary>
	/// Inserts a block of teletype-formatted text. </summary>
	/// <param name="s"> the teletype-formatted text to insert. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Teletype">
	/// &lt;TT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void teletype(String s) throws Exception
	public virtual void teletype(string s)
	{
		write("<tt>");
		addText(s);
		write("</tt>");
	}

	/// <summary>
	/// Ends a teletype block declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Teletype">
	/// &lt;TT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void teletypeEnd() throws Exception
	public virtual void teletypeEnd()
	{
		if (__ttL == 0)
		{
			return;
		}
		write("</tt>");
		__ttL--;
	}

	/// <summary>
	/// Starts a teletype block declaration. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Teletype">
	/// &lt;TT&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void teletypeStart() throws Exception
	public virtual void teletypeStart()
	{
		write("<tt>");
		__ttL++;
	}

	/// <summary>
	/// Inserts the document title given the text. </summary>
	/// <param name="s"> the title to insert. </param>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void title(String s) throws Exception
	public virtual void title(string s)
	{
		write("<title>");
		addText(s);
		write("</title>");
	}

	/// <summary>
	/// Inserts a block of underlined text. </summary>
	/// <param name="s"> the underlined text to insert. </param>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Underlined">
	/// &lt;U&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void underline(String s) throws Exception
	public virtual void underline(string s)
	{
		write("<u>");
		addText(s);
		write("</u>");
	}

	/// <summary>
	/// Ends a block of underlined text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Underlined">
	/// &lt;U&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void underlineEnd() throws Exception
	public virtual void underlineEnd()
	{
		if (__uL == 0)
		{
			return;
		}
		write("</u>");
		__uL--;
	}

	/// <summary>
	/// Starts a block of underlined text. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/lformat.html#Underlined">
	/// &lt;U&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void underlineStart() throws Exception
	public virtual void underlineStart()
	{
		write("<u>");
		__uL++;
	}

	/// <summary>
	/// Inserts a word break identifier, signifying where a word can be broken. </summary>
	/// <seealso cref= <a href="http://www.willcam.com/cmat/html/other.html#Word%20Break">
	/// &lt;WBR&gt; tag.</a> </seealso>
	/// <exception cref="Exception"> if an error occurs writing HTML text to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void wordBreak() throws Exception
	public virtual void wordBreak()
	{
		write("<wbr>");
	}

	/// <summary>
	/// Writes a String of text either to a file, or to an internal StringBuffer.
	/// No checks are done on the content, so it should contain proper open and closing tags.
	/// No newline is written so add the newline to the string before calling if needed for formatting. </summary>
	/// <param name="s"> the String to write. </param>
	/// <exception cref="Exception"> if an error occurs writing to a file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(String s) throws Exception
	public virtual void write(string s)
	{
		if (__writeToFile)
		{
			__htmlFile.Write(s);
		}
		else
		{
			__htmlBuffer.Append(s);
		}
	}

	}

}