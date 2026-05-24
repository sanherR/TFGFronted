namespace Apptech.selectors;

public class MainSelector : DataTemplateSelector
{
    public DataTemplate ProductosTemplate { get; set; }    
    public DataTemplate ProductosCategoriaTemplate { get; set; }
    public DataTemplate ChatTemplate { get; set; }
    public DataTemplate PerfilTemplate { get; set; }   

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        
        System.Diagnostics.Debug.WriteLine($"Selector recibiendo: {item ?? "NULO"}");

        // Convertimos a string por si acaso llega un objeto
        string key = item?.ToString() ?? string.Empty;

        return key switch
        {
            "Productos" => ProductosTemplate,
            "Categorias" => ProductosCategoriaTemplate,
            "Chat" => ChatTemplate,
            "Perfil" => PerfilTemplate,
            
            _ => ProductosTemplate 
        };
    } 
}