namespace Apptech.Messages;


using CommunityToolkit.Mvvm.Messaging.Messages;

using Apptech.views; 

public class NuevoProductoMessage : ValueChangedMessage<ProductosPage.ItemPop>
{
    public NuevoProductoMessage(ProductosPage.ItemPop producto) : base(producto) { }
}