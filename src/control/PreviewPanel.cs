using Godot;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GaiaLabs.src.control
{
    public partial class PreviewPanel : PanelContainer
    {
        private ImageTexture _texture;

        public PreviewPanel()
        {
            Event.Selected += Event_Selected;
        }

        private void Event_Selected(object obj)
        {
            if (_texture != null)
            {
                _texture.Free();
                _texture.Dispose();
                _texture = null;
            }

            if (obj is MetaEntry meta && meta.Reference != null)
                obj = meta.Reference.Resource;

            if(obj is TextureEntry tex)
                _texture = ImageTexture.CreateFromImage(tex.GetImage());

            QueueRedraw();
        }

        public override void _Draw()
        {
            if (_texture != null)
                DrawTextureRect(_texture, new(0, 0, Size.X, Size.Y), false);
        }
    }
}
