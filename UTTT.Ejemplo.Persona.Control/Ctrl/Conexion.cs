using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UTTT.Ejemplo.Persona.Control.Ctrl
{
    public class Conexion
    {
        #region Variables
     
        #endregion

        #region Constructores
        public Conexion()
        {
        }       
        #endregion


        public SqlConnection sqlConnection()
        {
            try
            {
                SqlConnection conexion = new SqlConnection("Data Source=PersonaUttt.mssql.somee.com;Initial Catalog=PersonaUttt;User ID=mickeyjr007_SQLLogin_1;Password=16wqg9r66a");
                return conexion;
            }
            catch (Exception _e)
            { 
            
            }
            return null;
        }
    }
}
