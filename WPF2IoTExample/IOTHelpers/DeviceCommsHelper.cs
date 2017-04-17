using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Client;
using System.IO;

namespace IOTHelpers
{
    /// <summary>
    /// DeviceMessageEventArgs inherits from EventArgs and adds a property for the
    /// received message.  Rather than sitting in an endless polling cycle we'll use events to signal the arrival of
    /// a new message
    /// </summary>
    public class DeviceMessageEventArgs : EventArgs
    {
        // Property for the ReceivedMesage
        public string ReceivedMessage { get; set; }

        /// <summary>
        /// Constuctor for DeviceMessageEventArgs
        /// </summary>
        /// <param name="receivedMessage">The message sent cloud to device</param>
        public DeviceMessageEventArgs(string receivedMessage) : base()
        {
            this.ReceivedMessage = receivedMessage;
        }
        
    }

    /// <summary>
    /// Class for working with Device communicationns to and from the cloud
    /// </summary>
    public class DeviceCommsHelper
    {
        // Class variables
        private static DeviceClient deviceClient;
        private static string iotHubUri = "";
        private static string deviceKey = "";
        private static string deviceName = "";

        // A delegate type for hooking up MessageReceived notifications.
        public delegate void MessageReceivedEventHandler(object sender, EventArgs e);
        public static event MessageReceivedEventHandler ReceivedMessage;

        /// <summary>
        /// Initialize should be called before using any of mmembers of this class.  It uses the parameters to create the DeviceClient for sending
        /// and receiving comms
        /// </summary>
        /// <param name="iotHubUri">Azure IoT Hub URI</param>
        /// <param name="deviceName">Name of the device sending the message</param>
        /// <param name="deviceKey">Key of the device sending the message</param>
        public static void Initialize(string iotHubUri, string deviceName, string deviceKey)
        {
            DeviceCommsHelper.iotHubUri = iotHubUri;
            DeviceCommsHelper.deviceName = deviceName;
            DeviceCommsHelper.deviceKey = deviceKey;

            try
            {
                deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey), Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");


                throw ex;
            }

        }

        /// <summary>
        /// UploadToBlobAsync uploads stream to the provided blobName
        /// </summary>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="stream">Stream for the blob</param>
        public static async void UploadToBlobAsync(string blobName, Stream stream)
        {
            try
            {
                await deviceClient.UploadToBlobAsync(blobName, stream);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");

                throw ex;
            }

            Console.WriteLine($"Stream with length of {stream.Length} written to {blobName}");
        }

        /// <summary>
        /// SendDeviceToCloudMessageAysnc send a JSON formmatted message to IoTHub
        /// </summary>
        /// <param name="jsonSearlizedMessage"></param>
        public static async void SendDeviceToCloudMessagesAsync(string jsonSearlizedMessage)
        {
            try
            {
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(jsonSearlizedMessage));

                await deviceClient.SendEventAsync(message);

                Console.WriteLine($"{ DateTime.Now} > Sending message: {jsonSearlizedMessage}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");

                throw ex;
            }
          
        }

        /// <summary>
        /// ReceiveCloudMessageAsync() receives a cloud message, but rather than processing the message directly it uses an event
        /// to send all listeners notification 
        /// </summary>
        public static async void ReceiveCloudMessageAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {

                try
                {
                    Microsoft.Azure.Devices.Client.Message receivedMessage = await deviceClient.ReceiveAsync();

                    if (receivedMessage == null) continue;

                    DeviceMessageEventArgs eventArgs = new DeviceMessageEventArgs(Encoding.ASCII.GetString(receivedMessage.GetBytes()));

                    ReceivedMessage?.Invoke(receivedMessage, eventArgs);

                    Console.WriteLine($"Received message: {eventArgs.ReceivedMessage}");

                    await deviceClient.CompleteAsync(receivedMessage);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");

                    throw ex;
                }
            }
        }


    }
}
