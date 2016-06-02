using SQLite;

namespace CreateDatabaseWithSQLite
{
    public class WeatherStructureDB // Some data structure that will be saved on SQLite DB
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Temperature { get; set; }
    }
}