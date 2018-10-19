﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace cityLife4.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string recreateDB)
        {
            //drop all DB tables and create a new DB schema with empty DB
            StreamReader sqlReader = new StreamReader(Server.MapPath("/cityLifeDB.edmx.sql"));
            string createDBsql = sqlReader.ReadToEnd();
            
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
           
            //db.Database.ExecuteSqlCommand(createDBsql);
            PopulateDB(db);
           

            return View();
        }
        /// <summary>
        /// The function reads the CSV file that contains the DB content and creates SQL statements to populate it
        /// Then it executes this SQL and populates the DB
        /// </summary>
        private void PopulateDB(cityLifeDBContainer1 db)
        {
            string CSVFilePath = Server.MapPath("/cityLifeDB.csv");
            using (TextFieldParser parser = new TextFieldParser(CSVFilePath))
            {
                parser.Delimiters = new string[] { "," };

                string tableName="undefined";
                List<string> columnNames = new List<string>();
                List<string> columnValues =  new List<string>();

                string[] lineFields = parser.ReadFields();
                int startAt = -1;
                while (lineFields !=null)
                {
                    if (lineFields[0] != "")
                    {
                        //This is a new table - the line contains the table name and then the field names
                        tableName = lineFields[0];
                        columnNames.Clear();
                        //Set current table to be ON, so we can insert id's explicitly
                      //  db.Database.ExecuteSqlCommand("SET IDENTITY_INSERT " + tableName + " ON");
                        columnNames = new List<string>();

                        //copy all column names to the columnNames list - if the first column is "id" - skip it, because it has autoamtic sequencing
                        if (lineFields[1] == "id")
                        {
                            //The first column is "id", which has an automatic sequencing - jump over it.
                            startAt = 2;
                        }
                        else
                        {
                            startAt = 1;
                        }

                        for (int i = startAt; i < lineFields.Count(); i++)
                        {
                            if (lineFields[i] != "")
                            {
                                columnNames.Add(lineFields[i]);
                            }
                        }
                    }
                    else if (lineFields[0].Contains("//"))
                    {
                        //This is a comment line - ignore it
                    }
                    else
                    {
                        //this is a data line - create an insert command to insert it into the DB
                        //Copy all value names into the valueName list
                        columnValues.Clear();
                       
                        for (int i = startAt; i < lineFields.Count(); i++)
                        {
                            if (lineFields[i] != "")
                            {
                                columnValues.Add(lineFields[i]);
                            }
                        }
                        //At this point we have all column names in columnName list and valus in columnValues list
                        if (columnValues.Count() == 0)
                        {
                            //This is an empty line - skip it
                            lineFields = parser.ReadFields();
                            continue;
                        }
                        StringBuilder insertCommand = new StringBuilder("INSERT INTO ");
                        insertCommand.Append(tableName);                   //INSERT INTO apartment
                        insertCommand.Append("(");                         //INSERT INTO apartment(
                        insertCommand.Append(columnNames[0]);              //INSERT INTO apartment(id
                        for (int i = 1; i < columnNames.Count(); i++)
                        {
                            insertCommand.Append(",");                     //INSERT INTO apartment(id,
                            insertCommand.Append(columnNames[i]);          //INSERT INTO apartment(id,name
                        }
                        insertCommand.Append(") VALUES (");                //INSERT INTO apartment(id,name,description) VALUES (
                        insertCommand.Append("N'"+columnValues[0]+"'");     //INSERT INTO apartment(id,name,description) VALUES (N'123'
                        for (int i = 1; i < columnValues.Count(); i++)
                        {
                            insertCommand.Append(",");                     //INSERT INTO apartment(id,name,description) VALUES (N'123',
                            insertCommand.Append("N'"+columnValues[i]+"'"); //INSERT INTO apartment(id,name,description) VALUES (N'123',N'nice'
                        }
                        insertCommand.Append(")");
                        string insertCommandSt = insertCommand.ToString();
                        db.Database.ExecuteSqlCommand(insertCommandSt);
                    }
                    lineFields = parser.ReadFields();
                }
                return;
            }
        }
    }
}