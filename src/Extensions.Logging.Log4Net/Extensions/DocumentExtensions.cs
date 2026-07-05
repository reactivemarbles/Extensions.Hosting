// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ReactiveMarbles.Extensions.Logging.Log4Net.Extensions;

/// <summary>Class with XmlDocument and XDocument extensions.</summary>
internal static class DocumentExtensions
{
    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="document">The receiver instance.</param>
    extension(XDocument document)
    {
        /// <summary>Converts a XDocument object into XmlDocument.</summary>
        /// <returns>The XDocument converted to XmlDocument.</returns>
        public XmlDocument ToXmlDocument()
        {
            using var memoryStream = new MemoryStream();
            document.Save(memoryStream);
            _ = memoryStream.Seek(0, SeekOrigin.Begin);

            var xmlDocument = new XmlDocument
            {
                XmlResolver = null,
            };
            using var reader = XmlReader.Create(memoryStream, CreateSecureXmlReaderSettings());
            xmlDocument.Load(reader);
            return xmlDocument;
        }
    }

    /// <summary>Provides extension members for this receiver.</summary>
    /// <param name="xmlDocument">The receiver instance.</param>
    extension(XmlDocument xmlDocument)
    {
        /// <summary>Converts a XmlDocument object into xDocument.</summary>
        /// <returns>The XmlDocument converted to XDocument.</returns>
        public XDocument ToXDocument()
        {
            using var memoryStream = new MemoryStream();
            xmlDocument.Save(memoryStream);
            _ = memoryStream.Seek(0, SeekOrigin.Begin);

            using var reader = XmlReader.Create(memoryStream, CreateSecureXmlReaderSettings());
            return XDocument.Load(reader);
        }
    }

    /// <summary>Creates XML reader settings that prevent DTD processing.</summary>
    /// <returns>The secure XML reader settings.</returns>
    private static XmlReaderSettings CreateSecureXmlReaderSettings() =>
        new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };
}
