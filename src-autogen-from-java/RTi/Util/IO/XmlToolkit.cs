using System.Collections.Generic;

// XmlToolkit - useful XML processing methods as a toolkit to avoid static methods

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

	using Element = org.w3c.dom.Element;
	using NamedNodeMap = org.w3c.dom.NamedNodeMap;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;

	using Message = RTi.Util.Message.Message;

	// TODO smalers 2017-07-12 the methods in this class need to be renamed/refactored.
	// Code was consolidated from several places and the methods are redundant with different naming conventions.

	/// <summary>
	/// Useful XML processing methods as a toolkit to avoid static methods. See: -
	/// http://www.drdobbs.com/jvm/easy-dom-parsing-in-java/231002580
	/// 
	/// Element and Node are generally interchangeable for basic functionality and different nomenclature is used
	/// only because of historical reasons.
	/// </summary>
	public class XmlToolkit
	{

		/// <summary>
		/// Create an instance of the toolkit.
		/// </summary>
		public XmlToolkit()
		{
		}

		/// <summary>
		/// Find an element (given a parent element) that matches the given element name. </summary>
		/// <param name="parentElement"> parent element to process </param>
		/// <param name="elementName"> the element name to match </param>
		/// <param name="attributeName"> the attribute name to match (if null, don't try to match the attribute name) </param>
		/// <returns> the first matched element, or null if none are matched </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.w3c.dom.Element findSingleElement(org.w3c.dom.Element parentElement, String elementName) throws java.io.IOException
		public virtual Element findSingleElement(Element parentElement, string elementName)
		{
			return findSingleElement(parentElement, elementName, null);
		}

		/// <summary>
		/// Find an element (given a starting element) that matches the given element name. </summary>
		/// <param name="startElement"> Starting element to process. </param>
		/// <param name="elementName"> The element name to match.  Namespace is ignored. </param>
		/// <param name="attributeName"> Only return the element if the attribute name is matched. </param>
		/// <returns> the first matched element, or null if none are matched </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.w3c.dom.Element findSingleElement(org.w3c.dom.Element startElement, String elementName, String attributeName) throws java.io.IOException
		public virtual Element findSingleElement(Element startElement, string elementName, string attributeName)
		{
			NodeList nodes = startElement.getElementsByTagNameNS("*",elementName);
			if (nodes.getLength() == 0)
			{
				return null;
			}
			else
			{
				if ((!string.ReferenceEquals(attributeName, null)) && !attributeName.Equals(""))
				{
					// Want to search to see if the node has a matching attribute name
					for (int i = 0; i < nodes.getLength(); i++)
					{
						Node node = nodes.item(i);
						NamedNodeMap nodeMap = node.getAttributes();
						if (nodeMap.getNamedItem(attributeName) != null)
						{
							// Found the node of interest
							return (Element)node;
						}
					}
					// No node had the requested attribute
					return null;
				}
				else
				{
					return nodes.getLength() > 0 ? (Element) nodes.item(0) : null;
				}
			}
		}

		/// <summary>
		/// Find an element (given a starting element) that matches the given element name and return its string content. </summary>
		/// <param name="startElement"> parent element to process </param>
		/// <param name="elementName"> the element name to match, returning the value from the elements getTextContent(). </param>
		/// <returns> the first string text of the first matched element, or null if none are matched </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String findSingleElementValue(org.w3c.dom.Element startElement, String elementName) throws java.io.IOException
		public virtual string findSingleElementValue(Element startElement, string elementName)
		{
			Element el = findSingleElement(startElement, elementName);
			return el == null ? null : el.getTextContent().Trim();
		}

		/// <summary>
		/// Get all elements matching the given name from a starting element </summary>
		/// <param name="startElement"> Starting element to process </param>
		/// <param name="elementName"> the element name to match, namespace is not used to match. </param>
		/// <returns> the elements matching elementName, or null if not matched </returns>
		/// <exception cref="IOException"> if the number of matching nodes is not 1 </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<org.w3c.dom.Element> getElements(org.w3c.dom.Element startElement, String name) throws java.io.IOException
		public virtual IList<Element> getElements(Element startElement, string name)
		{
			NodeList nodes = startElement.getElementsByTagNameNS("*",name);
			IList<Element> elements = new List<Element>();
			for (int i = 0; i < nodes.getLength(); i++)
			{
				elements.Add((Element)nodes.item(i));
			}
			return elements;
		}

		/// <summary>
		/// Get a list of element text values.
		/// Return the list of element text values, using values returned from node getTextContent(). </summary>
		/// <param name="startElement"> Start element to start the search. </param>
		/// <param name="name"> Name of element to match.  Namespace is ignored to match elements. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<String> getElementValues(org.w3c.dom.Element startElement, String name) throws java.io.IOException
		public virtual IList<string> getElementValues(Element startElement, string name)
		{
			NodeList nodes = startElement.getElementsByTagNameNS("*",name);
			List<string> vals = new List<string>(nodes.getLength());
			for (int i = 0; i < nodes.getLength(); i++)
			{
				vals.Add(((Element) nodes.item(i)).getTextContent().Trim());
			}
			return vals;
		}

		/// <summary>
		/// Return the node that matches an element tag name, given a list of nodes to search.
		/// </summary>
		/// <param name="tagName">
		///            element tag name </param>
		/// <param name="nodes">
		///            nodes to search (not recursive) </param>
		/// <returns> first node that is matched, or null if not matched </returns>
		public virtual Node getNode(string tagName, NodeList nodes)
		{
			for (int x = 0; x < nodes.getLength(); x++)
			{
				Node node = nodes.item(x);
				if (node.getNodeName().equalsIgnoreCase(tagName))
				{
					return node;
				}
			}

			return null;
		}

		/// <summary>
		/// Return the nodes that matches an element tag name, given a list of nodes to search.
		/// </summary>
		/// <param name="tagName">
		///            element tag name </param>
		/// <param name="nodes">
		///            nodes to search (not recursive) </param>
		/// <returns> first node that is matched, or null if not matched </returns>
		public virtual IList<Node> getNodes(string tagName, NodeList nodes)
		{
			IList<Node> nodeList = new List<Node>();
			for (int i = 0; i < nodes.getLength(); i++)
			{
				Node node = nodes.item(i);
				if (node.getNodeName().equalsIgnoreCase(tagName))
				{
					nodeList.Add(node);
				}
			}
			return nodeList;
		}

		/// <summary>
		/// Return the attribute value for a node or "" if not found.
		/// </summary>
		/// <param name="attributeName"> element attribute name, with namespace if present in file </param>
		/// <param name="node"> node to process </param>
		/// <returns> string value for attribute name, or "" if not matched </returns>
		public virtual string getNodeAttribute(string attributeName, Node node)
		{
			NamedNodeMap attributes = node.getAttributes();
			//Message.printStatus(2, "", "Node \"" + node.getNodeName() + "\" has " + attributes.getLength() + " attributes" );
			for (int i = 0; i < attributes.getLength(); i++)
			{
				Node attribute = attributes.item(i);
				//Message.printStatus(2, "", "Comparing requested attribute \"" + attributeName + "\" with element attribute \"" + attribute.getNodeName() +"\"");
				if (attribute.getNodeName().equalsIgnoreCase(attributeName))
				{
					//Message.printStatus(2, "", "Matched, returning \"" + attribute.getNodeValue() + "\"");
					return attribute.getNodeValue();
				}
				else
				{
					//Message.printStatus(2, "", "Not Matched" );
				}
			}
			return "";
		}

		/// <summary>
		/// Return the string value of an element or "" if not found.
		/// </summary>
		/// <param name="tagName">
		///            element tag name </param>
		/// <param name="nodes">
		///            nodes to search (not recursive) </param>
		/// <returns> string value for first node that is matched, or "" if not matched </returns>
		public virtual string getNodeValue(string tagName, NodeList nodes)
		{
			for (int x = 0; x < nodes.getLength(); x++)
			{
				Node node = nodes.item(x);
				if (node.getNodeName().equalsIgnoreCase(tagName))
				{
					NodeList childNodes = node.getChildNodes();
					for (int y = 0; y < childNodes.getLength(); y++)
					{
						Node data = childNodes.item(y);
						if (data.getNodeType() == Node.TEXT_NODE)
						{
							return data.getNodeValue();
						}
					}
				}
			}
			return "";
		}

		/// <summary>
		/// Return the text value of the single matching element. </summary>
		/// <param name="parentElement"> Starting element to process </param>
		/// <param name="elementName"> the element name to match, namespace is not used to match. </param>
		/// <returns> the first string text of the first matched element (from getTextContent), or null if none are matched </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getSingleElementValue(org.w3c.dom.Element startElement, String elementName) throws java.io.IOException
		public virtual string getSingleElementValue(Element startElement, string elementName)
		{
			return getSingleElement(startElement, elementName).getTextContent().Trim();
		}

		/// <summary>
		/// Get the single element matching the given name from a starting element </summary>
		/// <param name="startElement"> Starting element to process </param>
		/// <param name="elementName"> the element name to match, namespace is not used to match. </param>
		/// <returns> the element matching elementName, or null if not matched </returns>
		/// <exception cref="IOException"> if the number of matching nodes is not 1 </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.w3c.dom.Element getSingleElement(org.w3c.dom.Element startElement, String name) throws java.io.IOException
		public virtual Element getSingleElement(Element startElement, string name)
		{
			NodeList nodes = startElement.getElementsByTagNameNS("*",name);
			if (nodes.getLength() != 1)
			{
				throw new IOException("Expected to find 1 child \"" + name + "\" in \"" + startElement.getTagName() + "\" - found " + nodes.getLength());
			}
			return (Element) nodes.item(0);
		}

		/// <summary>
		/// Get the single element matching the given name moving up in the DOM relative to the starting element. </summary>
		/// <param name="startElement"> Starting element to process </param>
		/// <param name="elementName"> the element name to match, namespace is not used to match. </param>
		/// <returns> the element matching elementName, or null if not matched </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.w3c.dom.Element getSingleElementPrevious(org.w3c.dom.Element startElement, String elementName) throws java.io.IOException
		public virtual Element getSingleElementPrevious(Element startElement, string elementName)
		{
			Node node = startElement;
			while (true)
			{
				// Get the parent node
				node = node.getParentNode();
				if (node == null)
				{
					// Did not find the requested node
					break;
				}
				if (node.getNodeName().equalsIgnoreCase(elementName))
				{
					return (Element)node;
				}
			}
			return null;
		}

	}

}