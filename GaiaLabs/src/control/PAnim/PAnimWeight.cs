using Godot;

public partial class PAnimWeight : LineEdit
{
    public static PAnimWeight Instance;

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}

