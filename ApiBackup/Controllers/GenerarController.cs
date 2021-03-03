using ApiBackup.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApiBackup.Controllers
{
    public class GenerarController : ApiController
    {
       
        ClsConexion go_Sql;
        string ruta;
        string baseplantilla;
        string nbase;
        bool bandera = false;
        string tipo;
        string servidor;
        string basedatos;
        string usuario;
        string password;
        public IHttpActionResult Index([FromBody] Datos datos)
        {
            var path = "";
            path = System.Web.HttpContext.Current.Server.MapPath("~/bak/");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            if (datos.periodo == "")
            {              
                return BadRequest("Periodo esta vacio");
            }
            if (datos.razonsocial == "")
            {              
                return BadRequest("Razon social vacio");
            }
            if (datos.ruc == "")
            {
                return BadRequest("Ruc esta vacio");
            }
         
            if (datos.direccion == "")
            {               
                return BadRequest("Direccion esta Vacio");
            }
            //TxtRucPlantilla.Text.Trim = TxtRuc.Text.Trim And TxtPeriodoPLanitlla.Text.Trim = TxtPeriodo.Text.Substring(2, 2) Then
            //  if (TxtRucPlantilla.Text.Trim == TxtRuc.Text.Trim & TxtPeriodoPLanitlla.Text.Trim == TxtPeriodo.Text.Substring(2, 2))
            //   {               
            // return BadRequest("No se puede crear un mismo periodo o mismo ejercicio");
            //  }        
            // tipo = datos.rucplantilla;
            servidor = datos.servidor;
            basedatos = datos.bd;
            usuario = datos.usuario;
            password = datos.password;

            tipo = datos.rucplantilla.Trim().Substring(0, 3).Trim();
            string rutax = @" select top 1 reverse(right(reverse(physical_name),len(reverse(physical_name))-charindex('\',reverse(physical_name))))as ruta from sys.master_files where name like 'COM%' ";
            go_Sql = new ClsConexion();
            DataTable dtruta = go_Sql.EjecutarConsulta("ruta", rutax,datos.servidor,datos.bd,datos.usuario,datos.password).Tables[0];
            if (dtruta.Rows.Count > 0)
                 ruta = dtruta.Rows[0][0].ToString();         

            if (datos.btnperiodo == true)
            {    
                if (datos.rucplantilla != "")
                    GenerarBackup(datos.rucplantilla,path);
                else
                {                
                   return BadRequest("No Ha seleccionado ninguna base plantilla");
                }               
                RestaurarBackup(datos.btnperiodo,datos.ruc,datos.periodo,path);
                //   string estado = eliminarbackup1();
                eliminarbackup(path);                
                return Ok("Se genero correctamente el Nuevo Periodo"+"-");
            }            
            return Ok();
        }

        private void GenerarBackup(object bd,string path)
        {         
            try
            {
                var Consulta="";
            //    Consulta = "backup database " + bd + " to disk = N'" + ruta + @"\Temporal.bak' with noformat, noinit, name = N'" + bd + "',skip,stats = 10";
                Consulta = "backup database " + bd + " to disk = N'" + path + @"\Temporal.bak' with noformat, noinit, name = N'" + bd + "',skip,stats = 10";
                go_Sql.EjecutarConsulta("Me", Consulta,servidor, basedatos, usuario,password);
            }
            catch (Exception ex)
            {      
                return;
            }
        }
        private void RestaurarBackup(bool peri,string ruc,string periodo,string path)
        {
            DataSet ds_logicalname = new DataSet();
            string logicalname = " RESTORE FILELISTONLY FROM  DISK = N'" + path + @"\Temporal.bak' ";
            string mensaje;
            ds_logicalname = go_Sql.EjecutarConsulta("ma", logicalname,servidor,basedatos,usuario,password);
         
            if (peri == true)
            { //aquie errror
                nbase = tipo + ruc + periodo.Trim().Substring(2, 2).Trim();
            }
            else
            {
               //nbase = this.CboCompania.SelectedValue.Trim.Substring(0, 14) + periodo.Trim().Substring(2, 2).Trim;
            }               
            string dx = "select * from sys.databases where Name='" + nbase.Trim() + "'";    
            if (go_Sql.EjecutarConsulta("de", dx, servidor, basedatos, usuario, password).Tables[0].Rows.Count > 0)
            {
                if (peri == true)
                {                  
                    mensaje =("Ya existe este Periodo para la Compañia Actual");                    
                }
                else {                    
                    mensaje=("Ya existe la Compañia Creada con el periodo especificado");
                }
                eliminarbackup(path);     
                bandera = true;
                return ;
            }
            try
            {
                var Consulta="";
                Consulta = "RESTORE DATABASE  " + nbase + "  FROM  DISK = N'" + path + @"\Temporal.bak' WITH  FILE = 1, MOVE N'" + ds_logicalname.Tables[0].Rows[0][0].ToString() + "' TO N'" + ruta + @"\" + nbase.Trim() + ".mdf',MOVE N'" + ds_logicalname.Tables[0].Rows[1][0]+ "' TO N'" + ruta + @"\" + nbase.Trim() + ".ldf',  NOUNLOAD,  REPLACE,  STATS = 10 ";
                go_Sql.EjecutarConsulta("Me", Consulta, servidor, basedatos, usuario, password);
            }
            catch (Exception ex)
            {            
                return ;
            }
        }
        private void eliminarbackup(string path)
        {
            if (File.Exists((path + @"\Temporal.bak"))==true)
            {
                File.SetAttributes(path + @"\Temporal.bak", FileAttributes.Normal);
                File.Delete(path + @"\Temporal.bak");
            }
           // if (My.Computer.FileSystem.FileExists(ruta + @"\Temporal.bak") == true)
           //   My.Computer.FileSystem.DeleteFile(ruta + @"\Temporal.bak", Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently);
        }
      


        public class Datos
        {
            public string ruc { get; set; }
            public string  periodo { get; set; }
            public string razonsocial { get; set; }
            public string direccion { get; set; }
            public string rucplantilla { get; set; }
            public string periodoplantilla { get; set; }
            public bool btnperiodo { get; set; }

            //servidor
            public string servidor { get; set; }
            public string bd { get; set; }
            public string usuario { get; set; }
            public string password { get; set; }
        }
     }
    
}
