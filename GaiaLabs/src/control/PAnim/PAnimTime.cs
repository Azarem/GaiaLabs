using Godot;

public partial class PAnimTime : LineEdit
{
    public static PAnimTime Instance;

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}

