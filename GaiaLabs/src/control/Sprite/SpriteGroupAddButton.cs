using Godot;


public partial class SpriteGroupAddButton : Button
{
    public SpriteGroupAddButton()
    {
        Pressed += SpriteGroupAddButton_Pressed;
    }

    private void SpriteGroupAddButton_Pressed()
    {
        SpriteGroupList.Instance.AddGroup();
    }
}

