
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;

namespace LaptopKeyboardController
{
    public partial class MainWindow : Window
    {
        private HashSet<string>? initialDevices;

        public MainWindow()
        {
            InitializeComponent();
        }

        private HashSet<string> GetKeyboards()
        {
            HashSet<string> devices = new();

            using var searcher =
                new ManagementObjectSearcher(
                    "SELECT PNPDeviceID FROM Win32_Keyboard");

            using var results = searcher.Get();

            foreach (ManagementObject keyboard in results)
            {
                string? id =
                    keyboard["PNPDeviceID"]?.ToString();

                if (!string.IsNullOrEmpty(id))
                {
                    devices.Add(id);
                }
            }

            return devices;
        }

        private void SaveScan_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                initialDevices = GetKeyboards();

                KeyboardList.Text =
                    $"Saved {initialDevices.Count} devices:\n\n" +
                    string.Join("\n", initialDevices);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CompareDevices_Click(
            object sender, RoutedEventArgs e)
        {
            if (initialDevices == null)
            {
                MessageBox.Show(
                    "Please save an initial scan first.");
                return;
            }

            try
            {
                HashSet<string> currentDevices =
                    GetKeyboards();

                var removed =
                    initialDevices.Except(currentDevices);

                var added =
                    currentDevices.Except(initialDevices);

                StringBuilder output = new();

                output.AppendLine(
                    $"Initial: {initialDevices.Count}");

                output.AppendLine(
                    $"Current: {currentDevices.Count}");

                output.AppendLine();

                output.AppendLine("Removed devices:");

                foreach (string id in removed)
                {
                    output.AppendLine(id);
                }

                output.AppendLine();

                output.AppendLine("Added devices:");

                foreach (string id in added)
                {
                    output.AppendLine(id);
                }

                KeyboardList.Text = output.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void InspectDevices_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder output = new();

                using var searcher =
                    new ManagementObjectSearcher(
                        "SELECT DeviceID, Name, Manufacturer, " +
                        "Service, Status, ConfigManagerErrorCode " +
                        "FROM Win32_PnPEntity");

                using var devices = searcher.Get();

                int count = 0;

                // Only inspect devices found in keyboard scans
                HashSet<string> keyboardIds = GetKeyboards();

                foreach (ManagementObject device in devices)
                {
                    string id =
                        device["DeviceID"]?.ToString() ?? "";

                    if (!keyboardIds.Contains(id))
                        continue;

                    count++;

                    output.AppendLine($"DEVICE {count}");
                    output.AppendLine(
                        $"Name: {device["Name"]}");

                    output.AppendLine(
                        $"Manufacturer: {device["Manufacturer"]}");

                    output.AppendLine(
                        $"Driver Service: {device["Service"]}");

                    output.AppendLine(
                        $"Status: {device["Status"]}");

                    output.AppendLine(
                        $"Problem Code: {device["ConfigManagerErrorCode"]}");

                    output.AppendLine(
                        $"Device ID: {id}");

                    output.AppendLine(
                        "------------------------------");
                }

                KeyboardList.Text = count > 0
                    ? output.ToString()
                    : "No matching PnP devices found.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Inspection failed: {ex.Message}");
            }
        }

    }
}
