using System;
using System.Threading.Tasks;
using VRGadgetController.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("VR Gadget Controller Started");

        // Initialize VR gadget controller
        IVRGadgetController vrController = new VRGadgetController.Services.VRGadgetController();

        try
        {
            // Initialize the MQTT connection
            await vrController.InitializeAsync();
            Console.WriteLine("[Info] VR Gadget Controller initialized successfully.");

            // Demonstrate control features
            Console.WriteLine("\n=== Control Features Demo ===");
            
            // Heating control
            Console.WriteLine("[Info] Starting heating...");
            await vrController.StartHeatingAsync();
            await Task.Delay(2000); // Wait 2 seconds to simulate heating process
            
            Console.WriteLine("[Info] Finishing heating...");
            await vrController.FinishHeatingAsync();
            await Task.Delay(1000);
            
            // Cooling control
            Console.WriteLine("[Info] Starting cooling...");
            await vrController.StartCoolingAsync();
            await Task.Delay(2000); // Wait 2 seconds to simulate cooling process

            Console.WriteLine("[Info] Finishing cooling...");
            await vrController.FinishCoolingAsync();
            await Task.Delay(1000);
            
            // Splash control
            Console.WriteLine("[Info] Starting splash...");
            await vrController.StartSplashAsync();
            await Task.Delay(1500); // Wait 1.5 seconds to simulate splash process

            Console.WriteLine("[Info] Finishing splash...");
            await vrController.FinishSplashAsync();
            await Task.Delay(1000);

            Console.WriteLine("\n=== Demo completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Error occurred: {ex.Message}");
            Console.WriteLine($"[Error] Stack trace: {ex.StackTrace}");
        }

    }
}
