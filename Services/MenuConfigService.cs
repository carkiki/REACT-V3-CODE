using System;

using System.Collections.Generic;

using System.Linq;

using Microsoft.Data.Sqlite;

using ReactCRM.Database;



namespace ReactCRM.Services

{

    /// <summary>

    /// Manages sidebar menu configuration, order, and visibility for each user

    /// Supports drag & drop reordering and persistent storage

    /// </summary>

    public class MenuConfigService

    {

        private static MenuConfigService _instance;

        private static readonly object _lock = new object();

        private int _currentUserId;



        public List<MenuItem> MenuItems { get; private set; } = new List<MenuItem>();



        private MenuConfigService() { }



        public static MenuConfigService Instance

        {

            get

            {

                if (_instance == null)

                {

                    lock (_lock)

                    {

                        if (_instance == null)

                        {

                            _instance = new MenuConfigService();

                        }

                    }

                }

                return _instance;

            }

        }



        /// <summary>

        /// Initializes menu configuration for current user

        /// </summary>

        public void Initialize(int userId)

        {

            _currentUserId = userId;

            LoadMenuConfig();

        }



        /// <summary>

        /// Loads menu configuration from database

        /// </summary>

        private void LoadMenuConfig()

        {

            MenuItems.Clear();



            try

            {

                using var connection = DbConnection.GetConnection();

                connection.Open();



                // Try to load user's custom menu configuration

                string sql = @"

                    SELECT MenuItemId, DisplayOrder, IsVisible, MenuItem

                    FROM MenuConfiguration

                    WHERE UserId = @userId

                    ORDER BY DisplayOrder ASC";



                using var cmd = new SqliteCommand(sql, connection);

                cmd.Parameters.AddWithValue("@userId", _currentUserId);



                using var reader = cmd.ExecuteReader();

                while (reader.Read())

                {

                    MenuItems.Add(new MenuItem

                    {

                        Id = Convert.ToInt32(reader["MenuItemId"]),

                        Name = reader["MenuItem"].ToString(),

                        DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),

                        IsVisible = Convert.ToBoolean(reader["IsVisible"])

                    });

                }



                // If no custom config exists, create default configuration

                if (MenuItems.Count == 0)

                {

                    CreateDefaultMenuConfig();

                }

            }

            catch (SqliteException)

            {

                // If table doesn't exist yet, create default config

                CreateDefaultMenuConfig();

            }

        }



        /// <summary>

        /// Creates and saves default menu configuration

        /// </summary>

        private void CreateDefaultMenuConfig()

        {

            MenuItems.Clear();



            // Define default menu items with their order

            var defaultItems = new[]

            {

                new MenuItem { Id = 1, Name = "Clients", DisplayOrder = 1, IsVisible = true },

new MenuItem { Id = 2, Name = "Time Clock", DisplayOrder = 2, IsVisible = true },



                new MenuItem { Id = 3, Name = "Import CSV", DisplayOrder = 3, IsVisible = true },



                new MenuItem { Id = 4, Name = "Import Database", DisplayOrder = 4, IsVisible = true },



                new MenuItem { Id = 5, Name = "Custom Fields", DisplayOrder = 5, IsVisible = true },



                new MenuItem { Id = 6, Name = "Advanced Search", DisplayOrder = 6, IsVisible = true },



                new MenuItem { Id = 7, Name = "Analytics", DisplayOrder = 7, IsVisible = true },



                new MenuItem { Id = 8, Name = "Audit Logs", DisplayOrder = 8, IsVisible = true },



                new MenuItem { Id = 9, Name = "Backup", DisplayOrder = 9, IsVisible = true },



                new MenuItem { Id = 10, Name = "Manage Workers", DisplayOrder = 10, IsVisible = true },



                new MenuItem { Id = 11, Name = "PDF Templates", DisplayOrder = 11, IsVisible = true },
            };



            MenuItems = defaultItems.ToList();



            // Save to database

            SaveMenuConfig();

        }
        /// <summary>

        /// Forces a reset of the menu configuration for the current user

        /// This will recreate the menu with all default items including new ones

        /// </summary>

        public void ResetToDefaults()

        {

            try

            {

                using var connection = DbConnection.GetConnection();

                connection.Open();



                // Delete existing configuration for this user

                string deleteSql = "DELETE FROM MenuConfiguration WHERE UserId = @userId";

                using var deleteCmd = new SqliteCommand(deleteSql, connection);

                deleteCmd.Parameters.AddWithValue("@userId", _currentUserId);

                deleteCmd.ExecuteNonQuery();



                // Recreate default configuration

                CreateDefaultMenuConfig();



                System.Windows.Forms.MessageBox.Show(

                    "Menú restablecido a configuración por defecto.\n\nPor favor cierra y vuelve a abrir el Dashboard para ver los cambios.",

                    "Menú Actualizado",

                    System.Windows.Forms.MessageBoxButtons.OK,

                    System.Windows.Forms.MessageBoxIcon.Information);

            }

            catch (Exception ex)

            {

                System.Windows.Forms.MessageBox.Show($"Error resetting menu: {ex.Message}");

            }

        }



        /// <summary>

        /// Checks if a menu item exists and adds it if missing

        /// </summary>

        public void EnsureMenuItemExists(string menuName, int displayOrder)

        {

            var item = MenuItems.FirstOrDefault(m => m.Name == menuName);

            if (item == null)

            {

                // Find the highest ID

                int maxId = MenuItems.Any() ? MenuItems.Max(m => m.Id) : 0;



                MenuItems.Add(new MenuItem

                {

                    Id = maxId + 1,

                    Name = menuName,

                    DisplayOrder = displayOrder,

                    IsVisible = true

                });



                SaveMenuConfig();

            }

        }



        /// <summary>

        /// Saves current menu configuration to database

        /// </summary>

        public void SaveMenuConfig()

        {

            try

            {

                using var connection = DbConnection.GetConnection();

                connection.Open();



                // Delete existing configuration for this user

                string deleteSql = "DELETE FROM MenuConfiguration WHERE UserId = @userId";

                using var deleteCmd = new SqliteCommand(deleteSql, connection);

                deleteCmd.Parameters.AddWithValue("@userId", _currentUserId);

                deleteCmd.ExecuteNonQuery();



                // Insert new configuration

                foreach (var item in MenuItems)

                {

                    string insertSql = @"

                        INSERT INTO MenuConfiguration (UserId, MenuItemId, MenuItem, DisplayOrder, IsVisible)

                        VALUES (@userId, @menuItemId, @menuItem, @displayOrder, @isVisible)";



                    using var insertCmd = new SqliteCommand(insertSql, connection);

                    insertCmd.Parameters.AddWithValue("@userId", _currentUserId);

                    insertCmd.Parameters.AddWithValue("@menuItemId", item.Id);

                    insertCmd.Parameters.AddWithValue("@menuItem", item.Name);

                    insertCmd.Parameters.AddWithValue("@displayOrder", item.DisplayOrder);

                    insertCmd.Parameters.AddWithValue("@isVisible", item.IsVisible);



                    insertCmd.ExecuteNonQuery();

                }



                // Log the action

                AuditService.LogAction("UpdateMenuConfig", "MenuConfiguration", _currentUserId,

                    $"User reordered/configured menu items");

            }

            catch (Exception ex)

            {

                System.Windows.Forms.MessageBox.Show($"Error saving menu configuration: {ex.Message}");

            }

        }



        /// <summary>

        /// Reorders menu items

        /// </summary>

        public void ReorderMenu(List<MenuItem> newOrder)

        {

            MenuItems.Clear();

            int order = 1;

            foreach (var item in newOrder)

            {

                item.DisplayOrder = order++;

                MenuItems.Add(item);

            }

            SaveMenuConfig();

        }



        /// <summary>

        /// Toggles visibility of a menu item

        /// </summary>

        public void ToggleMenuItemVisibility(string menuName)

        {

            var item = MenuItems.FirstOrDefault(m => m.Name == menuName);

            if (item != null)

            {

                item.IsVisible = !item.IsVisible;

                SaveMenuConfig();

            }

        }



        /// <summary>

        /// Sets visibility of a menu item to a specific value

        /// </summary>

        public void SetMenuItemVisibility(string menuName, bool isVisible)

        {

            var item = MenuItems.FirstOrDefault(m => m.Name == menuName);

            if (item != null)

            {

                item.IsVisible = isVisible;

                SaveMenuConfig();

            }

        }



        /// <summary>

        /// Gets visible menu items ordered by DisplayOrder

        /// </summary>

        public List<MenuItem> GetVisibleMenuItems()

        {

            return MenuItems.Where(m => m.IsVisible).OrderBy(m => m.DisplayOrder).ToList();

        }



        /// <summary>

        /// Gets all menu items including hidden ones

        /// </summary>

        public List<MenuItem> GetAllMenuItems()

        {

            return MenuItems.OrderBy(m => m.DisplayOrder).ToList();

        }

    }



    /// <summary>

    /// Represents a menu item with display order and visibility

    /// </summary>

    public class MenuItem

    {

        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsVisible { get; set; }



        public override string ToString()

        {

            return Name;

        }

    }

}