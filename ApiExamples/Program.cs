using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Common.Logging;
using NDesk.Options;
using Newtonsoft.Json.Linq;
using Ninject;

namespace ApiExamples {
	internal class Program {
		private readonly ILog log;
		private readonly IApiRunner<JToken> jsonApiRunner;
		private readonly IApiRunner<XDocument> xmlApiRunner;
		private readonly IKernel kernel;

		private OptionSet options;

		public Program (ILog log, IApiRunner<JToken> jsonApiRunner, IApiRunner<XDocument> xmlApiRunner) {
			this.log = log;
			this.jsonApiRunner = jsonApiRunner;
			this.xmlApiRunner = xmlApiRunner;
		}

		private static void Main (string [] args) {
			var kernel = CreateKernel ();
			var program = kernel.Get<Program> ();
			program.Run (args);
		}

		private static IKernel CreateKernel () {
			var kernel = new StandardKernel ();
			kernel.Load (new SamplesModule ());
			return kernel;
		}

		private void Run (string [] args) {

			var showHelp = false;
			var sampleSettings = new SampleSettings ();
			var promptForPassword = false;
			var isJson = false;
			var isXml = false;
			options = new OptionSet {
				{"<>", "The url of the API.", v => sampleSettings.Api = v},
				{"u|username=", "The username used to log into RaMP.", v => sampleSettings.Username = v},
				{"e", "The username used to log into RaMP.", v => promptForPassword = v!=null},
				{"p|password=", "The password for the user.", v => sampleSettings.Password = v},
				{"j|json", "(Optional) Show output as JSON. (Default)", v => isJson = v!=null},
				{"x|xml", "(Optional) Show output as XML. (Default)", v => isXml = v!= null},
				{"h|?|help", "Show available options", v => showHelp = true}
			};

			try {
				options.Parse (args);
			} catch (OptionException exception) {
				log.Debug ("Encountered an error parsing arguments!", exception);
				Console.WriteLine ("Error parsing arguments!");
				showHelp = true;
			}

			if (!isJson && !isXml) {
				showHelp = true;
			}

			if (showHelp) {
				ShowHelp (options);
				return;
			}

			if (promptForPassword) {
				var pass = "";
				Console.Write ("Enter your password: ");
				ConsoleKeyInfo key;
				do {
					key = Console.ReadKey (true);
					if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter) {
						pass += key.KeyChar;
						Console.Write ("*");
					} else {
						if (key.Key == ConsoleKey.Backspace && pass.Length > 0) {
							pass = pass.Substring (0, (pass.Length - 1));
							Console.Write ("\b \b");
						}
					}
				} // Stops Receiving Keys Once Enter is Pressed
				while (key.Key != ConsoleKey.Enter);

				sampleSettings.Password = pass;
			}

			if (isJson) {
				Run (jsonApiRunner, sampleSettings);
			}

			if (isXml) {
				Run (xmlApiRunner, sampleSettings);
			}

			Console.WriteLine ("Complete. Press enter to exit.");
			Console.ReadLine ();

		}

		private void Run (IApiRunner<JToken> runner, SampleSettings settings) {
			//1. You will need to login to the site to retrieve the token to use for authentication on the other requests	
			runner.Login (settings);

			//2. Retrieve the list of racks (racks.xml)
			var collection = runner.RetrieveRackCollection (settings);
			WriteResults ("racks.json", collection);

			//I am just using the id from the first rack
			if (collection == null) {
				return;
			}
			var token = collection.SelectToken ("asset[0].id");
			if (token == null) {
				return;
			}
			var id = token.Value<string> ();

			//3. Retrieve the list of racks (rackContents.xml) 
			var rackContentsResponse = runner.RetrieveRackContents (settings, id);

			WriteResults ("rackContents.json", rackContentsResponse);

			//4. Retrieves the rack item (NLSRack.xml)

			var rackResponse = runner.RetrieveAsset (settings, id);
			WriteResults ("rack.json", rackResponse);
		}

		private void Run (IApiRunner<XDocument> runner, SampleSettings settings) {
			//1. You will need to login to the site to retrieve the token to use for authentication on the other requests	
			runner.Login (settings);

			//2. Retrieve the list of racks (racks.xml)
			var collection = runner.RetrieveRackCollection (settings);
			WriteResults ("racks.xml", collection);

			//I am just using the id from the first rack
			if (collection == null) {
				return;
			}
			var token = collection.XPathSelectElement ("/assets/asset[position()=1]/id");
			if (token == null) {
				return;
			}
			var id = token.Value;

			//3. Retrieve the list of racks (rackContents.xml) 
			var rackContentsResponse = runner.RetrieveRackContents (settings, id);

			WriteResults ("rackContents.xml", rackContentsResponse);

			//4. Retrieves the rack item (NLSRack.xml)
			var rackResponse = runner.RetrieveAsset (settings, id);
			WriteResults ("rack.xml", rackResponse);
		}

		private void WriteResults (string fileName, object item) {
			if (item == null) {
				return;
			}
			var s = item.ToString ();
			Console.WriteLine (s);
			if (!Directory.Exists ("output")) {
				Directory.CreateDirectory ("output");
			}
			var path = Path.Combine ("output", fileName);
			File.WriteAllText (path, s);
		}

		/// <summary>	Shows the help. </summary>
		///
		/// <remarks>	http://pcsupport.about.com/od/commandlinereference/a/command-syntax.htm </remarks>
		///
		/// <param name="options">	Options for controlling the operation. </param>
		private static void ShowHelp (OptionSet options) {
			Console.WriteLine ("USAGE: Samples.exe -u username {-p password | -e} [-j json] [-x xml] <api>");
			Console.WriteLine ("\tExample: Samples.exe -u test -p test12 http://ramp-staging.nolimitssoftware.com/api ");
			Console.WriteLine ();
			options.WriteOptionDescriptions (Console.Out);
		}
	}
}
