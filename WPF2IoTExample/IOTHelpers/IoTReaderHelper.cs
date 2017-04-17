using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace IOTHelpers
{
    /// <summary>
    /// This is a helper for reading from the IoTHub
    /// </summary>
    public class IoTReaderHelper
    {
        static string connectionString = "{iothub connection string}";
        static string iotHubDevice2CloudEndpoint = "messages/events";
        static EventHubClient eventHubClient;


        /// <summary>
        /// Initialize connection to the IoTHub
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            try
            {
                IoTReaderHelper.connectionString = connectionString;
                eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubDevice2CloudEndpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }

        }

        /// <summary>
        /// Receive Messages from device.  By default if reads partition 0 but you can specifiy the partition
        /// </summary>
        /// <param name="partition"></param>
        /// <returns></returns>
        public static async Task<EventData> ReceiveMessagesFromDeviceAsync(string partition = "0")
        {
            try
            {
                // If you are running this from a client you are likely to have to open up ports on the client
                // firewall of specifiy HTTP for transport
                //ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

                // get all messages
                var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition,
                    DateTime.UtcNow.Subtract(new TimeSpan(1,0,0)));

                EventData eventData = await eventHubReceiver.ReceiveAsync();

                if (eventData != null)
                {
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());

                    Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
                }

                return eventData;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }

        }

        /// <summary>
        /// For the connected IoT Hub get a list of partitions
        /// </summary>
        /// <returns></returns>
        public static string[] GetPartiionIds()
        {
            String[] partitionsIds = eventHubClient.GetRuntimeInformation().PartitionIds;

            return partitionsIds;
        }

        /// <summary>
        /// Turn byte stream into a string
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public static string EventDataToString(EventData eventData)
        {
            return Encoding.UTF8.GetString(eventData.GetBytes());

        }

    }
}
