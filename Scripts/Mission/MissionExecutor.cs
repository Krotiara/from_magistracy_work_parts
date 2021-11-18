using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Mission.Parser;
using CableWalker.Simulator.Modules;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CableWalker.Simulator.Mission
{
    [DisallowMultipleComponent]
    public class MissionExecutor : MonoBehaviour
    {
        private bool running = false;
        private Command[] commands = null;
        private int currentCommandIndex;
        private bool commandExecuting = false;
        private Coroutine executingCoroutine;

        public ParserCommandProvider commandProvider;
        public GameObject staticConsole;
        

        public InformationHolder infoHolder;
       
        public CableWalkerApi CableWalkerApi;

        public GameObject staticSaveButton;
        public GameObject staticLoadButton;
        public GameObject staticStartButton;
        public GameObject staticPauseButton;
        public GameObject staticStopButton;
        public GameObject staticEditButton;
        public GameObject staticBackwardButton;
        public GameObject staticConsoleButtonsParent;
        public GameObject addLineButton;
        public GameObject removeLineButton;

        public TMP_Dropdown modesDropdown;
        public bool isDebugMode = true;
        private int currentConsoleMode = 0; // 0 - debug, 1 -  communication
        public GameObject connectionWindow;

        private CableWalkerCondition startCondition;
        public Stack<CableWalkerCondition> conditions;

        private Spectator spectatorCamera;

        private int lastSuccessCommandIndex = -1;
        private bool isWasEdit;

        public CableWalkerClient cwClient;
        public bool isConnected = false;

        private void Start()
        {
            if (commandProvider == null)
            {
                Debug.Log("Command provider not specified.");
                enabled = false;
            }

            if (CableWalkerApi == null)
            {
                Debug.Log("CableWalkerApi not specified.");
                enabled = false;
            }

            infoHolder.SetActiveObstacleColliders(false);
            conditions = new Stack<CableWalkerCondition>();
            if (CableWalkerApi != null)
            {
                startCondition = CableWalkerApi.Condition;
                conditions.Push(startCondition);
            }
            
            spectatorCamera = GameObject.FindGameObjectWithTag("SpectatorCamera").GetComponent<Spectator>();
          
            commands = null;

        }

        #region ConsoleButtons
        public void RunStatic()
        {
            commandProvider.SetCommandInputInteractable(false);
            if (isWasEdit)
            {
                var mergeList = new List<Command>();
                foreach (var command in commands)
                    if (command.Status == ConsoleCommandStatus.Success)
                        mergeList.Add(command);
                List<Command> mergeCommands = null;
                try
                {
                    mergeCommands = commandProvider.GetEditCommands(lastSuccessCommandIndex);
                }
                catch(ParseCommandFailedException)
                {
                    EditStatic();
                    return;
                }
                
                mergeList.AddRange(commandProvider.GetEditCommands(lastSuccessCommandIndex));
                commands = mergeList.ToArray();
                currentCommandIndex = lastSuccessCommandIndex + 1;
                isWasEdit = false;
                
            }

            running = true;
            staticStartButton.SetActive(false);
            staticPauseButton.SetActive(true);
            staticEditButton.GetComponent<Button>().interactable = false;
            addLineButton.GetComponent<Button>().interactable = false;
            removeLineButton.GetComponent<Button>().interactable = false;
            staticLoadButton.GetComponent<Button>().interactable = false;
            staticSaveButton.GetComponent<Button>().interactable = false;



        }

        public void PauseStatic()
        {
            if (executingCoroutine != null)
            {
                StopCoroutine(executingCoroutine);
                commandExecuting = false;
            }

            if (currentCommandIndex < commands.Length)
            {
                commands[currentCommandIndex].Status = ConsoleCommandStatus.WaitingInLine;
                var prevCondition = conditions.Peek();
                prevCondition.SetConditionTo(CableWalkerApi);
            }
            running = false;
            staticStartButton.SetActive(true);
            staticPauseButton.SetActive(false);
            staticStartButton.SetActive(true);
            staticPauseButton.SetActive(false);
            staticEditButton.GetComponent<Button>().interactable = true;
            staticLoadButton.GetComponent<Button>().interactable = false;
            staticSaveButton.GetComponent<Button>().interactable = true;
        }

        public void StopStatic()
        {
            running = false;
            commands = null;
            spectatorCamera.Free();
            startCondition.SetConditionTo(CableWalkerApi);
            conditions.Clear();
            conditions.Push(startCondition);
            if (executingCoroutine != null)
            {
                StopCoroutine(executingCoroutine);
                commandExecuting = false;
            }
            staticStartButton.SetActive(true);
            staticPauseButton.SetActive(false);
            staticEditButton.GetComponent<Button>().interactable = false;
            addLineButton.GetComponent<Button>().interactable = true;
            removeLineButton.GetComponent<Button>().interactable = true;
            staticLoadButton.GetComponent<Button>().interactable = true;
            staticSaveButton.GetComponent<Button>().interactable = true;
            commandProvider.SetCommandInputInteractable(-1, true);
            commandProvider.SetDefaultCommandsStatusAndColor();
        }
      
        public void EditStatic()
        {
            if (lastSuccessCommandIndex == -1)
                return;
            commandProvider.SetCommandInputInteractable(lastSuccessCommandIndex, true);
            isWasEdit = true;
            addLineButton.GetComponent<Button>().interactable = true;
            removeLineButton.GetComponent<Button>().interactable = true;
            while (commands.Last().Status != ConsoleCommandStatus.Success)
            {
                Array.Resize(ref commands, commands.Length - 1);
                
            }
            currentCommandIndex = lastSuccessCommandIndex + 1;
        }

        /// <summary>
        /// Позволяет откатить выполненную команду. В консоли перед этим не должно быть невыполненных команд
        /// </summary>
        public void Backward()
        {
            if(conditions.Count > 1)
            {
                CableWalkerCondition currentCondition = conditions.Pop();
                CableWalkerCondition prevCondition = conditions.Peek();
                prevCondition.SetConditionTo(CableWalkerApi);
                lastSuccessCommandIndex--;
                Array.Resize(ref commands, commands.Length - 1);
                currentCommandIndex = lastSuccessCommandIndex + 1;
                commandProvider.RemoveLineBackward();
            }
        }
        #endregion

        

        #region CableWalker messages handlers

        public void RefreshCommandStatus(int commandIndex, ConsoleCommandStatus status, string inputMessage)
        {
           
            //пришел ответ от подкоманды текущей команды.
            if(CurrentCommand.IsCompositeCommand && !CurrentCommand.IsSubCommandsExecuted)
            {
                CurrentCommand.RefreshCurrentSubCommandStatus(status, inputMessage);
                return;
            }

            CurrentCommand.RefreshCommandStatus(commandIndex, status, inputMessage);
            
        }

        public void RefreshConnection(bool isConnect)
        {
            isConnected = isConnect;
        }
        #endregion

        #region CommandsStatusesDisplay
        private void Update()
        {

            if (currentConsoleMode != modesDropdown.value)
            {
                isDebugMode = modesDropdown.value == 0;
                currentConsoleMode = modesDropdown.value;
                connectionWindow.gameObject.SetActive(!isDebugMode);
                StopStatic();
            }
            staticBackwardButton.GetComponent<Button>().interactable = isDebugMode && isWasEdit && commandProvider.IsLastLineSuccess();

            if (commands == null)
                return;
            //Это излишне и не оптимально, но пусть будет пока так
            if (currentCommandIndex < commands.Length)
                commandProvider.RefreshCommandStatus(currentCommandIndex, commands[currentCommandIndex]);
            
            if (currentCommandIndex !=0)
                commandProvider.RefreshCommandStatus(currentCommandIndex-1, commands[currentCommandIndex-1]); //Дальше одного захода за длину массива не зайдет по задумке
        }
        #endregion

        #region ProcessConsole

        public Command CurrentCommand { get; set; }
        public Command CurrentSubCommand { get; set; }


       

        private void FixedUpdate()
        {
            if (commandExecuting || !running)
                return;
  
             ProcessStaticConsole();
        }

        private void ProcessStaticConsole()
        {

            if (commands == null)
            {
                currentCommandIndex = 0;
                try
                {
                    commands = commandProvider.GetCommands().ToArray();
                }
                catch(ParseCommandFailedException)
                {
                    StopStatic();
                    return;
                }
                commandProvider.InstantiateCommandsStatus(commands);
                return;
            }

            if (currentCommandIndex > commands.Length-1)
            {
                PauseStatic();
                return;
            }

            var command = commands[currentCommandIndex];
            try
            {
                executingCoroutine = StartCoroutine(ExecuteCommand(command));
            }
            catch(CommandExecutingException e)
            {
                CurrentCommand.Status = ConsoleCommandStatus.Crash;
                var message = e.Message;
                PauseStatic();
            }
        }

        /// <summary>
        /// Должно гарантироваться, что сюда можно входить только при разрешении коптера на исполнение следующей команды.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private IEnumerator ExecuteCommand(Command command)
        {
            CurrentCommand = command;
            commandExecuting = true;
            if (isDebugMode)
            {
                yield return command.DebugExecute(CableWalkerApi);
                conditions.Push(CableWalkerApi.Condition);
            }
            else
            {
                if (isConnected)
                { 
                   yield return command.Execute(CableWalkerApi, cwClient);
                }
            }
            //пусть делается в своих командах
           // command.SetParams(CableWalkerApi); //Устанавливает в CableWalkerApi относящиеся к команде параметры. Выполняется только после завершения команды
            commandExecuting = false;
            executingCoroutine = null;
            lastSuccessCommandIndex = currentCommandIndex;
            currentCommandIndex++;
        }
        #endregion


    }
}
