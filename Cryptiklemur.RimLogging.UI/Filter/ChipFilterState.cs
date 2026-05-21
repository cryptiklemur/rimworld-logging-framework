namespace Cryptiklemur.RimLogging.UI.Filter;

internal sealed class ChipFilterState
{
    // Slots map to LogLevel: [0]=Trace [1]=Debug [2]=Info [3]=Warn [4]=Error [5]=Fatal
    public string SearchText = "";
    public bool[] Levels = { true, true, true, true, true, true };
    public bool DslMode = false;
    public string DslSource = "";
}
