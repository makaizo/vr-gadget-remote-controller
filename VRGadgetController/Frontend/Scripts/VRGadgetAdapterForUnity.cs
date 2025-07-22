using System;
using System.Threading.Tasks;
using VRGadgetController.Services;
using UnityEngine;

public class VRGadgetAdapterForUnity : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("VR Gadget Controller Started");

        // Initialize VR gadget controller
        IVRGadgetController vrController = new VRGadgetController.Services.VRGadgetController();

        try
        {
            // Initialize the MQTT connection
            await vrController.InitializeAsync();
            Debug.Log("[Info] VR Gadget Controller initialized successfully.");

            // Demonstrate control features
            Debug.Log("\n=== Control Features Demo ===");

            // Heating control
            Debug.Log("[Info] Starting heating...");
            await vrController.StartHeatingAsync();
            await Task.Delay(2000); // Wait 2 seconds to simulate heating process

            Debug.Log("[Info] Finishing heating...");
            await vrController.FinishHeatingAsync();
            await Task.Delay(1000);

            // Cooling control
            Debug.Log("[Info] Starting cooling...");
            await vrController.StartCoolingAsync();
            await Task.Delay(2000); // Wait 2 seconds to simulate cooling process

            Debug.Log("[Info] Finishing cooling...");
            await vrController.FinishCoolingAsync();
            await Task.Delay(1000);

            // Splash control
            Debug.Log("[Info] Starting splash...");
            await vrController.StartSplashAsync();
            await Task.Delay(1500); // Wait 1.5 seconds to simulate splash process

            Debug.Log("[Info] Finishing splash...");
            await vrController.FinishSplashAsync();
            await Task.Delay(1000);

            Debug.Log("\n=== Demo completed successfully ===");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Error] Error occurred: {ex.Message}");
            Debug.LogError($"[Error] Stack trace: {ex.StackTrace}");
        }

    }
    void Update()
    {
        
    }
}
