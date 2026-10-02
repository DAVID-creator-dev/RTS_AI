using System.Collections.Generic;
using UnityEngine;

namespace AI.InfluenceMap
{
    public class InfluenceMap : MonoBehaviour
    {
        public UnitController controller;
        public ETeam team;

        //grid properties
        public int width;
        public int height;

        //subMaps
        public FogOfWarInfluenceSubMap fogMap;
        public InformationStalenessSubmap stalenessSubmap;
        public List<InfluenceSubMap> subMapList = new();

        public SpriteRenderer spriteRenderer;
        private Texture2D debugTexture;

        public bool showDebug = false;
        private Grid mergedGrid;
        private Color[] debugColor;
        public int mergedDebugMinInfluence = -80;
        public int mergedDebugMaxInfluence = 80;
        public Color mergedDebugDefaultColor;
        public Color mergedDebugMinColor;
        public Color mergedDebugMaxColor;

        [SerializeField]
        protected Transform quadParent;
        public Vector2 textureScale { get; private set; }

        [Range(2, 50)]
        [Tooltip("Updates the influence Map Every x frames")]
        public int updateFrequency = 5;
        private int updateFrameCount = 0;

        [Range(2, 50)]
        [Tooltip("Updates the debug texture Every x frames")]
        public int debugUpdateFrequency = 5;
        private int debugUpdateFrameCount = 0;

        public int updateCycle { get; private set; } = 0;

        public int mapToShow = 0;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            textureScale = new Vector2(quadParent.localScale.x / width,
                                       quadParent.localScale.y / height);
            CreateTexture(width, height, textureScale);
            debugColor = new Color[width * height];

            //fogMap.SetupForController(controller);
            subMapList.Add(fogMap);
            subMapList.Add(stalenessSubmap);

            team = controller.GetTeam();

            foreach (InfluenceSubMap subMap in subMapList)
            {
                subMap.SetInfluenceMap(this);
                subMap.Init(width, height);
            }
            foreach (InfluenceSubMap subMap in subMapList)
            {
                subMap.PopulateInfluenceMap(team);
                subMap.PopulateDebugTexture(team);
            }
            SetupStaggeredUpdate();
            MergeGrids();
        }

        

        // Update is called once per frame
        void Update()
        {
            //debug show
            spriteRenderer.enabled = showDebug;

            updateFrameCount++;
            debugUpdateFrameCount++;

            if (updateFrameCount >= updateFrequency)
            {
                MergeGrids();
                updateFrameCount = 0;
                updateCycle++;
            }
            foreach (InfluenceSubMap subMap in subMapList)
            {
                if(subMap.updateFrame == updateFrameCount)
                {
                    subMap.PopulateInfluenceMap(team);
                }
            }
            if(debugUpdateFrameCount >= debugUpdateFrequency)
            {
                if (mapToShow >= 0 && mapToShow < subMapList.Count)
                {
                    subMapList[mapToShow].PopulateDebugTexture(team);
                }
                else if(mapToShow == -1)
                {
                    PopulateMergedDebugTexture();
                }
                debugUpdateFrameCount = 0;
            }
            if(mapToShow >= 0 && mapToShow < subMapList.Count)
            {
                debugTexture.SetPixels(subMapList[mapToShow].colors);
                debugTexture.Apply();
            }
            else if (mapToShow == -1)
            {
                debugTexture.SetPixels(debugColor);
                debugTexture.Apply();
            }
            
        }

        public void ChangeController(UnitController newController)
        {
            controller = newController;
            //fogMap.SetupForController(controller);
            team = controller.GetTeam();
            foreach (InfluenceSubMap subMap in subMapList)
            {
                subMap.PopulateInfluenceMap(team);
                subMap.PopulateDebugTexture(team);
            }
        }

        public void PopulateMergedDebugTexture()
        {
            for (int i = 0; i < mergedGrid.Size; ++i)
            {
                int influence = mergedGrid.Get(i);

                Color color = mergedDebugDefaultColor;
                if (influence < 0)
                    color = Color.Lerp(mergedDebugDefaultColor, mergedDebugMinColor, Mathf.Abs((float)influence / mergedDebugMinInfluence));
                else if (influence > 0)
                    color = Color.Lerp(mergedDebugDefaultColor, mergedDebugMaxColor, Mathf.Abs((float)influence / mergedDebugMaxInfluence));

                debugColor[i] = color;
            }
        }

        public void CreateTexture(int width, int height, Vector2 scale)
        {
            debugTexture = new Texture2D(width, height);
            spriteRenderer.sprite = Sprite.Create(debugTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1);
            spriteRenderer.transform.localScale = scale;

            Color startColor = new Color(0f, 0f, 0f, 0f);

            int size = width * height;
            Color[] colors = new Color[size];
            for (int i = 0; i < size; ++i)
                colors[i] = startColor;

            debugTexture.SetPixels(colors);
            debugTexture.Apply();
        }

        void MergeGrids()
        {
            if (mergedGrid == null)
                mergedGrid = new Grid(width, height, 0);
            else
                mergedGrid.Clear();

            foreach (InfluenceSubMap subMap in subMapList)
            {
                // Only unit-driven influence should count toward decisions; buildings project a constant,
                // non-decaying zone that would otherwise mask real troop movement. Fog/staleness aren't influence at all.
                // BuildingInfluenceSubMap itself is untouched and still computed/viewable in debug, just excluded here.
                // EnemyUnitInfluenceSubMap duplicates units already counted in UnitInfluenceSubMap, so it's excluded
                // here too - it's meant to be read directly (GetSubMapInfluenceAtPosition), not merged.
                if (subMap == fogMap || subMap == stalenessSubmap || subMap is BuildingInfluenceSubMap || subMap is EnemyUnitInfluenceSubMap)
                    continue;

                for (int i = 0; i < mergedGrid.Size; ++i)
                {
                    mergedGrid.Values[i] += subMap.grid.Values[i];
                }
            }
        }

        /// <summary>
        /// Sets up the SubMaps to update in a staggered fashion
        /// </summary>
        void SetupStaggeredUpdate()
        {
            for(int i = 0; i < subMapList.Count; ++i)
            {
                subMapList[i].updateFrame = (i % (updateFrequency-1));
            }
        }

        public Vector2Int GetPositionInGrid(Vector2 p)
        {
            return new Vector2Int
            {
                x = Mathf.RoundToInt(p.x * width / quadParent.localScale.x),
                y = Mathf.RoundToInt(p.y * height / quadParent.localScale.y)
            };
        }

        public Vector2Int GetPositionInGrid(Vector3 p)
        {
            return new Vector2Int
            {
                x = Mathf.RoundToInt(p.x * width / quadParent.localScale.x),
                y = Mathf.RoundToInt(p.z * height / quadParent.localScale.y)
            };
        }

        public bool IsVisible(int x, int y, int team)
        {
            return fogMap.IsVisible(x+y*width, team);
        }

        public bool IsVisible(int pos, int team)
        {
            return fogMap.IsVisible(pos, team);
        }

        public bool WasVisible(int x, int y, int team)
        {
            return fogMap.WasVisible(x + y * width, team);
        }

        public bool WasVisible(int pos, int team)
        {
            return fogMap.WasVisible(pos, team);
        }

        public bool IsVisible(Vector3 pos, int team)
        {
            Vector2Int gridPos = GetPositionInGrid(new Vector2(pos.x, pos.z));
            
            int index = gridPos.y * width + gridPos.x;
            if (index < 0 || index >= mergedGrid.Size) 
                return false;
            
            return fogMap.WasVisible(index, team);        
        }

        public bool WasVisible(Vector3 pos, int team)
        {
            Vector2Int gridPos = GetPositionInGrid(new Vector2(pos.x, pos.z));
            
            int index = gridPos.y * width + gridPos.x;
            if (index < 0 || index >= mergedGrid.Size) 
                return false;
            
            return fogMap.WasVisible(index, team);
        }

        public int GetInfluenceAtPosition(Vector3 pos)
        {
            Vector2Int gridPos = GetPositionInGrid(pos);
            int index = gridPos.y * width + gridPos.x;
            if (index < 0 || index >= mergedGrid.Size)
                return 0;
            return mergedGrid.Get(index);
        }

        public int GetInfluenceAtPosition(int x, int y)
        {
            int index = y * width + x;
            if (index < 0 || index >= mergedGrid.Size)
                return 0;
            return mergedGrid.Get(index);
        }

        public int GetSubMapInfluenceAtPosition<T>(Vector3 pos) where T : InfluenceSubMap
        {
            T subMap = GetSubMap<T>();
            if (subMap == null)
                return 0;

            Vector2Int gridPos = GetPositionInGrid(pos);
            int index = gridPos.y * width + gridPos.x;
            if (index < 0 || index >= subMap.grid.Size)
                return 0;
            return subMap.grid.Get(index);
        }

        public void SetStalenessAtPosition(int x, int y)
        {
            if (!stalenessSubmap.grid.Contains(x, y))
                return;
            stalenessSubmap.SetCell(x, y, updateCycle);
        }

        public int GetStalenessAtPosition(int x, int y)
        {
            if (!stalenessSubmap.grid.Contains(x, y))
                return 0;
            return updateCycle-stalenessSubmap.grid.Get(x+y*width);
        }

        public int GetStalenessAtPosition(Vector2Int pos)
        {
            return updateCycle - stalenessSubmap.grid.Get(pos.x + pos.y * width);
        }

        public bool Contains(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public T GetSubMap<T>() where T : InfluenceSubMap
        {
            foreach (InfluenceSubMap subMap in subMapList)
            {
                if (subMap is T)
                    return subMap as T;
            }
            return null;
        }
    }
}