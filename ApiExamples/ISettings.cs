namespace ApiExamples {
	public interface ISettings {
		string Username { get; set; }
		string Password { get; set; }
		string HashedPassword { get; }
		string Api { get; set; }
		string Token { get; set; }
	}
}