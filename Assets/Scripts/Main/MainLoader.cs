using Share.Scripts.Services;
using UnityEngine;
using App;

public class MainLoader : MonoBehaviour {
    
    private void Awake() {
        //Đây là class MonoBehaviour duy nhất có awake ở mỗi scene
        FirstClassServices.Initialize();

        // Disable logs in production
        Debug.unityLogger.logEnabled = !AppConfig.IsProduction;

        var loaderList = GetComponentsInChildren<ILoader>();
        
        foreach (var loader in loaderList) {
            loader.Initialize();
        }
    }
}
