    
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Diagnostics;
using System.Reflection;

namespace IOTHelpers
{
    public class ManageDeviceHelper
    {
        // Class members
        private static RegistryManager registryManager;
        private static string connectionString = "{iot hub connection string}";

        /// <summary>
        /// Initialize initialized the ManageDevicesHelper by creating a RegistryManage from a connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            try
            {
                ManageDeviceHelper.connectionString = connectionString;
                registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }
        }

        /// <summary>
        /// Get a Device based on its Device ID 
        /// </summary>
        /// <param name="deviceId">the DeviceID</param>
        /// <returns></returns>
        public static async Task<Device> GetDeviceAsync(string deviceId)
        {
            Device device =null;

            try
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            catch (DeviceNotFoundException ex)
            {
                Console.WriteLine($"Device with id: {deviceId} not found: {ex.Message}");
            }

            Console.WriteLine($"Getting device with id: {deviceId}");

            return device;
        }

        /// <summary>
        /// Add a Device using DeviceID
        /// </summary>
        /// <param name="deviceId">A string representing the device name</param>
        /// <returns></returns>
        public static async Task<Device> AddDeviceAsync(string deviceId)
        {
            Device device;

            try
            {
                Device deviceToBeAdded = new Device(deviceId);
                device = await registryManager.AddDeviceAsync(deviceToBeAdded);
                Console.WriteLine("Created new device with generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
                Console.WriteLine("Device existed return device with generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            }

            return device;
        }

        /// <summary>
        /// Register a list of devices
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        public static async Task<BulkRegistryOperationResult> AddDevices2Async(IEnumerable<Device> devices)
        {
            BulkRegistryOperationResult operationResults;

            try
            {
                operationResults = await registryManager.AddDevices2Async(devices);
                return operationResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }
        }

        /// <summary>
        /// Remove a device from the IoT Hub Registry using a reference to the device
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns></returns>
        public static async Task RemoveDeviceAsync(Device device)
        {

            try
            {
                await registryManager.RemoveDeviceAsync(device);
            }
            catch (DeviceNotFoundException ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }


            Console.WriteLine($"Delete device with DeviceId {device.Id}");

            return;
        }

        ///// <summary>
        ///// Remove a device fromthe IoT Hub Registry using the device id
        ///// </summary>
        ///// <param name="deviceId">A string representing the Device ID</param>
        ///// <returns></returns>
        public static async Task RemoveDeviceAsync(string deviceId)
        {

            try
            {
                await registryManager.RemoveDeviceAsync(deviceId);
            }
            catch (DeviceNotFoundException ex)
            {
                Console.WriteLine($"Could not delete Device with DeviceId  {($"{Utils.FormatExceptionMessage(ex)}")}");
                throw ex;
            }


            Console.WriteLine($"Delete device with DeviceId {deviceId}");

            return;
        }

        /// <summary>
        /// Retrieves specified number of devices from every Iot Hub partition. This is an
        ///     approximation and not a definitive list. Results are not ordered.
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns>List of devices</returns>
        public static async Task<IEnumerable<Device>> GetDevicesAsync(int maxCount)
        {
            try
            {
                var results = await registryManager.GetDevicesAsync(maxCount);
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Utils.FormatExceptionMessage(ex)}");
                throw ex;
            }
        }

    }
}
