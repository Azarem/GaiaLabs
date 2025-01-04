using Godot;

public partial class PriorityButton : CheckButton
{
    public static PriorityButton Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}
