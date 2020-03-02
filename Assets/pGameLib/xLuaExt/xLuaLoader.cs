using UnityEngine;
using UnityEngine.Networking;

namespace pGameLib
{
    public static class xLuaLoader
    {
        public static byte[] LoadFromResource(ref string filepath)
        {
            var _asset = Resources.Load<TextAsset>(filepath);
#if UNITY_EDITOR
            if(_asset==null)
            {
                string _path = System.IO.Path.Combine(Application.dataPath, filepath);
                if(!_path.EndsWith(".lua"))
                {
                    _path += ".lua";
                }
                if(System.IO.File.Exists(_path))
                {
                    System.IO.StreamReader _file = new System.IO.StreamReader(_path);
                    return System.Text.Encoding.UTF8.GetBytes(_file.ReadToEnd());
                }
            }
#endif
            return
                _asset !=null ?
                System.Text.Encoding.UTF8.GetBytes(_asset.text) : 
                null;
        }
    }
}