using System.Collections.Generic;
using Common;

namespace Toolkit
{
    public interface IMapToolkit
    {
        ToolkitType ToolkitType { get; }
        DistanceResult DistanceMatrix(Place start, List<Place> destinations);
    }
}