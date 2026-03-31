namespace Apptech.views;

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

	private async void OnRegisterTapped2(object sender, EventArgs e)
	{
    
    await Navigation.PushAsync(new MainPage());
	}
}
