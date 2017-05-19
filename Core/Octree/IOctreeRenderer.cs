using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fusee.Tutorial.Core.Octree
{
    /// <summary>
    /// Interface which provides the logic for a render entity to apply.
    /// </summary>
    public interface IOctreeRenderer
    {
        void AddNode(OctreeNode node);
        void RemoveNode(OctreeNode node);
    }
}
