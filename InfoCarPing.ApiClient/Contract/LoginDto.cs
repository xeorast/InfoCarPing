namespace InfoCarPing.ApiClient.Contract;

internal class LoginDto : Dictionary<string, string>
{
	public string UserName { get => this["username"]; set => this["username"] = value; }
	public string Password { get => this["password"]; set => this["password"] = value; }
	public string Csrf { get => this["_csrf"]; set => this["_csrf"] = value; }
}
