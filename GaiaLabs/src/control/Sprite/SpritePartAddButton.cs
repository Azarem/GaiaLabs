using Godot;


public partial class SpritePartAddButton : Button
{
    public SpritePartAddButton()
    {
        Pressed += SpritePartAddButton_Pressed;
    }

    private void SpritePartAddButton_Pressed()
    {
        SpritePartList.Instance.AddPart();
    }
}

