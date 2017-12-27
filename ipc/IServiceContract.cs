using System.ServiceModel;

namespace ipc
{
    [ServiceContract(Namespace = "")]
    public interface IServiceContract
    {
        [OperationContract]
        int Add(int first, int second);

        [OperationContract]
        int Subtract(int first, int second);

        [OperationContract]
        void RequestIpcTerminate();
    }
}
