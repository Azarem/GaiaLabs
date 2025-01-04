using Godot;

public partial class BlockButton : CheckButton
{
    public static BlockButton Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}
