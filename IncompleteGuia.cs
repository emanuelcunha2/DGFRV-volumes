using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAllocationApp
{
    public class IncompleteGuia : ViewModelBase
    {
        private DateTime _dtDocumento;
        private string _nrMaterial;
        private double _deltaQty;
        private string _worked;
        private string _nrGuiaRect;
        private bool _guiaPrepared;
        private string _location;
        private bool _isMainGuia = false;
        private string _dn;

        // Document Date
        public DateTime DtDocumento
        {
            get => _dtDocumento;
            set
            {
                _dtDocumento = value;
                OnPropertyChanged(nameof(DtDocumento));
            }
        }

        // Material Number
        public string NrMaterial
        {
            get => _nrMaterial;
            set
            {
                _nrMaterial = value;
                OnPropertyChanged(nameof(NrMaterial));
            }
        }

        // Delta Quantity
        public double DeltaQty
        {
            get => _deltaQty;
            set
            {
                _deltaQty = value;
                OnPropertyChanged(nameof(DeltaQty));
            }
        }

        // Work Status
        public string Worked
        {
            get => _worked;
            set
            {
                _worked = value;
                OnPropertyChanged(nameof(Worked));
            }
        }

        // Guide Rectified
        public string NrGuiaRect
        {
            get => _nrGuiaRect;
            set
            {
                _nrGuiaRect = value;
                OnPropertyChanged(nameof(NrGuiaRect));
            }
        }

        public bool GuiaPrepared
        {
            get => _guiaPrepared;
            set
            {
                _guiaPrepared = value;
                OnPropertyChanged(nameof(GuiaPrepared));
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        public bool IsMainGuia
        {
            get => _isMainGuia;
            set
            {
                _isMainGuia = value;
                OnPropertyChanged();
            }
        }

        public string Dn
        {
            get => _dn;
            set
            {
                _dn = value;
                OnPropertyChanged(nameof(Dn));
            }
        }

    }
}
