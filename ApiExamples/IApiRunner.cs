
namespace ApiExamples {
	public interface IApiRunner<TReturnType> where TReturnType : class {
		void Login (ISettings settings);
		TReturnType RetrieveRackCollection (ISettings settings);
		TReturnType RetrieveRackContents (ISettings settings, string id);
		TReturnType RetrieveAsset (ISettings settings, string id);
	}
}