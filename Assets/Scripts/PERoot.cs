using PEUtils;
using UnityEngine;

public class PERoot : MonoBehaviour {
    void Start() {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");
    }
    void Update() {
        
    }
}