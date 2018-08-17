using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;
using LiteCore.Interop;
using Couchbase.Lite.DI;
using Couchbase.Lite.Logging;
using Couchbase.Lite.Util;
using Couchbase.Lite.Support;
using System.IO;

namespace CouchbaseLite
{
    class CouchbaseLite
    {
        static void Main(string[] args)
        {

            CouchbaseLite P = new CouchbaseLite();



            Database db;
            Database dbproducts;
            Database dborders;

            try
            {
                NetDesktop.Activate();


                /*This part of code is used for bulk loading data from a File into CBL
                DataTable dtcustmaster = new DataTable();

                /dtcustmaster = P.showDataFromTextLogFiles(@"C:\Personal\09106684\Desktop\Orders.txt");
                DatabaseConfiguration config = new DatabaseConfiguration() { Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) };

                dborders = new Database("Ordersdb", config);

                foreach (DataRow drr in dtcustmaster.Rows)
                {

                    using (var CustomerDoc = new MutableDocument(drr["DOC_NBR"].ToString()))
                    {

                        CustomerDoc.SetString("DOC_NBR", drr["DOC_NBR"].ToString());
                        CustomerDoc.SetString("CUST_NBR", drr["CUST_NBR"].ToString());
                        CustomerDoc.SetString("ORDER_CREATED_DT", drr["ORDER_CREATED_DT"].ToString());
                        CustomerDoc.SetString("ITEM_ID", drr["ITEM_ID"].ToString());
                        CustomerDoc.SetString("ORD_QTY", drr["ORD_QTY"].ToString());
                        CustomerDoc.SetString("RETL_AMT", drr["RETL_AMT"].ToString());

                        dborders.Save(CustomerDoc);

                    }
                }
                
                P.runselectOrders(dborders); */

                db = P.OpenCustDB();

                dbproducts = P.OpenProdDB();

                dborders = P.OpenOrderDB();

                P.PromptMaster(db, dbproducts, dborders);

            }

            catch (Exception ex)
            {
                Console.WriteLine("Error Unable to connect : " + ex.ToString());
            }

        }

        public DataTable showDataFromTextLogFiles(string fileName)
        {
            const Int32 BufferSize = 128;
            int lineCount = 0;
            DataTable dt = new DataTable();
            using (var fileStream = File.OpenRead(fileName))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    dt = new DataTable(); lineCount = 1;
                    dt.Clear();
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] cellValue = line.Split('|');
                        if (lineCount == 1)
                        {
                            for (int i = 0; i < cellValue.Length; i++)
                            {
                                dt.Columns.Add(cellValue[i]);
                            }
                            lineCount++;

                        }
                        else
                        {
                            DataRow rowAdd = dt.NewRow();
                            for (int j = 0; j < cellValue.Length; j++)
                            {
                                rowAdd[j] = cellValue[j];
                            }
                            dt.Rows.Add(rowAdd);
                            lineCount++;
                        }
                    }
                }
            }

            return dt;
        }

        public void Promptuser(Database db, String strdbtype)
        {

            String strNextAction = String.Empty;
            String stractionstatus = String.Empty;
            Console.WriteLine();
            Console.Write("To Individual Select (S), To Insert (I), To Delete (D) and Main Menu (B) : ");
            strNextAction = Console.ReadLine();

            if (strNextAction == "S")
            {
                if (strdbtype == "Customer")
                {
                    Console.WriteLine();
                    Console.Write("Please enter CUST_NBR : ");
                    runselectcus(db, Console.ReadLine().ToString());
                }
                else if (strdbtype == "Product")
                {
                    Console.WriteLine();
                    Console.Write("Please enter ITEM ID : ");
                    runselectprod(db, Console.ReadLine().ToString());
                }
                else if (strdbtype == "Order")
                {
                    Console.WriteLine();
                    Console.Write("Please enter Order Number : ");
                    runselectord(db, Console.ReadLine().ToString());
                }

            }
            else if (strNextAction == "I")
            {
                if (strdbtype == "Customer")
                {
                    AddCustomerMaster(db);
                }
                else if (strdbtype == "Product")
                {
                    AddProductMaster(db);
                }
                else if (strdbtype == "Order")
                {
                    AddOrderMaster(db);
                }

            }
            else if (strNextAction == "D")
            {
                if (strdbtype == "Customer")
                {
                    DeleteCustomerMaster(db);
                }
                else if (strdbtype == "Product")
                {
                    DeleteProductMaster(db);
                }
                else if (strdbtype == "Order")
                {
                    DeleteOrderMaster(db);
                }
            }
            else if (strNextAction == "B")
            {
                PromptMaster(OpenCustDB(), OpenProdDB(), OpenOrderDB());
            }
            else
            {
                Console.WriteLine("Invalid Entry. Please press enter to Exit the Application");
            }
        }

        public void PromptMaster(Database custdb, Database proddb, Database orderdb)
        {

            String strNextAction = String.Empty;
            Console.WriteLine();
            Console.Write("Select View -->  Customers (C), Products (P), Orders (O) and Exit (E) : ");
            strNextAction = Console.ReadLine();

            if (strNextAction == "C")
            {
                runselect(custdb);
            }
            else if (strNextAction == "P")
            {
                runselectProduct(proddb);
            }
            else if (strNextAction == "O")
            {
                runselectOrders(orderdb);
            }
            else if (strNextAction == "E")
            {
                Environment.Exit(0);
            }

        }

        public void runselect(Database db)
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            DataTable dtselect = new DataTable();

            synccust(db);

            string[] arr1 = new string[] { "CUST_NBR", "CUST_EFF_DT", "CUST_END_DT", "CUST_NM", "STORE_NBR" };

            foreach (String strselect in arr1)
            {
                dtselect.Columns.Add(strselect, typeof(String));
            }

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID),
            SelectResult.Property("CUST_NBR"),
            SelectResult.Property("CUST_EFF_DT"),
            SelectResult.Property("CUST_END_DT"),
            SelectResult.Property("CUST_NM"),
            SelectResult.Property("STORE_NBR")
            ).From(DataSource.Database(db)))


            {

                foreach (var result in query.Execute())
                {
                    DataRow dr = dtselect.NewRow();

                    foreach (String strselect in arr1)
                    {
                        dr[strselect] = result.GetString(strselect);
                    }


                    dtselect.Rows.Add(dr);
                    dtselect.AcceptChanges();

                }

                PrettyPrintDataTable(dtselect);

            }

            Promptuser(db, "Customer");
        }

        public void runselectProduct(Database db)
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            DataTable dtselect = new DataTable();
            string[] arr1 = new string[] { "ITEM_ID", "PRD_EFF_DT", "PRD_END_DT", "UPC", "ITEM_DESCP", "SDV_AMT" };

            syncprod(db);

            foreach (String strselect in arr1)
            {
                dtselect.Columns.Add(strselect, typeof(String));
            }

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID),
            SelectResult.Property("ITEM_ID"),
            SelectResult.Property("PRD_EFF_DT"),
            SelectResult.Property("PRD_END_DT"),
            SelectResult.Property("UPC"),
            SelectResult.Property("ITEM_DESCP"),
            SelectResult.Property("SDV_AMT")
            ).From(DataSource.Database(db)))


            {

                foreach (var result in query.Execute())
                {
                    DataRow dr = dtselect.NewRow();

                    foreach (String strselect in arr1)
                    {
                        dr[strselect] = result.GetString(strselect);
                    }


                    dtselect.Rows.Add(dr);
                    dtselect.AcceptChanges();

                }

                PrettyPrintDataTable(dtselect);

            }

            Promptuser(db, "Product");
        }

        public void runselectOrders(Database db)
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            DataTable dtselect = new DataTable();
            string[] arr1 = new string[] { "DOC_NBR", "CUST_NBR", "ORDER_CREATED_DT", "ITEM_ID", "ORD_QTY", "RETL_AMT" };

            syncorder(db);

            foreach (String strselect in arr1)
            {
                dtselect.Columns.Add(strselect, typeof(String));
            }

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID),
            SelectResult.Property("DOC_NBR"),
            SelectResult.Property("CUST_NBR"),
            SelectResult.Property("ORDER_CREATED_DT"),
            SelectResult.Property("ITEM_ID"),
            SelectResult.Property("ORD_QTY"),
            SelectResult.Property("RETL_AMT")
            ).From(DataSource.Database(db)))


            {

                foreach (var result in query.Execute())
                {
                    DataRow dr = dtselect.NewRow();

                    foreach (String strselect in arr1)
                    {
                        dr[strselect] = result.GetString(strselect);
                    }


                    dtselect.Rows.Add(dr);
                    dtselect.AcceptChanges();

                }

                PrettyPrintDataTable(dtselect);

            }

            Promptuser(db, "Order");
        }

        //Cust

        public void AddCustomerMaster(Database db)
        {
            String straddstatus = String.Empty;
            Hashtable hstcustadd = new Hashtable();
            Console.WriteLine();
            string[] arr1 = new string[] { "CUST_NBR", "CUST_NM", "STORE_NBR" };

            foreach (String strcol in arr1)
            {
                String mutestr = String.Empty;
                bool userentry = true;

                Console.Write("Please enter " + strcol + " : ");
                mutestr = Console.ReadLine();
                Console.WriteLine();

                while (userentry)
                {
                    if (!String.IsNullOrEmpty(mutestr))
                    {
                        hstcustadd.Add(strcol, mutestr);
                        break;

                    }
                    else
                    {
                        Console.WriteLine("Please enter " + strcol);
                        mutestr = Console.ReadLine();
                        userentry = true;
                    }

                }

            }

            using (var CustomerDoc = new MutableDocument(hstcustadd["CUST_NBR"].ToString()))
            {

                CustomerDoc.SetString("CUST_NBR", hstcustadd["CUST_NBR"].ToString());
                CustomerDoc.SetString("CUST_EFF_DT", DateTime.Now.ToString("MM/dd/yyyy"));
                CustomerDoc.SetString("CUST_END_DT", "12/25/9999");
                CustomerDoc.SetString("CUST_NM", hstcustadd["CUST_NM"].ToString());
                CustomerDoc.SetString("STORE_NBR", hstcustadd["STORE_NBR"].ToString());

                db.Save(CustomerDoc);
                Console.WriteLine();
                Console.WriteLine("Customer " + hstcustadd["CUST_NBR"].ToString() + " Added Succefully");

                synccust(db);

            }

            Console.WriteLine();
            runselect(db);
            Console.WriteLine();
            Promptuser(db, "Customer");


        }

        public void UpdateCustomerMaster(Database db)
        {
            String struptstatus = String.Empty;
            String id = String.Empty;
            string strnameupdate = String.Empty;
            runselect(db);
            Console.WriteLine();
            Console.Write("Please enter valid Customer Number from above Table : ");
            id = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Enter latest Customer Name which needs update:");
            strnameupdate = Console.ReadLine();
            Console.WriteLine();
            using (var doc = db.GetDocument(id))
            using (var mutableDoc = doc.ToMutable())
            {
                mutableDoc.SetString("CUST_NM", strnameupdate);
                db.Save(mutableDoc);
            }

            Console.WriteLine("Customer ID " + id + " Update successful, please refer the below table");

            Console.WriteLine();

            runselect(db);

            Console.WriteLine();

            Promptuser(db, "Customer");

        }

        public void DeleteCustomerMaster(Database db)
        {
            String struptstatus = String.Empty;
            String id = String.Empty;
            string strnameupdate = String.Empty;
            //runselect(db);
            Console.WriteLine();
            Console.Write("Please enter valid Customer Number from above Table : ");
            id = Console.ReadLine();
            Console.WriteLine();

            using (var doc = db.GetDocument(id))
            using (var mutableDoc = doc.ToMutable())
            {

                db.Delete(mutableDoc);
            }

            Console.WriteLine("Customer ID " + id + " deleted successfully, please refer the below table");

            synccust(db);

            Console.WriteLine();

            runselect(db);

            Console.WriteLine();

            Promptuser(db, "Customer");

        }

        public void runselectcus(Database db, String strcustid)
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            DataTable dtselect = new DataTable();
            string[] arr1 = new string[] { "CUST_NBR", "CUST_EFF_DT", "CUST_END_DT", "CUST_NM", "STORE_NBR" };

            synccust(db);

            foreach (String strselect in arr1)
            {
                dtselect.Columns.Add(strselect, typeof(String));
            }

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID),
            SelectResult.Property("CUST_NBR"),
            SelectResult.Property("CUST_EFF_DT"),
            SelectResult.Property("CUST_END_DT"),
            SelectResult.Property("CUST_NM"),
            SelectResult.Property("STORE_NBR")
            ).From(DataSource.Database(db))
             .Where(Expression.Property("CUST_NBR").EqualTo(Expression.String(strcustid))))

            {

                foreach (var result in query.Execute())
                {
                    DataRow dr = dtselect.NewRow();

                    foreach (String strselect in arr1)
                    {
                        dr[strselect] = result.GetString(strselect);
                    }


                    dtselect.Rows.Add(dr);
                    dtselect.AcceptChanges();

                }

                PrettyPrintDataTable(dtselect);

            }

            Promptuser(db, "Customer");
        }

        //Cust End

        //Products

        public void AddProductMaster(Database db)
        {
            String straddstatus = String.Empty;
            Hashtable hstcustadd = new Hashtable();
            Console.WriteLine();
            string[] arr1 = new string[] { "ITEM_ID", "UPC", "SDV_AMT" };

            foreach (String strcol in arr1)
            {
                String mutestr = String.Empty;
                bool userentry = true;

                Console.Write("Please enter " + strcol + " : ");
                mutestr = Console.ReadLine();
                Console.WriteLine();

                while (userentry)
                {
                    if (!String.IsNullOrEmpty(mutestr))
                    {
                        hstcustadd.Add(strcol, mutestr);
                        break;

                    }
                    else
                    {
                        Console.WriteLine("Please enter " + strcol);
                        mutestr = Console.ReadLine();
                        userentry = true;
                    }

                }

            }

            using (var CustomerDoc = new MutableDocument(hstcustadd["ITEM_ID"].ToString()))
            {

                CustomerDoc.SetString("ITEM_ID", hstcustadd["ITEM_ID"].ToString());
                CustomerDoc.SetString("PRD_EFF_DT", DateTime.Now.ToString("MM/dd/yyyy"));
                CustomerDoc.SetString("PRD_END_DT", "12/25/9999");
                CustomerDoc.SetString("UPC", hstcustadd["UPC"].ToString());
                CustomerDoc.SetString("ITEM_DESCP", "PRODUCT " + hstcustadd["UPC"].ToString());
                CustomerDoc.SetString("SDV_AMT", hstcustadd["SDV_AMT"].ToString());

                db.Save(CustomerDoc);
                Console.WriteLine();
                Console.WriteLine("Product " + hstcustadd["ITEM_ID"].ToString() + " Added Succefully");

            }

            syncprod(db);

            Console.WriteLine();
            runselectProduct(db);
            Console.WriteLine();
            Promptuser(db, "Product");


        }

        public void DeleteProductMaster(Database db)
        {
            String struptstatus = String.Empty;
            String id = String.Empty;
            string strnameupdate = String.Empty;
            //runselect(db);
            Console.WriteLine();
            Console.Write("Please enter valid ITEM ID from above Table : ");
            id = Console.ReadLine();
            Console.WriteLine();

            using (var doc = db.GetDocument(id))
            using (var mutableDoc = doc.ToMutable())
            {

                db.Delete(mutableDoc);
            }

            Console.WriteLine("ITEM ID " + id + " deleted successfully, please refer the below table");

            syncprod(db);

            Console.WriteLine();

            runselectProduct(db);

            Console.WriteLine();

            Promptuser(db, "Product");

        }

        public void runselectprod(Database db, String strcustid)
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            DataTable dtselect = new DataTable();
            string[] arr1 = new string[] { "ITEM_ID", "PRD_EFF_DT", "PRD_END_DT", "UPC", "ITEM_DESCP", "SDV_AMT" };

            syncprod(db);

            foreach (String strselect in arr1)
            {
                dtselect.Columns.Add(strselect, typeof(String));
            }

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID),
           SelectResult.Property("ITEM_ID"),
           SelectResult.Property("PRD_EFF_DT"),
           SelectResult.Property("PRD_END_DT"),
           SelectResult.Property("UPC"),
           SelectResult.Property("ITEM_DESCP"),
           SelectResult.Property("SDV_AMT")
           ).From(DataSource.Database(db))
           .Where(Expression.Property("ITEM_ID").EqualTo(Expression.String(strcustid))))

            {

                foreach (var result in query.Execute())
                {
                    DataRow dr = dtselect.NewRow();

                    foreach (String strselect in arr1)
                    {
                        dr[strselect] = result.GetString(strselect);
                    }


                    dtselect.Rows.Add(dr);
                    dtselect.AcceptChanges();

                }

                PrettyPrintDataTable(dtselect);

            }

            Promptuser(db, "Product");
        }

        //Products End

        //Orders

        public void AddOrderMaster(Database db)
        {
            String straddstatus = String.Empty;
            Hashtable hstcustadd = new Hashtable();
            Console.WriteLine();
            string[] arr1 = new string[] { "DOC_NBR", "CUST_NBR" };

            foreach (String strcol in arr1)
            {
                String mutestr = String.Empty;
                bool userentry = true;

                Console.Write("Please enter " + strcol + " : ");
                mutestr = Console.ReadLine();
                Console.WriteLine();

                while (userentry)
                {
                    if (!String.IsNullOrEmpty(mutestr))
                    {
                        hstcustadd.Add(strcol, mutestr);
                        break;

                    }
                    else
                    {
                        Console.WriteLine("Please enter " + strcol);
                        mutestr = Console.ReadLine();
                        userentry = true;
                    }

                }

            }

            using (var CustomerDoc = new MutableDocument(hstcustadd["DOC_NBR"].ToString()))
            {

                CustomerDoc.SetString("DOC_NBR", hstcustadd["DOC_NBR"].ToString());
                CustomerDoc.SetString("CUST_NBR", hstcustadd["CUST_NBR"].ToString());
                CustomerDoc.SetString("ORDER_CREATED_DT", DateTime.Now.ToString("MM/dd/yyyy"));
                CustomerDoc.SetString("ITEM_ID", "06417601");
                CustomerDoc.SetString("ORD_QTY", "10");
                CustomerDoc.SetString("RETL_AMT", "100");
                //CustomerDoc.SetString("ORDER_DETAILS", "<DocNbr>" + hstcustadd["DOC_NBR"].ToString() + "</DocNbr><CustNbr>" + hstcustadd["CUST_NBR"].ToString() + "</CustNbr><ItemId>06417601</ItemId><OrdQty>1</OrdQty><RetlPriceAmt>0.99</RetlPriceAmt><TaxFlg>1</TaxFlg></SaleLineItemDtl>");

                db.Save(CustomerDoc);
                Console.WriteLine();
                Console.WriteLine("Order " + hstcustadd["DOC_NBR"].ToString() + " Created Succefully");

            }

            syncorder(db);

            Console.WriteLine();
            runselectOrders(db);
            Console.WriteLine();
            Promptuser(db, "Order");


        }

        public void DeleteOrderMaster(Database db)
        {
            String struptstatus = String.Empty;
            String id = String.Empty;
            string strnameupdate = String.Empty;
            //runselect(db);
            Console.WriteLine();
            Console.Write("Please enter valid Order Number from above Table : ");
            id = Console.ReadLine();
            Console.WriteLine();

            using (var doc = db.GetDocument(id))
            using (var mutableDoc = doc.ToMutable())
            {

                db.Delete(mutableDoc);
            }

            Console.WriteLine("Order Number " + id + " deleted successfully, please refer the below table");

            syncorder(db);

            Console.WriteLine();

            runselectOrders(db);

            Console.WriteLine();

            Promptuser(db, "Order");

        }

        public void runselectord(Database db, String strcustid)
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            DataTable dtselect = new DataTable();
            string[] arr1 = new string[] { "DOC_NBR", "CUST_NBR", "ORDER_CREATED_DT", "ITEM_ID", "ORD_QTY", "RETL_AMT" };

            syncorder(db);

            foreach (String strselect in arr1)
            {
                dtselect.Columns.Add(strselect, typeof(String));
            }

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID),
             SelectResult.Property("DOC_NBR"),
             SelectResult.Property("CUST_NBR"),
             SelectResult.Property("ORDER_CREATED_DT"),
             SelectResult.Property("ITEM_ID"),
             SelectResult.Property("ORD_QTY"),
             SelectResult.Property("RETL_AMT")
            ).From(DataSource.Database(db))
           .Where(Expression.Property("DOC_NBR").EqualTo(Expression.String(strcustid))))

            {

                foreach (var result in query.Execute())
                {
                    DataRow dr = dtselect.NewRow();

                    foreach (String strselect in arr1)
                    {
                        dr[strselect] = result.GetString(strselect);
                    }


                    dtselect.Rows.Add(dr);
                    dtselect.AcceptChanges();

                }

                PrettyPrintDataTable(dtselect);

            }

            Promptuser(db, "Order");
        }

        //Orders End


        public void PrettyPrintDataTable(DataTable table)
        {

            int zeilen = table.Rows.Count;
            int spalten = table.Columns.Count;

            // Header
            for (int i = 0; i < table.Columns.Count; i++)
            {
                string s = table.Columns[i].ToString();
                Console.Write(String.Format("{0,-20} | ", s));
            }
            Console.Write(Environment.NewLine);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Console.Write("---------------------|-");
            }
            Console.Write(Environment.NewLine);

            // Data
            for (int i = 0; i < zeilen; i++)
            {
                DataRow row = table.Rows[i];
                //Debug.WriteLine("{0} {1} ", row[0], row[1]);
                for (int j = 0; j < spalten; j++)
                {
                    string s = row[j].ToString();
                    if (s.Length > 20) s = s.Substring(0, 17) + "...";
                    Console.Write(String.Format("{0,-20} | ", s));
                }
                Console.Write(Environment.NewLine);
            }
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Console.Write("---------------------|-");
            }
            Console.Write(Environment.NewLine);
        }

        //------------------------------------------------------------------------------------------------------------


        public Database OpenCustDB()
        {
            Database dbcust;
            DatabaseConfiguration config = new DatabaseConfiguration() { Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) };

            dbcust = new Database("CustomersMasterdb", config);

            return dbcust;
        }

        public Database OpenProdDB()
        {
            DatabaseConfiguration config = new DatabaseConfiguration() { Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) };
            Database dbprod;

            dbprod = new Database("Productsdb", config);

            return dbprod;
        }

        public Database OpenOrderDB()
        {
            DatabaseConfiguration config = new DatabaseConfiguration() { Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) };
            Database dborder;

            dborder = new Database("Ordersdb", config);

            return dborder;
        }

        //-------------------------------------------------------------------------------------------------------------

        public void synccust(Database syncdb)
        {

            var url = new Uri("ws://104.214.56.109:4984/customer");

            var target = new URLEndpoint(url);
            var config = new ReplicatorConfiguration(syncdb, target)
            {
                ReplicatorType = ReplicatorType.PushAndPull
            };

            var replicator = new Replicator(config);
            replicator.Start();
        }

        public void syncprod(Database syncdb)
        {


            var url = new Uri("ws://104.214.56.109:4984/product");

            var target = new URLEndpoint(url);
            var config = new ReplicatorConfiguration(syncdb, target)
            {
                ReplicatorType = ReplicatorType.PushAndPull
            };

            var replicator = new Replicator(config);
            replicator.Start();
        }

        public void syncorder(Database syncdb)
        {

            var url = new Uri("ws://104.214.56.109:4984/order");

            var target = new URLEndpoint(url);
            var config = new ReplicatorConfiguration(syncdb, target)
            {
                ReplicatorType = ReplicatorType.Push
            };

            var replicator = new Replicator(config);
            replicator.Start();
        }


    }
}
