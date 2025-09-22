using Godot;

public partial class MainMenu : CanvasLayer
{
    private void _OnCreatePressed()
    {
        var network = GetNode<Network>("/root/Network");
        
        network.serverInfo.Name = GetNode<LineEdit>("Control/BoxContainer/PanelHost/txtServerName").Text;
        network.serverInfo.MaxPlayers = (int)GetNode<SpinBox>("Control/BoxContainer/PanelHost/txtMaxPlayers").Value - 1;
        network.serverInfo.UsedPort = int.Parse(GetNode<LineEdit>("Control/BoxContainer/PanelHost/txtServerPort").Text);
        
        network.CreateServer();
    }

    private void _OnJoinPressed()
    {
        var port = int.Parse(GetNode<LineEdit>("Control/BoxContainer/PanelJoin/txtJoinPort").Text);
        var ip = GetNode<LineEdit>("Control/BoxContainer/PanelJoin/txtJoinIP").Text;
        
        var network = GetNode<Network>("/root/Network");
        network.JoinServer(ip, port);
    }

    private void _OnTxtPlayerNameTextChanged(string newText)
    {
        var client = GetNode<Client>("/root/Client");
        client.clientInfo.Name = newText;
    }
}