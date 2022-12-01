using SAMLHost.Models;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace SAMLHost.Services
{
    public class SamlService
    {
        private readonly UserService _userService;
        private readonly SamlIdentityProviderOptions _identityProvider;
        private readonly SamlServiceProviderOptions _serviceProvider;

        public SamlService(UserService userService, SamlIdentityProviderOptions identityProvider, SamlServiceProviderOptions serviceProvider)
        {
            _userService = userService;
            _identityProvider = identityProvider;
            _serviceProvider = serviceProvider;
        }

        public string GenerateRequest(string id, string instant)
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlWriterSettings writerSetting = new XmlWriterSettings { OmitXmlDeclaration = true };

                using (XmlWriter xmlWriter = XmlWriter.Create(writer, writerSetting))
                {
                    xmlWriter.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xmlWriter.WriteAttributeString("ID", id);
                    xmlWriter.WriteAttributeString("Version", "2.0");
                    xmlWriter.WriteAttributeString("IssueInstant", instant);
                    xmlWriter.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                    xmlWriter.WriteAttributeString("AssertionConsumerServiceURL", _serviceProvider.AssertionConsumerServiceUrl);
                    xmlWriter.WriteAttributeString("Destination", _identityProvider.SingleSignOnServiceUrl);
                    xmlWriter.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xmlWriter.WriteString(_serviceProvider.EntityID);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }

                byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(writer.ToString());
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream zip = new DeflateStream(output, CompressionMode.Compress))
                        zip.Write(toEncodeAsBytes, 0, toEncodeAsBytes.Length);
                    byte[] compressed = output.ToArray();
                    return Convert.ToBase64String(compressed);
                }
            }
        }

        public bool ResponseIsValid(XmlResponse response)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(response.Document.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            XmlNodeList nodeList = response.Document.SelectNodes("//ds:Signature", manager);
            SignedXml signedXml = new SignedXml(response.Document);
            if (nodeList == null || nodeList.Count == 0)
                return false;
            signedXml.LoadXml((XmlElement)nodeList[0]);

            using var certificate = new X509Certificate2(_identityProvider.CertificateFileName);
            return CheckSignature(signedXml, certificate);
        }

        public static string IssueInstant()
        {
            return DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static string GenerateId()
        {
            return string.Concat("_", Guid.NewGuid());
        }

        public static bool CheckSignature(SignedXml signedXml, X509Certificate2 cert)
        {
            return signedXml.CheckSignature(cert, true);
        }

        public string SetSsoToken(string userId)
        {
            var person = _userService.Get(userId);
            var token = Guid.NewGuid().ToString().Replace("-", string.Empty);
            person.SSOToken = token;
            return token;
        }
    }
}