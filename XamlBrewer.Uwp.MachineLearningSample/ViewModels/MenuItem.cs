using System;
using System.Windows.Input;

namespace Mvvm
{
    internal class MenuItem : BindableBase
    {
        private string _glyph;
        private string _text;
        private DelegateCommand _command;
        private Type _navigationDestination;

        public string Glyph
        {
            get { return _glyph; }
            set { SetProperty(ref _glyph, value); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public ICommand Command
        {
            get { return _command; }
            set { SetProperty(ref _command, (DelegateCommand)value); }
        }

        public Type NavigationDestination
        {
            get { return _navigationDestination; }
            set { SetProperty(ref _navigationDestination, value); }
        }

        public bool IsNavigation => _navigationDestination != null;
    }
}
