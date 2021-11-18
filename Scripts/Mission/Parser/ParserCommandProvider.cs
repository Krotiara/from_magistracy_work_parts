using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CableWalker.Simulator.Mission.Commands;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CableWalker.Simulator.Mission.Parser
{

    public class ParserCommandProvider : MonoBehaviour, CommandProvider
    {
        private Transform consoleLinesParent;
        public GameObject consoleLinePrefab;
        private int currentLineNumber = 0;

        string numberObjName = "Number";
        string commandObjName = "Command";
        string statusObjName = "Status";

        //For save load
        public string Title = "";
        public string Directory = "";
        public string FileName = "";
        public string Extension = "";
        public bool Multiselect = false;
        private string loadText = "";

        private void Start()
        {
            consoleLinesParent = this.transform;
        }

        public void AddConsoleLine(string command)
        {
            var line = GameObject.Instantiate(consoleLinePrefab);
            line.transform.SetParent(consoleLinesParent);
            
            line.transform.Find(numberObjName).GetComponent<TextMeshProUGUI>().text = currentLineNumber.ToString();
            line.name = currentLineNumber.ToString();
            line.transform.Find(commandObjName).GetComponent<TMP_InputField>().text = command;
            currentLineNumber++;
        }

        public void RemoveLine()
        {
            if (consoleLinesParent.childCount > 0 &&
                consoleLinesParent.GetChild(consoleLinesParent.childCount - 1).Find(statusObjName).GetComponent<TextMeshProUGUI>().text != "Success")
            {

                GameObject.DestroyImmediate(consoleLinesParent.GetChild(consoleLinesParent.childCount - 1).gameObject);
                currentLineNumber--;
            }
        }

        public bool IsLastLineSuccess()
        {
            return consoleLinesParent.childCount > 0 &&
                consoleLinesParent.GetChild(consoleLinesParent.childCount - 1).Find(statusObjName).GetComponent<TextMeshProUGUI>().text == "Success";
        }

        public void RemoveLineBackward()
        {
            if (consoleLinesParent.childCount > 0)
            {
                GameObject.DestroyImmediate(consoleLinesParent.GetChild(consoleLinesParent.childCount - 1).gameObject);
                currentLineNumber--;
            }
        }

        public void SaveScript()
        {
            var path = StandaloneFileBrowser.SaveFilePanel(Title, Directory, FileName, Extension);
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, GetConsoleTextToSave());
            }
        }

        private string GetConsoleTextToSave()
        {
            string result = "";
            foreach (Transform consoleLine in consoleLinesParent)
            {
                result += consoleLine.Find(commandObjName).GetComponent<TMP_InputField>().text;
                result += "\r\n";
            }
            return result;
        }

        public void LoadScript()
        {
            loadText = "";
            ClearConsole();
            var paths = StandaloneFileBrowser.OpenFilePanel(Title, Directory, Extension, Multiselect);

            if (!string.IsNullOrEmpty(paths[0]))
            {
                using (var reader = new StreamReader(paths[0], Encoding.Default))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;
                        AddConsoleLine(line);
                    }
                }
            }

        }

        public Transform GetConsoleLine(int lineNumber)
        {
            return consoleLinesParent.Find(lineNumber.ToString());
        }

        //public List<Message> GetMessages()
        //{
        //    try
        //    {
        //        return ParseMessages();
        //    }
        //    catch(MissingMethodException e)
        //    {
        //        Debug.LogError($"Incorrect arguments in some commands");
        //        return null;
        //    }
        //    catch(NotSupportedCommandForMessageException e)
        //    {
        //        Debug.LogError(e);
        //        return null;
        //    }
        //    catch(ParseCommandFailedException e)
        //    {
        //        Debug.LogError(e);
        //        return null;
        //    }
        //}

        //private List<Message> ParseMessages()
        //{
        //    List<Message> messages = new List<Message>();
        //    using (var reader = new StringReader(inputField.text))
        //    {
        //        int currentLineIndex = 0;
        //        while (true)
        //        {
        //            var line = reader.ReadLine();

        //            if (line == null)
        //                break;

        //            var result = CommandParser.ParseLine(line);
        //            if (result.Status != ParseStatus.Correct)
        //            {
        //                throw new ParseCommandFailedException(line);
        //            }

        //            Command command = null;

        //            command = Activator.CreateInstance(result.Command, result.Args) as Command;
        //            messages.Add(command.GetMessageToSend());
        //            currentLineIndex++;

        //        }

        //    }
        //    return messages;

        //}

        //public bool IsInputCommandsCorrect()
        //{
        //    bool correct = true;
        //    using (var reader = new StringReader(inputField.text))
        //    {
        //        int currentLineIndex = 0;
        //        while (true)
        //        {
        //            var line = reader.ReadLine();

        //            if (line == null)
        //                break;

        //            var result = CommandParser.ParseLine(line);
        //            if (result.Status != ParseStatus.Correct)
        //            {
        //                Debug.LogError($"{result.Status}: строка {currentLineIndex}, символ {result.BreakChar}."); //Переделать под возврат сообщения не в консоль
        //                correct = false;
        //                break;
        //            }

        //            Command command = null;
        //            try
        //            {
        //                command = Activator.CreateInstance(result.Command, result.Args) as Command;
        //            }
        //            catch (MissingMethodException)
        //            {
        //                Debug.LogError($"Incorrect arguments for {command.GetType()}"); ////Переделать под возврат сообщения не в консоль
        //                correct = false;
        //                break;
        //            }

        //            currentLineIndex++;

        //        }

        //    }
        //    return correct;
        //}

        public List<Command> GetEditCommands(int lastSuccessCommandIndex)
        {
            List<Command> resultCommands = new List<Command>();
            TextMeshProUGUI commandNumber;
            TMP_InputField commandInput;
            TextMeshProUGUI commandStatus;
            foreach (Transform consoleLine in consoleLinesParent)
            {
                commandNumber = consoleLine.Find(numberObjName).GetComponent<TextMeshProUGUI>();
                commandInput = consoleLine.Find(commandObjName).GetComponent<TMP_InputField>();
                commandStatus = consoleLine.Find(statusObjName).GetComponent<TextMeshProUGUI>();
                if (int.Parse(commandNumber.text) <= lastSuccessCommandIndex)
                    continue;
                CommandParser.ParseResult parseResult = CommandParser.ParseLine(commandInput.text);
                if (parseResult.Status != ParseStatus.Correct)
                {
                    commandStatus.text = $"{parseResult.Status}.";
                    throw new ParseCommandFailedException();
                }
                Command command = null;
                try
                {
                    command = Activator.CreateInstance(parseResult.Command, parseResult.Args) as Command;
                    command.Number = int.Parse(commandNumber.text);
                }
                catch (MissingMethodException)
                {
                    commandStatus.text = $"Incorrect arguments";
                    throw new ParseCommandFailedException();
                }
                RefreshCommandStatus(consoleLine, command);
                resultCommands.Add(command);

            }
            return resultCommands;
        }

        public void SetCommandInputInteractable(int lastSuccessCommandIndex, bool isInteractable)
        {
            for(int i = 0; i < consoleLinesParent.childCount; i++)
            {
                if (i <= lastSuccessCommandIndex)
                    continue;
                consoleLinesParent.GetChild(i).Find(commandObjName).GetComponent<TMP_InputField>().interactable = isInteractable;
            }
        }

        public void SetCommandInputInteractable(bool isInteractable)
        {
            SetCommandInputInteractable(-1, isInteractable);
        }


        public List<Command> GetCommands()
        {
            List<Command> resultCommands = new List<Command>();

            
            TextMeshProUGUI commandNumber;
            TMP_InputField commandInput;
            TextMeshProUGUI commandStatus;
            foreach (Transform consoleLine in consoleLinesParent)
            {
                commandNumber = consoleLine.Find(numberObjName).GetComponent<TextMeshProUGUI>();
                commandInput = consoleLine.Find(commandObjName).GetComponent<TMP_InputField>();
                commandStatus = consoleLine.Find(statusObjName).GetComponent<TextMeshProUGUI>();
                CommandParser.ParseResult parseResult = CommandParser.ParseLine(commandInput.text);
                if (parseResult.Status != ParseStatus.Correct)
                {
                    commandStatus.text = $"{parseResult.Status}.";
                    throw new ParseCommandFailedException();
                }
                Command command = null;
                try
                {
                    command = Activator.CreateInstance(parseResult.Command, parseResult.Args) as Command;
                    command.Number = int.Parse(commandNumber.text);
                    command.Status = ConsoleCommandStatus.WaitingInLine;
                }
                catch (MissingMethodException)
                {
                    commandStatus.text = $"Incorrect arguments";
                    throw new ParseCommandFailedException();
                }
               
                resultCommands.Add(command);
               

            }
            return resultCommands;
        }

        public void InstantiateCommandsStatus(Command[] commands)
        {
            for(int i =0; i < commands.Length;i++)
            {
                var status = commands[i].Status;
                var consoleLine = consoleLinesParent.GetChild(i);
                consoleLine.GetComponent<Image>().color = GetColorForCommandStatus(status);
                consoleLine.Find(statusObjName).GetComponent<TextMeshProUGUI>().text = status.ToString();
            }

        }

        /// <summary>
        /// В текущей реализации и имена и индексы имеют одинаковый вид от 0 до N
        /// </summary>
        /// <param name="commandNumber"></param>
        public void RefreshCommandStatus(int commandNumber, Command command)
        {
            if (commandNumber < consoleLinesParent.childCount)
            {
                var consoleLine = consoleLinesParent.GetChild(commandNumber);
                RefreshCommandStatus(consoleLine, command);
            }
        }

        public void RefreshCommandStatus(Transform consoleLine, Command command)
        {
            var status = command.Status;
            consoleLine.GetComponent<Image>().color = GetColorForCommandStatus(status);
            consoleLine.Find(statusObjName).GetComponent<TextMeshProUGUI>().text = status.ToString();
        }

        public void SetDefaultCommandsStatusAndColor()
        {
            foreach(Transform line in consoleLinesParent)
            {
                line.Find(statusObjName).GetComponent<TextMeshProUGUI>().text = "";
                line.GetComponent<Image>().color = Color.white;
            }
        }

        public Color GetColorForCommandStatus(ConsoleCommandStatus commandStatus)
        {
            
            Color color;
            if (commandStatus == ConsoleCommandStatus.Running)
                color = Color.yellow;
            else if (commandStatus == ConsoleCommandStatus.Success)
                color = Color.green;
            else if (commandStatus == ConsoleCommandStatus.WaitingInLine)
                color = Color.white;
            else
                color = Color.red;
            return color;
        }


        public void ClearConsole()
        {
            while (consoleLinesParent.childCount > 0)
                RemoveLine();
            currentLineNumber = 0;
        }
    }
}
