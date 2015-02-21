using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace CallWall.Web.GoogleProvider
{
    public static class XmlEx
    {
        private static readonly XmlNamespaceManager _ns;

        static XmlEx()
        {
            _ns = new XmlNamespaceManager(new NameTable());
            Ns.AddNamespace("x", "http://www.w3.org/2005/Atom");
            Ns.AddNamespace("openSearch", "http://a9.com/-/spec/opensearch/1.1/");
            Ns.AddNamespace("gContact", "http://schemas.google.com/contact/2008");
            Ns.AddNamespace("batch", "http://schemas.google.com/gdata/batch");
            Ns.AddNamespace("gd", "http://schemas.google.com/g/2005");
        }

        public static XmlNamespaceManager Ns { get { return _ns; } }

        public static XName ToXName(string prefix, string name)
        {
            var xNamespace = Ns.LookupNamespace(prefix);
            if (xNamespace == null)
                throw new InvalidOperationException(prefix + " namespace prefix is not valid");
            return XName.Get(name, xNamespace);
        }

        public static XElement Element(this XContainer source, string prefix, string name)
        {
            return source.Element(ToXName(prefix, name));
        }

        public static IEnumerable<XElement> Elements(this XContainer source, string prefix, string name)
        {
            return source.Elements(ToXName(prefix, name));
        }

        public static class OpenSearch
        {
            static OpenSearch()
            {
                TotalResults = XmlEx.ToXName("openSearch", "totalResults");
                StartIndex = XmlEx.ToXName("openSearch", "startIndex");
                ItemsPerPage = XmlEx.ToXName("openSearch", "itemsPerPage");
            }

            public static XName TotalResults { get; private set; }
            public static XName ItemsPerPage { get; private set; }
            public static XName StartIndex { get; private set; }
        }

        public static class Atom
        {
            static Atom()
            {
                Entry = XmlEx.ToXName("x", "entry");
                Title = XmlEx.ToXName("x", "title");
            }

            public static XName Entry { get; private set; }
            public static XName Title { get; private set; }
        }

        public static class Gd
        {
            static Gd()
            {
                ETag = XmlEx.ToXName("gd", "etag");
                Email = XmlEx.ToXName("gd", "email");
            }

            public static XName ETag { get; private set; }
            public static XName Email { get; private set; }
        }
    }
}
