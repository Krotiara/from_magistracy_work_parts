using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CableWalker.AgentModel
{
    public class AgentModelController : MonoBehaviour
    {
        public InformationHolder informationHolder;
        public AgentPanelScriptV2 agentPanelScript;
        private AgentModelBuilder agentModelBuilder;
        private GameObject agentsRoot;
        public Dictionary<string, Agent> Agents { get; set; } = new Dictionary<string, Agent>();
        public List<string> SpansAgentsNumbers => Agents.Values.Where(a => a.GetType().Equals(typeof(AgentSpan))).Select(a=>a.Number).ToList();
        public string EnvironmentAgentName => "Environment";
        public string PowerLineAgentName => "Power line";

        private Coroutine c;
        public MessagesConsoleScript messagesConsoleScript;

        // Start is called before the first frame update
        void Start()
        {
            Invoke("Instantiate", 0.005f);
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.L))
        //        RefreshDefects();
        //}

        public void SendMessageOutside(string to, Message message)
        {
            Agents[to].ReceiveMessage(message, Agents[PowerLineAgentName]);
        }

        private void RefreshPanel()
        {
            //LayoutRebuilder.ForceRebuildLayoutImmediate(agentPanelScript.TopContent.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
            //foreach (Transform a in agentPanelScript.lines)
            //{
            //    AgentLineScript aScript = a.GetComponent<AgentLineScript>();
            //    bool activeFlag = aScript.Connections.gameObject.activeInHierarchy;
            //    aScript.Connections.gameObject.SetActive(!activeFlag);
            //    aScript.Connections.gameObject.SetActive(activeFlag);
            //}          
            //agentPanelScript.TopContent.gameObject.SetActive(false);
            //agentPanelScript.TopContent.gameObject.SetActive(true);
            
        }

        void Instantiate()
        {
            agentModelBuilder = new AgentModelBuilder();
            InstantiateAgentModel();
            agentPanelScript.AddAgents(Agents["Power line"], Agents["Environment"], informationHolder);
            //c = StartCoroutine(ExecuteAfterTime());
            foreach (Transform b in agentPanelScript.lines)
            {
                b.GetComponent<Button>().onClick.AddListener(delegate { RefreshPanel(); });
            }

            List<string> consoleToOptions = new List<string>();
            consoleToOptions.Add(EnvironmentAgentName);
            consoleToOptions.AddRange(SpansAgentsNumbers);
            messagesConsoleScript.FillDropdowns(consoleToOptions);

            ProcessAfterBuild();
        }


        void ProcessAfterBuild()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (Span span in informationHolder.GetList<Span>())
            {
                //var t = new Thread(() => RecalculateDimensionsOutside((AgentSpan)Agents[span.Number]));
                //t.Start();
                RecalculateDimensionsOutside((AgentSpan)Agents[span.Number]);
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log("Рекалькуляция расстояний: " + stopwatch.ElapsedMilliseconds * 0.001);
        }

        void RecalculateDimensionsOutside(AgentSpan agentSpan)
        {
            agentSpan.ProcessMessage(new Message("Outside", agentSpan.Number, MessagesTexts.updateCableDimensions, new List<string>().ToArray()),null);
        }
        //public IEnumerator ExecuteAfterTime()
        //{
        //    while(true)
        //    {
        //        agentPanelScript.TopContent.gameObject.SetActive(false);
        //        yield return new WaitForSeconds(0.5f);
        //        UnityEngine.Debug.Log("Cor");
        //        agentPanelScript.TopContent.gameObject.SetActive(true);

        //    }

        //    // yield return new WaitForEndOfFrame();
        //    //AgentPanelContent.SetActive(false);
        //    //AgentPanelContent.SetActive(true);
        //    // Code to execute after the delay
        //}

        public void InstantiateAgentModel()
        {
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            agentsRoot = new GameObject("Agents");
            var agents = agentModelBuilder.Build(informationHolder);
            if (agents == null)
                throw new System.Exception("Error in Agent model build");
            Agents = agents;
            stopwatch.Stop();
            UnityEngine.Debug.Log("Поcтроение агентной модели: " + stopwatch.ElapsedMilliseconds * 0.001);
            //stopWatch.Stop();
            //UnityEngine.Debug.Log("AgentsBuild = " + stopWatch.Elapsed.Milliseconds);
        }

        public void RefreshDefects()
        {
            informationHolder.RefreshDefects();
            foreach(SpanDefect spanDefect in informationHolder.GetList<SpanDefect>())
            {
                if (spanDefect.TypeFromDataBase == "1")
                {
                    float actualGladeWidthValue = 20; //заглушка
                    try
                    {
                        actualGladeWidthValue = float.Parse(spanDefect.ArgsFromFiles[0]);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.Log(e);
                    }
                    SendMessageOutside(spanDefect.Model.Number,
                            new Message("OutSide", spanDefect.Model.Number,
                            MessagesTexts.updateGladeWidth, new string[1] { actualGladeWidthValue.ToString() }));
                   
                }
            }

            foreach(CableDefect cableDefect in informationHolder.GetList<CableDefect>())
            {
                Span span = ((Cable)cableDefect.Model).Span;
                if (cableDefect.TypeFromDataBase == "11" || cableDefect.TypeFromDataBase == "35")
                {
                    //{breakRodInClamp, new List<string>(){"Phase","Number","Distance from tower 1 [m.]", "Rod type [0 - Aluminium, 1 - Steel]" } },
                    int numberOfAluminiumRods = int.Parse(cableDefect.ArgsFromFiles[0]);
                    int numberOfSteelRods = int.Parse(cableDefect.ArgsFromFiles[1]);
                    bool isInClamps = Convert.ToBoolean(Convert.ToInt16(cableDefect.ArgsFromFiles[2]));
                    float distance = cableDefect.DistanceFromTower1;
                    string[] args = new string[4] { ((Cable)cableDefect.Model).Phase, distance.ToString(), numberOfAluminiumRods.ToString(), numberOfSteelRods.ToString() };
                    //string[] aluminiumArgs = new string[4] { ((Cable)cableDefect.Model).Phase, cableDefect.Number, distance.ToString(), "0" };
                    //string[] steelArgs = new string[4] { ((Cable)cableDefect.Model).Phase, cableDefect.Number, distance.ToString(), "1" };
                    if (isInClamps)
                        SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.breakRodInClamp, args));
                    else
                        SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.breakRodOutClamp, args));
                    //for (int i=0; i < numberOfAluminiumRods;i++)
                    //{
                    //    aluminiumArgs[1] = i.ToString();
                    //    if(isInClamps)
                    //        SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.breakRodInClamp, aluminiumArgs));
                    //    else
                    //        SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.breakRodOutClamp, aluminiumArgs));
                    //}
                    //for (int i = 0; i < numberOfSteelRods; i++)
                    //{
                    //    steelArgs[1] = i.ToString();
                    //    if (isInClamps)
                    //        SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.breakRodInClamp, steelArgs));
                    //    else
                    //        SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.breakRodOutClamp, steelArgs));
                    //}
                    //Обрыв проволок
                }
                //if (cableDefect.TypeFromDataBase == 15)
                //{
                //    //Повреждение у зажимов
                //}
                if (cableDefect.TypeFromDataBase == "14")
                {
                    string[] args = new string[1] { ((Cable)cableDefect.Model).Phase};
                    //Коррозия
                    SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.cableCorrosion, args));
                }
                if (cableDefect.TypeFromDataBase == "18" || cableDefect.TypeFromDataBase == "0052")
                {
                    int number = int.Parse(cableDefect.ArgsFromFiles[0]);
                    string[] args = new string[2] { ((Cable)cableDefect.Model).Phase, number.ToString() };
                    SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.vibrationDamperAbsence, args));
                    //Отсутсвие гасителя вибрации
                }
                if (cableDefect.TypeFromDataBase == "32")
                {
                    int number = int.Parse(cableDefect.ArgsFromFiles[0]);
                    string[] args = new string[2] { ((Cable)cableDefect.Model).Phase, number.ToString() };
                    SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.vibrationDamperOffset, args));
                    //Смещение гасителя вибрации
                }
                if (cableDefect.TypeFromDataBase == "0053")
                {
                    int number = int.Parse(cableDefect.ArgsFromFiles[0]);
                    string[] args = new string[2] { ((Cable)cableDefect.Model).Phase, number.ToString() };
                    SendMessageOutside(span.Number, new Message("OutSide", span.Number, MessagesTexts.vibrationDamperWeightsAbsence, args));
                    //Отсутсвие грузов гасителя
                }

            }
        }



        

     
    }
}
