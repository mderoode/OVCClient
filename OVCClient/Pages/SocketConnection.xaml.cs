using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OVCClient.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SocketConnection : Page
    {
        SocketClient socketClient;


        public SocketConnection()
        {
            this.InitializeComponent();

            //init();
        }

        public async void init()
        {
            Output.Text += "Trying to connect " + Environment.NewLine;

            socketClient = new SocketClient();
            await socketClient.connect("192.168.9.108", "9001", "echo init connection");

            Output.Text += "Connected to socket " + Environment.NewLine;
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {

            //await socketClient.send("echo 9");
            //Output.Text += await socketClient.read() + Environment.NewLine;

           String text = await DoCommand("echo test3");
            Output.Text += "Received: " + text;
        }

        private async Task<string> DoCommand(string command)
        {
            StringBuilder strBuilder = new StringBuilder();
            using (StreamSocket clientSocket = new StreamSocket())
            {
                await clientSocket.ConnectAsync(new HostName("192.168.9.108"),  "9001");
                using (DataWriter writer = new DataWriter(clientSocket.OutputStream))
                {
                    writer.WriteString(command);
                    await writer.StoreAsync();
                    writer.DetachStream();
                }
                using (DataReader reader = new DataReader(clientSocket.InputStream))
                {
                    reader.InputStreamOptions = InputStreamOptions.Partial;
                    await reader.LoadAsync(8192);
                    while (reader.UnconsumedBufferLength > 0)
                    {
                        strBuilder.Append(reader.ReadString(reader.UnconsumedBufferLength));
                        await reader.LoadAsync(8192);
                    }
                    reader.DetachStream();
                }
            }
            return (strBuilder.ToString());
        }
    }

    public class SocketClient
    {
        StreamSocket socket;

        /// <summary>
        /// CONNECT TO SERVER
        /// </summary>
        /// <param name="host">Host name/IP address</param>
        /// <param name="port">Port number</param>
        /// <param name="message">Message to server</param>
        /// <returns>Response from server</returns>
        public async Task connect(string host, string port, string message)
        {
            HostName hostName;

            using (socket = new StreamSocket())
            {
                hostName = new HostName(host);

                // Set NoDelay to false so that the Nagle algorithm is not disabled
                socket.Control.NoDelay = false;

                try
                {
                    // Connect to the server
                    await socket.ConnectAsync(hostName, port);
                    // Send the message
                    await this.send(message);
                    //// Read response
                    String receivedText = await this.read();
                    System.Diagnostics.Debug.WriteLine("Done");
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            // Handle HostNotFound Error
                            throw;
                        default:
                            // If this is an unknown status it means that the error is fatal and retry will likely fail.
                            throw;
                    }
                }
            }
        }

        /// <summary>
        /// SEND DATA
        /// </summary>
        /// <param name="message">Message to server</param>
        /// <returns>void</returns>
        public async Task send(string message)
        {
            DataWriter writer;

            // Create the data writer object backed by the in-memory stream. 
            using (writer = new DataWriter(socket.OutputStream))
            {
                // Set the Unicode character encoding for the output stream
                writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                // Specify the byte order of a stream.
                writer.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                // Gets the size of UTF-8 string.
                writer.MeasureString(message);
                // Write a string value to the output stream.
                writer.WriteString(message);

                // Send the contents of the writer to the backing stream.
                try
                {
                    await writer.StoreAsync();
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            // Handle HostNotFound Error
                            throw;
                        default:
                            // If this is an unknown status it means that the error is fatal and retry will likely fail.
                            throw;
                    }
                }

                await writer.FlushAsync();
                // In order to prolong the lifetime of the stream, detach it from the DataWriter
                writer.DetachStream();
            }
        }

        /// <summary>
        /// READ RESPONSE
        /// </summary>
        /// <returns>Response from server</returns>
        public async Task<String> read()
        {
            DataReader reader;
            StringBuilder strBuilder;

            using (reader = new DataReader(socket.InputStream))
            {
                strBuilder = new StringBuilder();

                // Set the DataReader to only wait for available data (so that we don't have to know the data size)
                reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;
                // The encoding and byte order need to match the settings of the writer we previously used.
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                // Send the contents of the writer to the backing stream. 
                // Get the size of the buffer that has not been read.
                await reader.LoadAsync(256);

                // Keep reading until we consume the complete stream.
                while (reader.UnconsumedBufferLength > 0)
                {
                    strBuilder.Append(reader.ReadString(reader.UnconsumedBufferLength));
                    await reader.LoadAsync(256);
                }

                reader.DetachStream();
                return strBuilder.ToString();
            }
        }
    }
}
