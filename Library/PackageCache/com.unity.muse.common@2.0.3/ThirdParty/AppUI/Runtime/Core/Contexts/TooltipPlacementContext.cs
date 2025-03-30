using System;
using Unity.Muse.AppUI.UI;

namespace Unity.AppUI.Core
{
    internal record TooltipPlacementContext(PopoverPlacement placement) : IContext
    {
        public PopoverPlacement placement { get; } = placement;
    }
}
