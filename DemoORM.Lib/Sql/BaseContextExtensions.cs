using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DemoORM.Lib.Sql
{
    public static class BaseContextExtensions
    {
        private static readonly Type TableInterfaceType = typeof(ITable<>);

        public static string CreateSql(this BaseContext context)
        {
            var builder = new StringBuilder();

            // Get the types for which we have to create tables
            var types = GetTypes(context);

            GenerateTablesSql(types, builder);

            return builder.ToString();
        }

        /// <summary>
        /// Generate SQL for all tables
        /// </summary>
        /// <param name="types">Types of the table and names</param>
        /// <param name="builder"></param>
        private static void GenerateTablesSql(Dictionary<Type, string> types, StringBuilder builder)
        {
            foreach (var (type, tableName) in types)
            {
                GenerateTableSql(type, tableName, builder);
            }
        }


        /// <summary>
        /// Generate SQL for a table
        /// </summary>
        /// <param name="type">Type to generate table from</param>
        /// <param name="name">Name of the table</param>
        /// <param name="builder"></param>
        private static void GenerateTableSql(Type type, string name, StringBuilder builder)
        {
            builder.Append("\r\n")
                .Append("CREATE TABLE [dbo].[").Append(name).Append("] (\r\n");

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var first = true;
            foreach (var propertyInfo in properties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(",\r\n");
                }
                
                GeneratePropertySql(propertyInfo.PropertyType, propertyInfo.Name, builder);
            }

            builder.Append("\r\n);");

        }

        /// <summary>
        /// Create SQL for a property
        /// </summary>
        /// <param name="type">Type of the property</param>
        /// <param name="name">Name of the property</param>
        /// <param name="builder"></param>
        private static void GeneratePropertySql(Type type, string name, StringBuilder builder)
        {
            builder.Append(name).Append(" ");
            switch (type)
            {
                case { } t when t == typeof(string):
                    builder.Append("NVARCHAR(MAX)");
                    break;
                case { } t when t == typeof(int):
                    builder.Append("INT");
                    // Field with name id is Primary key
                    if (name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                    {
                        builder.Append(" PRIMARY KEY IDENTITY (1, 1)");
                    }
                    break;
                default:
                    throw new NotSupportedException("Invalid type " + type.FullName);
            }
        }

        /// <summary>
        /// Returns all types and name of properties of type ITable&lt;T&gt;
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Dictionary<Type, string> GetTypes(BaseContext context)
        {
            var contextType = context.GetType();

            // Get all public non static properties
            var properties = contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var types = new Dictionary<Type, string>();

            foreach (var propertyInfo in properties)
            {
                // If property type isn't an interface or property isn't based on a generic. Exit
                var propertyType = propertyInfo.PropertyType;
                if (!propertyType.IsInterface || !propertyType.IsConstructedGenericType)
                {
                    continue;
                }
                // Get type of the generic
                var isDboProperty = propertyType.GetGenericTypeDefinition() == TableInterfaceType;

                if (!isDboProperty)
                {
                    continue;
                }

                // Add type and property name to the collection
                types.Add(propertyType.GenericTypeArguments[0], propertyInfo.Name);
            }

            return types;

        }
    }
}