namespace Apptech.views;
public partial class MainPage: ContentPage
{

	public MainPage()
	{
        InitializeComponent();
	}
    private void IrAProductos(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(0, animate: true);    }

    private void IrACategorias(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(1, animate: true);
    }

    private void IrAChat(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(2, animate: true);
    }

    private void IrAPerfil(object sender, EventArgs e)
    {

        miCarrusel.ScrollTo(3, animate: true);
    }

	
    
}