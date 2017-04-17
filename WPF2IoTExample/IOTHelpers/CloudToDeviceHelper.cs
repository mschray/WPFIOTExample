using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace IOTHelpers
{
    /// <summary>
    /// This is a helper to make it easier to communicate from the cloud to the device
    /// </summary>
    class CloudToDeviceHelper
    {

        static ServiceClient serviceClient;
        static string connectionString = "";

        /// <summary>
        /// Initialize Connect to IoT Hub
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            try
            {
                CloudToDeviceHelper.connectionString = connectionString;

                serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }
        }

        /// <summary>
        /// Send Cloud to device message
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="message">Message to send to device</param>
        /// <param name="requestFeedback">Whether you want to receive feedback on receipt of message from client
        /// the default is None.</param>
        /// <returns></returns>
        public async static Task SendCloudToDeviceMessageAsync(string deviceId, string message, DeliveryAcknowledgement requestFeedback = DeliveryAcknowledgement.None)
        {

            try
            {
                var commandMessage = new Message(Encoding.ASCII.GetBytes(message))
                {
                    Ack = requestFeedback
                };
                await serviceClient.SendAsync(deviceId, commandMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw;
            }
            
        }

        /// <summary>
        /// Recieve feedback from client
        /// </summary>
        /// <returns></returns>
        public async static Task<IEnumerable<FeedbackRecord>> ReceiveFeedbackAsync()
        {
            try
            {
                var feedbackReceiver = serviceClient.GetFeedbackReceiver();

                var feedbackBatch = await feedbackReceiver.ReceiveAsync();

                // Get all the feedback
                IEnumerable<FeedbackRecord> feedback = feedbackBatch.Records.Select(f => f);

                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));

                await feedbackReceiver.CompleteAsync(feedbackBatch);

                return feedback;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");

                throw ex;
            }


        }
    }
}
