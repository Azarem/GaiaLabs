using Godot;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GaiaLabs
{
    public partial class PropertyPanel : GridContainer
    {
        public PropertyPanel()
        {
            Event.Selected += Event_Selected;
            Columns = 2;
        }

        private void Event_Selected(object obj)
        {
            var props = obj.GetType().GetProperties();

            var groups = (from a in
                              (from p in props
                               let d = p.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute
                               let n = d?.Description ?? p.Name
                               orderby n
                               select new { p, n, g = d?.GroupName })
                          group a by a.g into pGrp
                          orderby pGrp.Key
                          select pGrp);

            foreach(var c in GetChildren())
            {
                RemoveChild(c);
                c.QueueFree();
            }

            foreach (var group in groups)
            {
                foreach (var prop in group)
                {
                    AddChild(new Label { Text = prop.n });
                    AddChild(new Label { Text = prop.p.GetValue(obj)?.ToString() });
                }
            }
        }
    }
}
