using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityWeld;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class LoadingPopUpPage : PageViewModel
    {
        private bool _load;

        [Binding]
        public bool Load
        {
            get => _load;
            set
            {
                _load = value;
                OnPropertyChanged(nameof(Load));
            }
        }
    }
}