using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;

namespace CableWalker.Simulator.Mission
{
    public interface CommandProvider
    {
        List<Command> GetCommands();
        void ClearConsole();
    }
}
