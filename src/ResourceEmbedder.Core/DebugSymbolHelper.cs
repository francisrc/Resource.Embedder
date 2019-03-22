using System;

namespace ResourceEmbedder.Core
{
    public static class DebugSymbolHelper
    {
        public static DebugSymbolType FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return DebugSymbolType.None;
            if ("portable".Equals(input, StringComparison.OrdinalIgnoreCase))
                return DebugSymbolType.Portable;
            if ("full".Equals(input, StringComparison.OrdinalIgnoreCase))
                return DebugSymbolType.Full;

            throw new NotSupportedException("Unsupported " + input);
        }
    }
}
