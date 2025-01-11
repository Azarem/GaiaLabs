using Godot;


public partial class SpriteGroupRemoveButton : Button
{
    public SpriteGroupRemoveButton()
    {
        Pressed += SpriteGroupRemoveButton_Pressed;
    }

    private void SpriteGroupRemoveButton_Pressed()
    {
        SpriteGroupList.Instance.RemoveGroup();
    }
}

