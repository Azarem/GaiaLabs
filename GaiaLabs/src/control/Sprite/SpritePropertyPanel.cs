using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaiaLabs.src.control.Sprite
{
    public partial class SpritePropertyPanel : PropertyPanel
    {
        public static SpritePropertyPanel Instance { get; private set; }

        public override void _EnterTree()
        {
            Instance = this;
            base._EnterTree();
        }
    }
}
