using Jint;
using System;
using System.Collections.Generic;
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

namespace Pebble_Time_Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JavascriptPage : Page
    {
        public JavascriptPage()
        {
            this.InitializeComponent();
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var engine = new Engine()
                    .SetValue("log", new Action<object>(Debug))
                    .SetValue("localStorage", new LocalStorage());
                    ;

                engine.Execute(txtScript.Text);
            }
            catch (Exception exp)
            {
                txtResult.Text = exp.Message;
            }
        }

        private void Debug(object text)
        {
            txtResult.Text = text.ToString();
        }

        private class LocalStorage
        {
            public object getItem(String obj)
            {
                return "{}";
            }
        }
    }
}
