using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser.Types
{
    public interface IEntityWithVertices
    {
        ICollection<Vertex> Vertices { get; }
    }
}
