using Godot;

public partial class PAnimOutput : TextEdit
{
    public static PAnimOutput Instance;

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}

