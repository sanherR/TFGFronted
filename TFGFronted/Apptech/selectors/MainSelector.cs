namespace Apptech.selectors;
public class MainSelector: DataTemplateSelector
{


    public DataTemplate ProductosTemplate { get; set; }    
    public DataTemplate ProductosCategoriaTemplate { get; set; }
    public DataTemplate ChatTemplate { get; set; }
    public DataTemplate PerfilTemplate { get; set; }   

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
       return item switch
        {
            "Productos" => ProductosTemplate,
            "Categorias" => ProductosCategoriaTemplate,
            "Chat" => ChatTemplate,
            "Perfil" => PerfilTemplate,
            _ => null
        };
    } 

}