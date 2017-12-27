using System.ServiceModel;
using System.Threading;

namespace ipc
{
    /// <summary>
    /// ServiceBehaviorAttribute
    /// ========================
    /// InstanceContextMode.PerSession
    ///     Creates a unique instance of this class for each connection the service recieves.
    /// 
    /// AddressFilterMode.Any
    ///     Required because "nettcp://localhost:0/" != "nettcp://localhost:50995/" after kernel port selection
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, AddressFilterMode = AddressFilterMode.Any, IncludeExceptionDetailInFaults = true, MaxItemsInObjectGraph = 2147483647)]
    public class Service : IServiceContract
    {
        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
        public int Add(int first, int second)
        {
            return first + second;
        }

        public int Subtract(int first, int second)
        {
            return first - second;
        }

        public void RequestIpcTerminate()
        {
            // signal service termination
            CancellationTokenSource.Cancel();
        }
    }
}