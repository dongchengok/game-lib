using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class Runtime : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}

namespace pGameLib
{
    [XLua.LuaCallCSharp]
    public class Runtime : MonoBehaviour
    {
        public string launchFile = "main";
        public int defaultFrameRate = 60;
        private static Runtime Singleton = null;

        private void Awake()
        {
            this.SetupFrameRate(defaultFrameRate);
            this.SavePowerModel(false);

            Debug.Assert(Singleton == null);
            Singleton = this;
            DontDestroyOnLoad(this);

            xLuaRuntime.Init(launchFile);
        }

        public static void Restart()
        {
            xLuaRuntime.Reload();
        }

        //省电模式
        public void SetupFrameRate( int frameRate )
        {
            Application.targetFrameRate = frameRate;
            QualitySettings.vSyncCount = 0;
        }

        public void SavePowerModel( bool save, int interval=3 )
        {
            UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = save?(interval>1?interval:1):1;
        }
    }
}