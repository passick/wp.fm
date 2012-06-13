﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;

namespace lastfm
{
    public partial class tagSearchPage : PhoneApplicationPage
    {
        ObservableCollection<tagInfo> lstResults = new ObservableCollection<tagInfo>();
        ProgressIndicator prog;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string searchText = "";
            if (NavigationContext.QueryString.TryGetValue("searchText", out searchText))
                getList(searchText);
        }

        public tagSearchPage()
        {
            InitializeComponent();
            this.DataContext = lstResults;
            prog = new ProgressIndicator();
        }

        private async void getList(string searchText)
        {
            txtSearchBox.Text = searchText;
            SystemTray.SetProgressIndicator(this, prog);
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            prog.Text = "Loading...";
            foreach (tagInfo info in await tag.search(searchText))
                lstResults.Add(info);
            prog.IsIndeterminate = false;
            prog.IsVisible = false;
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                getList(txtSearchBox.Text);
        }

        private void searchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex != -1)
                this.NavigationService.Navigate(new Uri("/tagInfoPage.xaml?tagName="+((tagInfo)((ListBox)sender).SelectedItem).name, UriKind.Relative));
        }
    }
}