using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Common.Logging;

namespace ApiExamples {
	public class XmlApiRunner : ApiRunnerBase<XDocument> {
		public XmlApiRunner (ILog log) : base (log) { }

		protected override string ContentType {
			get { return "application/xml"; }
		}

		protected override string SelectToken (XDocument response, string path) {
			var element = response.XPathSelectElement ("/user/token");
			if (element == null) {
				return null;
			}

			var value = element.Value;
			return value;
		}

		protected override XDocument CreateResponse (Stream stream) {
			var doc = XDocument.Load (stream);
			return doc;
		}
	}
}