using Godot;


public partial class SpritePartRemoveButton : Button
{
    public SpritePartRemoveButton()
    {
        Pressed += SpritePartRemoveButton_Pressed;
    }

    private void SpritePartRemoveButton_Pressed()
    {
        SpritePartList.Instance.RemovePart();
    }
}

