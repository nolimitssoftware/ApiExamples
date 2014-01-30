using System.IO;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiExamples {
	public class JsonApiRunner : ApiRunnerBase<JToken> {
		public JsonApiRunner (ILog log) : base (log) { }

		protected override string ContentType {
			get { return "application/json"; }
		}

		protected override string SelectToken (JToken response, string path) {
			var element = response.SelectToken ("token");
			if (element == null) {
				return null;
			}

			var value = element.Value<string> ();
			return value;
		}

		protected override JToken CreateResponse (Stream stream) {
			using (var reader = new JsonTextReader (new StreamReader (stream))) {
				var doc = JToken.Load (reader);
				return doc;
			}
		}
	}
}