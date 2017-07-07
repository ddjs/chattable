using System;
using System.Collections.Generic;
using System.Text;

namespace Chattable
{
    public class ExtendedListView : Xamarin.Forms.ListView
    {
        public ExtendedListView()
        {
            this.ItemAppearing += this.ExtendedListView_ItemAppearing;
        }

        private void ExtendedListView_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {
          
        }
    }
}
