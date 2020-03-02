using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pGameLib
{
    public class xLuaRuntime : MonoBehaviour
    {
        internal static xLuaRuntime Singleton { get; private set; } = null;
        private static string ms_launchFile = null;

        private XLua.LuaEnv m_luaEnv = null;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(Singleton == null);
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);

            m_luaEnv = new XLua.LuaEnv();
            m_luaEnv.AddLoader(xLuaLoader.LoadFromResource);
            m_luaEnv.Global.Set("launch_file", ms_launchFile);
            m_luaEnv.DoString(Resources.Load<TextAsset>("runtime").text, "runtime");
        }

        // Update is called once per frame
        void Update()
        {
            //if (m_luaEnv != null)
            //{
            //    m_luaEnv.Tick();
            //}
        }

        private void OnDestroy()
        {
            if (m_luaEnv != null)
            {
                m_luaEnv.DoString("__lua_runtime__.Cleanup()");
                m_luaEnv.Dispose();
            }
            m_luaEnv = null;
            if(Singleton==this)
            {
                Singleton = null;
            }
        }

        internal static void Init(string launchFile)
        {
            xLuaRuntime.ms_launchFile = launchFile;
            GameObject _obj = new GameObject("__lua_runtime__");
            _obj.AddComponent<xLuaRuntime>();
        }

        internal static void Reload()
        {
            GameObject _go = xLuaRuntime.Singleton.gameObject;
            xLuaRuntime.Singleton.m_luaEnv.DoString("__lua_runtime__.Cleanup()");
            GameObject.Destroy(xLuaRuntime.Singleton);
            xLuaRuntime.Singleton = null;
            _go.AddComponent<xLuaRuntime>();
            //Init(xLuaRuntime.ms_launchFile);
        }
    }

}