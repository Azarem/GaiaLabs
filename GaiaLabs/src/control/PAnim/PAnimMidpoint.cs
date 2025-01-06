using Godot;

public partial class PAnimMidpoint : LineEdit
{
    public static PAnimMidpoint Instance;

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}
