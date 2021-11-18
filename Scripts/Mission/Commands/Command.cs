using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;

namespace CableWalker.Simulator.Mission.Commands
{
    /// <summary>
    /// Абстрактный класс команды для платформы.
    /// </summary>
    public abstract class Command
    {

        private int waitingReceivedCount;

        public bool IsCommandResultReceived { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public ConsoleCommandStatus Status { get; set; }

        public string ReceiveExecutingMessage { get; set; }

        public List<Command> SubCommands { get; set; }

        public int CurSubCommandIndex { get; set; }

        public bool IsCompositeCommand => SubCommands != null && SubCommands.Count > 0;

        public bool IsSubCommandsExecuted => IsCompositeCommand && CurSubCommandIndex == SubCommands.Count - 1;

       
        public void RefreshCurrentSubCommandStatus(ConsoleCommandStatus status, string inputMessage)
        {
            SubCommands[CurSubCommandIndex].IsCommandResultReceived = true;
            SubCommands[CurSubCommandIndex].Status = status;
            SubCommands[CurSubCommandIndex].ReceiveExecutingMessage = inputMessage;
        }

        public void RefreshCommandStatus(int commandIndex, ConsoleCommandStatus status, string inputMessage)
        {
            Status = status;
            ReceiveExecutingMessage = inputMessage;
            IsCommandResultReceived = true;
        }

        public IEnumerator SendMessageWithWaitingAnswer(CableWalkerClient cableWalkerClient)
        {
            cableWalkerClient.Send(GetMessageToSend());
            yield return WaitAnswer();
        }

        /// <summary>
        /// Определите этот метод, чтобы задать логику работы команды в режиме построения миссии без подключения к канатоходу.
        /// </summary>
        /// <param name="cableWalkerApi">Api канатохода</param>
        /// <returns></returns>
        public abstract IEnumerator DebugExecute(CableWalkerApi cableWalkerApi);

        public IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            yield return SendMessageWithWaitingAnswer(cableWalkerClient);
            if (Status != ConsoleCommandStatus.Success && Status != ConsoleCommandStatus.Accepted)
                throw new CommandExecutingException(ReceiveExecutingMessage);
            SetParams(cableWalkerApi);
            yield break;
        }

        public abstract void SetParams(CableWalkerApi cableWalkerApi);


        public abstract Message GetMessageToSend();

        /// <summary>
        /// Ожидание установки флага при получении ответа от сервера
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitAnswer()
        {
            while (!IsCommandResultReceived)
            {
                if (waitingReceivedCount > 100)
                    throw new CommandExecutingException("No answer from server");
                waitingReceivedCount++;
                yield return null;

            }
            waitingReceivedCount = 0;
        }
    }
}
