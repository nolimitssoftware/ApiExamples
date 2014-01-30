using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Common.Logging;

namespace ApiExamples {

	public abstract class ApiRunnerBase<TReturnType> : IApiRunner<TReturnType> where TReturnType : class {
		private readonly ILog log;

		protected ApiRunnerBase (ILog log) {
			this.log = log;
		}

		protected ILog Log {
			get { return log; }
		}

		protected abstract string ContentType { get; }

		public void Login (ISettings settings) {
			const string route = "account/login";
			var parameters = new Dictionary<string, string> () {
				{"userName", settings.Username},
				{"password", settings.HashedPassword}
			};

			var loginResponse = MakeApiRequest (settings.Api, route, "POST", null, null, parameters, null);
			if (loginResponse == null) {
				return;
			}
			var token = SelectToken (loginResponse, "/user/token");
			settings.Token = token;
		}

		public TReturnType RetrieveRackCollection (ISettings settings) {
			const string route = "asset/";
			var filters = new List<Tuple<string, string>> {
				Tuple.Create ("type", "rack")
			};
			var properties = new List<string> () {
				"power",
				"location"
			};
			var rackCollectionResponse = MakeApiRequest (settings.Api, route, "GET", settings.Token, filters, null, properties);

			return rackCollectionResponse;
		}

		public TReturnType RetrieveRackContents (ISettings settings, string id) {
			const string route = "asset/";
			var filters = new List<Tuple<string, string>> () {
				Tuple.Create ("parent", id)
			};
			var properties = new List<string> () {
				"power",
				"location"
			};
			var rackContentsResponse = MakeApiRequest (settings.Api, route, "GET", settings.Token, filters, null, properties);

			return rackContentsResponse;
		}

		public TReturnType RetrieveAsset (ISettings settings, string id) {
			var route = "asset/" + id;
			var rackResponse = MakeApiRequest (settings.Api, route, "GET", settings.Token, null, null, null);
			return rackResponse;
		}

		protected abstract string SelectToken (TReturnType response, string path);

		protected abstract TReturnType CreateResponse (Stream stream);

		protected TReturnType MakeApiRequest (string api, string route, string method = "GET", string token = null, IEnumerable<Tuple<string, string>> filters = null, IDictionary<string, string> parameters = null, IEnumerable<string> properties = null) {

			var parameterList = string.Empty;
			var filterList = string.Empty;
			var propertyList = string.Empty;

			if (parameters != null && parameters.Any ()) {
				var urlBuilder = new StringBuilder ();

				foreach (var parameter in parameters) {
					urlBuilder.AppendFormat ("{0}={1}&", parameter.Key, parameter.Value);
				}
				urlBuilder.Length--;

				parameterList = urlBuilder.ToString ();
			}

			if (filters != null && filters.Any ()) {
				var urlBuilder = new StringBuilder ();
				var filterItems = new HashSet<string> ();
				foreach (var filter in filters) {
					filterItems.Add (filter.Item1);
					urlBuilder.AppendFormat ("{0}={1}&", filter.Item1, filter.Item2);
				}
				urlBuilder.AppendFormat ("filterOn={0}", string.Join (",", filterItems));

				filterList = urlBuilder.ToString ();
			}

			if (properties != null && properties.Any ()) {
				propertyList = string.Join (",", properties);
			}
			var uri = new Uri (new Uri (api), route);
			var url = string.Format ("{0}?{1}{2}{3}{4}{5}", uri, parameterList, string.IsNullOrEmpty (filterList) ? string.Empty : "&", filterList, string.IsNullOrEmpty (propertyList) ? string.Empty : "&", propertyList);

			var request = WebRequest.Create (url) as HttpWebRequest;
			if (request == null) {
				return null;
			}
			request.Accept = ContentType;
			request.Expect = ContentType;
			request.UserAgent = "6sigma";
			request.Method = method;

			if (!string.IsNullOrEmpty (token)) {
				request.Headers.Add ("token", token);
			}

			if (method == "POST") {
				using (var requestStream = request.GetRequestStream ()) {
					var data = string.Empty;
					var dataBytes = Encoding.UTF8.GetBytes (data);
					requestStream.Write (dataBytes, 0, dataBytes.Length);
				}
			}

			try {
				using (var response = request.GetResponse ()) {
					using (var stream = response.GetResponseStream ()) {
						return CreateResponse (stream);
					}
				}
			} catch (WebException webEx) {
				var response = webEx.Response as HttpWebResponse;
				//log error
				if (response != null) {
					Log.ErrorFormat ("Error login: {0}", response.StatusCode);
					return null;
				}

				Log.ErrorFormat ("Error {0}", webEx.ToString ());
				return null;
			}
		}
	}
}

/*public class JsonApiRunner : ApiRunnerBase,
		IApiRunner {
		public ApiRunner (ILog log) : base (log) {}

		//protected string LoginXml (SampleSettings settings) {

		//}

		//protected string LoginJson (SampleSettings settings) {

		//}

		/*protected string Login (SampleSettings settings) {
			//1. You will need to login to the site to retrieve the token to use for authentication on the other requests	

			var loginUrl = settings.Api + "account/login";
			var parameters = new Dictionary<string, string> () {
				{"userName", settings.Username},
				{"password", settings.HashedPassword}
			};

			var loginResponse = MakeApiRequest (loginUrl, "POST", null, null, parameters, null);
			if (loginResponse == null) {
				return;
			}
			var token = loginResponse.XPathSelectElement ("/user/token")
				.Value;
		}#1#

		protected JToken MakeJsonApiRequest (string api, string route, string method = "GET", string token = null, IEnumerable<Tuple<string, string>> filters = null, IDictionary<string, string> parameters = null,
			IEnumerable<string> properties = null) {
			var contentType = "application/json";
			return MakeApiRequest (contentType, CreateJToken, api, route, method, token, filters, parameters, properties);
		}

		public XDocument MakeXmlApiRequest (string api, string route, string method = "GET", string token = null, IEnumerable<Tuple<string, string>> filters = null, IDictionary<string, string> parameters = null, IEnumerable<string> properties = null) {
			const string contentType = "application/xml";

			return MakeApiRequest (contentType, CreateXDocument, url, method, token, filters, parameters, properties);
		}

		private XDocument CreateXDocument (Stream stream) {
			var doc = XDocument.Load (stream);
			return doc;
		}

		private JToken CreateJToken (Stream stream) {
			using (var reader = new JsonTextReader (new StreamReader (stream))) {
				var doc = JToken.Load (reader);
				return doc;
			}
		}
	}
}*/