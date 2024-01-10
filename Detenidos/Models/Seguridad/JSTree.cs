using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class JsTreeModel
    {
        public int id { get; set; }
        public string text { get; set; }
        public JsTreeAttribute state { get; set; }
        public List<JsTreeModel> children { get; set; }
        public JsTreeAAtribute a_attr { get; set; }
    }

    public class JsTreeModelMenu
    {
        public string id { get; set; }
        public string text { get; set; }
        public JsTreeAttribute state { get; set; }
        public List<JsTreeModelMenu> children { get; set; }
        public JsTreeAAtribute a_attr { get; set; }
    }
    public class JsTreeAttribute
    {
        public bool opened { get; set; }
        public bool selected { get; set; }
    }
    public class JsTreeAAtribute
    {
        public string href { get; set; }
        public string style { get; set; }
    }
}
