using System;
using System.Xml;
using System.Xml.Linq;

namespace CallWall.Web.LinkedInProvider
{
    public static class XmlEx
    {
        private static readonly XmlNamespaceManager _ns;

        static XmlEx()
        {
            _ns = new XmlNamespaceManager(new NameTable());
            Ns.AddNamespace("x", "http://www.w3.org/2005/Atom");
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
    }
}