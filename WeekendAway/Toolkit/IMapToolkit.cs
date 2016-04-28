using System.Collections.Generic;
using Common;

namespace Toolkit
{
    public interface IMapToolkit
    {
        DistanceResult DistanceMatrix(Place start, List<Place> others);
    }
}