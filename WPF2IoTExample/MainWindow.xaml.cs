using IOTHelpers;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Automation.Peers;

namespace WPF2IoTExample
{

    class IoTData
    {
        public string ClientDevice { get; set; }
        public string LockId { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Device device;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }

        /// <summary>
        /// Afer the Windows is loaded this event is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">RoutedEventArgs</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // write up the method to recieve events for Cloud to device messages
            DeviceCommsHelper.ReceivedMessage += DeviceCommsHelper_ReceivedMessage;

        }

        /// <summary>
        /// Create a new device or if it already exists pull back an existing (e.g. registered) device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize the ManageDeviceHelper
                ManageDeviceHelper.Initialize(Utils.ReadSetting("IOTHubConnectionString"));

                Messages.Items.Add($"Initialized Device registry...");

                // Add a device
                device = await ManageDeviceHelper.AddDeviceAsync(Utils.ReadSetting("DeviceName"));

                Messages.Items.Add($"Device {device.Id} created or retrieved...");

            }
            catch (Exception ex)
            {
                Messages.Items.Add($"{Utils.FormatExceptionMessage(ex)}");
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }
            

        }

        /// <summary>
        /// Send message from the device to the cloud
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessge_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // you have to have a device registered before you send a message.  If not display message
                // and return
                if (device == null) {
                    Messages.Items.Add($"Please make sure to register the device by clicking Create Device");
                    MessageBox.Show("Please make sure to register the device by clicking Create Device", "Create Device");
                    return;
                }

                // Initialize the DeviceCommsHelper so we can send/recieve messages
                DeviceCommsHelper.Initialize(Utils.ReadSetting("iotHubUri"), Utils.ReadSetting("DeviceName"),
                    device.Authentication.SymmetricKey.PrimaryKey);

                Messages.Items.Add($"Initialized IoT Hub Comms");

                // create the sample data
                IoTData ioTData = new IoTData()
                {
                    ClientDevice = device.Id,
                    LockId = "57"
                };

                // Searlize the sample data to JSON
                var jsonMessage = JsonConvert.SerializeObject(ioTData);

                // Send the message to the IOTHub
                DeviceCommsHelper.SendDeviceToCloudMessagesAsync(jsonMessage);

                Messages.Items.Add($"Sent message to IoT Hub");

            }
            catch (Exception ex)
            {
                Messages.Items.Add($"{Utils.FormatExceptionMessage(ex)}");
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }

        }

        /// <summary>
        /// This method will wait for cloud to device messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadMessge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize the DeviceCommsHelper so we can send/recieve messages
                DeviceCommsHelper.Initialize(Utils.ReadSetting("iotHubUri"), Utils.ReadSetting("DeviceName"),
                    device.Authentication.SymmetricKey.PrimaryKey);

                Messages.Items.Add($"Initialize connection to IoT Hub");

                DeviceCommsHelper.ReceiveCloudMessageAsync();

                Messages.Items.Add($"Tune into Channel for Cloud to device messages");


            }
            catch (Exception ex)
            {
                Messages.Items.Add($"{Utils.FormatExceptionMessage(ex)}");
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }


        }

        /// <summary>
        /// This is the event handler that will receive Cloud to Device Messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event contains the cloud to device mesage in ReceivedMessage</param>
        private void DeviceCommsHelper_ReceivedMessage(object sender, System.EventArgs e)
        {
            
            try
            {
                // Do the cast from EventArgs to DeviceMessageEventArgs
                var eventMessage = e as DeviceMessageEventArgs;

                // Grab the message content
                string data = eventMessage.ReceivedMessage;

                System.Console.WriteLine($"Received Cloud to device {DateTime.Now.ToString()} {data}");
                Messages.Items.Add($"Received Cloud to device message: {data}");
            }
            catch (Exception ex)
            {
                Messages.Items.Add($"{Utils.FormatExceptionMessage(ex)}");
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }

        }

        /// <summary>
        /// Typically this code would reside on a server but to illustrate reading messages we have it here in the client.  For a 
        /// far more rubust implementation I wuold recommend writing the incoming message to a ServiceBus topic and than
        /// procesing it from there
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadIOTHub_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // Connect to IoTHub
                IoTReaderHelper.Initialize(Utils.ReadSetting("IOTHubConnectionString"));

                // Grab list of paritions
                string[] partitionIds = IoTReaderHelper.GetPartiionIds();
                EventData eventData = await IoTReaderHelper.ReceiveMessagesFromDeviceAsync(partitionIds[0] ?? "0");

                // there was data to retreive
                if (eventData != null)
                {
                    System.Console.WriteLine($"Read message {DateTime.Now.ToString()} {eventData.GetBytes().ToString()}");

                    Messages.Items.Add($"{DateTime.Now.ToString()} {eventData.GetBytes().ToString()}");

                }
                else  //// there was NO data to retreive
                {
                    System.Console.WriteLine("Read message read {DateTime.Now.ToLocalString()}");

                    Messages.Items.Add($"Read message read {DateTime.Now.ToString()}");
                }
            }
            catch (Exception ex)
            {

                Messages.Items.Add($"{Utils.FormatExceptionMessage(ex)}");
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");

            }


        }

        /// <summary>
        /// This would typically be server code, but for example sake this code will send a message from Cloud to Client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendMessageToClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloudToDeviceHelper.Initialize(Utils.ReadSetting("IOTHubConnectionString"));
                Messages.Items.Add($"Connecting to IoT hub...");

                await CloudToDeviceHelper.SendCloudToDeviceMessageAsync(Utils.ReadSetting("DeviceName"), "I am a test mesage");

                Messages.Items.Add($"Sent message to IoT hub for cloud to device message delivery...");

            }
            catch (Exception ex)
            {
                Messages.Items.Add($"{Utils.FormatExceptionMessage(ex)}");
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }
        }
    }
}
