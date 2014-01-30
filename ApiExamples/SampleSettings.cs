using System.Security.Cryptography;
using System.Text;

namespace ApiExamples {
	public class SampleSettings : ISettings {
		public string Username { get; set; }
		public string Password { get; set; }

		public string HashedPassword {
			get { return CreateHash (Password); }
		}

		public string Api { get; set; }
		public string Token { get; set; }

		private string CreateHash (string value) {
			if (string.IsNullOrEmpty (value)) {
				return value;
			}
			using (var md5 = MD5.Create ()) {
				var inputBytes = Encoding.UTF8.GetBytes (value);
				var hash = md5.ComputeHash (inputBytes);

				var builder = new StringBuilder ();

				for (int i = 0; i < hash.Length; i++) {
					builder.Append (hash [i].ToString ("x2"));
				}
				return builder.ToString ();
			}
		}
	}
}