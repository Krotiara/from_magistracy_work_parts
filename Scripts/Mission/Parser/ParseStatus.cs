namespace CableWalker.Simulator.Mission.Parser
{
    public enum ParseStatus
    {
        Correct,
        Empty,
        UnknownCommand,
        UnexpectedLineEnd,
        MissingOpenBracket,
        MissingCloseBracket
    }
}
