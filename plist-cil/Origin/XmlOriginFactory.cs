using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Claunia.PropertyList.Origin
{
    class XmlOriginFactory
    {
        private readonly Stream sourceStream;

        public XmlOriginFactory(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
        }

        public XmlOrigin GetOrigin(XNode xNode)
        {
            var currentNodePositionInfo = GetPositionInfo(xNode);
            var nextNodePositionInfo = GetPositionInfo(xNode.NextNode);

            var currentNodeStreamPosition = GetPositionInStream(currentNodePositionInfo.lineNumber, currentNodePositionInfo.linePosition);
            var nextNodeStreamPosition = GetPositionInStream(nextNodePositionInfo.lineNumber, nextNodePositionInfo.linePosition);

            return new XmlOrigin((int)currentNodeStreamPosition, (int)(nextNodeStreamPosition - currentNodeStreamPosition),
                currentNodePositionInfo.linePosition, currentNodePositionInfo.lineNumber);
        }

        private readonly List<long> streamPositionAtLineNumber = new List<long> { 0 };

        private long GetPositionInStream(int lineNumber, int linePosition)
        {
            if (streamPositionAtLineNumber.Count != 1)
            {
                if (lineNumber < streamPositionAtLineNumber.Count)
                {
                    return streamPositionAtLineNumber[lineNumber] + linePosition;
                }
                else
                {
                    return streamPositionAtLineNumber.Last();
                }
            }
            else
            {
                sourceStream.Position = 0;
                using (var streamReader = new StreamReader(sourceStream))
                {
                    int lineCounter = 1;
                    var line = streamReader.ReadLine();
                    while (line != null)
                    {
                        if (lineCounter < streamPositionAtLineNumber.Count)
                        {
                            continue;
                        }

                        streamPositionAtLineNumber.Add(streamPositionAtLineNumber.Last() + line.Length);

                        lineCounter++;
                        line = streamReader.ReadLine();
                    }
                }
            }

            return GetPositionInStream(lineNumber, linePosition);
        }

        private static (int lineNumber, int linePosition) GetPositionInfo(XNode xNode)
        {
            IXmlLineInfo xmlLineInfo = xNode;
            if (!xmlLineInfo.HasLineInfo())
            {
                throw new ArgumentException();
            }
            return (xmlLineInfo.LineNumber - 1, xmlLineInfo.LinePosition - 1);
        }
    }
}
