using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data; 
using System.Windows.Input; 

namespace CheckAllocationApp;

public partial class ItemIncompleteGuias : ContentPage
{
	public ICommand CancelCommand { get; }
    public ICommand ConfirmCommand { get; }

    public ObservableCollection<IncompleteGuia> IncompleteGuias { get; set; } = new();
    public bool WasConfirmed = false;
    private MainPage _parentPage;
    private AllocationItem _allocationObj;
    private string db_string = "user id=program_user;password=praga;initial catalog=PCL;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=60";

    public ItemIncompleteGuias(string partNumber, string sourceGuia, MainPage parentPage, AllocationItem allocationObj)
    {
        InitializeComponent();
        _parentPage = parentPage;
        _allocationObj = allocationObj;

        InitializeData(partNumber, sourceGuia);
        CancelCommand = new RelayCommand(async() =>
		{
            WasConfirmed = false;
            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
                _parentPage._isGuiasAlertOpen = false;
            }
        });

        ConfirmCommand = new RelayCommand(async() =>
        {   
            foreach(IncompleteGuia guia in IncompleteGuias)
            {
                if (guia.GuiaPrepared)
                {
                    InsertGuiaIfNotExists(db_string
                        , guia.NrGuiaRect);
                }
                else
                {
                    DeleteGuia(db_string
    , guia.NrGuiaRect);
                }
            }

            WasConfirmed = true;
            if (Navigation.ModalStack.Count > 0)
            {
                OnClosingExecuteThis();
                await Navigation.PopModalAsync();
                _parentPage._isGuiasAlertOpen = false;
            }
        }); 
    }

    public void OnClosingExecuteThis()
    {
        bool isMainGuiaPrepared = false;
        bool oneOrMorePrepared = false;

        // Check the prepared status of each Guia
        foreach (IncompleteGuia guia in IncompleteGuias)
        {
            if (guia.GuiaPrepared)
            {
                oneOrMorePrepared = true;
                if (guia.IsMainGuia)
                {
                    isMainGuiaPrepared = true;
                }
            }
        }

        // Set the status based on the prepared condition
        if (oneOrMorePrepared)
        {
            if (isMainGuiaPrepared)
            {
                _parentPage.SetStatusGuia(_allocationObj, "Preparado");
            }
            else
            {
                _parentPage.SetStatusGuia(_allocationObj, "Preparado");
            }
        }
        else
        {
            _parentPage.SetStatusGuia(_allocationObj, "Pendente");
        }
    }
 

    public static async Task<List<IncompleteGuia>> GetMaterialDetailsAsync(string connectionString, string materialNumber)
    {
        List<IncompleteGuia> incompleteGuiasList = new List<IncompleteGuia>();

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("GetMaterialDetails", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@NrMaterial", materialNumber);

                await connection.OpenAsync();

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        IncompleteGuia guias = new IncompleteGuia
                        {
                            DtDocumento = reader.GetDateTime(reader.GetOrdinal("DtDocumento")),
                            NrMaterial = reader.GetString(reader.GetOrdinal("NrMaterial")),
                            DeltaQty = Convert.ToDouble(reader["deltaQty"]),
                            Worked = reader.GetString(reader.GetOrdinal("WORKED")),
                            NrGuiaRect = reader.GetString(reader.GetOrdinal("Nr Guia Rect")),
                            GuiaPrepared = reader.GetInt32(reader.GetOrdinal("ExistsInAllocationPreparedGuias")) == 1,
                            Location = reader.GetString(reader.GetOrdinal("GuiaLocation")),
                            Dn = reader.GetString(reader.GetOrdinal("Dn")),
                        };
                        incompleteGuiasList.Add(guias);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Thread.Sleep(500);
        return incompleteGuiasList;
    }
     
    public static List<IncompleteGuia> GetMaterialDetails(string connectionString, string materialNumber)
    {
        List<IncompleteGuia> incompleteGuiasList = new List<IncompleteGuia>(); 
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("GetMaterialDetails", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@NrMaterial", materialNumber);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        IncompleteGuia guias = new IncompleteGuia
                        {
                            DtDocumento = reader.GetDateTime(reader.GetOrdinal("DtDocumento")),
                            NrMaterial = reader.GetString(reader.GetOrdinal("NrMaterial")),
                            DeltaQty = Convert.ToDouble(reader["deltaQty"]),
                            Worked = reader.GetString(reader.GetOrdinal("WORKED")),
                            NrGuiaRect = reader.GetString(reader.GetOrdinal("Nr Guia Rect")),
                            GuiaPrepared = reader.GetInt32(reader.GetOrdinal("ExistsInAllocationPreparedGuias")) == 1,
                            Location = reader.GetString(reader.GetOrdinal("GuiaLocation")),
                            Dn = reader.GetString(reader.GetOrdinal("Dn")),
                        };

                        incompleteGuiasList.Add(guias);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        return incompleteGuiasList;
    }
     
    public async void InitializeData(string partNumber, string sourceGuia)
    {
        var list = await Task.Run(() => GetMaterialDetailsAsync(db_string
, partNumber));

        this.Dispatcher.Dispatch(() =>
        {
            foreach (IncompleteGuia item in list)
            {
                if (sourceGuia == item.NrGuiaRect)
                {
                    item.IsMainGuia = true;
                }
                IncompleteGuias.Add(item);
            }
        });
    }

    public static bool InsertGuiaIfNotExists(string connectionString, string guia)
    {
        bool isSuccess = false;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to check if a record with the same guia already exists
                string checkQuery = @"SELECT COUNT(*) 
                                  FROM [PCL].[dbo].[AllocationPreparedGuias]
                                  WHERE [guia] = @guia";

                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@guia", guia);

                    connection.Open();

                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        // Insert the new record if not found
                        string insertQuery = @"INSERT INTO [PCL].[dbo].[AllocationPreparedGuias]
                                          ([guia])
                                          VALUES (@guia)";

                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@guia", guia);

                            int rowsAffected = insertCommand.ExecuteNonQuery();
                            isSuccess = (rowsAffected > 0);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Record already exists with the given guia.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return isSuccess;
    }
     
    public static bool DeleteGuia(string connectionString, string guia)
    {
        bool isSuccess = false;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Define the SQL query for deleting a record
                string query = @"DELETE FROM [PCL].[dbo].[AllocationPreparedGuias]
                             WHERE [guia] = @guia";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@guia", guia);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    isSuccess = (rowsAffected > 0);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return isSuccess;
    } 
}