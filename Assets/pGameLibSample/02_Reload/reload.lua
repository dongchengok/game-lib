
function onClick()
    --由于Unity有缓存，所以需要hack一下，调用Invoke才会移除lua引用，这里需要根据具体需要处理
    -- CS.UnityEngine.GameObject.Find("btn_reload"):GetComponent("Button").onClick:RemoveListener(onClick);
    -- CS.UnityEngine.GameObject.Find("btn_reload"):GetComponent("Button").onClick:Invoke();

    CS.pGameLib.Runtime.Restart()
    
end

--print(CS.UnityEngine.GameObject.Find("btn_reload"):GetComponent("Button").onClick)
--CS.UnityEngine.GameObject.Find("btn_reload"):GetComponent("Button").onClick:AddListener(onClick)
__lua_runtime__.BindUnityUIEvent(
    CS.UnityEngine.GameObject.Find("btn_reload"):GetComponent("Button").onClick,
    onClick
)

--修改下面代码，点击reload后会自动重新加载
print("reload 0")