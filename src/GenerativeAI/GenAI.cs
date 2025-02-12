using GenerativeAI.Core;

namespace GenerativeAI;

public abstract class GenAI
{
    protected IPlatformAdapter Platform { get; }
    protected GenAI(IPlatformAdapter platform)
    {
        this.Platform = platform;
    }
}