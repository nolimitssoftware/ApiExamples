using System.Xml.Linq;
using Common.Logging;
using Newtonsoft.Json.Linq;
using Ninject.Modules;

namespace ApiExamples {
	internal class SamplesModule : NinjectModule {
		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load () {
			Bind<ILog> ()
				.ToMethod (context => {
					if (context.Request.ParentRequest != null && context.Request.ParentRequest.Service != null) {
						return LogManager.GetLogger (context.Request.ParentRequest.Service.FullName);
					}
					return LogManager.GetLogger ("");
				});

			Bind<IApiRunner<JToken>> ()
				.To<JsonApiRunner> ()
				.Named ("json");
			Bind<IApiRunner<XDocument>> ()
				.To<XmlApiRunner> ()
				.Named ("xml");

		}
	}
}