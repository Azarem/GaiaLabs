using Godot;
using System;

public partial class TilesetWindow : Window
{
    public static TilesetWindow Instance { get; private set; }

    public TilesetWindow()
    {
        Title = "";
        ExtendToTitle = true;

        this.FocusExited += new(() => { this.Hide(); });
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}
