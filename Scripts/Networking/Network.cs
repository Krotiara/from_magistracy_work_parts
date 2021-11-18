using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CableWalker.Simulator.Networking
{
    public static class Network 
    {
        public static bool authorized = false;
        private static string token;
        private static string serverIP = "localhost:8080/"; //"192.168.0.102";
        private static string loginURI = "api/v1/users/login";
        private static string towersURI = "api/v1/simulator/towers";
        private static string towerDefectsURI = "api/v1/simulator/towers/defects";
        private static string towersTypesURI = "api/v1/simulator/towers/types";

        public static void InstantiateNetworkAsync(string login, string password)
        {
            Task.Run(() => { InstantiateNetwork(login, password); });
        }

        public static void LoadDefectsAsync(string lineName, string localConfigPath, string localPhotoPath)
        {
            if (authorized)
                Task.Run(() => { LoadObjectData(lineName, localConfigPath, localPhotoPath, towerDefectsURI); });
            else
            Debug.Log("you should log in before with action");
        }

        public static void LoadTowersAsync(string lineName, string localConfigPath, string localPhotoPath)
        {
            if (authorized)
                Task.Run(() => { LoadObjectData(lineName, localConfigPath, localPhotoPath, towersURI); });
            else
                Debug.Log("you should log in before with action");
        }

        private static void InstantiateNetwork(string login, string password)
        {
            authorized = false;
            string uri = "http://" + serverIP + loginURI;
            string json = "{\"email\":\"" + login + "\","+ 
                  "\"password\":\"" + password + "\"}";
            User user = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentType = "application/json";
                request.Method = "POST";
                user = JsonUtility.FromJson<User>(Post(json, request));
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }

            if (user != null)
            {
                token = user.token;
                authorized = true;
                Debug.Log("Login successful");
            }
            else
            {
                Debug.Log("Login failed");
            }
        }

        private static void LoadObjectData(string lineName, string localConfigPath, string localPhotoPath, string objectURI)
        {
            string uri = "http://" + serverIP + objectURI;  
            string json = "{\"powerline\":\"" + lineName+"\"}";
            DefectsList defectsList = new DefectsList();
            var response = MakeDataRequest(uri, json);
         //   defectsList = JsonUtility.FromJson<DefectsList>(defectsInJson);
            SaveData(response, localConfigPath);
           // foreach (var defect in defectsList.Defects)
              //  DownloadFile(defect.PhotoPath, localPhotoPath);
        }

        private static string MakeDataRequest(string uri, string json)
        {
            var response = "";
            Debug.Log(uri);
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers.Add("x-access-token", token);
                request.ContentType = "application/json";
                request.Method = "POST";

                response = Post(json, request);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
            return response;
        }

        private static string Post(string body, HttpWebRequest request)
        {
            var response = "";
            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(body);
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
            return response;
        }

        public static void SaveData(string data, string localFileName, bool useEncoding = false)
        {
            StreamWriter localStream = null;
            string dir = Path.GetDirectoryName(localFileName);
            FileTool.CheckDirectoryExists(dir);

            try
            {
                if (useEncoding)
                    localStream = new StreamWriter(localFileName, false, Encoding.GetEncoding(1251));
                else
                    localStream = new StreamWriter(localFileName);
                localStream.WriteLine(data);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
            finally
            {
                if (localStream != null) localStream.Close();
            }
        }

        public static string LoadData(string localFileName)
        {
            string data = "";
            StreamReader localStream = null;
            try
            {
                localStream = new StreamReader(localFileName);
                data = localStream.ReadToEnd();
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
            finally
            {
                if (localStream != null) localStream.Close();
            }
            return data;
        }

        public static int DownloadFile(string remoteFilename, string localFolder)
        {
            int bytesProcessed = 0;
            string fileName = Path.GetFileName(remoteFilename);
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(remoteFilename);
                request.Headers.Add("x-access-token", token);
                {
                    response = request.GetResponse();
                    {
                        remoteStream = response.GetResponseStream();
                        localStream = File.Create(localFolder + fileName);
                        
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        do
                        {
                            bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                            localStream.Write(buffer, 0, bytesRead);
                            bytesProcessed += bytesRead;
                        }
                        while (bytesRead > 0);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
            return bytesProcessed;
        }
    }
}
