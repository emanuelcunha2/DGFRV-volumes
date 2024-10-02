using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAllocationApp
{
    public class AllocationItem : ViewModelBase
    {
        private string _partnumber = string.Empty;
        private double _coverage;
        private int _qty;
        private string _supplier = string.Empty;
        private string _worked = string.Empty;
        private string _guia = string.Empty;
        private string _location = string.Empty;
        private string _deliveryNote = string.Empty;
        private bool _quality = false;
        private string _building = string.Empty;
        private string _status = string.Empty;
        private string _statusString = string.Empty;
        private Color _color = Color.FromHex("#c2c2c2");
        private bool _itemVisibility = true;
        private GridLength _gridLength = GridLength.Auto;
        private Boolean _hasOtherGuias;

        public bool HasOtherGuias
        {
            get => _hasOtherGuias;
            set
            {
                if (_hasOtherGuias != value)
                {
                    _hasOtherGuias = value;
                    OnPropertyChanged();
                }
            }
        }

        public GridLength GridLength
        {
            get => _gridLength;
            set
            {
                _gridLength = value;
                OnPropertyChanged();
            }
        }

        public bool ItemVisibility
        {
            get => _itemVisibility;
            set
            {
                if (_itemVisibility != value)
                {
                    _itemVisibility = value;
                    OnPropertyChanged();
                }
            }
        }
        public Color Color 
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public string StatusString
        {
            get { return _statusString; }
            set
            {
                if (_statusString != value)
                {
                    _statusString = value;
                    OnPropertyChanged();
                }
            }

        }
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PartNumber
        {
            get { return _partnumber; }
            set
            {
                if (_partnumber != value)
                {
                    _partnumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Coverage
        {
            get { return _coverage; }
            set
            {
                if (_coverage != value)
                {
                    _coverage = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Qty
        {
            get { return _qty; }
            set
            {
                if (_qty != value)
                {
                    _qty = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Supplier
        {
            get { return _supplier; }
            set
            {
                if (_supplier != value)
                {
                    _supplier = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Worked
        {
            get { return _worked; }
            set
            {
                if (_worked != value)
                {
                    _worked = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Guia
        {
            get { return _guia; }
            set
            {
                if (_guia != value)
                {
                    _guia = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Location
        {
            get { return _location; }
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DeliveryNote
        {
            get { return _deliveryNote; }
            set
            {
                if (_deliveryNote != value)
                {
                    _deliveryNote = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Quality
        {
            get { return _quality; }
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Building
        {
            get { return _building; }
            set
            {
                if (_building != value)
                {
                    _building = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
