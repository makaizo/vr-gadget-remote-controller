using System.Threading.Tasks;

namespace VRGadgetController.Services
{
    public interface IVRGadgetController
    {
        Task InitializeAsync();
        
        // Control features
        Task StartHeatingAsync();
        Task FinishHeatingAsync();
        Task StartCoolingAsync();
        Task FinishCoolingAsync();
        Task StartSplashAsync();
        Task FinishSplashAsync();
    }
}
