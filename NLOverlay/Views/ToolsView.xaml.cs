using NLOverlay.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace NLOverlay.Views
{
    /// <summary>
    /// Interaction logic for ToolsView.xaml
    /// </summary>
    public partial class ToolsView : UserControl
    {
        public ToolsView()
        {
            InitializeComponent();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Settings.GetFilePath());
        }

        private void EnableElevated_Click(object sender, RoutedEventArgs e)
        {
            var path = "C:\\ProgramData\\Locktime\\NetLimiter\\5\\nl_settings.xml";
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            var rootElement = xmlDoc.DocumentElement;
            var requireElevationNodes = rootElement.GetElementsByTagName("RequireElevationLocal");

            if (requireElevationNodes.Count > 0)
            {
                foreach (XmlNode node in requireElevationNodes)
                {
                    if (node.NodeType == XmlNodeType.Element && node.InnerText.ToLower() == "true")
                    {
                        node.InnerText = "false";
                    }
                }
            }
            else
            {
                // Create the RequireElevationLocal node under the root
                var requireElevationNode = xmlDoc.CreateElement("RequireElevationLocal");
                requireElevationNode.InnerText = "false";
                rootElement.AppendChild(requireElevationNode);
            }
            xmlDoc.Save(path);
        }
    }
}
