namespace Apptech;

public partial class LoginPage: ContentPage
{


	public LoginPage()
	{
		InitializeComponent();
	}

	private async void OnRegisterTapped(object sender, EventArgs e)
{
    
    await Navigation.PushAsync(new RegisterPage());
}
}
