using Godot;

public partial class VMirrorButton : CheckButton
{
    public static VMirrorButton Instance { get; private set; }

    public VMirrorButton()
    {
        Toggled += VMirrorButton_Toggled;
    }


    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    private void VMirrorButton_Toggled(bool toggledOn)
    {
    }
}

