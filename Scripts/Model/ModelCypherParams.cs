using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Model
{ 
    public static class ModelCypherParams
    {
        private static readonly Dictionary<string, CableKind> cablesDict = new Dictionary<string, CableKind>()
        {

            {"АС-150/24", new CableKind("АС-150/24",false,0.0171f,173.1f,0.00334f,599,8250,0.0000192f, -60,5, 90,9f, 13.5f, 0.00019798f,0.0042f,450).SetComponentsParams(26,7,2.7f,2.1f) },
            {"С-50", new CableKind("С-50",true ,0.009f,166.8f,0.00334f,599,7700,0.0000198f,-35,5,90,25,12.2f, 0, 0,0).SetComponentsParams(0,19,0,1.82f)},
            {"АС-150/19", new CableKind("АС-150/19",false,0.0168f,166.59f,0.00334f,554,7700,0.0000198f,-60,5,90,8.4f,12.6f, 0.00019919f,0.0042f,450).SetComponentsParams(24,7,2.8f,1.85f) },
            {"АС-70/11", new CableKind("АС-70/11",false,0.0168f,166.59f,0.00334f,554,7700,0.0000198f,-60,5,90,8.4f,12.6f, 0.00019919f,0.0042f,450).SetComponentsParams(6,1,3.8f,3.8f) },
            {"АС-70", new CableKind("АС-70",false,0.0168f,166.59f,0.00334f,554,7700,0.0000198f,-60,5,90,8.4f,12.6f, 0.00019919f,0.0042f,450).SetComponentsParams(6,1,3.8f,3.8f) },
            {"АС-120/19", new CableKind("АС-120/19",false,0.0152f,136.43f,0.00334f,471,8250,0.0000192f,-70,5,90,9f,13.5f, 0.000249f,0.0042f,390).SetComponentsParams(26,7,2.4f,1.85f) },
            {"АС-95/16", new CableKind("АС-95/16",false,0.0135f,111.33f,0.00334f,385,8250,0.0000192f,-60,5,40,9,12f, 0.00030599f,0.0042f,330).SetComponentsParams(6,1,4.5f,4.5f) },
            {"АС-300", new CableKind("АС-300",false,0.0221f,340.19f,0.00334f,1132,7700,0.0000198f,-60,5,90,8.4f,12.6f, 0.0001017f, 0.0042f,710).SetComponentsParams(24,7,4f,2.65f) },
            {"С-70", new CableKind("С-70",true,0.009f,166.8f,0.00334f,599,7700,0.0000198f,-35,5,90,25,12.2f, 0, 0,0).SetComponentsParams(0,19,0,1.82f) },
            {"ПС-35", new CableKind("ПС-35",true,0.009f,166.8f,0.00334f,599,7700,0.0000198f,-35,5,90,25,12.2f, 0, 0,0).SetComponentsParams(0,7,0,1.3f) },
            {"2X795_MCM_DRAKE", new CableKind("2X795_MCM_DRAKE",false,0.0795f,173.1f,0.00334f,400,8250,0.0000192f, -60,5, 90,9f, 13.5f, 0.00019798f,0.0042f,450) }, // надо найти инфу
            {"9.1-Г-В-Ж-1372", new CableKind("9.1-Г-В-Ж-1372",false,0.0171f,173.1f,0.00334f,599,8250,0.0000192f, -60,5, 90,9f, 13.5f, 0.00019798f,0.0042f,450) }
        };

        private static readonly Dictionary<string, TowerKind> towersDict = new Dictionary<string, TowerKind>()
        {
            //string cypher, string typicalProjectCypher, string allowedCableKinds, string chainNumber, string voltage, string material
            {"ПД110-3", new TowerKind("ПД110-3","Типовой проект 3.407-69, Том 2","АС-70, АС-95, АС-120",1,110)  },
            {"ПУБ110-1", new TowerKind("ПУБ110-1.1т","Типовой проект 3.407.1-164","",1,110)  },
            {"УД110-3", new TowerKind("УД110-3","Типовой проект 3.407-69, Том 3","АС-50, АС-70, АС-95, АС-120, АС-150, АС-185",1,110)  },
            {"УД110-1", new TowerKind("УД110-1","Типовой проект 3.407-69, Том 3","АС-50, АС-70, АС-95, АС-120, АС-150, АС-185",1,110)  },
            {"ПД110-5", new TowerKind("ПД110-5","Типовой проект 3.407-69, Том 2","АС-70, АС-95, АС-120",1,110)  },
            {"УД110-5", new TowerKind("УД110-5","Типовой проект 3.407-69, Том 3","АС-50, АС-70, АС-95, АС-120, АС-150, АС-185",1,110)   },
            {"У110-1", new TowerKind("У110-1","Типовой проект 3.407-68.73, Том 10","",1,110)  },
            {"У110-2", new TowerKind("У110-2","Типовой проект 3.407-68.73, Том 10","",1,110)  },
            {"П110-6", new TowerKind("П110-6","Типовой проект 3.407-68/73, Том 9","АС 70/11 - АС 240/32",2,110)  },
            {"У110-8", new TowerKind("У110-8","Типовой проект 3.407-68.73, Том 10","",1,110)  },
            {"У110-4", new TowerKind("У110-4","Типовой проект 3.407-68.73, Том 10","",1,110)  },
            {"У110-1+5", new TowerKind("У110-1+5","Типовой проект 3.407-68.73, Том 10","",1,110)  },
            {"ПБ110-1", new TowerKind("ПБ110-1","Серия 3.407-124","АС95/16, АС150/24",1,110)  },
            {"ПБ110-2", new TowerKind("ПБ110-2","Серия 3.407-124","АС95/16, АС150/24",1,110)  },
            {"УД110-7", new TowerKind("УД110-7","Типовой проект 3.407-69, Том 3","АС-50, АС-70, АС-95, АС-120, АС-150, АС-185",1,110)  },
            {"УБ110-1", new TowerKind("УБ110-1","Серия 3.407-124","АС95/16, АС150/24, АС240/32",1,110)  },
            {"TO17", new TowerKind("TO17","Серия 3.407-124","АС95/16, АС150/24, АС240/32",1,110)  }, // надо найти инфу
            {"TO11", new TowerKind("TO11","Серия 3.407-124","АС95/16, АС150/24, АС240/32",1,110)  },// надо найти инфу
            {"TO13", new TowerKind("TO13","Серия 3.407-124","АС95/16, АС150/24, АС240/32",1,110)  },// надо найти инфу
            {"TO18", new TowerKind("TO18","Серия 3.407-124","АС95/16, АС150/24, АС240/32",1,110)  }// надо найти инфу
        };

        private static readonly Dictionary<string, SuspensionKind> suspensionsDict = new Dictionary<string, SuspensionKind>()
        {
            {"ЭС-10578", new SuspensionKind("ЭС-10578","ИЗОЛИРУЮЩИЕ ПОДВЕСКИ ВЛ 35-750 кВ, Альбом 2.", "Узел крепления - КГП-16-3;\nСерьга специальная - СРС-7-16;\nИзолятор;\nУшко однолапчатое - У1К-7-16;\nЗажим поддерживающий-ПГН-3-5.") },
            {"ЭС-10589", new SuspensionKind("ЭС-10589","ИЗОЛИРУЮЩИЕ ПОДВЕСКИ ВЛ 35-750 кВ, Альбом 2.", "Скоба - СК-12-1А;\nСерьга - СР-12-16;\nИзолятор;\nУшко двухлапчатое - У2-12-16;\nЗвено промежуточное-ПР-12-6;\nЗажим натяжной - НБ-3-6.") },
            {"ЭС-10606", new SuspensionKind("ЭС-10606","ИЗОЛИРУЮЩИЕ ПОДВЕСКИ ВЛ 35-750 кВ, Альбом 2.", "Узел крепления - КГП-16-3;\nСерьга специальная - СРС-7-16;\nУшко однолапчатое - У1К-7-16;\nЗажим поддерживающий-ПГН-1-5.") },
            {"ЭС-10612", new SuspensionKind("ЭС-10612","ИЗОЛИРУЮЩИЕ ПОДВЕСКИ ВЛ 35-750 кВ, Альбом 2.", "Скоба - СКД-10-1;\nСкоба - СК-7-1А;\nЗвено промежуточное регулируемое - ПРР-7-1;\nЗвено промежуточное прямое - ПР-7-6;\nЗвено промежуточное монтажное - ПТМ-7-3;\nСерьга - СР-7-16;\nИзолятор;\nУшко однолапчатое - У1К-7-16;\nЗажим натяжной клиновой-НКК-1-15;\nЗажим заземляющий прессуемый.") }
        };


        public static CableKind GetCableParams(string cypher)
        {
            try
            {
                return cablesDict[cypher];
            }
            catch(KeyNotFoundException)
            {
                Debug.Log($"Cant find cable kind with cypher {cypher}");
                return null;
            }
        }

        public static TowerKind GetTowerParams(string cypher)
        {
            try
            {
                return towersDict[cypher];
            }
            catch (KeyNotFoundException)
            {
                Debug.Log($"Cant find tower kind with cypher {cypher}");
                return null;
            }
}

        public static SuspensionKind GetSuspensionParams(string cypher)
        {
            try
            {
                return suspensionsDict[cypher];
            }
            catch (KeyNotFoundException)
            {
                Debug.Log($"Cant find suspention kind with cypher {cypher}");
                return null;
            }
        }


        /// <summary>
        /// Возвращает монтажную стрелу провеса провода в зависимости от входных значений длины пролета и температуры
        /// </summary>
        /// <param name="cypher"></param>
        /// <param name="x">Длина пролета [м.]</param>
        /// <param name="y">Температура [Цельсий]</param>
        /// <returns></returns>
        public static float GetNormativeCableSag(string cypher, float x, float y)
        {
            switch(cypher)
            {
                case "АС-150/24":
                    return (float)Math.Round(-0.023130276398565464 + 0.0 + -0.013363297181105883 * x + 0.00519387023679597 * y + 0.00016318803859947085 * x * x + 7.635304235747657e-05 * x * y + 2.285773406881212e-05 * y * y, 2);
                case "АС-120/19":
                    return (float)Math.Round(-1.1687413993460147 + 0.0 + -0.0020363827299850665 * x + 0.014451569164640287 * y + 0.00018074006818229562 * x * x + 2.3909086051117333e-05 * x * y + 8.669705315764353e-06 * y * y, 2);
                case "АС-95/16":
                    return (float)Math.Round(-1.1962006763236808 + 0.0 + 0.0 * x + 0.01694041282015106 * y + 0.0002170122632367633 * x * x + 0.0 * x * y + 0.0 * y * y, 2);
                case "С-50":
                    return (float)Math.Round(3.810121261915119 + 0.0 + -0.06366833255818403 * x + -0.0025648274700203473 * y + 0.00028041973385439925 * x * x + 6.852492593861686e-05 * x * y + 2.670572456175355e-06 * y * y, 2);
                case "ПС-35": //костыль для теста
                    return (float)Math.Round(3.810121261915119 + 0.0 + -0.06366833255818403 * x + -0.0025648274700203473 * y + 0.00028041973385439925 * x * x + 6.852492593861686e-05 * x * y + 2.670572456175355e-06 * y * y, 2);
                case "АС-70/11":
                    return (float)Math.Round(-0.9357516953868315 + 0.0 + 0.00289405361359971 * x + 0.012836140236973983 * y + 0.00034225866391053485 * x * x + 0 * x * y + 6.174219572974928e-05 * y * y, 2);

                default:
                    throw new System.Exception(string.Format("GetNormativeCableSag dont know cypher {0}", cypher));
            }
        }
    }


}
