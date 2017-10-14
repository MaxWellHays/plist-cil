// plist-cil - An open source library to parse and generate property lists for .NET
// Copyright (C) 2015 Natalia Portillo
//
// This code is based on:
// plist - An open source library to parse and generate property lists
// Copyright (C) 2014 Daniel Dreibrodt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Claunia.PropertyList
{
    /// <summary>
    /// Parses XML property lists.
    /// </summary>
    /// @author Daniel Dreibrodt
    /// @author Natalia Portillo
    public static class XmlPropertyListParser
    {
        /// <summary>
        /// Parses a XML property list file.
        /// </summary>
        /// <param name="f">The XML property list file.</param>
        /// <returns>The root object of the property list. This is usually a NSDictionary but can also be a NSArray.</returns>
        public static NSObject Parse(FileInfo f)
        {
            using (Stream stream = f.OpenRead())
            {
                return Parse(stream);
            }
        }

        /// <summary>
        /// Parses a XML property list from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the property list's data.</param>
        /// <returns>The root object of the property list. This is usually a NSDictionary but can also be a NSArray.</returns>
        public static NSObject Parse(byte[] bytes)
        {
            MemoryStream bis = new MemoryStream(bytes);
            return Parse(bis);
        }

        /// <summary>
        /// Parses a XML property list from an input stream.
        /// </summary>
        /// <param name="str">The input stream pointing to the property list's data.</param>
        /// <returns>The root object of the property list. This is usually a NSDictionary but can also be a NSArray.</returns>
        public static NSObject Parse(Stream str)
        {
            XDocument doc;

            XmlReaderSettings settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

            using (XmlReader reader = XmlReader.Create(str, settings))
            {
                doc = XDocument.Load(reader);
            }

            return ParseDocument(doc);
        }

        /// <summary>
        /// Parses the XML document by generating the appropriate NSObjects for each XML node.
        /// </summary>
        /// <returns>The root NSObject of the property list contained in the XML document.</returns>
        /// <param name="doc">The XML document.</param>
        static NSObject ParseDocument(XDocument doc)
        {
            var docType = doc.Nodes().OfType<XDocumentType>().SingleOrDefault();

            if (docType == null)
            {
                if (!doc.Root?.Name?.LocalName?.Equals("plist") ?? false)
                {
                    throw new XmlException("The given XML document is not a property list.");
                }
            }
            else if (!docType.Name.Equals("plist"))
            {
                throw new XmlException("The given XML document is not a property list.");
            }

            XElement rootNode;

            if (doc.Root?.Name?.LocalName?.Equals("plist") ?? false)
            {
                //Root element wrapped in plist tag
                List<XElement> rootNodes = doc.Root.Elements().ToList(); ;
                if (rootNodes.Count == 0)
                    throw new PropertyListFormatException("The given XML property list has no root element!");
                if (rootNodes.Count == 1)
                    rootNode = rootNodes[0];
                else
                    throw new PropertyListFormatException("The given XML property list has more than one root element!");
            }
            else
                //Root NSObject not wrapped in plist-tag
                rootNode = doc.Root;

            return ParseObject(rootNode);
        }

        /// <summary>
        /// Parses a node in the XML structure and returns the corresponding NSObject
        /// </summary>
        /// <returns>The corresponding NSObject.</returns>
        /// <param name="n">The XML node.</param>
        static NSObject ParseObject(XElement n)
        {
            if (n.Name.LocalName.Equals("dict"))
            {
                NSDictionary dict = new NSDictionary();
                List<XElement> children = n.Elements().ToList();
                for (int i = 0; i < children.Count; i += 2)
                {
                    XElement key = children[i];
                    XElement val = children[i + 1];

                    string keyString = GetNodeTextContents(key);

                    dict.Add(keyString, ParseObject(val));
                }
                return dict;
            }
            if (n.Name.LocalName.Equals("array"))
            {
                List<XElement> children = n.Elements().ToList();
                NSArray array = new NSArray(children.Count);
                for (int i = 0; i < children.Count; i++)
                {
                    array.Add(ParseObject(children[i]));
                }
                return array;
            }
            if (n.Name.LocalName.Equals("true"))
                return new NSNumber(true);
            if (n.Name.LocalName.Equals("false"))
                return new NSNumber(false);
            if (n.Name.LocalName.Equals("integer"))
                return new NSNumber(GetNodeTextContents(n), NSNumber.INTEGER);
            if (n.Name.LocalName.Equals("real"))
                return new NSNumber(GetNodeTextContents(n), NSNumber.REAL);
            if (n.Name.LocalName.Equals("string"))
                return new NSString(GetNodeTextContents(n));
            if (n.Name.LocalName.Equals("data"))
                return new NSData(GetNodeTextContents(n));
            return n.Name.LocalName.Equals("date") ? new NSDate(GetNodeTextContents(n)) : null;
        }

        /// <summary>
        /// Returns a node's text content.
        /// This method will return the text value represented by the node's direct children.
        /// If the given node is a TEXT or CDATA node, then its value is returned.
        /// </summary>
        /// <returns>The node's text content.</returns>
        /// <param name="n">The node.</param>
        static string GetNodeTextContents(XElement n)
        {
            if (n.NodeType == XmlNodeType.Text || n.NodeType == XmlNodeType.CDATA)
            {
                string content = n.Value; //This concatenates any adjacent text/cdata/entity nodes
                return content ?? "";
            }
            List<XNode> children = n.Nodes().ToList();
            if (children.Any())
            {
                foreach (XNode child in children)
                {
                    //Skip any non-text nodes, like comments or entities
                    var xText = child as XText;
                    if (xText != null)
                    {
                        return xText.Value ?? string.Empty;
                    }
                }
                return "";
            }
            return "";
        }
    }
}

