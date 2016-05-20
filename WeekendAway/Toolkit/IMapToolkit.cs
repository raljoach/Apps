using System.Collections.Generic;
using Common;
using WeekendAway.Common;

namespace WeekendAway.Toolkit
{
    public interface IMapToolkit
    {
        ToolkitType ToolkitType { get; }
        DistanceResult DistanceMatrix(Place start, List<Place> destinations);
    }
}