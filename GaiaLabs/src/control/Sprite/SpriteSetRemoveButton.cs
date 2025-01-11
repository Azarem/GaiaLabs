using Godot;


public partial class SpriteSetRemoveButton : Button
{
    public SpriteSetRemoveButton()
    {
        Pressed += SpriteSetRemoveButton_Pressed;
    }

    private void SpriteSetRemoveButton_Pressed()
    {
        SpriteSetList.Instance.RemoveSet();
    }
}

