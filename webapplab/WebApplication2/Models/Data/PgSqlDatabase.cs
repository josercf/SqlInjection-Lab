﻿using Npgsql;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace webappsql.Models.Data
{
    public class PgSqlDatabase : IDatabase
    {
        private readonly PgConnectionConfig _pgConfig;

        public PgSqlDatabase(IConfiguration configuration)
        {
            _pgConfig = new PgConnectionConfig
            {
                ConnectionUri = configuration["DATABASE_URL"],
                Password = configuration["DATABASE_PASSWORD"],
                Username = configuration["DATABASE_USER"]
            };
        }

        static string GetConnectionString(PgConnectionConfig pgConfig)
        {
            //var databaseUri = new Uri(pgConfig.ConnectionUri);
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = pgConfig.ConnectionUri,
                Port = 5432,
                Username = pgConfig.Username,
                Password = pgConfig.Password,
                Database = "",
                SslMode = SslMode.Disable,
                TrustServerCertificate = true,
                //UseSslStream = true,
                Pooling = true
            };

            return builder.ToString();
        }

        public IList<Product> Search(string searchString)
        {
            List<Product> productColl = new List<Product>();
            using (var conn = new NpgsqlConnection())
            {
                conn.ConnectionString = GetConnectionString(_pgConfig);

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @$"SELECT * FROM ""Product"" WHERE ""Name"" LIKE '%{searchString}%'";

                    conn.Open();

                    using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            string id = reader.GetString(0);
                            string name = reader.GetString(1);
                            string price = reader.GetString(2);

                            productColl.Add(new Product
                            {
                                Id = id,
                                Name = name,
                                Price = price
                            });
                        }
                    }

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }
            }
            return productColl;
        }
    }
}
