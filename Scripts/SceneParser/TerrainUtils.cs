using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldComposer;

namespace CableWalker.Simulator
{
    public static class TerrainUtils
    {
        public static List<string> ActiveTerrainsNames = new List<string>();

        public static Terrain GetCurrentTerrain(Terrain terrain, Vector3 position)
        {
            position += new Vector3(0, 10000, 0);
            
            RaycastHit[] hits = Physics.RaycastAll(position, -Vector3.up,20000);
            RaycastHit hit = hits.First(x => x.transform.gameObject.GetComponent<Terrain>() != null);
            Terrain result = hit.transform.gameObject.GetComponent<Terrain>();
            AddToActiveList(result);
            return result;
        }

        private static void AddToActiveList(Terrain activeTerrain)
        {
            ActiveTerrainsNames.Add(activeTerrain.name);
            var terrainNeibs = activeTerrain.GetComponent<TerrainNeighbors>();
            if (terrainNeibs.top != null)
                ActiveTerrainsNames.Add(terrainNeibs.top.name);
            if (terrainNeibs.left != null)
                ActiveTerrainsNames.Add(terrainNeibs.left.name);
            if (terrainNeibs.right != null)
                ActiveTerrainsNames.Add(terrainNeibs.right.name);
            if (terrainNeibs.bottom != null)
                ActiveTerrainsNames.Add(terrainNeibs.bottom.name);
        }

        public static void OffTerrains()
        {
            var terrainsHolder = GameObject.FindGameObjectWithTag("TerrainsHolder");
            List<GameObject> allTerrains = new List<GameObject>();
            foreach (Transform terrain in terrainsHolder.transform)
            {
                terrain.gameObject.SetActive(false);
            }
        }

        public static void OffNotActiveTerrains()
        {
            var terrainsHolder = GameObject.FindGameObjectWithTag("TerrainsHolder");
            if (terrainsHolder == null)
                throw new System.Exception("On TerrainsHolder no tag TerrainsHolder");

            List<GameObject> allTerrains = new List<GameObject>();
            foreach (Transform terrain in terrainsHolder.transform)
            {
                terrain.gameObject.layer = LayerMask.NameToLayer("Ground");
                terrain.gameObject.tag = "Ground";
                allTerrains.Add(terrain.gameObject);
            }
            List<GameObject> notActiveTerrains = new List<GameObject>();
            notActiveTerrains = allTerrains.Where(x => !ActiveTerrainsNames.Contains(x.name)).ToList();
            foreach (GameObject terrain in notActiveTerrains)
                terrain.SetActive(false);
        }

        public static float GetSampleHeight(Vector3 position)
        {
            var terrain = GetCurrentTerrain(Terrain.activeTerrain, position);
            return terrain.SampleHeight(position);
        }

        public static Vector3 GetTerrainPointOnScene(Vector3 position)
        {
            var terrain = GetCurrentTerrain(Terrain.activeTerrain, position);
            return new Vector3(position.x, terrain.SampleHeight(position), position.z);
        }

      

        public static float GetHeightOnTerrain(Vector3 position)
        {
            var terrain = GetCurrentTerrain(Terrain.activeTerrain, position);
            var posOnTerrain = GetPositionOnTerrain(position);
            var height = terrain.terrainData.GetHeights((int)posOnTerrain.x, (int)posOnTerrain.y, 1, 1);
            return height[0, 0];
        } 

        public static void SetHeightOnTerrain(Vector3 position, float value, bool isValueInRange01)
        {
            var terrain = GetCurrentTerrain(Terrain.activeTerrain, position);
            var posOnTerrain = GetPositionOnTerrain(position);
            var height = terrain.terrainData.GetHeights((int)posOnTerrain.x, (int)posOnTerrain.y, 1, 1);
            value = isValueInRange01 ? value : 1.0f / terrain.terrainData.size.y * value;
            height[0, 0] = value;
            terrain.terrainData.SetHeights((int)posOnTerrain.x, (int)posOnTerrain.y, height);
        }

        public static void SetHeightsOnTerrain(List<Vector3> points)
        {
            Dictionary<string, float[,]> terrains = new Dictionary<string, float[,]>();
            var terrain = GetCurrentTerrain(Terrain.activeTerrain, points[0]);
            int xRes = terrain.terrainData.heightmapWidth;
            int yRes = terrain.terrainData.heightmapHeight;
            terrains[terrain.name] = terrain.terrainData.GetHeights(0, 0, xRes, yRes);
            float ftop = float.NegativeInfinity;
            float fright = float.NegativeInfinity;
            float fbottom = Mathf.Infinity;
            float fleft = Mathf.Infinity;
            foreach (Vector3 point in points)
            {
                var posOnTerrain = GetPositionOnTerrain(point, terrain);
                terrains[terrain.name][(int)posOnTerrain.x, (int)posOnTerrain.y] =  1.0f / terrain.terrainData.size.y * point.y; 
                ////find the outmost points
                //if (ftop < point.z)
                //{
                //    ftop = point.z;
                //}
                //if (fright < point.x)
                //{
                //    fright = point.x;
                //}
                //if (fbottom > point.z)
                //{
                //    fbottom = point.z;
                //}
                //if (fleft > point.x)
                //{
                //    fleft = point.x;
                //}

            }
            terrain.terrainData.SetHeights(0, 0, terrains[terrain.name]);
            //int top = Mathf.RoundToInt(ftop);
            //int right = Mathf.RoundToInt(fright);
            //int bottom = Mathf.RoundToInt(fbottom);
            //int left = Mathf.RoundToInt(fleft);
            //int terrainXmax = right - left; // the rightmost edge of the terrain
            //int terrainZmax = top - bottom; // the topmost edge of the terrain 
            //float[,] shapeHeights = terrain.terrainData.GetHeights(left, bottom, terrainXmax, terrainZmax);
            
            

        }

        public static Vector2 GetPositionOnTerrain(Vector3 position,Terrain terrain)
        {
           
            var hmWidth = terrain.terrainData.heightmapWidth;
            var hmHeight = terrain.terrainData.heightmapHeight;

            // get the normalized position of this game object relative to the terrain
            Vector3 tempCoord = (position - terrain.transform.position);
            Vector3 coord = new Vector3();
            coord.x = tempCoord.x / terrain.terrainData.size.x;
            coord.y = tempCoord.y / terrain.terrainData.size.y;
            coord.z = tempCoord.z / terrain.terrainData.size.z;

            int posXOnTerrain = (int)(coord.x * hmWidth);
            int posYOnTerrain = (int)(coord.z * hmHeight);
            if (posXOnTerrain >= hmWidth)
            {
                //Костыль
                posXOnTerrain = hmWidth - 1;
            }
            if (posYOnTerrain >= hmHeight)
            {
                //Костыль
                posYOnTerrain = hmHeight - 1;
            }
            return new Vector2(posXOnTerrain, posYOnTerrain);
        }

        public static Vector2 GetPositionOnTerrain(Vector3 position)
        {
            var terrain = GetCurrentTerrain(Terrain.activeTerrain, position);
            var hmWidth = terrain.terrainData.heightmapWidth;
            var hmHeight = terrain.terrainData.heightmapHeight;

            // get the normalized position of this game object relative to the terrain
            Vector3 tempCoord = (position - terrain.transform.position);
            Vector3 coord = new Vector3();
            coord.x = tempCoord.x / terrain.terrainData.size.x;
            coord.y = tempCoord.y / terrain.terrainData.size.y;
            coord.z = tempCoord.z / terrain.terrainData.size.z;

            int posXOnTerrain = (int)(coord.x * hmWidth);
            int posYOnTerrain = (int)(coord.z * hmHeight);
            if (posXOnTerrain >= hmWidth)
            {
                //Костыль
                posXOnTerrain = hmWidth - 1;
            }
            if (posYOnTerrain >= hmHeight)
            {
                //Костыль
                posYOnTerrain = hmHeight - 1;
            }
            return new Vector2(posXOnTerrain, posYOnTerrain);
        }

        public static void CheckTerrainsLayers()
        {
            foreach (Terrain terrain in Terrain.activeTerrains)
                terrain.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
