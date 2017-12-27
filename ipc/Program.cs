using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ipc
{
    public class Program
    {
        public static void Main()
        {
            // catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            Task.Run(() =>
            {
                Console.WriteLine("IPC service starting ...");
                StartIpcServer();
                Console.WriteLine("IPC service terminated");
            });

            Console.WriteLine("Press [Enter] when the service has started to connect a client\n");
            Console.ReadLine();

            var client = CreateChannel(ServiceUri);
            var result = client.Add(1, 2);

            Console.WriteLine($"IPC call result={result}\n");
            Console.WriteLine("Press [Enter] to terminate the IPC service");
            Console.ReadLine();

            client.RequestIpcTerminate();

            Console.WriteLine("Press [Enter] to exit\n");
            Console.ReadLine();
        }

        private static readonly Uri ServiceUri = new Uri("net.tcp://127.0.0.1:8080/");

        private static IServiceContract CreateChannel(Uri uri)
        {
            // define a reader quota to override the default content length of 8092
            var readerQuotas = new XmlDictionaryReaderQuotas
            {
                MaxStringContentLength = int.MaxValue
            };
            
            // define binding
            var binding = new NetTcpBinding(SecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                ReaderQuotas = readerQuotas,
                Name = "clientBinding"
            };
            
            return ChannelFactory<IServiceContract>.CreateChannel(binding, new EndpointAddress(uri));
        }

        private static void StartIpcServer()
        {
            try
            {
                // define a reader quota to override the default content length of 8092
                var readerQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = int.MaxValue
                };

                // define binding
                var binding = new NetTcpBinding(SecurityMode.None)
                {
                    MaxReceivedMessageSize = int.MaxValue,
                    ReaderQuotas = readerQuotas,
                    Name = "serviceBinding"
                };

                // define service host
                var serviceHost = new ServiceHost(typeof(Service));

                // create an ipc endpoint and open the service
                var serviceEndPoint = serviceHost.AddServiceEndpoint(typeof(IServiceContract), binding, ServiceUri);

                // set ListenUriMode.Unique if the port number selection is deferred to the kernel (otherwise ChannelDispatcher Listener returns port 0)
                serviceEndPoint.ListenUriMode = ServiceUri.Port == 0
                    ? ListenUriMode.Unique
                    : ListenUriMode.Explicit;

                // open host
                serviceHost.Open();

                // get the port number the service has bound to
                var dispatcher = serviceHost.ChannelDispatchers.FirstOrDefault();
                if (dispatcher?.Listener != null)
                {
                    Console.WriteLine($"IPC service started on port {ServiceUri.Port}.");

                    // wait for the ipc service to signal termination
                    Service.CancellationTokenSource.Token.WaitHandle.WaitOne(Timeout.Infinite);

                    // close the service host
                    serviceHost.Close(TimeSpan.Zero);
                }
                else
                {
                    throw new Exception("IPC host process started, but has no valid listener was configured in channel dispatchers. Terminating process");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            // cast
            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;

            Console.WriteLine($"UnhandledException: {ex.Message}");
        }

    }
}