using System;
using System.Windows;
using ExcelAddinUI.Utilities;

namespace ExcelAddinUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServiceDocumentParser serviceDocParser;
        ServiceDocument serviceDocument;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        public delegate void DocumentDownloadCompletedDelegate();

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadCollectionsInService("http://odata.netflix.com/v2/Catalog/");
        }

        private void LoadCollectionsInService(string serviceUri)
        {
            serviceDocParser = new ServiceDocumentParser();
            serviceDocParser.ParseComplete += new ServiceDocumentParser.OnParseComplete(serviceDocParserParseComplete);
            serviceDocParser.BeginParse(serviceUri);
        }

        void serviceDocParserParseComplete(ServiceDocument serviceDocument, Exception Error)
        {
            if (Error == null)
            {
                this.serviceDocument = serviceDocument;
                Dispatcher.BeginInvoke(new DocumentDownloadCompletedDelegate(DisplayServiceCollections), null);
            }
        }

        public void DisplayServiceCollections()
        {
            lbCollections.ItemsSource = serviceDocument.EntitySetUris;
        }
    }
}
