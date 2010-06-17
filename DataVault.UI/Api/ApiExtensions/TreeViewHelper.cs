using System.Linq;
using System.Windows.Forms;
using DataVault.Core.Api;

namespace DataVault.UI.Api.ApiExtensions
{
    public static class TreeViewHelper
    {
        public static TreeNode SelectNode(this TreeView tree, VPath vpath)
        {
            return SelectNode(tree.Nodes, vpath);
        }

        public static TreeNode SelectNode(this TreeNode ctx, VPath vpath)
        {
            return vpath == VPath.Empty ? ctx : SelectNode(ctx.Nodes, vpath);
        }

        private static TreeNode SelectNode(this TreeNodeCollection nodes, VPath vpath)
        {
            return nodes.Cast<TreeNode>().Select(n => MatchNodeAndPath(n, vpath)).SingleOrDefault(n => n != null);
        }

        private static TreeNode MatchNodeAndPath(this TreeNode ctx, VPath vpath)
        {
            if (ctx == null || vpath == VPath.Empty) return null;
            if (ctx.Text != vpath.Steps.First()) return null;
            if (vpath.Steps.Count() == 1) return ctx;
            return SelectNode(ctx.Nodes, vpath.Steps.Skip(1).ToArray());
        }
    }
}