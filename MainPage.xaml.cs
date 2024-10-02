
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using System.Windows.Input;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Collections.Generic;

namespace CheckAllocationApp
{
    public partial class MainPage : ContentPage
    {
        private bool _isShowPreparedToggled = false;
        public bool IsShowPreparedToggled
        {
            get => _isShowPreparedToggled;
            set
            {
                _isShowPreparedToggled = value;
                OnPropertyChanged();
            }
        }

        private Color _filterTextColor = Colors.Black;
        public Color FilterTextColor
        {
            get => _filterTextColor;
            set
            {
                _filterTextColor = value;
                OnPropertyChanged();
            }
        }

        private string _chosenFilter = "Limpar Filtros";
        public string ChoosenFilter
        {
            get => _chosenFilter;
            set
            {
                _chosenFilter = value;
                IsFilterVisible = value != "Limpar Filtros";

                if(IsFilterVisible) { FilterTextColor = Colors.DarkSlateBlue; }
                else { FilterTextColor = Colors.Black; }

                OnPropertyChanged();
            }
        }

        private bool _isFilterVisible = false;
        public bool IsFilterVisible
        {
            get { return _isFilterVisible; }
            set
            {
                _isFilterVisible = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<string> StatusList { get; set; } = new ObservableCollection<string>() 
        {
            "Preparado",
            "Não Encontrado",
            "Pendente"
        };

        public Dictionary<string, string> StatusColorDictionary { get; set; } = new Dictionary<string, string>()
        {
            { "Preparado", "#6be86b" },                   // Verde
            { "Preparado Previamente", "#75ade6" },       // Azul
            { "Preparado Com Outra Guia", "#fa9b5c" },    // Laranja
            { "Não Encontrado", "#ed4747" },              // Vermelho
            { "Pendente", "#c2c2c2" },                    // Cinza
            { "Guia a ser trabalhada", "#f5db49" }        // Amarelo
        };

        public List<string> SupplierList { get; set; } = new();
        public List<string> PartNumberList { get; set; } = new();
        public List<string> BuildingList { get; set; } = new() { "ED1", "BSI" };
        
        public List<AllocationItem> AllocationItemsUnfiltered { get; set; } = new();
        public ObservableCollection<AllocationItem> AllocationItems { get; set; } = new ObservableCollection<AllocationItem>();
        public ObservableCollection<String> SearchResults { get; set; } = new();
        public ICommand ClickedItem { get; }
        public ICommand ClickedItemIncompleteGuias { get; }
        public ICommand ClickedFilter { get; }
        private IDispatcherTimer _timer;
        private bool _isAlertWindowOpen = false;
        public bool _isGuiasAlertOpen = false;
        private int _countWarnings = 0;

        private string db_string = "user id=program_user;password=praga;initial catalog=PCL;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=60";

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;
            try
            {
                _timer = this.Dispatcher.CreateTimer();
                _timer.Interval = TimeSpan.FromSeconds(20);
                _timer.Tick += async (s, e) =>
                {
                    await Task.Run(async() =>
                    {
                        // Code here runs on a background thread
                        if (GetMaterialWarning() == "on" && _isAlertWindowOpen == false && _countWarnings < 3)
                        {
                            _isAlertWindowOpen = true;
                            _countWarnings++;

                            await this.Dispatcher.DispatchAsync(async() =>
                            {
                                // Update UI here
                                await Application.Current.MainPage.DisplayAlert(
                                    "Alerta",
                                    "Falta de Material No Posto de Labeling. \n Alertar Supervisor",
                                    "OK"
                                );
                            });

                            _isAlertWindowOpen = false;
                        }
                        else if(GetMaterialWarning() != "on")
                        {
                            _countWarnings = 0;
                        }

                        var list = await GetAllAllocationItemsAsync(db_string);
                        SetFilterCampsData(list);

                        bool foundDiference = false;
                        bool numberDiference = false;

                        if (list.Count() != AllocationItemsUnfiltered.Count())
                        {
                            foundDiference = true;
                            numberDiference = true;
                        }
                        else
                        {
                            int count = 0;
                            foreach (AllocationItem item in list)
                            {
                                if (item.PartNumber != AllocationItemsUnfiltered[count].PartNumber
                                   || item.Worked != AllocationItemsUnfiltered[count].Worked
                                   || item.Status != AllocationItemsUnfiltered[count].Status)
                                {
                                    foundDiference = true;
                                }
                                count++;
                            }
                        }

                        // If found diference update
                        if (foundDiference)
                        {
                             
                            // Doesnt let user know if the diference isn't in numbers
                            if (numberDiference && _isAlertWindowOpen == false)
                            {
                                _isAlertWindowOpen = true;

                                await Task.Run(async() =>
                                {
                                    var list = await GetAllAllocationItemsAsync(db_string);

                                    await this.Dispatcher.DispatchAsync(async () =>
                                    {
                                        list = await GetAllAllocationItemsAsync(db_string);
                                        SetAllocationItems(list);
                                        // Update UI here
                                        await Application.Current.MainPage.DisplayAlert(
                                            "Alerta Update",
                                            "A lista de alocação foi atualizada",
                                            "OK"
                                        );

                                        _isAlertWindowOpen = false;
                                    });
                                });

                            }
                            else if (!numberDiference && _isAlertWindowOpen == false)
                            {
                                await Task.Run(async () =>
                                {
                                    var list = await GetAllAllocationItemsAsync(db_string);

                                    // Update the UI on the main thread
                                    this.Dispatcher.Dispatch(() =>
                                    {
                                        SetAllocationItems(list);
                                    });
                                });
                            }
                        }
                    });
                };
                _timer.Start();
             
                var list = GetAllAllocationItems(db_string);

                SetFilterCampsData(list);
                SetAllocationItems(list);

                ClickedItemIncompleteGuias = new RelayCommand(async(parameter) =>
                {
                    if (parameter is AllocationItem item)
                    {
                        if(_isGuiasAlertOpen == true) { return; }
                        _isGuiasAlertOpen = true;

                        var modal = new ItemIncompleteGuias(item.PartNumber, item.Guia, this, item);
                        modal.BindingContext = modal;
                         
                        _isGuiasAlertOpen = true;
                        await Shell.Current.Navigation.PushAsync(modal);
                    }
 
                });

                ClickedItem = new RelayCommand(async (parameter) =>
                {
                }); 

            }
            catch(Exception ex)
            {
                Application.Current.MainPage.DisplayAlert(
                        "Erro",
                        ""+ex.Message,
                        "OK"
                    );
            }
        }

        private void Modal_Disappearing(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SetFilterCampsData(List<AllocationItem> AllocationItems)
        {
            // Supplier and Partnumber Items
            foreach(AllocationItem item in AllocationItems)
            {
                if(SupplierList.Where(x=>x == item.Supplier).Count() == 0) { SupplierList.Add(item.Supplier); }
                if (PartNumberList.Where(x => x == item.PartNumber).Count() == 0) { PartNumberList.Add(item.PartNumber); }
            }
        }

        public void SetAllocationItems (List<AllocationItem> list)
        {
            AllocationItems.Clear();
            AllocationItemsUnfiltered.Clear();

            foreach (AllocationItem item in list)
            {
                item.Color = Color.FromHex(StatusColorDictionary[item.Status]);

                AllocationItemsUnfiltered.Add(item);

                if (!IsShowPreparedToggled)
                {
                    if((item.Status != "Preparado" && item.Status != "Preparado Previamente"))
                    {
                        AllocationItems.Add(item);
                    }
                }
                else
                {
                    AllocationItems.Add(item);
                }
            }

            if (IsFilterVisible)
            {
                SearchBar_TextChanged(FilterSB, new TextChangedEventArgs("", "x")); 
            }
        }

        public async void SetStatusGuia(object parameter, string selectedStatus)
        {
            if (parameter is AllocationItem item)
            {
                item.Status = selectedStatus;
                item.Color = Color.FromHex(StatusColorDictionary[selectedStatus]);

                UpdateAllocationItemStatus(db_string,
                    item.PartNumber, item.Guia, item.Status);
            }
        }

        public void SetPreparedToggleVisibilty()
        {
            if (IsFilterVisible) 
            {
                SearchBar_TextChanged(FilterSB, new TextChangedEventArgs("","x"));;
                return;
            }

            AllocationItems.Clear();
            List<AllocationItem> FilteredItems = new();

            if (IsShowPreparedToggled)
            {
                FilteredItems = AllocationItemsUnfiltered.ToList();
            }
            else
            {
                FilteredItems = AllocationItemsUnfiltered.Where(i => i.Status != "Preparado" && i.Status != "Preparado Previamente").ToList();
            }

            foreach (AllocationItem item in FilteredItems)
            {
                AllocationItems.Add(item);
            }
        }
         


        public static List<AllocationItem> GetAllAllocationItems(string connectionString)
        {
            List<AllocationItem> allocationItems = new List<AllocationItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand("GetAllocationItems", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AllocationItem allocationItem = new AllocationItem
                            {
                                PartNumber = reader["partnumber"].ToString(),
                                Coverage = Convert.ToDouble(reader["coverage"]),
                                Qty = Convert.ToInt32(reader["qty"]),
                                Supplier = reader["supplier"].ToString(),
                                Worked = reader["worked"].ToString(),
                                Guia = reader["guia"].ToString(),
                                Location = reader["location"].ToString(),
                                DeliveryNote = reader["deliveryNote"].ToString(),
                                Quality = Convert.ToBoolean(reader["quality"]),
                                Building = reader["building"].ToString(),
                                Status = reader["status"].ToString()
                            };
                            allocationItem.Coverage = Math.Round(allocationItem.Coverage, 1);
                            allocationItems.Add(allocationItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return allocationItems;
        }
                 
        public static async Task<List<AllocationItem>> GetAllAllocationItemsAsync(string connectionString)
        {
            List<AllocationItem> allocationItems = new List<AllocationItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand("GetAllocationItems", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            AllocationItem allocationItem = new AllocationItem
                            {
                                PartNumber = reader["partnumber"].ToString(),
                                Coverage = Convert.ToDouble(reader["coverage"]),
                                Qty = Convert.ToInt32(reader["qty"]),
                                Supplier = reader["supplier"].ToString(),
                                Worked = reader["worked"].ToString(),
                                Guia = reader["guia"].ToString(),
                                Location = reader["location"].ToString(),
                                DeliveryNote = reader["deliveryNote"].ToString(),
                                Quality = Convert.ToBoolean(reader["quality"]),
                                Building = reader["building"].ToString(),
                                Status = reader["status"].ToString()
                            };
                            allocationItem.Coverage = Math.Round(allocationItem.Coverage, 1);
                            allocationItems.Add(allocationItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            } 
            return allocationItems;
        }


        // Helper function to check if otheer guia has other matches by partnumber
        private static bool DoOtherUnpreparedGuiasExist(SqlConnection connection, string partNumber, string guia)
        {
            string storedProcedureName = "GetMaterialDetails";

            using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@NrMaterial", partNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string existingGuia = reader["Nr Guia Rect"].ToString();
                        if (!existingGuia.Equals(guia, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            } 
            return false;
        }

        public static void UpdateAllocationItemStatus(string connectionString, string partnumber, string guia, string statusX)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(@"UPDATE AllocationItems
                                                             SET status = @statusX
                                                             WHERE partnumber = @partnumber AND guia = @guia", connection))
                {
                    command.Parameters.AddWithValue("@statusX", statusX);
                    command.Parameters.AddWithValue("@partnumber", partnumber);
                    command.Parameters.AddWithValue("@guia", guia);
                    connection.Open();

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Status updated successfully to {statusX} for PartNumber: {partnumber}, Guia: {guia}");
                    }
                    else
                    {
                        Console.WriteLine($"No records found for PartNumber: {partnumber}, Guia: {guia}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
         
        public static void UpdateNewGuia(string connectionString, string partnumber, string guia, string newGuia)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(@"UPDATE AllocationItems
                                                             SET NewGuia = @newGuia
                                                             WHERE partnumber = @partnumber AND guia = @guia", connection))
                {
                    command.Parameters.AddWithValue("@newGuia", newGuia);
                    command.Parameters.AddWithValue("@partnumber", partnumber);
                    command.Parameters.AddWithValue("@guia", guia);

                    connection.Open();

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"NewGuia updated successfully to {newGuia} for PartNumber: {partnumber}, Guia: {guia}");
                    }
                    else
                    {
                        Console.WriteLine($"No records found for PartNumber: {partnumber}, Guia: {guia}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            SetPreparedToggleVisibilty();
        }

        private async void ImageButton_Clicked(object sender, EventArgs e)
        {
            var selectedFilter = await Application.Current.MainPage.DisplayActionSheet(
                "Escolher Filtro",
                "Cancelar",
                null,
                "Supplier",
                "Edificio",
                "Part Number",
                "Location",
                "Limpar Filtros"
            );

            if (selectedFilter != null && selectedFilter != "Cancelar")
            {
                ChoosenFilter = selectedFilter.ToString();
            }

            if (ChoosenFilter == "Limpar Filtros")
            {
                FilterSB.Text = "";
                SetPreparedToggleVisibilty();
                return;
            } 
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {

            SearchBar searchBar = (SearchBar)sender;
            if (ChoosenFilter == "Limpar Filtros") 
            {
                searchBar.Text = "";
                SetPreparedToggleVisibilty();
                return;
            }

            AllocationItems.Clear();
            List<AllocationItem> FilteredItems = new();

            if (IsShowPreparedToggled)
            {
                switch (ChoosenFilter)
                {
                    case "Supplier":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Supplier.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                    case "Part Number":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.PartNumber.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                    case "Edificio":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Building.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                    case "Location":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Location.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                }
            }
            else
            {
                switch (ChoosenFilter)
                {
                    case "Supplier":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Status != "Preparado" && i.Status != "Preparado Previamente"
                        && i.Supplier.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                    case "Part Number":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Status != "Preparado" && i.Status != "Preparado Previamente"
                        && i.PartNumber.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                    case "Edificio":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Status != "Preparado" && i.Status != "Preparado Previamente"
                        && i.Building.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                    case "Location":
                        FilteredItems = AllocationItemsUnfiltered.Where(i => i.Status != "Preparado" && i.Status != "Preparado Previamente"
                        && i.Location.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        break;
                }
            }

            foreach (AllocationItem item in FilteredItems)
            {
                AllocationItems.Add(item);
            }

        }

        public static string GetMaterialWarning()
        {
            string res = "";
            string db_string = "user id=program_user;password=praga;initial catalog=PCL;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=60";

            using (SqlConnection cn = new SqlConnection(db_string))
            using (SqlCommand cmd = new SqlCommand("SELECT value FROM [PCL].[dbo].[DatabaseVariables] WHERE variable = 'RECP_materialWarning' AND value = 'on'", cn))
            {
                cn.Open();

                cmd.CommandType = CommandType.Text;

                try
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            res = dr["value"].ToString();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    res = "error";
                }
            }

            return res;
        }
    }
}