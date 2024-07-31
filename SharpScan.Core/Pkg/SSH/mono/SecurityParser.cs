//
// Mono.Xml.SecurityParser.cs class implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security;

namespace Mono.Xml
{
    // convert an XML document into SecurityElement objects
    internal class SecurityParser : MiniParser, MiniParser.IHandler, MiniParser.IReader
    {
        private readonly Stack stack;
        private SecurityElement current;
        private int pos;
        private SecurityElement root;

        // IReader

        private string xmldoc;

        public SecurityParser()
        {
            stack = new Stack();
        }

        #region IHandler Members

        public void OnStartParsing(MiniParser parser)
        {
        }

        public void OnStartElement(string name, IAttrList attrs)
        {
            var newel = new SecurityElement(name);
            if (root == null)
            {
                root = newel;
                current = newel;
            }
            else
            {
                var parent = (SecurityElement) stack.Peek();
                parent.AddChild(newel);
            }
            stack.Push(newel);
            current = newel;
            // attributes
            int n = attrs.Length;
            for (int i = 0; i < n; i++)
                current.AddAttribute(attrs.GetName(i), attrs.GetValue(i));
        }

        public void OnEndElement(string name)
        {
            current = (SecurityElement) stack.Pop();
        }

        public void OnChars(string ch)
        {
            current.Text = ch;
        }

        public void OnEndParsing(MiniParser parser)
        {
        }

        #endregion

        #region IReader Members

        public int Read()
        {
            if (pos >= xmldoc.Length)
                return -1;
            return xmldoc[pos++];
        }

        #endregion

        public void LoadXml(string xml)
        {
            root = null;
            xmldoc = xml;
            pos = 0;
            stack.Clear();
            Parse(this, this);
        }

        public SecurityElement ToXml()
        {
            return root;
        }
    }
}